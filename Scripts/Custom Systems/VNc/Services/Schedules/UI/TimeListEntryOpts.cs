#region Header
//   Vorspire    _,-'/-'/  TimeListEntryOpts.cs
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
	public class ScheduleTimeListEntryGump : MenuGump
	{
		public ScheduleTimeListEntryGump(
			PlayerMobile user,
			Schedule schedule,
			Gump parent = null,
			GumpButton clicked = null,
			TimeSpan? time = null,
			bool useConfirm = true)
			: base(user, parent, clicked: clicked)
		{
			Schedule = schedule;
			Time = time ?? TimeSpan.Zero;
			UseConfirmDialog = useConfirm;

			CanMove = false;
			CanResize = false;
		}

		public Schedule Schedule { get; set; }
		public TimeSpan Time { get; set; }
		public bool UseConfirmDialog { get; set; }

		protected override void CompileOptions(MenuGumpOptions list)
		{
			base.CompileOptions(list);

			list.PrependEntry(
				new ListGumpEntry(
					"Delete",
					button =>
					{
						if (UseConfirmDialog)
						{
							Send(
								new ConfirmDialogGump(
									User,
									Refresh(),
									title: "Delete Time?",
									html:
										"All data associated with this time will be deleted.\nThis action can not be reversed!\nDo you want to continue?",
									onAccept: OnConfirmDelete));
						}
						else
						{
							OnConfirmDelete(button);
						}
					},
					HighlightHue));
		}

		protected virtual void OnConfirmDelete(GumpButton button)
		{
			if (Selected == null)
			{
				Close();
				return;
			}

			Schedule.Info.Times.Remove(Time);
			Schedule.InvalidateNextTick(DateTime.UtcNow);
			Close();
		}
	}
}