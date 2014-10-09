#region Header
//   Vorspire    _,-'/-'/  Notify.cs
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
using System.Text.RegularExpressions;

using Server;
using Server.Commands;
using Server.Mobiles;
using Server.Network;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Commands
{
	public static class NotifyCommand
	{
		private static readonly Type[] _SubTypes = typeof(WorldNotifyGump).GetConstructableChildren(t => t.IsNested);

		private static int _SubIndex;

		public static void Initialize()
		{
			CommandSystem.Register("Notify", AccessLevel.GameMaster, HandleNotify);
			CommandSystem.Register("NotifyAC", AccessLevel.GameMaster, HandleNotifyAC);
		}

		[Usage("Notify <text | html | bbc>")]
		[Description("Send a global notification gump to all online clients, " + //
					 "containing a message parsed from HTML, BBS or plain text.")]
		private static void HandleNotify(CommandEventArgs e)
		{
			if(ValidateCommand(e))
			{
				Notify(e.Mobile, e.ArgString, false);
			}
		}

		[Usage("NotifyAC <text | html | bbc>")]
		[Description("Send a global notification gump to all online clients, " + //
					 "containing a message parsed from HTML, BBS or plain text, " + //
					 "which auto-closes after 10 seconds.")]
		private static void HandleNotifyAC(CommandEventArgs e)
		{
			if(ValidateCommand(e))
			{
				Notify(e.Mobile, e.ArgString, true);
			}
		}

		private static bool ValidateCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null)
			{
				return false;
			}

			if(String.IsNullOrWhiteSpace(e.ArgString) ||
			   String.IsNullOrWhiteSpace(Regex.Replace(e.ArgString, @"<[^>]*>", String.Empty)))
			{
				e.Mobile.SendMessage(0x22, "Html/BBC message must be at least 1 character and not all white-space after parsing.");
				e.Mobile.SendMessage(0x22, "Usage: {0}{1} <text | html | bbc>", CommandSystem.Prefix, e.Command);
				return false;
			}

			return true;
		}

		public static void Notify(Mobile m, string message)
		{
			Notify(m, message, false);
		}

		public static void Notify(Mobile m, string message, bool autoClose)
		{
			if(m == null || m.Deleted || !(m is PlayerMobile))
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					var type = _SubTypes[_SubIndex++];

					if(type == null)
					{
						return;
					}

					message = String.Format("[{0}] {1}:\n{2}", DateTime.Now.ToSimpleString("t@h:m@"), m.RawName, message);

					NetState.Instances.AsParallel()
							.Where(ns => ns != null && ns.Mobile != null && !ns.Mobile.Deleted && ns.Mobile is PlayerMobile)
							.Select(ns => type.CreateInstanceSafe<WorldNotifyGump>((PlayerMobile)ns.Mobile, message, autoClose))
							.Where(g => g != null)
							.ForEach(g => g.Send());
				});

			if(_SubIndex >= _SubTypes.Length)
			{
				_SubIndex = 0;
			}
		}

		public class WorldNotifyGump : NotifyGump
		{
			public WorldNotifyGump(PlayerMobile user, string html, bool autoClose)
				: base(user, html)
			{
				AnimDuration = TimeSpan.FromSeconds(1.0);
				PauseDuration = TimeSpan.FromSeconds(10.0);
				HtmlColor = Color.Yellow;
				AutoClose = autoClose;
			}

			public class Sub0 : WorldNotifyGump
			{
				public Sub0(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub1 : WorldNotifyGump
			{
				public Sub1(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub2 : WorldNotifyGump
			{
				public Sub2(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub3 : WorldNotifyGump
			{
				public Sub3(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub4 : WorldNotifyGump
			{
				public Sub4(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub5 : WorldNotifyGump
			{
				public Sub5(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub6 : WorldNotifyGump
			{
				public Sub6(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub7 : WorldNotifyGump
			{
				public Sub7(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub8 : WorldNotifyGump
			{
				public Sub8(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}

			public class Sub9 : WorldNotifyGump
			{
				public Sub9(PlayerMobile user, string html, bool autoClose)
					: base(user, html, autoClose)
				{ }
			}
		}
	}
}