#region Header
//   Vorspire    _,-'/-'/  Timing.cs
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
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPBattleTiming : PropertyObject
	{
		public PvPBattleTiming()
		{
			PreparePeriod = TimeSpan.FromMinutes(5.0);
			RunningPeriod = TimeSpan.FromMinutes(15.0);
			EndedPeriod = TimeSpan.FromMinutes(2.5);
			OpenedWhen = DateTime.UtcNow;
			PreparedWhen = DateTime.UtcNow;
			StartedWhen = DateTime.UtcNow;
			EndedWhen = DateTime.UtcNow;
		}

		public PvPBattleTiming(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(AutoPvP.Access)]
		public virtual TimeSpan PreparePeriod { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual TimeSpan RunningPeriod { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual TimeSpan EndedPeriod { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public TimeSpan TotalCycleTime { get { return (PreparePeriod + RunningPeriod + EndedPeriod); } }

		public DateTime OpenedWhen { get; protected set; }
		public DateTime PreparedWhen { get; protected set; }
		public DateTime StartedWhen { get; protected set; }
		public DateTime EndedWhen { get; protected set; }

		public void SetAllPeriods(TimeSpan duration)
		{
			PreparePeriod = duration;
			RunningPeriod = duration;
			EndedPeriod = duration;
		}

		public void SetPeriod(PvPBattleState state, TimeSpan duration)
		{
			switch (state)
			{
				case PvPBattleState.Preparing:
					{
						PreparePeriod = duration;
					}
					break;
				case PvPBattleState.Running:
					{
						RunningPeriod = duration;
					}
					break;
				case PvPBattleState.Ended:
					{
						EndedPeriod = duration;
					}
					break;
			}
		}

		public void SetAllTimes(DateTime when)
		{
			OpenedWhen = when;
			PreparedWhen = when;
			StartedWhen = when;
			EndedWhen = when;
		}

		public void SetTime(PvPBattleState state, DateTime when)
		{
			switch (state)
			{
				case PvPBattleState.Queueing:
					{
						OpenedWhen = when;
					}
					break;
				case PvPBattleState.Preparing:
					{
						PreparedWhen = when;
					}
					break;
				case PvPBattleState.Running:
					{
						StartedWhen = when;
					}
					break;
				case PvPBattleState.Ended:
					{
						EndedWhen = when;
					}
					break;
			}
		}

		public override string ToString()
		{
			return "Battle Timing";
		}

		public override void Clear()
		{
			RunningPeriod = TimeSpan.Zero;
			PreparePeriod = TimeSpan.Zero;
			EndedPeriod = TimeSpan.Zero;
			OpenedWhen = DateTime.UtcNow;
			PreparedWhen = DateTime.UtcNow;
			StartedWhen = DateTime.UtcNow;
			EndedWhen = DateTime.UtcNow;
		}

		public override void Reset()
		{
			PreparePeriod = TimeSpan.FromMinutes(5.0);
			RunningPeriod = TimeSpan.FromMinutes(15.0);
			EndedPeriod = TimeSpan.FromMinutes(2.5);
			OpenedWhen = DateTime.UtcNow;
			PreparedWhen = DateTime.UtcNow;
			StartedWhen = DateTime.UtcNow;
			EndedWhen = DateTime.UtcNow;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(PreparePeriod);
						writer.Write(RunningPeriod);
						writer.Write(EndedPeriod);
						writer.Write(OpenedWhen);
						writer.Write(PreparedWhen);
						writer.Write(StartedWhen);
						writer.Write(EndedWhen);
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
						PreparePeriod = reader.ReadTimeSpan();
						RunningPeriod = reader.ReadTimeSpan();
						EndedPeriod = reader.ReadTimeSpan();
						OpenedWhen = reader.ReadDateTime();
						PreparedWhen = reader.ReadDateTime();
						StartedWhen = reader.ReadDateTime();
						EndedWhen = reader.ReadDateTime();
					}
					break;
			}
		}
	}
}