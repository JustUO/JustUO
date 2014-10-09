#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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

namespace VitaNex.Updates
{
	public class UpdateServiceOptions : CoreServiceOptions
	{
		[CommandProperty(UpdateService.Access)]
		public Schedule Schedule { get; set; }

		[CommandProperty(UpdateService.Access)]
		public TimeSpan Timeout { get; set; }

		[CommandProperty(UpdateService.Access)]
		public AccessLevel NotifyAccess { get; set; }

		[CommandProperty(UpdateService.Access)]
		public bool NotifyStaff { get; set; }

		[CommandProperty(UpdateService.Access)]
		public VersionInfo VersionLocal { get { return UpdateService.LocalVersion; } }

		[CommandProperty(UpdateService.Access)]
		public VersionInfo VersionRemote { get { return UpdateService.RemoteVersion; } }

		public UpdateServiceOptions()
			: base(typeof(UpdateService))
		{
			Schedule = new Schedule("UpdateService", true, ScheduleMonths.All, ScheduleDays.All, ScheduleTimes.EveryHour);

			Timeout = TimeSpan.FromSeconds(10.0);
			NotifyAccess = AccessLevel.Developer;
			NotifyStaff = true;
		}

		public UpdateServiceOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			base.Clear();

			Schedule.Info.Clear();

			Timeout = TimeSpan.Zero;
			NotifyAccess = AccessLevel.Developer;
			NotifyStaff = true;
		}

		public override void Reset()
		{
			base.Reset();

			Schedule.Info.Clear();

			Timeout = TimeSpan.FromSeconds(10.0);
			NotifyAccess = AccessLevel.Developer;
			NotifyStaff = true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						Schedule.Serialize(writer);

						writer.Write(Timeout);
						writer.WriteFlag(NotifyAccess);
						writer.Write(NotifyStaff);
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
						Schedule = new Schedule(reader);

						Timeout = reader.ReadTimeSpan();
						NotifyAccess = reader.ReadFlag<AccessLevel>();
						NotifyStaff = reader.ReadBool();
					}
					break;
			}
		}
	}
}