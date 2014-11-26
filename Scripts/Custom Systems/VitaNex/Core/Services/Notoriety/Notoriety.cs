#region Header
//   Vorspire    _,-'/-'/  Notoriety.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Misc;
#endregion

namespace VitaNex
{
	public delegate T NotorietyHandler<out T>(Mobile x, Mobile y, out bool handled);

	public class NotorietyEntry<T>
	{
		public int Priority { get; set; }
		public NotorietyHandler<T> Handler { get; set; }

		public NotorietyEntry(int priority, NotorietyHandler<T> handler)
		{
			Priority = priority;
			Handler = handler;
		}
	}

	[CoreService("Notoriety", "1.0.0.0", TaskPriority.High)]
	public static class NotoUtility
	{
		public const int Bubble = -1;

		private static NotorietyHandler _NotorietyParent;
		private static AllowBeneficialHandler _BeneficialParent;
		private static AllowHarmfulHandler _HarmfulParent;

		public static NotorietyHandler NotorietyParent { get { return _NotorietyParent ?? (_NotorietyParent = Notoriety.Handler); } }
		public static AllowBeneficialHandler BeneficialParent { get { return _BeneficialParent ?? (_BeneficialParent = NotorietyHandlers.Mobile_AllowBeneficial); } }
		public static AllowHarmfulHandler HarmfulParent { get { return _HarmfulParent ?? (_HarmfulParent = NotorietyHandlers.Mobile_AllowHarmful); } }

		private static readonly List<NotorietyEntry<int>> _NameHandlers = new List<NotorietyEntry<int>>();
		private static readonly List<NotorietyEntry<bool>> _BeneficialHandlers = new List<NotorietyEntry<bool>>();
		private static readonly List<NotorietyEntry<bool>> _HarmfulHandlers = new List<NotorietyEntry<bool>>();

		public static List<NotorietyEntry<int>> NameHandlers { get { return _NameHandlers; } }
		public static List<NotorietyEntry<bool>> BeneficialHandlers { get { return _BeneficialHandlers; } }
		public static List<NotorietyEntry<bool>> HarmfulHandlers { get { return _HarmfulHandlers; } }

		private static void CSInvoke()
		{
			if (_NotorietyParent == null && Notoriety.Handler != MobileNotoriety)
			{
				_NotorietyParent = Notoriety.Handler ?? NotorietyHandlers.MobileNotoriety;
			}

			if (_BeneficialParent == null && Mobile.AllowBeneficialHandler != AllowBeneficial)
			{
				_BeneficialParent = Mobile.AllowBeneficialHandler ?? NotorietyHandlers.Mobile_AllowBeneficial;
			}

			if (_HarmfulParent == null && Mobile.AllowHarmfulHandler != AllowHarmful)
			{
				_HarmfulParent = Mobile.AllowHarmfulHandler ?? NotorietyHandlers.Mobile_AllowHarmful;
			}

			Notoriety.Handler = MobileNotoriety;
			Mobile.AllowBeneficialHandler = AllowBeneficial;
			Mobile.AllowHarmfulHandler = AllowHarmful;
		}

		public static void CSDispose()
		{
			Notoriety.Handler = _NotorietyParent ?? NotorietyHandlers.MobileNotoriety;
			Mobile.AllowBeneficialHandler = _BeneficialParent ?? NotorietyHandlers.Mobile_AllowBeneficial;
			Mobile.AllowHarmfulHandler = _HarmfulParent ?? NotorietyHandlers.Mobile_AllowHarmful;

			_NotorietyParent = null;
			_BeneficialParent = null;
			_HarmfulParent = null;
		}

		public static void RegisterNameHandler(NotorietyHandler<int> handler, int priority = 0)
		{
			UnregisterNameHandler(handler);
			_NameHandlers.Add(new NotorietyEntry<int>(priority, handler));
		}

		public static void UnregisterNameHandler(NotorietyHandler<int> handler)
		{
			_NameHandlers.RemoveAll(e => e.Handler == handler);
		}

		public static void RegisterBeneficialHandler(NotorietyHandler<bool> handler, int priority = 0)
		{
			UnregisterBeneficialHandler(handler);
			_BeneficialHandlers.Add(new NotorietyEntry<bool>(priority, handler));
		}

		public static void UnregisterBeneficialHandler(NotorietyHandler<bool> handler)
		{
			_BeneficialHandlers.RemoveAll(e => e.Handler == handler);
		}

		public static void RegisterHarmfulHandler(NotorietyHandler<bool> handler, int priority = 0)
		{
			UnregisterHarmfulHandler(handler);
			_HarmfulHandlers.Add(new NotorietyEntry<bool>(priority, handler));
		}

		public static void UnregisterHarmfulHandler(NotorietyHandler<bool> handler)
		{
			_HarmfulHandlers.RemoveAll(e => e.Handler == handler);
		}

		public static bool AllowBeneficial(Mobile a, Mobile b)
		{
			if (_BeneficialParent == null)
			{
				_BeneficialParent = NotorietyHandlers.Mobile_AllowBeneficial;
			}

			if (a == null || a.Deleted || b == null || b.Deleted)
			{
				return _BeneficialParent(a, b);
			}

			foreach (var handler in
				_BeneficialHandlers.OrderByDescending(e => e.Priority).Where(e => e.Handler != null).Select(e => e.Handler))
			{
				bool handled;
				bool result = handler(a, b, out handled);

				if (handled)
				{
					return result;
				}
			}

			return _BeneficialParent(a, b);
		}

		public static bool AllowHarmful(Mobile a, Mobile b)
		{
			if (_HarmfulParent == null)
			{
				_HarmfulParent = NotorietyHandlers.Mobile_AllowHarmful;
			}

			if (a == null || a.Deleted || b == null || b.Deleted)
			{
				return _HarmfulParent(a, b);
			}

			foreach (var handler in
				_HarmfulHandlers.OrderByDescending(e => e.Priority).Where(e => e.Handler != null).Select(e => e.Handler))
			{
				bool handled;
				bool result = handler(a, b, out handled);

				if (handled)
				{
					return result;
				}
			}

			return _HarmfulParent(a, b);
		}

		public static int MobileNotoriety(Mobile a, Mobile b)
		{
			if (_NotorietyParent == null)
			{
				_NotorietyParent = NotorietyHandlers.MobileNotoriety;
			}

			if (a == null || a.Deleted || b == null || b.Deleted)
			{
				return _NotorietyParent(a, b);
			}

			foreach (var handler in
				_NameHandlers.OrderByDescending(e => e.Priority).Where(e => e.Handler != null).Select(e => e.Handler))
			{
				bool handled;
				int result = handler(a, b, out handled);

				if (!handled)
				{
					continue;
				}

				if (result <= Bubble)
				{
					break;
				}

				return result;
			}

			return _NotorietyParent(a, b);
		}
	}
}