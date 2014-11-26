#region Header
//   Vorspire    _,-'/-'/  BattleCategoryList.cs
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
using System.Linq;
using System.Text;

using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

using VitaNex.Schedules;
using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPBattleCategoryListGump : ListGump<string>
	{
		private readonly Dictionary<string, int> _CategoryIndex = new Dictionary<string, int>();

		public PvPBattleCategoryListGump(PlayerMobile user, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "There are no battles to display.", title: "PvP Battles")
		{
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
			AutoRefresh = true;

			Sorted = true;
		}

		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			if (AutoPvP.SeasonSchedule.Enabled)
			{
				Title = "PvP Battles (Season " + AutoPvP.CurrentSeason.Number + ")";
			}

			base.Compile();
		}

		public override int SortCompare(string a, string b)
		{
			int result = 0;

			if (a.CompareNull(b, ref result))
			{
				return result;
			}

			return String.Compare(a, b, StringComparison.Ordinal);
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
						HighlightHue));

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

		protected override void CompileList(List<string> list)
		{
			list.Clear();
			_CategoryIndex.Clear();

			bool viewInternal = (User.AccessLevel >= AutoPvP.Access);

			foreach (string cat in
				AutoPvP.Battles.Values.Where(
					battle =>
					battle != null && !battle.Deleted && ((battle.State != PvPBattleState.Internal && !battle.Hidden) || viewInternal))
					   .Select(battle => (String.IsNullOrWhiteSpace(battle.Category) ? "Misc" : battle.Category).ToUpperWords()))
			{
				if (!list.Contains(cat))
				{
					list.Add(cat);
				}

				if (!_CategoryIndex.ContainsKey(cat))
				{
					_CategoryIndex.Add(cat, 1);
				}
				else
				{
					_CategoryIndex[cat]++;
				}
			}

			base.CompileList(list);
		}

		public override string GetSearchKeyFor(string key)
		{
			return key;
		}

		protected override void SelectEntry(GumpButton button, string entry)
		{
			base.SelectEntry(button, entry);

			if (button != null)
			{
				Send(new PvPBattleListGump(User, this, entry, UseConfirmDialog));
			}
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

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, string entry)
		{
			base.CompileEntryLayout(layout, length, index, pIndex, yOffset, entry);

			layout.AddReplace(
				"label/list/entry/" + index,
				() =>
				{
					AddHtml(
						65,
						2 + yOffset,
						195,
						20,
						String.Format(
							"<basefont color=#{0:X6}>{1}", GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry)),
						false,
						false);
					AddHtml(
						270,
						2 + yOffset,
						125,
						20,
						String.Format(
							"<basefont color=#{0:X6}>{1}", GetCountLabelHue(index, pIndex, entry), GetCountLabelText(index, pIndex, entry)),
						false,
						false);
				});
		}

		protected override void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, string> range)
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

		protected override string GetLabelText(int index, int pageIndex, string entry)
		{
			return entry;
		}

		protected override int GetLabelHue(int index, int pageIndex, string entry)
		{
			return Color.LightSkyBlue.ToArgb();
		}

		protected virtual string GetCountLabelText(int index, int pageIndex, string entry)
		{
			if (entry != null && _CategoryIndex.ContainsKey(entry))
			{
				return String.Format("[{0}]", _CategoryIndex[entry].ToString("#,0"));
			}

			return String.Empty;
		}

		protected virtual int GetCountLabelHue(int index, int pageIndex, string entry)
		{
			return Color.LightGreen.ToArgb();
		}
	}
}