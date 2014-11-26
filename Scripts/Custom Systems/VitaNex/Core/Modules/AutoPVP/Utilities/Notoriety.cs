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

using Server;
using Server.Misc;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public delegate T BattleNotorietyHandler<out T>(PlayerMobile x, PlayerMobile y);

	public static class BattleNotoriety
	{
		public const int Bubble = -1;

		private static NotorietyHandler _NotorietyParent;
		private static AllowBeneficialHandler _BeneficialParent;
		private static AllowHarmfulHandler _HarmfulParent;

		private static readonly Dictionary<PvPBattle, BattleNotorietyHandler<int>> _NameHandlers =
			new Dictionary<PvPBattle, BattleNotorietyHandler<int>>();

		private static readonly Dictionary<PvPBattle, BattleNotorietyHandler<bool>> _BeneficialHandlers =
			new Dictionary<PvPBattle, BattleNotorietyHandler<bool>>();

		private static readonly Dictionary<PvPBattle, BattleNotorietyHandler<bool>> _HarmfulHandlers =
			new Dictionary<PvPBattle, BattleNotorietyHandler<bool>>();

		public static NotorietyHandler NotorietyParent
		{
			//
			get { return _NotorietyParent ?? (_NotorietyParent = Notoriety.Handler); }
		}

		public static AllowBeneficialHandler BeneficialParent
		{
			//
			get { return _BeneficialParent ?? (_BeneficialParent = NotorietyHandlers.Mobile_AllowBeneficial); }
		}

		public static AllowHarmfulHandler HarmfulParent
		{
			//
			get { return _HarmfulParent ?? (_HarmfulParent = NotorietyHandlers.Mobile_AllowHarmful); }
		}

		public static Dictionary<PvPBattle, BattleNotorietyHandler<int>> NameHandlers { get { return _NameHandlers; } }
		public static Dictionary<PvPBattle, BattleNotorietyHandler<bool>> BeneficialHandlers { get { return _BeneficialHandlers; } }
		public static Dictionary<PvPBattle, BattleNotorietyHandler<bool>> HarmfulHandlers { get { return _HarmfulHandlers; } }

		public static void RegisterNotorietyHandler(PvPBattle battle, BattleNotorietyHandler<int> handler)
		{
			if (!_NameHandlers.ContainsKey(battle))
			{
				_NameHandlers.Add(battle, handler);
			}
			else
			{
				_NameHandlers[battle] = handler;
			}
		}

		public static void RegisterAllowBeneficialHandler(PvPBattle battle, BattleNotorietyHandler<bool> handler)
		{
			if (!_BeneficialHandlers.ContainsKey(battle))
			{
				_BeneficialHandlers.Add(battle, handler);
			}
			else
			{
				_BeneficialHandlers[battle] = handler;
			}
		}

		public static void RegisterAllowHarmfulHandler(PvPBattle battle, BattleNotorietyHandler<bool> handler)
		{
			if (!_HarmfulHandlers.ContainsKey(battle))
			{
				_HarmfulHandlers.Add(battle, handler);
			}
			else
			{
				_HarmfulHandlers[battle] = handler;
			}
		}

		public static void Enable()
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

		public static void Disable()
		{
			Notoriety.Handler = _NotorietyParent ?? NotorietyHandlers.MobileNotoriety;
			Mobile.AllowBeneficialHandler = _BeneficialParent ?? NotorietyHandlers.Mobile_AllowBeneficial;
			Mobile.AllowHarmfulHandler = _HarmfulParent ?? NotorietyHandlers.Mobile_AllowHarmful;

			_NotorietyParent = null;
			_BeneficialParent = null;
			_HarmfulParent = null;
		}

		public static bool AllowBeneficial(Mobile a, Mobile b)
		{
			if (_BeneficialParent == null)
			{
				return NotorietyHandlers.Mobile_AllowBeneficial(a, b);
			}

			if (a == null || a.Deleted || b == null || b.Deleted || a == b)
			{
				return _BeneficialParent(a, b);
			}

			if (a is PlayerMobile && b is PlayerMobile)
			{
				PlayerMobile x = (PlayerMobile)a, y = (PlayerMobile)b;
				PvPBattle battleA = AutoPvP.FindBattle(x), battleB = AutoPvP.FindBattle(y);

				if (battleA == null || battleA.Deleted || battleB == null || battleB.Deleted || battleA != battleB || x == y)
				{
					return _BeneficialParent(x, y);
				}

				PvPBattle battle = battleA;

				if (_BeneficialHandlers.ContainsKey(battle) && _BeneficialHandlers[battle] != null)
				{
					_BeneficialHandlers[battle](x, y);
				}
			}

			return _BeneficialParent(a, b);
		}

		public static bool AllowHarmful(Mobile a, Mobile b)
		{
			if (_HarmfulParent == null)
			{
				return NotorietyHandlers.Mobile_AllowHarmful(a, b);
			}

			if (a == null || a.Deleted || b == null || b.Deleted || a == b)
			{
				return _HarmfulParent(a, b);
			}

			if (a is PlayerMobile && b is PlayerMobile)
			{
				PlayerMobile x = (PlayerMobile)a, y = (PlayerMobile)b;
				PvPBattle battleA = AutoPvP.FindBattle(x), battleB = AutoPvP.FindBattle(y);

				if (battleA == null || battleA.Deleted || battleB == null || battleB.Deleted || battleA != battleB || x == y)
				{
					return _HarmfulParent(x, y);
				}

				PvPBattle battle = battleA;

				if (_HarmfulHandlers.ContainsKey(battle) && _HarmfulHandlers[battle] != null)
				{
					return _HarmfulHandlers[battle](x, y);
				}
			}

			return _HarmfulParent(a, b);
		}

		public static int MobileNotoriety(Mobile a, Mobile b)
		{
			if (_NotorietyParent == null)
			{
				return NotorietyHandlers.MobileNotoriety(a, b);
			}

			if (a == null || a.Deleted || b == null || b.Deleted || a == b)
			{
				return _NotorietyParent(a, b);
			}

			if (a is PlayerMobile && b is PlayerMobile)
			{
				PlayerMobile x = (PlayerMobile)a, y = (PlayerMobile)b;
				PvPBattle battleA = AutoPvP.FindBattle(x), battleB = AutoPvP.FindBattle(y);

				if (battleA == null || battleA.Deleted || battleB == null || battleB.Deleted || battleA != battleB || x == y)
				{
					return _NotorietyParent(x, y);
				}

				PvPBattle battle = battleA;

				if (_NameHandlers.ContainsKey(battle) && _NameHandlers[battle] != null)
				{
					int val = _NameHandlers[battle](x, y);

					if (val == Bubble)
					{
						val = _NotorietyParent(x, y);
					}

					return val;
				}
			}

			return _NotorietyParent(a, b);
		}
	}
}