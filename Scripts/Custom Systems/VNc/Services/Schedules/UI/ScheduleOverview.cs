#region Header
//   Vorspire    _,-'/-'/  ScheduleOverview.cs
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
using System.Drawing;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Schedules
{
	public class ScheduleOverviewGump : HtmlPanelGump<Schedule>
	{
		public ScheduleOverviewGump(PlayerMobile user, Schedule schedule, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "Schedule Unavailable", title: "Schedule Overview", selected: schedule)
		{
			UseConfirmDialog = useConfirm;

			HtmlColor = Color.GreenYellow;
			ForceRecompile = true;
			AutoRefresh = true;
		}

		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			base.Compile();

			if (Selected != null)
			{
				Html = Selected.ToHtmlString();
			}
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.AddReplace(
				"label/header/title",
				() => AddLabelCropped(90, 15, Width - 235, 20, GetTitleHue(), String.IsNullOrEmpty(Title) ? DefaultTitle : Title));

			layout.AddReplace(
				"label/header/subtitle",
				() =>
				AddLabelCropped(
					90 + (Width - 235), 15, 100, 20, HighlightHue, Schedules.FormatTime(DateTime.UtcNow.TimeOfDay, true)));
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (Selected != null && User.AccessLevel >= Schedules.Access)
			{
				if (!Selected.Enabled)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Enable",
							b =>
							{
								Selected.Enabled = true;
								Refresh(true);
							},
							HighlightHue));
				}
				else
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Disable",
							b =>
							{
								Selected.Enabled = false;
								Refresh(true);
							},
							HighlightHue));

					list.AppendEntry(
						new ListGumpEntry("Edit Months", b => Send(new ScheduleMonthsMenuGump(User, Selected, this, b)), HighlightHue));
					list.AppendEntry(
						new ListGumpEntry("Edit Days", b => Send(new ScheduleDaysMenuGump(User, Selected, this, b)), HighlightHue));
					list.AppendEntry(
						new ListGumpEntry("Edit Times", b => Send(new SheduleTimeListGump(User, Selected, Hide(true))), HighlightHue));
					list.AppendEntry(
						new ListGumpEntry(
							"Clear Schedule",
							b =>
							{
								if (UseConfirmDialog)
								{
									Send(
										new ConfirmDialogGump(
											User,
											this,
											title: "Clear Schedule?",
											html:
												"The schedule will be cleared, erasing all data associated with its entries.\nThis action can not be reversed.\n\nDo you want to continue?",
											onAccept: OnConfirmClearSchedule));
								}
								else
								{
									OnConfirmClearSchedule(b);
								}
							},
							HighlightHue));
				}
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmClearSchedule(GumpButton button)
		{
			if (Selected == null)
			{
				Close();
				return;
			}

			Selected.Info.Clear();
			Selected.InvalidateNextTick(DateTime.UtcNow);
			Refresh(true);
		}
	}
}