#region Header
//   Vorspire    _,-'/-'/  Regions.cs
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
using System.Linq;

using Server;
using Server.Items;
using Server.Regions;
using Server.Targeting;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract class PvPRegion : BaseRegion
	{
		public PvPBattle Battle { get; private set; }

		public PvPRegion(PvPBattle battle, string name, params Rectangle3D[] bounds)
			: base(
				name,
				battle.Options.Locations.Map,
				battle.Options.Locations.BattlePriority,
				(bounds ?? new Rectangle3D[0]).ZFix().ToArray())
		{
			Battle = battle;
		}

		public PvPRegion(PvPBattle battle, string name, GenericReader reader)
			: this(battle, name)
		{
			Deserialize(reader);
		}

		public void MicroSync()
		{
			if (Battle != null)
			{
				OnMicroSync();
			}
		}

		public virtual void OnMicroSync()
		{ }

		public override bool AllowBeneficial(Mobile from, Mobile target)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden &&
				!Battle.AllowBeneficial(from, target))
			{
				return false;
			}

			return base.AllowBeneficial(from, target);
		}

		public override bool AllowHarmful(Mobile from, Mobile target)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.AllowHarmful(from, target))
			{
				return false;
			}

			return base.AllowHarmful(from, target);
		}

		public override bool AllowHousing(Mobile from, Point3D p)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.AllowHousing(from, p))
			{
				return false;
			}

			return base.AllowHousing(from, p);
		}

		public override bool AllowSpawn()
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.AllowSpawn())
			{
				return false;
			}

			return base.AllowSpawn();
		}

		public override bool CanUseStuckMenu(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.CanUseStuckMenu(m))
			{
				return false;
			}

			return base.CanUseStuckMenu(m);
		}

		public override void OnAggressed(Mobile aggressor, Mobile aggressed, bool criminal)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnAggressed(aggressor, aggressed, criminal);
			}

			base.OnAggressed(aggressor, aggressed, criminal);
		}

		public override bool AcceptsSpawnsFrom(Region region)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.AcceptsSpawnsFrom(region))
			{
				return false;
			}

			return base.AcceptsSpawnsFrom(region);
		}

		public override void AlterLightLevel(Mobile m, ref int global, ref int personal)
		{
			base.AlterLightLevel(m, ref global, ref personal);

			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.AlterLightLevel(m, ref global, ref personal);
			}
		}

		public override bool CheckAccessibility(Item item, Mobile from)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden &&
				!Battle.CheckAccessibility(item, from))
			{
				return false;
			}

			return base.CheckAccessibility(item, from);
		}

		public override TimeSpan GetLogoutDelay(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				return Battle.GetLogoutDelay(m);
			}

			return base.GetLogoutDelay(m);
		}

		public override bool OnDecay(Item item)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnDecay(item))
			{
				return false;
			}

			return base.OnDecay(item);
		}

		public override bool OnBeginSpellCast(Mobile m, ISpell s)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnBeginSpellCast(m, s))
			{
				return false;
			}

			return base.OnBeginSpellCast(m, s);
		}

		public override void OnBeneficialAction(Mobile helper, Mobile target)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnBeneficialAction(helper, target);
			}

			base.OnBeneficialAction(helper, target);
		}

		public override bool OnCombatantChange(Mobile m, Mobile oldMob, Mobile newMob)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden &&
				!Battle.OnCombatantChange(m, oldMob, newMob))
			{
				return false;
			}

			return base.OnCombatantChange(m, oldMob, newMob);
		}

		public override void OnCriminalAction(Mobile m, bool message)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnCriminalAction(m, message);
			}

			base.OnCriminalAction(m, message);
		}

		public override bool OnDamage(Mobile m, ref int damage)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden &&
				!Battle.OnDamage(m.FindMostRecentDamager(true), m, ref damage))
			{
				return false;
			}

			return base.OnDamage(m, ref damage);
		}

		public override bool OnBeforeDeath(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnBeforeDeath(m))
			{
				return false;
			}

			return base.OnBeforeDeath(m);
		}

		public override void OnDeath(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnDeath(m);
			}

			base.OnDeath(m);
		}

		public override void OnDidHarmful(Mobile harmer, Mobile harmed)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnDidHarmful(harmer, harmed);
			}

			base.OnDidHarmful(harmer, harmed);
		}

		public override bool OnSingleClick(Mobile m, object o)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnSingleClick(m, o))
			{
				return false;
			}

			return base.OnSingleClick(m, o);
		}

		public override bool OnDoubleClick(Mobile m, object o)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnDoubleClick(m, o))
			{
				return false;
			}

			return base.OnDoubleClick(m, o);
		}

		public override void OnEnter(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnEnter(this, m);
			}

			base.OnEnter(m);
		}

		public override void OnExit(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnExit(this, m);
			}

			base.OnExit(m);
		}

		public override void OnGotBeneficialAction(Mobile helper, Mobile target)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnGotBeneficialAction(helper, target);
			}

			base.OnGotBeneficialAction(helper, target);
		}

		public override void OnGotHarmful(Mobile harmer, Mobile harmed)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnGotHarmful(harmer, harmed);
			}

			base.OnGotHarmful(harmer, harmed);
		}

		public override bool OnHeal(Mobile m, ref int heal)
		{
			// There is no way to retrieve the healer.
			// There is no HealStore implementation of the DamageStore feature.
			//Mobile from = null;

			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnHeal(null, m, ref heal))
			{
				return false;
			}

			return base.OnHeal(m, ref heal);
		}

		public override void OnLocationChanged(Mobile m, Point3D oldLocation)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnLocationChanged(m, oldLocation);
			}

			base.OnLocationChanged(m, oldLocation);
		}

		public override bool OnMoveInto(Mobile m, Direction d, Point3D newLocation, Point3D oldLocation)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden &&
				!Battle.OnMoveInto(m, d, newLocation, oldLocation))
			{
				return false;
			}

			return base.OnMoveInto(m, d, newLocation, oldLocation);
		}

		public override bool OnResurrect(Mobile m)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnResurrect(m))
			{
				return false;
			}

			return base.OnResurrect(m);
		}

		public override bool OnSkillUse(Mobile m, int skill)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnSkillUse(m, skill))
			{
				return false;
			}

			return base.OnSkillUse(m, skill);
		}

		public override void OnSpeech(SpeechEventArgs args)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnSpeech(args);
			}

			base.OnSpeech(args);
		}

		public override void OnSpellCast(Mobile m, ISpell s)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.OnSpellCast(m, s);
			}

			base.OnSpellCast(m, s);
		}

		public override bool OnTarget(Mobile m, Target t, object o)
		{
			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden && !Battle.OnTarget(m, t, o))
			{
				return false;
			}

			return base.OnTarget(m, t, o);
		}

		public override void SpellDamageScalar(Mobile caster, Mobile target, ref double damage)
		{
			base.SpellDamageScalar(caster, target, ref damage);

			if (Battle != null && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Battle.SpellDamageScalar(caster, target, ref damage);
			}
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					break;
			}
		}
	}

	[PropertyObject]
	public class PvPBattleRegion : PvPRegion
	{
		public bool FloorItemDelete { get; set; }

		public PvPBattleRegion(PvPBattle battle)
			: base(battle, battle.BattleRegionName, battle.Options.Locations.BattleBounds.ToArray())
		{ }

		public PvPBattleRegion(PvPBattle battle, GenericReader reader)
			: base(battle, battle.BattleRegionName, reader)
		{ }

		public override void OnMicroSync()
		{
			base.OnMicroSync();

			if (FloorItemDelete && Battle.State != PvPBattleState.Internal && !Battle.Hidden)
			{
				Area.ForEach(
					r =>
					{
						var items = Map.GetItemsInBounds(r.ToRectangle2D());

						items.OfType<Item>()
							 .Not(i => i == null || i.Deleted || i is Static || i is LOSBlocker || i is Blocker)
							 .Where(i => i.Movable && i.Visible && i.Decays)
							 .ForEach(i => i.Delete());

						items.Free();
					});
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					writer.Write(FloorItemDelete);
					goto case 0;
				case 0:
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 1:
					FloorItemDelete = reader.ReadBool();
					goto case 0;
				case 0:
					break;
			}
		}
	}

	[PropertyObject]
	public class PvPSpectateRegion : PvPRegion
	{
		public PvPSpectateRegion(PvPBattle battle)
			: base(battle, battle.SpectateRegionName, battle.Options.Locations.SpectateBounds.ToArray())
		{ }

		public PvPSpectateRegion(PvPBattle battle, GenericReader reader)
			: base(battle, battle.SpectateRegionName, reader)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}