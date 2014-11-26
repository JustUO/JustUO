#region Header
//   Vorspire    _,-'/-'/  Info.cs
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

namespace VitaNex.Schedules
{
	[PropertyObject]
	public class ScheduleInfo
	{
		private static readonly TimeSpan _OneDay = TimeSpan.FromDays(1.0);

		public ScheduleInfo(
			ScheduleMonths months = ScheduleMonths.All, ScheduleDays days = ScheduleDays.All, ScheduleTimes times = null)
		{
			Months = months;
			Days = days;
			Times = times ?? new ScheduleTimes();
		}

		public ScheduleInfo(GenericReader reader)
		{
			Deserialize(reader);
		}

		[CommandProperty(Schedules.Access)]
		public ScheduleMonths Months { get; set; }

		[CommandProperty(Schedules.Access)]
		public ScheduleDays Days { get; set; }

		[CommandProperty(Schedules.Access)]
		public ScheduleTimes Times { get; set; }

		public virtual void Clear()
		{
			Months = ScheduleMonths.None;
			Days = ScheduleDays.None;
			Times.Clear();
		}

		public virtual bool HasMonth(ScheduleMonths month)
		{
			return (Months & month) == month;
		}

		public virtual bool HasDay(ScheduleDays day)
		{
			return (Days & day) == day;
		}

		public virtual bool HasTime(TimeSpan time)
		{
			return Times.Contains(time);
		}

		public virtual void Validate(DateTime dt, out TimeSpan time)
		{
			time = new TimeSpan(0, dt.TimeOfDay.Hours, dt.TimeOfDay.Minutes, 0, 0);
		}

		public virtual void Validate(ref DateTime dt)
		{
			dt = new DateTime(dt.Year, dt.Month, dt.Day, dt.TimeOfDay.Hours, dt.TimeOfDay.Minutes, 0, 0);
		}

		public virtual DateTime? FindBefore(DateTime dt)
		{
			Validate(ref dt);
			TimeSpan ts;
			Validate(dt, out ts);

			bool past = false;

			for (int year = dt.Year; year >= dt.Year - 10; year--)
			{
				for (int month = dt.Month; month >= 1; month--)
				{
					if (!HasMonth(Schedules.ConvertMonth(month)))
					{
						past = true;
						continue;
					}

					try
					{
						var start = new DateTime(year, month, dt.Day);
						var end = new DateTime(year, month, 1);

						for (DateTime date = start; date >= end; date = date.Subtract(_OneDay))
						{
							if (!HasDay(Schedules.ConvertDay(date.DayOfWeek)))
							{
								past = true;
								continue;
							}

							for (int i = Times.Count - 1; i >= 0; i--)
							{
								var time = Times[i];

								if (time != null && (past || time.Value < ts))
								{
									return date.AddHours(time.Value.Hours).AddMinutes(time.Value.Minutes);
								}
							}

							past = true;
						}
					}
					catch
					{ }
				}
			}

			return null;
		}

		public virtual DateTime? FindAfter(DateTime dt)
		{
			Validate(ref dt);
			TimeSpan ts;
			Validate(dt, out ts);

			bool future = false;

			for (int year = dt.Year; year < dt.Year + 10; year++)
			{
				for (int month = dt.Month; month <= 12; month++)
				{
					if (!HasMonth(Schedules.ConvertMonth(month)))
					{
						future = true;
						continue;
					}

					try
					{
						var start = new DateTime(year, month, dt.Day);
						var end = new DateTime(year, month, DateTime.DaysInMonth(year, month));

						for (DateTime date = start; date <= end; date = date.Add(_OneDay))
						{
							if (!HasDay(Schedules.ConvertDay(date.DayOfWeek)))
							{
								future = true;
								continue;
							}

							for (int i = 0; i < Times.Count; i++)
							{
								var time = Times[i];

								if (time != null && (future || time.Value > ts))
								{
									return date.AddHours(time.Value.Hours).AddMinutes(time.Value.Minutes);
								}
							}

							future = true;
						}
					}
					catch
					{ }
				}
			}

			return null;
		}

		public override string ToString()
		{
			return "Schedule Info";
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteFlag(Months);
						writer.WriteFlag(Days);

						writer.WriteType(
							Times,
							t =>
							{
								if (t != null)
								{
									Times.Serialize(writer);
								}
							});
					}
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Months = reader.ReadFlag<ScheduleMonths>();
						Days = reader.ReadFlag<ScheduleDays>();
						Times = reader.ReadTypeCreate<ScheduleTimes>(reader) ?? new ScheduleTimes(reader);
					}
					break;
			}
		}
	}
}