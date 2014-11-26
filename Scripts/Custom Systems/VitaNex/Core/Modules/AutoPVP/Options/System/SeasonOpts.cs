#region Header
//   Vorspire    _,-'/-'/  SeasonOpts.cs
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

using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class AutoPvPSeasonOptions : PropertyObject
	{
		[CommandProperty(AutoPvP.Access)]
		public ScheduleInfo ScheduleInfo { get { return AutoPvP.SeasonSchedule.Info; } set { AutoPvP.SeasonSchedule.Info = value; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int CurrentSeason { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int TopListCount { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int RunnersUpCount { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPRewards Rewards { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int SkipTicks { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int SkippedTicks { get; set; }

		public AutoPvPSeasonOptions()
		{
			CurrentSeason = 1;
			TopListCount = 3;
			RunnersUpCount = 7;
			Rewards = new PvPRewards();
		}

		public AutoPvPSeasonOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			ScheduleInfo.Clear();
			TopListCount = 0;
			RunnersUpCount = 0;
			SkipTicks = 0;
			SkippedTicks = 0;

			Rewards.Clear();
		}

		public override void Reset()
		{
			ScheduleInfo.Clear();
			TopListCount = 3;
			RunnersUpCount = 7;
			SkipTicks = 0;
			SkippedTicks = 0;

			Rewards.Reset();
		}

		public override string ToString()
		{
			return "Season Options";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					{
						writer.Write(SkipTicks);
						writer.Write(SkippedTicks);
					}
					goto case 0;
				case 0:
					{
						writer.WriteBlock(
							w =>
							{
								writer.Write(CurrentSeason);
								writer.Write(TopListCount);
								writer.Write(RunnersUpCount);

								writer.WriteType(ScheduleInfo, t => ScheduleInfo.Serialize(w));

								writer.Write(AutoPvP.SeasonSchedule.Enabled);
							});

						writer.WriteBlock(w => writer.WriteType(Rewards, t => Rewards.Serialize(w)));
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
				case 1:
					{
						SkipTicks = reader.ReadInt();
						SkippedTicks = reader.ReadInt();
					}
					goto case 0;
				case 0:
					{
						reader.ReadBlock(
							r =>
							{
								CurrentSeason = r.ReadInt();
								TopListCount = r.ReadInt();
								RunnersUpCount = r.ReadInt();

								ScheduleInfo = r.ReadTypeCreate<ScheduleInfo>(r) ?? new ScheduleInfo(r);
								AutoPvP.SeasonSchedule.Enabled = r.ReadBool();
							});

						reader.ReadBlock(r => Rewards = reader.ReadTypeCreate<PvPRewards>(r) ?? new PvPRewards(r));
					}
					break;
			}
		}
	}
}