#region Header
//   Vorspire    _,-'/-'/  BattleList.cs
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
using System.Drawing;
using System.Text;

using Server;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

using VitaNex.Schedules;
using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPBattleListGump : ListGump<PvPBattle>
	{
		public PvPBattleListGump(PlayerMobile user, Gump parent = null, string category = "All", bool useConfirm = true)
			: base(user, parent, emptyText: "There are no battles to display.", title: "PvP Battles")
		{
			Category = category.ToUpperWords() ?? "All";
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
			AutoRefresh = true;
		}

		public string Category { get; set; }
		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			if (AutoPvP.SeasonSchedule.Enabled)
			{
				Title = "PvP Battles (Season " + AutoPvP.CurrentSeason.Number + ")";
			}

			base.Compile();
		}

		public override int SortCompare(PvPBattle a, PvPBattle b)
		{
			int result = 0;

			if (a.CompareNull(b, ref result))
			{
				return result;
			}

			if (a.Deleted && b.Deleted)
			{
				return 0;
			}

			if (a.Deleted)
			{
				return 1;
			}

			if (b.Deleted)
			{
				return -1;
			}

			if (a.Hidden && b.Hidden)
			{
				return 0;
			}

			if (a.Hidden)
			{
				return 1;
			}

			if (b.Hidden)
			{
				return -1;
			}

			var x = a.GetStateTimeLeft();
			var y = b.GetStateTimeLeft();

			if (x < y)
			{
				return -1;
			}

			if (x > y)
			{
				return 1;
			}

			return Insensitive.Compare(a.ToString(), b.ToString());
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (User.AccessLevel >= AutoPvP.Access)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"View Season Schedule",
						b => Send(new ScheduleOverviewGump(User, AutoPvP.SeasonSchedule, Hide(true))),
						(User.AccessLevel >= AutoPvP.Access) ? HighlightHue : TextHue));

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
										title: "Delete All Battles?",
										html:
											"All battles in the database will be deleted, erasing all data associated with them.\nThis action can not be reversed.\n\nDo you want to continue?",
										onAccept: OnConfirmDeleteAllBattles));
							}
							else
							{
								OnConfirmDeleteAllBattles(b);
							}
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"Internalize All",
						b =>
						{
							if (UseConfirmDialog)
							{
								Send(
									new ConfirmDialogGump(
										User,
										this,
										title: "Internalize All Battles?",
										html:
											"All battles in the database will be internalized, forcing them to end.\nThis action can not be reversed.\n\nDo you want to continue?",
										onAccept: OnConfirmInternalizeAllBattles));
							}
							else
							{
								OnConfirmInternalizeAllBattles(b);
							}
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"New Battle", b => Send(new PvPScenarioListGump(User, Hide(true), UseConfirmDialog)), HighlightHue));
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmDeleteAllBattles(GumpButton button)
		{
			AutoPvP.DeleteAllBattles();
			Refresh(true);
		}

		protected virtual void OnConfirmInternalizeAllBattles(GumpButton button)
		{
			AutoPvP.InternalizeAllBattles();
			Refresh(true);
		}

		protected override void CompileList(List<PvPBattle> list)
		{
			list.Clear();

			bool viewInternal = (User.AccessLevel >= AutoPvP.Access);

			foreach (PvPBattle battle in AutoPvP.Battles.Values)
			{
				if (battle == null || battle.Deleted ||
					((battle.State == PvPBattleState.Internal || battle.Hidden) && !viewInternal))
				{
					continue;
				}

				string cat = (String.IsNullOrWhiteSpace(battle.Category) ? "Misc" : battle.Category).ToUpperWords();

				if (!String.IsNullOrWhiteSpace(Category))
				{
					if (!Insensitive.Equals(cat, Category) && !Insensitive.Equals(Category, "All"))
					{
						continue;
					}
				}

				list.Add(battle);
			}

			base.CompileList(list);
		}

		public override string GetSearchKeyFor(PvPBattle key)
		{
			return key != null && !key.Deleted ? String.Format("{0} {1}", key.Name, key.Category) : base.GetSearchKeyFor(key);
		}

		protected override void SelectEntry(GumpButton button, PvPBattle entry)
		{
			base.SelectEntry(button, entry);

			if (button == null || entry == null || entry.Deleted)
			{
				return;
			}

			MenuGumpOptions list = new MenuGumpOptions();

			list.AppendEntry(
				new ListGumpEntry(
					"Overview", b => Send(new PvPBattleOverviewGump(User, Hide(true), entry, UseConfirmDialog)), TextHue));

			if (User.AccessLevel >= AutoPvP.Access)
			{
				if (entry.State == PvPBattleState.Internal)
				{
					if (Selected.Validate(User))
					{
						list.AppendEntry(
							new ListGumpEntry(
								"Publish",
								b =>
								{
									Selected.State = PvPBattleState.Queueing;
									Refresh(true);
								},
								HighlightHue));
					}
				}
				else
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Internalize",
							b =>
							{
								Selected.State = PvPBattleState.Internal;
								Refresh(true);
							},
							HighlightHue));

					if (!Selected.Hidden)
					{
						if (Selected.Validate(User))
						{
							list.AppendEntry(
								new ListGumpEntry(
									"Hide",
									b =>
									{
										Selected.Hidden = true;
										Refresh(true);
									},
									HighlightHue));
						}
					}
					else
					{
						list.AppendEntry(
							new ListGumpEntry(
								"Unhide",
								b =>
								{
									Selected.Hidden = false;
									Refresh(true);
								},
								HighlightHue));
					}
				}

				list.AppendEntry(
					new ListGumpEntry(
						"Delete",
						b =>
						{
							if (UseConfirmDialog)
							{
								Send(
									new ConfirmDialogGump(
										User,
										this,
										title: "Delete Battle?",
										html:
											"All data associated with this battle will be deleted.\nThis action can not be reversed!\nDo you want to continue?",
										onAccept: OnConfirmDeleteBattle));
							}
							else
							{
								OnConfirmDeleteBattle(b);
							}
						},
						HighlightHue));
			}

			list.AppendEntry(
				new ListGumpEntry(
					"View Teams",
					b => Send(new PvPTeamListGump(User, entry, Hide(true))),
					(User.AccessLevel >= AutoPvP.Access) ? HighlightHue : TextHue));

			if (entry.IsParticipant(User))
			{
				list.AppendEntry(new ListGumpEntry("Quit & Leave", b => entry.Eject(User, true)));
			}
			else
			{
				if (entry.IsQueued(User))
				{
					list.AppendEntry(new ListGumpEntry("Leave Queue", b => entry.Dequeue(User)));
				}
				else if (entry.CanQueue(User))
				{
					list.AppendEntry(new ListGumpEntry("Join Queue", b => entry.Enqueue(User)));
				}

				if (entry.IsSpectator(User))
				{
					list.AppendEntry(new ListGumpEntry("Leave Spectators", b => entry.RemoveSpectator(User, true)));
				}
				else if (entry.CanSpectate(User))
				{
					list.AppendEntry(new ListGumpEntry("Join Spectators", b => entry.AddSpectator(User, true)));
				}
			}

			Send(new MenuGump(User, Refresh(), list, button));
		}

		protected virtual void OnConfirmDeleteBattle(GumpButton button)
		{
			if (Selected == null || Selected.Deleted)
			{
				Refresh(true);
				return;
			}

			Selected.Delete();
			Refresh(true);
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
							Send(new PvPProfileListGump(User, null, Hide(true), UseConfirmDialog));
						}
						else
						{
							plg.Refresh(true);
						}
					}));

			layout.AddReplace(
				"label/header/title",
				() => AddLabelCropped(160, 15, 215, 20, GetTitleHue(), String.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title));
		}

		protected override void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, PvPBattle> range)
		{
			layout.Add(
				"background/footer/help",
				() =>
				{
					AddBackground(0, 75 + (range.Count * 30), 420, 130, 9270);
					AddImageTiled(10, 85 + (range.Count * 30), 400, 110, 2624);
					//AddAlphaRegion(10, 85 + (range.Count * 30), 400, 110);
				});

			layout.Add("html/footer/help", () => AddHtml(20, 85 + (range.Count * 30), 390, 110, GetHelpText(), false, false));

			layout.Add(
				"background/footer/states",
				() =>
				{
					AddBackground(0, 205 + (range.Count * 30), 420, 130, 9270);
					AddImageTiled(10, 215 + (range.Count * 30), 400, 110, 2624);
					//AddAlphaRegion(10, 215 + (range.Count * 30), 400, 110);
				});

			layout.Add(
				"html/footer/states", () => AddHtml(20, 215 + (range.Count * 30), 390, 110, GetStatesText(), false, false));

			base.CompileEntryLayout(layout, range);
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, PvPBattle entry)
		{
			base.CompileEntryLayout(layout, length, index, pIndex, yOffset, entry);

			layout.AddReplace(
				"label/list/entry/" + index,
				() =>
				{
					AddHtml(
						65,
						2 + yOffset,
						125,
						20,
						String.Format(
							"<basefont color=#{0:X6}>{1}", GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry)),
						false,
						false);
					AddHtml(
						200,
						2 + yOffset,
						60,
						20,
						String.Format(
							"<basefont color=#{0:X6}>{1}", GetStateLabelHue(index, pIndex, entry), GetStateLabelText(index, pIndex, entry)),
						false,
						false);
					AddHtml(
						270,
						2 + yOffset,
						125,
						20,
						String.Format(
							"<basefont color=#{0:X6}>{1}", GetTimeLabelHue(index, pIndex, entry), GetTimeLabelText(index, pIndex, entry)),
						false,
						false);
				});
		}

		protected virtual string GetHelpText()
		{
			return
				String.Format(
					"PvP Battles are a fun, unique way to engage in thrilling combat with other players in different scenarios.\nNo deaths, no insurance loss and no looting (unless specified)!\nEach battle may offer a reward for both winners and losers!\nJoin a battle now and earn your rank amongst the top players of {0}!",
					ServerList.ServerName).WrapUOHtmlColor(DefaultHtmlColor);
		}

		protected virtual string GetStatesText()
		{
			StringBuilder html = new StringBuilder();

			html.AppendLine("Queueing: Waiting for players to join the queue.".WrapUOHtmlColor(Color.Orange));
			html.AppendLine("Preparing: Allows time for players to prepare for battle.".WrapUOHtmlColor(Color.Gold));
			html.AppendLine("Running: The battle has started.".WrapUOHtmlColor(Color.LawnGreen));
			html.AppendLine(
				"Ended: A period of relief before the battle starts queueing again.".WrapUOHtmlColor(Color.OrangeRed));

			return html.ToString();
		}

		protected override string GetLabelText(int index, int pageIndex, PvPBattle entry)
		{
			return entry != null && !entry.Deleted ? entry.Name : base.GetLabelText(index, pageIndex, entry);
		}

		protected override int GetLabelHue(int index, int pageIndex, PvPBattle entry)
		{
			return GetStateLabelHue(index, pageIndex, entry);
		}

		protected virtual string GetStateLabelText(int index, int pageIndex, PvPBattle entry)
		{
			return entry != null ? entry.State.ToString() : String.Empty;
		}

		protected virtual int GetStateLabelHue(int index, int pageIndex, PvPBattle entry)
		{
			if (entry == null || entry.Deleted)
			{
				return Color.OrangeRed.ToArgb();
			}

			switch (entry.State)
			{
				case PvPBattleState.Queueing:
					return Color.Orange.ToArgb();
				case PvPBattleState.Preparing:
					return Color.Gold.ToArgb();
				case PvPBattleState.Running:
					return Color.SeaGreen.ToArgb();
			}

			return Color.OrangeRed.ToArgb();
		}

		protected virtual string GetTimeLabelText(int index, int pageIndex, PvPBattle entry)
		{
			if (entry != null && entry.State != PvPBattleState.Internal)
			{
				TimeSpan ts = entry.GetStateTimeLeft(DateTime.UtcNow);

				if (ts > TimeSpan.Zero)
				{
					return ts.Hours > 24 ? String.Format("> 1 Day") : ts.ToSimpleString("h:m:s");
				}

				if (!entry.IgnoreCapacity)
				{
					if (entry.CurrentCapacity < entry.MinCapacity)
					{
						return String.Format("Req: {0} / {1} ({2} Max)", entry.CurrentCapacity, entry.MinCapacity, entry.MaxCapacity);
					}
				}

				return (entry.IsFull) ? "Full" : String.Format("{0} / {1}", entry.CurrentCapacity, entry.MaxCapacity);
			}

			return String.Empty;
		}

		protected virtual int GetTimeLabelHue(int index, int pageIndex, PvPBattle entry)
		{
			return GetStateLabelHue(index, pageIndex, entry);
		}
	}
}