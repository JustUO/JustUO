#region Header
//   Vorspire    _,-'/-'/  Battle_Regions.cs
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

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract partial class PvPBattle
	{
		private PvPBattleRegion _BattleRegion;
		private PvPSpectateRegion _SpectateRegion;

		private bool _FloorItemDelete = true;
		private int _LightLevel = 100;

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleRegion BattleRegion
		{
			get { return _BattleRegion; }
			set
			{
				if (_BattleRegion == value)
				{
					return;
				}

				_BattleRegion = value;

				if (!Deserializing)
				{
					InvalidateBattleRegion();
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPSpectateRegion SpectateRegion
		{
			get { return _SpectateRegion; }
			set
			{
				if (_SpectateRegion == value)
				{
					return;
				}

				_SpectateRegion = value;

				if (!Deserializing)
				{
					InvalidateSpectateRegion();
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual bool FloorItemDelete
		{
			//
			get { return _BattleRegion == null ? _FloorItemDelete : (_FloorItemDelete = _BattleRegion.FloorItemDelete); }
			set { _FloorItemDelete = _BattleRegion == null ? value : (_BattleRegion.FloorItemDelete = value); }
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual int LightLevel { get { return _LightLevel; } set { _LightLevel = Math.Max(0, Math.Min(100, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual string BattleRegionName { get { return String.Format("{0} ({1})", Name, Serial); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual string SpectateRegionName { get { return String.Format("{0} (Safe) ({1})", Name, Serial); } }

		public virtual void InvalidateRegions()
		{
			if (Deserializing)
			{
				return;
			}

			InvalidateBattleRegion();
			InvalidateSpectateRegion();
		}

		public virtual void InvalidateBattleRegion()
		{
			if (Deserializing)
			{
				return;
			}

			if (_BattleRegion != null)
			{
				if (_BattleRegion.Map == Map &&
					_BattleRegion.Area.GetBoundsHashCode() == Options.Locations.BattleBounds.GetBoundsHashCode())
				{
					return;
				}

				_BattleRegion.Unregister();
			}

			if (Options.Locations.BattleFixedPoint == Point3D.Zero)
			{
				_BattleRegion = null;
				return;
			}

			_BattleRegion = _BattleRegion != null ? _BattleRegion.Clone(this) : RegionExtUtility.Create<PvPBattleRegion>(this);

			if (_BattleRegion == null)
			{
				return;
			}

			_BattleRegion.GoLocation = Options.Locations.BattleFixedPoint;
			_BattleRegion.Register();
		}

		public virtual void InvalidateSpectateRegion()
		{
			if (Deserializing)
			{
				return;
			}

			if (_SpectateRegion != null)
			{
				if (_SpectateRegion.Map == Map &&
					_SpectateRegion.Area.GetBoundsHashCode() == Options.Locations.SpectateBounds.GetBoundsHashCode())
				{
					return;
				}

				_SpectateRegion.Unregister();
			}

			if (Options.Locations.SpectateFixedPoint == Point3D.Zero)
			{
				_SpectateRegion = null;
				return;
			}

			_SpectateRegion = _SpectateRegion != null
								  ? _SpectateRegion.Clone(this)
								  : RegionExtUtility.Create<PvPSpectateRegion>(this);

			if (_SpectateRegion == null)
			{
				return;
			}

			_SpectateRegion.GoLocation = Options.Locations.SpectateFixedPoint;
			_SpectateRegion.Register();
		}

		public virtual int NotorietyHandler(PlayerMobile source, PlayerMobile target)
		{
			if (source == null || source.Deleted || target == null || target.Deleted)
			{
				return BattleNotoriety.Bubble;
			}

			if (State == PvPBattleState.Internal)
			{
				return BattleNotoriety.Bubble;
			}

			PvPTeam teamA, teamB;

			if (IsParticipant(source, out teamA) && IsParticipant(target, out teamB))
			{
				if (State != PvPBattleState.Running)
				{
					return Notoriety.Invulnerable;
				}

				if (teamA == teamB)
				{
					if (CanDamageOwnTeam(source, target))
					{
						return Notoriety.Enemy;
					}

					return Notoriety.Ally;
				}

				if (CanDamageEnemyTeam(source, target))
				{
					return Notoriety.Enemy;
				}

				return Notoriety.Invulnerable;
			}

			if ((source.Region != null && (source.Region.IsPartOf(SpectateRegion) || source.Region.IsPartOf(BattleRegion))) ||
				(target.Region != null && (target.Region.IsPartOf(SpectateRegion) || target.Region.IsPartOf(BattleRegion))))
			{
				return Notoriety.Invulnerable;
			}

			return BattleNotoriety.Bubble;
		}

		public virtual bool AcceptsSpawnsFrom(Region region)
		{
			return AllowSpawn() && region != null && (region.IsPartOf(BattleRegion) || region.IsPartOf(SpectateRegion));
		}

		public virtual void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			personal = personal != LightLevel ? LightLevel : personal;
		}

		public virtual void OnLocationChanged(Mobile m, Point3D oldLocation)
		{
			if (m == null || m.Deleted)
			{
				return;
			}

			if (m.Region != null && (m.Region.IsPartOf(SpectateRegion) || m.Region.IsPartOf(BattleRegion)))
			{
				CheckDismount(m);
				InvalidateStray(m);
			}

			PlayerMobile pm = m as PlayerMobile;

			if (pm == null)
			{
				return;
			}

			PvPTeam team;

			if (!IsParticipant(pm, out team))
			{
				return;
			}

			if (team != null && !team.Deleted)
			{
				team.UpdateActivity(pm);
			}
		}

		public virtual bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			return true;
		}

		public virtual void OnEnter(PvPRegion region, Mobile m)
		{
			if (region == null || m == null || m.Deleted)
			{
				return;
			}

			PlayerMobile pm = m as PlayerMobile;

			if (pm != null)
			{
				if (region.IsPartOf(BattleRegion))
				{
					pm.SendMessage("You have entered {0}", Name);
				}
				else if (region.IsPartOf(SpectateRegion))
				{
					pm.SendMessage("You have entered {0} spectator area.", Name);

					if (!IsSpectator(pm))
					{
						AddSpectator(pm, false);
					}
				}
			}

			m.Delta(MobileDelta.Noto);
		}

		public virtual void OnExit(PvPRegion region, Mobile m)
		{
			if (region == null || m == null || m.Deleted)
			{
				return;
			}

			PlayerMobile pm = m as PlayerMobile;

			if (pm != null)
			{
				if (region.IsPartOf(BattleRegion))
				{
					if (IsParticipant(pm))
					{
						Eject(pm, false);
					}

					pm.SendMessage("You have left {0}", Name);
				}
				else if (region.IsPartOf(SpectateRegion))
				{
					pm.SendMessage("You have left {0} spectator area", Name);

					if (IsSpectator(pm))
					{
						RemoveSpectator(pm, false);
					}
				}
			}

			m.Delta(MobileDelta.Noto);
		}

		public bool AllowBeneficial(Mobile m, Mobile target)
		{
			if (CheckAllowBeneficial(m, target))
			{
				OnAllowBeneficialAccept(m, target);
				return true;
			}

			OnAllowBeneficialDeny(m, target);
			return false;
		}

		public virtual bool CheckAllowBeneficial(Mobile m, Mobile target)
		{
			if (m == null || m.Deleted || target == null || target.Deleted)
			{
				return false;
			}

			if (State != PvPBattleState.Preparing && State != PvPBattleState.Running)
			{
				return State == PvPBattleState.Internal || Hidden;
			}

			if (!Options.Rules.AllowBeneficial)
			{
				return false;
			}

			bool checkRegions = false;

			if (m is BaseCreature && target is BaseCreature)
			{
				BaseCreature mC = (BaseCreature)m;
				BaseCreature targetC = (BaseCreature)target;

				if (BattleNotoriety.BeneficialParent != null)
				{
					return BattleNotoriety.BeneficialParent(mC, targetC);
				}

				checkRegions = true;
			}
			else if (m is BaseCreature && target is PlayerMobile)
			{
				BaseCreature mC = (BaseCreature)m;
				PlayerMobile targetP = (PlayerMobile)target;

				if (IsParticipant(targetP))
				{
					if (BattleNotoriety.BeneficialParent != null)
					{
						return BattleNotoriety.BeneficialParent(mC, targetP);
					}
				}
				else
				{
					checkRegions = true;
				}
			}
			else if (m is PlayerMobile && target is BaseCreature)
			{
				PlayerMobile mP = (PlayerMobile)m;
				BaseCreature targetC = (BaseCreature)target;

				if (IsParticipant(mP))
				{
					if (BattleNotoriety.BeneficialParent != null)
					{
						return BattleNotoriety.BeneficialParent(mP, targetC);
					}
				}
				else
				{
					checkRegions = true;
				}
			}
			else if (m is PlayerMobile && target is PlayerMobile)
			{
				PvPTeam teamA, teamB;
				PlayerMobile mP = (PlayerMobile)m, targetP = (PlayerMobile)target;

				if (IsParticipant(mP, out teamA) && IsParticipant(targetP, out teamB))
				{
					if (teamA == teamB)
					{
						if (!CanHealOwnTeam(mP, targetP))
						{
							return false;
						}
					}
					else if (!CanHealEnemyTeam(mP, targetP))
					{
						return false;
					}
				}
				else
				{
					checkRegions = true;
				}
			}

			return !checkRegions || m.Region == null || target.Region == null ||
				   ((!m.InRegion(BattleRegion) || !target.InRegion(BattleRegion)) &&
					(!m.InRegion(SpectateRegion) || !target.InRegion(SpectateRegion)) &&
					(!m.InRegion(BattleRegion) || !target.InRegion(SpectateRegion)) &&
					(!m.InRegion(SpectateRegion) || !target.InRegion(BattleRegion)));
		}

		protected virtual void OnAllowBeneficialAccept(Mobile m, Mobile target)
		{ }

		protected virtual void OnAllowBeneficialDeny(Mobile m, Mobile target)
		{
			if (m != null && !m.Deleted && target != null && !target.Deleted && m != target)
			{
				m.SendMessage("You can not perform beneficial actions on your target.");
			}
		}

		public bool AllowHarmful(Mobile m, Mobile target)
		{
			if (CheckAllowHarmful(m, target))
			{
				OnAllowHarmfulAccept(m, target);
				return true;
			}

			OnAllowHarmfulDeny(m, target);
			return false;
		}

		public virtual bool CheckAllowHarmful(Mobile m, Mobile target)
		{
			if (m == null || m.Deleted || target == null || target.Deleted)
			{
				return false;
			}

			if (State != PvPBattleState.Running)
			{
				return State == PvPBattleState.Internal || Hidden;
			}

			if (!Options.Rules.AllowHarmful)
			{
				return false;
			}

			bool checkRegions = false;

			if (m is BaseCreature && target is BaseCreature)
			{
				BaseCreature mC = (BaseCreature)m;
				BaseCreature targetC = (BaseCreature)target;

				if (BattleNotoriety.HarmfulParent != null)
				{
					return BattleNotoriety.HarmfulParent(mC, targetC);
				}

				checkRegions = true;
			}
			else if (m is BaseCreature && target is PlayerMobile)
			{
				BaseCreature mC = (BaseCreature)m;
				PlayerMobile targetP = (PlayerMobile)target;

				if (IsParticipant(targetP))
				{
					if (BattleNotoriety.HarmfulParent != null)
					{
						return BattleNotoriety.HarmfulParent(mC, targetP);
					}
				}
				else
				{
					checkRegions = true;
				}
			}
			else if (m is PlayerMobile && target is BaseCreature)
			{
				PlayerMobile mP = (PlayerMobile)m;
				BaseCreature targetC = (BaseCreature)target;

				if (IsParticipant(mP))
				{
					if (BattleNotoriety.HarmfulParent != null)
					{
						return BattleNotoriety.HarmfulParent(mP, targetC);
					}
				}
				else
				{
					checkRegions = true;
				}
			}
			else if (m is PlayerMobile && target is PlayerMobile)
			{
				PvPTeam teamA, teamB;
				PlayerMobile mP = (PlayerMobile)m, targetP = (PlayerMobile)target;

				if (IsParticipant(mP, out teamA) && IsParticipant(targetP, out teamB))
				{
					if (teamA == teamB)
					{
						if (!CanDamageOwnTeam(mP, targetP))
						{
							return false;
						}
					}
					else if (!CanDamageEnemyTeam(mP, targetP))
					{
						return false;
					}
				}
				else
				{
					checkRegions = true;
				}
			}

			return !checkRegions || m.Region == null || target.Region == null ||
				   ((!m.InRegion(BattleRegion) || !target.InRegion(BattleRegion)) &&
					(!m.InRegion(SpectateRegion) || !target.InRegion(SpectateRegion)) &&
					(!m.InRegion(BattleRegion) || !target.InRegion(SpectateRegion)) &&
					(!m.InRegion(SpectateRegion) || !target.InRegion(BattleRegion)));
		}

		protected virtual void OnAllowHarmfulAccept(Mobile m, Mobile target)
		{ }

		protected virtual void OnAllowHarmfulDeny(Mobile m, Mobile target)
		{
			if (m != null && !m.Deleted && target != null && !target.Deleted && m != target)
			{
				m.SendMessage("You can not perform harmful actions on your target.");
			}
		}
	}
}