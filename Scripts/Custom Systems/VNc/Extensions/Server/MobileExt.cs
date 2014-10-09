#region Header
//   Vorspire    _,-'/-'/  MobileExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

using Server.Mobiles;

using VitaNex;
using VitaNex.SuperGumps.UI;
#endregion

namespace Server
{
	public static class MobileExtUtility
	{
		public static TimeSpan CombatHeatDelay = TimeSpan.FromSeconds(5.0);

		public static bool InCombat(this Mobile m)
		{
			return m != null &&
				   (m.Aggressed.Any(info => info.LastCombatTime + CombatHeatDelay > DateTime.UtcNow) ||
					m.Aggressors.Any(info => info.LastCombatTime + CombatHeatDelay > DateTime.UtcNow));
		}

		public static bool IsControlled(this Mobile m)
		{
			Mobile master;
			return IsControlled(m, out master);
		}

		public static bool IsControlled(this Mobile m, out Mobile master)
		{
			if (m is BaseCreature)
			{
				BaseCreature c = (BaseCreature)m;
				master = c.GetMaster();
				return c.Controlled;
			}

			master = null;
			return false;
		}

		public static void TryParalyze(this Mobile m, TimeSpan duration, TimerStateCallback<Mobile> callback = null)
		{
			m.Paralyze(duration);

			if (callback != null)
			{
				Timer.DelayCall(duration, callback, m);
			}
		}

		public static void TryFreeze(this Mobile m, TimeSpan duration, TimerStateCallback<Mobile> callback = null)
		{
			m.Freeze(duration);

			if (callback != null)
			{
				Timer.DelayCall(duration, callback, m);
			}
		}

		private static readonly MethodInfo _SleepImpl = typeof(Mobile).GetMethod("Sleep") ?? typeof(Mobile).GetMethod("DoSleep");

		public static void TrySleep(this Mobile m, TimeSpan duration, TimerStateCallback<Mobile> callback = null)
		{
			if (_SleepImpl != null)
			{
				VitaNexCore.TryCatch(
					() =>
					{
						_SleepImpl.Invoke(m, new object[] {duration});

						if (callback != null)
						{
							Timer.DelayCall(duration, callback, m);
						}
					});
			}
		}

		public static void SendNotification(
			this Mobile m,
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<NotifyGump> beforeSend = null,
			Action<NotifyGump> afterSend = null)
		{
			SendNotification<NotifyGump>(m, html, autoClose, delay, pause, color, beforeSend, afterSend);
		}

		public static void SendNotification<TGump>(
			this Mobile m,
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<TGump> beforeSend = null,
			Action<TGump> afterSend = null) where TGump : NotifyGump
		{
			if (!(m is PlayerMobile))
			{
				return;
			}

			PlayerMobile pm = (PlayerMobile)m;

			if (!pm.IsOnline())
			{
				return;
			}

			var ng = typeof(TGump).CreateInstanceSafe<TGump>(pm, html);

			if (ng != null)
			{
				ng.AutoClose = autoClose;
				ng.AnimDuration = TimeSpan.FromSeconds(Math.Max(0, delay));
				ng.PauseDuration = TimeSpan.FromSeconds(Math.Max(0, pause));
				ng.HtmlColor = color ?? Color.White;

				if (beforeSend != null)
				{
					beforeSend(ng);
				}

				ng.Send();

				if (afterSend != null)
				{
					afterSend(ng);
				}

				return;
			}

			html.Split(new[] {"\n", "<br>", "<BR>"}, StringSplitOptions.RemoveEmptyEntries)
				.Select(s => Regex.Replace(s, @"<[^>]*>", String.Empty))
				.ForEach(m.SendMessage);
		}

		public static Color GetNotorietyColor(this Mobile source, Mobile target = null)
		{
			if (source == null && target != null)
			{
				source = target;
			}

			if (source != null)
			{
				switch (Notoriety.Compute(source, target ?? source))
				{
					case Notoriety.Innocent:
						return Color.SkyBlue;
					case Notoriety.Ally:
						return Color.LawnGreen;
					case Notoriety.CanBeAttacked:
					case Notoriety.Criminal:
						return Color.Silver;
					case Notoriety.Enemy:
						return Color.OrangeRed;
					case Notoriety.Murderer:
						return Color.Red;
					case Notoriety.Invulnerable:
						return Color.Yellow;
				}
			}

			return Color.White;
		}
	}
}