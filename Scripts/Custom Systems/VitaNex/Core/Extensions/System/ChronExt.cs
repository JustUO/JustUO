#region Header
//   Vorspire    _,-'/-'/  ChronExt.cs
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
using System.Globalization;

using VitaNex;
#endregion

namespace System
{
	[Flags]
	public enum Months
	{
		None = 0x000,
		January = 0x001,
		Febuary = 0x002,
		March = 0x004,
		April = 0x008,
		May = 0x010,
		June = 0x020,
		July = 0x040,
		August = 0x080,
		September = 0x100,
		October = 0x200,
		November = 0x400,
		December = 0x800,

		All = January | Febuary | March | April | May | June | July | August | September | October | November | December
	}

	public static class ChronExtUtility
	{
		public static TimeStamp ToTimeStamp(this DateTime date)
		{
			return date;
		}

		public static Months GetMonth(this DateTime date)
		{
			switch (date.Month)
			{
				case 1:
					return Months.January;
				case 2:
					return Months.Febuary;
				case 3:
					return Months.March;
				case 4:
					return Months.April;
				case 5:
					return Months.May;
				case 6:
					return Months.June;
				case 7:
					return Months.July;
				case 8:
					return Months.August;
				case 9:
					return Months.September;
				case 10:
					return Months.October;
				case 11:
					return Months.November;
				case 12:
					return Months.December;
				default:
					return Months.None;
			}
		}

		public static string ToSimpleString(this DateTime date, string format = "t D d M y")
		{
			var strs = new List<string>(format.Length);

			bool noformat = false;

			for (int i = 0; i < format.Length; i++)
			{
				if (format[i] == '#')
				{
					noformat = !noformat;
					continue;
				}

				if (noformat)
				{
					strs.Add(format[i].ToString(CultureInfo.InvariantCulture));
					continue;
				}

				switch (format[i])
				{
					case '\\':
						strs.Add((i + 1 < format.Length) ? format[++i].ToString(CultureInfo.InvariantCulture) : String.Empty);
						break;
					case 'z':
						{
							string tzo = TimeZoneInfo.Local.DisplayName;
							int s = tzo.IndexOf(' ');

							if (s > 0)
							{
								tzo = tzo.Substring(0, s);
							}

							strs.Add(tzo);
						}
						break;
					case 'Z':
						{
							string tzo = date.IsDaylightSavingTime() ? TimeZoneInfo.Local.DaylightName : TimeZoneInfo.Local.DisplayName;
							int s = tzo.IndexOf(' ');

							if (s > 0)
							{
								tzo = tzo.Substring(0, s);
							}

							strs.Add(tzo);
						}
						break;
					case 'D':
						strs.Add(date.DayOfWeek.ToString());
						break;
					case 'd':
						strs.Add(date.Day.ToString(CultureInfo.InvariantCulture));
						break;
					case 'M':
						strs.Add(date.GetMonth().ToString());
						break;
					case 'm':
						strs.Add(date.Month.ToString(CultureInfo.InvariantCulture));
						break;
					case 'y':
						strs.Add(date.Year.ToString(CultureInfo.InvariantCulture));
						break;
					case 't':
						{
							string tf = String.Empty;

							if (i + 1 < format.Length)
							{
								if (format[i + 1] == '@')
								{
									++i;

									while (++i < format.Length && format[i] != '@')
									{
										tf += format[i];
									}
								}
							}

							strs.Add(date.TimeOfDay.ToSimpleString(!String.IsNullOrWhiteSpace(tf) ? tf : "h-m-s"));
						}
						break;
					default:
						strs.Add(format[i].ToString(CultureInfo.InvariantCulture));
						break;
				}
			}

			var str = String.Join(String.Empty, strs);

			strs.Free(true);

			return str;
		}

		public static string ToSimpleString(this TimeSpan time, string format = "h-m-s")
		{
			var strs = new string[format.Length];

			bool noformat = false;

			for (int i = 0; i < format.Length && i < strs.Length; i++)
			{
				if (format[i] == '#')
				{
					noformat = !noformat;
					continue;
				}

				if (noformat)
				{
					strs[i] = format[i].ToString(CultureInfo.InvariantCulture);
					continue;
				}

				switch (format[i])
				{
					case '\\':
						strs[i] = ((i + 1 < format.Length) ? format[++i].ToString(CultureInfo.InvariantCulture) : String.Empty);
						break;
					case 'z':
						{
							string tzo = TimeZoneInfo.Local.DisplayName;
							int s = tzo.IndexOf(' ');

							if (s > 0)
							{
								tzo = tzo.Substring(0, s);
							}

							strs[i] = tzo;
						}
						break;
					case 'Z':
						{
							string tzo = DateTime.Now.IsDaylightSavingTime()
											 ? TimeZoneInfo.Local.DaylightName
											 : TimeZoneInfo.Local.DisplayName;
							int s = tzo.IndexOf(' ');

							if (s > 0)
							{
								tzo = tzo.Substring(0, s);
							}

							strs[i] = tzo;
						}
						break;
					case 'D':
						strs[i] = String.Format("{0:F2}", time.TotalDays);
						break;
					case 'H':
						strs[i] = String.Format("{0:F2}", time.TotalHours);
						break;
					case 'M':
						strs[i] = String.Format("{0:F2}", time.TotalMinutes);
						break;
					case 'S':
						strs[i] = String.Format("{0:F2}", time.TotalSeconds);
						break;
					case 'd':
						strs[i] = String.Format("{0:D2}", time.Days);
						break;
					case 'h':
						strs[i] = String.Format("{0:D2}", time.Hours);
						break;
					case 'm':
						strs[i] = String.Format("{0:D2}", time.Minutes);
						break;
					case 's':
						strs[i] = String.Format("{0:D2}", time.Seconds);
						break;
					default:
						strs[i] = format[i].ToString(CultureInfo.InvariantCulture);
						break;
				}
			}

			return String.Join(String.Empty, strs);
		}
	}
}