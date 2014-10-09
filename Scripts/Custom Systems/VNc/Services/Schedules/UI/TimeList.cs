#region Header
//   Vorspire    _,-'/-'/  TimeList.cs
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
using System.Collections.Generic;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Schedules
{
	public class SheduleTimeListGump : ListGump<TimeSpan>
	{
		public static string HelpText =
			"Schedule Times: List specific times for this schedule.\nThese times determine what time of day the schedule will tick.";

		public SheduleTimeListGump(PlayerMobile user, Schedule schedule, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "There are no times to display.", title: "Schedule Times")
		{
			Schedule = schedule;
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
			CanMove = false;
			CanResize = false;
		}

		public Schedule Schedule { get; set; }
		public bool UseConfirmDialog { get; set; }

		protected override string GetLabelText(int index, int pageIndex, TimeSpan entry)
		{
			return Schedules.FormatTime(entry, true);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.AppendEntry(new ListGumpEntry("Delete All", OnDeleteAll, HighlightHue));
			list.AppendEntry(new ListGumpEntry("Add Time", OnAddTime, HighlightHue));
			list.AppendEntry(new ListGumpEntry("Use Preset", OnPresetsMenu, HighlightHue));
			list.AppendEntry(new ListGumpEntry("Help", ShowHelp));

			base.CompileMenuOptions(list);
		}

		protected virtual void ShowHelp(GumpButton button)
		{
			if (User == null || User.Deleted)
			{
				return;
			}

			Send(new NoticeDialogGump(User, this, title: "Help", html: HelpText));
		}

		protected virtual void OnDeleteAll(GumpButton button)
		{
			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						this,
						title: "Delete All Times?",
						html:
							"All times in the schedule will be deleted, erasing all data associated with them.\nThis action can not be reversed.\n\nDo you want to continue?",
						onAccept: subButton =>
						{
							Schedule.Info.Times.Clear();
							Schedule.InvalidateNextTick(DateTime.UtcNow);
							Refresh(true);
						},
						onCancel: b => Refresh(true)));
			}
			else
			{
				Schedule.Info.Times.Clear();
				Schedule.InvalidateNextTick(DateTime.UtcNow);
				Refresh(true);
			}
		}

		protected virtual void OnAddTime(GumpButton button)
		{
			TimeSpan nowTime = DateTime.UtcNow.TimeOfDay;

			Send(
				new InputDialogGump(
					User,
					this,
					title: "Add Schedule Time",
					html:
						"Enter the time of day to add to this schedule.\nFormat: HH:MM\nExample: " +
						String.Format("{0:D2}:{1:D2}", nowTime.Hours, nowTime.Minutes) +
						"\n\nYou can also load a preset list of times, but be aware that presets will overwrite any custom entries you have created.",
					callback: (b, text) =>
					{
						int hh, mm;
						ParseTime(text, out hh, out mm);

						if (hh == -1 || mm == -1)
						{
							OnAddTime(button);
							return;
						}

						Schedule.Info.Times.Add(new TimeSpan(0, hh, mm, 0, 0));
						Schedule.InvalidateNextTick(DateTime.UtcNow);
						OnAddTime(button);
					},
					onCancel: b => Refresh(true)));
		}

		protected virtual void OnPresetsMenu(GumpButton button)
		{
			var entries = new[]
			{
				new ListGumpEntry("Noon", b => UsePreset(b, ScheduleTimes.Noon), HighlightHue),
				new ListGumpEntry("Midnight", b => UsePreset(b, ScheduleTimes.Midnight), HighlightHue),
				new ListGumpEntry("Every Hour", b => UsePreset(b, ScheduleTimes.EveryHour), HighlightHue),
				new ListGumpEntry("Every 30 Minutes", b => UsePreset(b, ScheduleTimes.EveryHalfHour), HighlightHue),
				new ListGumpEntry("Every 15 Minutes", b => UsePreset(b, ScheduleTimes.EveryQuarterHour), HighlightHue),
				new ListGumpEntry("Every 10 Minutes", b => UsePreset(b, ScheduleTimes.EveryTenMinutes), HighlightHue),
				new ListGumpEntry("Every 5 Minutes", b => UsePreset(b, ScheduleTimes.EveryFiveMinutes), HighlightHue),
				new ListGumpEntry("Every Minute", b => UsePreset(b, ScheduleTimes.EveryMinute), HighlightHue),
				new ListGumpEntry("Four Twenty", b => UsePreset(b, ScheduleTimes.FourTwenty), HighlightHue)
			};

			Send(new MenuGump(User, this, entries, button));
		}

		protected virtual void UsePreset(GumpButton button, ScheduleTimes times)
		{
			Schedule.Info.Times.Clear();
			Schedule.Info.Times.Add(times);
			Schedule.InvalidateNextTick(DateTime.UtcNow);
			Refresh(true);
		}

		protected virtual void ParseTime(string text, out int hh, out int mm)
		{
			var parts = text.Split(':');

			if (parts.Length >= 2)
			{
				if (!Int32.TryParse(parts[0], out hh))
				{
					hh = -1;
				}

				if (!Int32.TryParse(parts[1], out mm))
				{
					mm = -1;
				}

				hh = (hh < 0) ? 0 : (hh > 23) ? 23 : hh;
				mm = (mm < 0) ? 0 : (mm > 59) ? 59 : mm;
			}
			else
			{
				hh = -1;
				mm = -1;
			}
		}

		protected override void CompileList(List<TimeSpan> list)
		{
			list.Clear();
			list.AddRange(Schedule.Info.Times);

			base.CompileList(list);
		}

		public override string GetSearchKeyFor(TimeSpan key)
		{
			return Schedules.FormatTime(key, true);
		}

		protected override void SelectEntry(GumpButton button, TimeSpan entry)
		{
			base.SelectEntry(button, entry);

			if (button != null)
			{
				Send(new ScheduleTimeListEntryGump(User, Schedule, Refresh(), button, entry, UseConfirmDialog));
			}
		}
	}
}