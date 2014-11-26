#region Header
//   Vorspire    _,-'/-'/  TeamList.cs
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

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPTeamListGump : ListGump<PvPTeam>
	{
		public PvPTeamListGump(PlayerMobile user, PvPBattle battle, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "There are no battles to display.", title: "PvP Teams")
		{
			Battle = battle;
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
		}

		public PvPBattle Battle { get; set; }
		public bool UseConfirmDialog { get; set; }

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (User.AccessLevel >= AutoPvP.Access)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Delete All",
						b =>
						{
							if (UseConfirmDialog)
							{
								Send(
									new ConfirmDialogGump(
										User,
										this,
										title: "Delete All Teams?",
										html:
											"All teams in the battle will be deleted, erasing all data associated with them.\nThis action can not be reversed.\n\nDo you want to continue?",
										onAccept: OnConfirmDeleteAllTeams));
							}
							else
							{
								OnConfirmDeleteAllTeams(b);
							}
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"New Team",
						b =>
						Send(
							new InputDialogGump(
								User,
								this,
								title: "Team Name",
								html: "Enter a name for this team.\n255 Chars Max.",
								input: NameList.RandomName("daemon"),
								limit: 255,
								callback: (b1, name) =>
								{
									if (String.IsNullOrEmpty(name))
									{
										name = NameList.RandomName("daemon");
									}

									Battle.AddTeam(name, 1, 5, 11 + (List.Count + 1));
									Refresh(true);
								})),
						HighlightHue));
			}

			if (Battle.IsParticipant(User))
			{
				list.AppendEntry(new ListGumpEntry("Quit & Leave", b => Battle.Eject(User, true)));
			}
			else
			{
				if (Battle.IsQueued(User))
				{
					if (Battle.Queue[User] == Selected)
					{
						list.AppendEntry(new ListGumpEntry("Leave Queue", b => Battle.Dequeue(User)));
					}
				}
				else if (!Battle.AutoAssign && Battle.CanQueue(User))
				{
					list.AppendEntry(new ListGumpEntry("Join Queue", b => Battle.Enqueue(User)));
				}
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmDeleteAllTeams(GumpButton button)
		{
			foreach (PvPTeam team in List)
			{
				team.Delete();
			}

			Refresh(true);
		}

		protected override void CompileList(List<PvPTeam> list)
		{
			list.Clear();
			list.AddRange(Battle.Teams);
			base.CompileList(list);
		}

		public override string GetSearchKeyFor(PvPTeam key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override void SelectEntry(GumpButton button, PvPTeam entry)
		{
			base.SelectEntry(button, entry);

			if (button == null || entry == null || entry.Deleted)
			{
				return;
			}

			MenuGumpOptions list = new MenuGumpOptions();

			list.AppendEntry(
				new ListGumpEntry("Overview", b => Send(new PvPTeamOverviewGump(User, entry, Hide(true), UseConfirmDialog))));

			PvPTeam team;

			if (entry.Battle.IsParticipant(User, out team))
			{
				if (team == entry)
				{
					list.AppendEntry(new ListGumpEntry("Quit & Leave", b => entry.Battle.Eject(User, true)));
				}
			}
			else
			{
				if (entry.Battle.IsQueued(User))
				{
					if (entry.Battle.Queue[User] == entry)
					{
						list.AppendEntry(new ListGumpEntry("Leave Queue", b => entry.Battle.Dequeue(User)));
					}
				}
				else if (!entry.Battle.AutoAssign && entry.Battle.CanQueue(User))
				{
					list.AppendEntry(new ListGumpEntry("Join Queue", b => entry.Battle.Enqueue(User, entry)));
				}
			}

			Send(new MenuGump(User, Refresh(), list, button));
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"button/header/war",
				() => AddButton(
					85,
					15,
					2026,
					2025,
					b =>
					{
						PvPProfileListGump plg = Parent as PvPProfileListGump;

						if (plg == null)
						{
							Send(
								new PvPProfileListGump(
									User, null, Hide(true), UseConfirmDialog, AutoPvP.CMOptions.Advanced.Profiles.RankingOrder));
						}
						else
						{
							plg.Refresh(true);
						}
					}));

			layout.AddReplace(
				"label/header/title",
				() => AddLabelCropped(160, 15, 215, 20, GetTitleHue(), String.IsNullOrEmpty(Title) ? DefaultTitle : Title));
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, PvPTeam entry)
		{
			base.CompileEntryLayout(layout, length, index, pIndex, yOffset, entry);

			layout.AddReplace(
				"label/list/entry/" + index,
				() =>
				{
					AddLabelCropped(65, 2 + yOffset, 150, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry));
					AddLabelCropped(
						205, 2 + yOffset, 170, 20, GetCapacityLabelHue(index, pIndex, entry), GetCapacityLabelText(index, pIndex, entry));
				});
		}

		protected override int GetLabelHue(int index, int pageIndex, PvPTeam entry)
		{
			return entry != null ? ((!entry.Validate(User)) ? ErrorHue : entry.Color) : base.GetLabelHue(index, pageIndex, null);
		}

		protected override string GetLabelText(int index, int pageIndex, PvPTeam entry)
		{
			return entry != null ? entry.Name : base.GetLabelText(index, pageIndex, null);
		}

		protected virtual string GetCapacityLabelText(int index, int pageIndex, PvPTeam entry)
		{
			return entry == null
					   ? String.Empty
					   : (!Battle.IgnoreCapacity && entry.Count < entry.MinCapacity
							  ? String.Format("Req: {0:#,0} / {1:#,0} ({2:#,0} Max)", entry.Count, entry.MinCapacity, entry.MaxCapacity)
							  : (entry.IsFull ? "Full" : String.Format("{0:#,0} / {1:#,0}", entry.Count, entry.MaxCapacity)));
		}

		protected virtual int GetCapacityLabelHue(int index, int pageIndex, PvPTeam entry)
		{
			return entry == null
					   ? ErrorHue
					   : (!Battle.IgnoreCapacity && entry.Count < entry.MinCapacity
							  ? HighlightHue
							  : (entry.IsFull ? ErrorHue : TextHue));
		}
	}
}