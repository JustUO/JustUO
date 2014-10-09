#region Header
//   Vorspire    _,-'/-'/  ScheduleTest.cs
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

using VitaNex.Schedules;
#endregion

namespace VitaNex.Sandbox.Tests
{
	public sealed class ScheduleTest : ISandboxTest
	{
		private Schedule _Schedule;
		private int _Ticks;

		public void EntryPoint()
		{
			if (!VitaNexCore.FirstBoot)
			{
				return;
			}

			_Schedule = Schedules.Schedules.CreateSchedule(
				"Sandbox Schedule", true, true, ScheduleMonths.All, ScheduleDays.All, ScheduleTimes.EveryMinute, OnScheduleTick);
		}

		public void OnSuccess()
		{ }

		public void OnException(Exception e)
		{
			Sandbox.CSOptions.ToConsole(e);
		}

		private void OnScheduleTick(Schedule s)
		{
			Sandbox.SafeInvoke(
				() =>
				{
					if (_Schedule == null || _Schedule.CurrentGlobalTick == null)
					{
						return;
					}

					DateTime dt = _Schedule.CurrentGlobalTick.Value;
					Sandbox.CSOptions.ToConsole("{0} Current Tick: {1}", _Schedule.Name, dt.ToSimpleString());

					if (_Schedule.NextGlobalTick == null)
					{
						return;
					}

					dt = _Schedule.NextGlobalTick.Value;
					Sandbox.CSOptions.ToConsole("{0} Next Tick: {1}", _Schedule.Name, dt.ToSimpleString());

					if (++_Ticks >= 5)
					{
						_Schedule.Enabled = false;
						Sandbox.CSOptions.ToConsole("{0} Disabled.", _Schedule.Name);
					}
				},
				this);
		}
	}
}