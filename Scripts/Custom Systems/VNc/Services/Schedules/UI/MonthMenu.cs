#region Header
//   Vorspire    _,-'/-'/  MonthMenu.cs
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

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Schedules
{
	public class ScheduleMonthsMenuGump : MenuGump
	{
		public ScheduleMonthsMenuGump(
			PlayerMobile user, Schedule schedule, Gump parent = null, GumpButton clicked = null, bool useConfirm = true)
			: base(user, parent, clicked: clicked)
		{
			Schedule = schedule;
			UseConfirmDialog = useConfirm;

			CanMove = false;
			CanResize = false;
		}

		public Schedule Schedule { get; set; }
		public bool UseConfirmDialog { get; set; }

		protected override void CompileOptions(MenuGumpOptions list)
		{
			list.AppendEntry(new ListGumpEntry("None", b => SetMonth(b, ScheduleMonths.None)));
			list.AppendEntry(new ListGumpEntry("All", b => SetMonth(b, ScheduleMonths.All)));
			list.AppendEntry(new ListGumpEntry("January", b => SetMonth(b, ScheduleMonths.January)));
			list.AppendEntry(new ListGumpEntry("February", b => SetMonth(b, ScheduleMonths.February)));
			list.AppendEntry(new ListGumpEntry("March", b => SetMonth(b, ScheduleMonths.March)));
			list.AppendEntry(new ListGumpEntry("April", b => SetMonth(b, ScheduleMonths.April)));
			list.AppendEntry(new ListGumpEntry("May", b => SetMonth(b, ScheduleMonths.May)));
			list.AppendEntry(new ListGumpEntry("June", b => SetMonth(b, ScheduleMonths.June)));
			list.AppendEntry(new ListGumpEntry("July", b => SetMonth(b, ScheduleMonths.July)));
			list.AppendEntry(new ListGumpEntry("August", b => SetMonth(b, ScheduleMonths.August)));
			list.AppendEntry(new ListGumpEntry("September", b => SetMonth(b, ScheduleMonths.September)));
			list.AppendEntry(new ListGumpEntry("October", b => SetMonth(b, ScheduleMonths.October)));
			list.AppendEntry(new ListGumpEntry("November", b => SetMonth(b, ScheduleMonths.November)));
			list.AppendEntry(new ListGumpEntry("December", b => SetMonth(b, ScheduleMonths.December)));

			base.CompileOptions(list);

			list.Replace("Cancel", new ListGumpEntry("Done", Cancel));
		}

		protected override int GetLabelHue(int index, int pageIndex, ListGumpEntry entry)
		{
			if (Schedule == null)
			{
				return ErrorHue;
			}

			switch (entry.Label)
			{
				case "January":
					return Schedule.Info.HasMonth(ScheduleMonths.January) ? HighlightHue : ErrorHue;
				case "February":
					return Schedule.Info.HasMonth(ScheduleMonths.February) ? HighlightHue : ErrorHue;
				case "March":
					return Schedule.Info.HasMonth(ScheduleMonths.March) ? HighlightHue : ErrorHue;
				case "April":
					return Schedule.Info.HasMonth(ScheduleMonths.April) ? HighlightHue : ErrorHue;
				case "May":
					return Schedule.Info.HasMonth(ScheduleMonths.May) ? HighlightHue : ErrorHue;
				case "June":
					return Schedule.Info.HasMonth(ScheduleMonths.June) ? HighlightHue : ErrorHue;
				case "July":
					return Schedule.Info.HasMonth(ScheduleMonths.July) ? HighlightHue : ErrorHue;
				case "August":
					return Schedule.Info.HasMonth(ScheduleMonths.August) ? HighlightHue : ErrorHue;
				case "September":
					return Schedule.Info.HasMonth(ScheduleMonths.September) ? HighlightHue : ErrorHue;
				case "October":
					return Schedule.Info.HasMonth(ScheduleMonths.October) ? HighlightHue : ErrorHue;
				case "November":
					return Schedule.Info.HasMonth(ScheduleMonths.November) ? HighlightHue : ErrorHue;
				case "December":
					return Schedule.Info.HasMonth(ScheduleMonths.December) ? HighlightHue : ErrorHue;
			}

			return base.GetLabelHue(index, pageIndex, entry);
		}

		protected virtual void SetMonth(GumpButton button, ScheduleMonths month)
		{
			if (Schedule == null)
			{
				Close();
				return;
			}

			switch (month)
			{
				case ScheduleMonths.None:
					Schedule.Info.Months = ScheduleMonths.None;
					break;
				case ScheduleMonths.All:
					Schedule.Info.Months = ScheduleMonths.All;
					break;
				default:
					Schedule.Info.Months ^= month;
					break;
			}

			Schedule.InvalidateNextTick(DateTime.UtcNow);
			Refresh(true);
		}
	}
}