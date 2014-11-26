#region Header
//   Vorspire    _,-'/-'/  TvT.cs
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

using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
	public class TvTBattle : PvPBattle
	{
		public TvTBattle()
		{
			Name = "Team vs Team";
			Category = "Team vs Team";
			Description = "The last team alive wins!";
			Ranked = true;

			AddTeam(NameList.RandomName("daemon"), 1, 1, 0x22);
			AddTeam(NameList.RandomName("daemon"), 1, 1, 0x55);

			Schedule.Info.Months = ScheduleMonths.All;
			Schedule.Info.Days = ScheduleDays.All;
			Schedule.Info.Times = ScheduleTimes.EveryQuarterHour;

			Options.Timing.PreparePeriod = TimeSpan.FromMinutes(2.0);
			Options.Timing.RunningPeriod = TimeSpan.FromMinutes(8.0);
			Options.Timing.EndedPeriod = TimeSpan.FromMinutes(1.0);

			Options.Rules.AllowBeneficial = true;
			Options.Rules.AllowHarmful = true;
			Options.Rules.AllowHousing = false;
			Options.Rules.AllowPets = false;
			Options.Rules.AllowSpawn = false;
			Options.Rules.AllowSpeech = true;
			Options.Rules.CanBeDamaged = true;
			Options.Rules.CanDamageEnemyTeam = true;
			Options.Rules.CanDamageOwnTeam = false;
			Options.Rules.CanDie = false;
			Options.Rules.CanHeal = true;
			Options.Rules.CanHealEnemyTeam = false;
			Options.Rules.CanHealOwnTeam = true;
			Options.Rules.CanMount = false;
			Options.Rules.CanResurrect = false;
			Options.Rules.CanUseStuckMenu = false;
		}

		public TvTBattle(GenericReader reader)
			: base(reader)
		{ }

		public override int CompareTeamRank(PvPTeam a, PvPTeam b)
		{
			if (a == b)
			{
				return 0;
			}

			if (b == null)
			{
				return -1;
			}

			if (a == null)
			{
				return 1;
			}

			if (a.Deleted && b.Deleted)
			{
				return 0;
			}

			if (b.Deleted)
			{
				return -1;
			}

			if (a.Deleted)
			{
				return 1;
			}

			if (!a.RespawnOnDeath && !b.RespawnOnDeath)
			{
				if (a.Dead.Count > b.Dead.Count)
				{
					return -1;
				}

				if (a.Dead.Count < b.Dead.Count)
				{
					return 1;
				}

				return 0;
			}

			return base.CompareTeamRank(a, b);
		}

		public override void OnTeamWin(PvPTeam team)
		{
			WorldBroadcast("Team {0} has won {1}!", team.Name, Name);

			base.OnTeamWin(team);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
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
				case 0:
					break;
			}
		}
	}
}