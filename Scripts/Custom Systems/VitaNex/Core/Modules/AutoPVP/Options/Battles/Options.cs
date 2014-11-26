#region Header
//   Vorspire    _,-'/-'/  Options.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPBattleOptions : PropertyObject
	{
		public PvPBattleOptions()
		{
			Broadcasts = new PvPBattleBroadcasts();
			Locations = new PvPBattleLocations();
			Restrictions = new PvPBattleRestrictions();
			Rewards = new PvPRewards();
			Rules = new PvPBattleRules();
			Sounds = new PvPBattleSounds();
			SuddenDeath = new PvPBattleSuddenDeath();
			Timing = new PvPBattleTiming();
			Weather = new PvPBattleWeather();
		}

		public PvPBattleOptions(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleBroadcasts Broadcasts { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleLocations Locations { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleRestrictions Restrictions { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPRewards Rewards { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleRules Rules { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleSounds Sounds { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleSuddenDeath SuddenDeath { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleTiming Timing { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleWeather Weather { get; set; }

		public override void Clear()
		{
			Broadcasts.Clear();
			Locations.Clear();
			Restrictions.Clear();
			Rewards.Clear();
			Rules.Clear();
			Sounds.Clear();
			SuddenDeath.Clear();
			Timing.Clear();
			Weather.Clear();
		}

		public override void Reset()
		{
			Broadcasts.Reset();
			Locations.Reset();
			Restrictions.Reset();
			Rewards.Reset();
			Rules.Reset();
			Sounds.Reset();
			SuddenDeath.Reset();
			Timing.Reset();
			Weather.Reset();
		}

		public override string ToString()
		{
			return "Advanced Options";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlock(w => w.WriteType(Broadcasts, t => Broadcasts.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Locations, t => Locations.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Restrictions, t => Restrictions.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Rewards, t => Rewards.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Rules, t => Rules.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Sounds, t => Sounds.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(SuddenDeath, t => SuddenDeath.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Timing, t => Timing.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Weather, t => Weather.Serialize(w)));
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlock(r => Broadcasts = r.ReadTypeCreate<PvPBattleBroadcasts>(r) ?? new PvPBattleBroadcasts(r));
						reader.ReadBlock(r => Locations = r.ReadTypeCreate<PvPBattleLocations>(r) ?? new PvPBattleLocations(r));
						reader.ReadBlock(r => Restrictions = r.ReadTypeCreate<PvPBattleRestrictions>(r) ?? new PvPBattleRestrictions(r));
						reader.ReadBlock(r => Rewards = r.ReadTypeCreate<PvPRewards>(r) ?? new PvPRewards(r));
						reader.ReadBlock(r => Rules = r.ReadTypeCreate<PvPBattleRules>(r) ?? new PvPBattleRules(r));
						reader.ReadBlock(r => Sounds = r.ReadTypeCreate<PvPBattleSounds>(r) ?? new PvPBattleSounds(r));
						reader.ReadBlock(r => SuddenDeath = r.ReadTypeCreate<PvPBattleSuddenDeath>(r) ?? new PvPBattleSuddenDeath(r));
						reader.ReadBlock(r => Timing = r.ReadTypeCreate<PvPBattleTiming>(r) ?? new PvPBattleTiming(r));
						reader.ReadBlock(r => Weather = r.ReadTypeCreate<PvPBattleWeather>(r) ?? new PvPBattleWeather(r));
					}
					break;
			}
		}
	}
}