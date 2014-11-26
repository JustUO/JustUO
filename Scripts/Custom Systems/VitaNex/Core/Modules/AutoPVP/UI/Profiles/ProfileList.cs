#region Header
//   Vorspire    _,-'/-'/  ProfileList.cs
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
using System.Globalization;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Misc;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPProfileListGump : ListGump<PvPProfile>
	{
		public PvPProfileListGump(
			PlayerMobile user,
			PvPSeason season,
			Gump parent = null,
			bool useConfirm = true,
			PvPProfileRankOrder sortOrder = PvPProfileRankOrder.None)
			: base(user, parent, emptyText: "There are no profiles to display.", title: "PvP Profiles")
		{
			UseConfirmDialog = useConfirm;
			RankSortOrder = (sortOrder != PvPProfileRankOrder.None)
								? sortOrder
								: AutoPvP.CMOptions.Advanced.Profiles.RankingOrder;
			Season = season ?? AutoPvP.CurrentSeason;

			CanSearch = User.AccessLevel >= AccessLevel.Counselor || AutoPvP.CMOptions.Advanced.Profiles.AllowPlayerSearch;

			ForceRecompile = true;
			AutoRefresh = true;
		}

		public PvPProfileRankOrder RankSortOrder { get; set; }
		public PvPSeason Season { get; set; }
		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			if (AutoPvP.SeasonSchedule.Enabled)
			{
				Title = "PvP Profiles (Season " + (Season != null ? Season.Number : AutoPvP.CurrentSeason.Number) + ")";
			}

			base.Compile();
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (User.AccessLevel >= AutoPvP.Access)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Delete All",
						button =>
						Send(
							new ConfirmDialogGump(
								User,
								this,
								title: "Delete All Profiles?",
								html:
									"All profiles in the database will be deleted, erasing all data associated with them.\nThis action can not be reversed.\n\nDo you want to continue?",
								onAccept: subButton =>
								{
									var profiles = new List<PvPProfile>(AutoPvP.Profiles.Values);

									foreach (PvPProfile p in profiles.Where(p => p != null && !p.Deleted))
									{
										p.Delete();
									}

									Refresh(true);
								})),
						HighlightHue));
			}

			list.AppendEntry(new ListGumpEntry("My Profile", OnMyProfile));

			list.AppendEntry(
				new ListGumpEntry("Sort By (" + RankSortOrder + ")", b => Send(new PvPProfileListSortGump(User, this, this, b))));

			if (Season != null)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Overall Ranks",
						b =>
						{
							Season = null;
							Refresh(true);
						}));

				if (!Season.Active && User.AccessLevel >= AutoPvP.Access)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Issue Winner Rewards",
							b => Season.Winners.Keys.Select(m => AutoPvP.EnsureProfile(m)).ForEach(
								p =>
								{
									Season.IssueWinnerRewards(p);
									Refresh();
								})));

					list.AppendEntry(
						new ListGumpEntry(
							"Issue Loser Rewards",
							b => Season.Losers.Keys.Select(m => AutoPvP.EnsureProfile(m)).ForEach(
								p =>
								{
									Season.IssueLoserRewards(p);
									Refresh();
								})));
				}
			}

			PvPSeason season = AutoPvP.CurrentSeason;

			if (Season != season)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Season " + season.Number + " Ranks",
						b =>
						{
							Season = season;
							Refresh(true);
						}));
			}

			if (season.Number > 1)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Select Season",
						b =>
						Send(
							new InputDialogGump(
								User,
								this,
								title: "Select Season",
								html: "Enter the number for the season you wish to view rankings for.\nSeasons 1 to " + season.Number,
								input: Season == null ? "" : Season.Number.ToString(CultureInfo.InvariantCulture),
								callback: (ib, text) =>
								{
									int num;

									if (Int32.TryParse(text, out num))
									{
										if ((Season = (AutoPvP.Seasons.ContainsKey(num) ? AutoPvP.Seasons[num] : null)) == null)
										{
											User.SendMessage(ErrorHue, "Invalid Season selection.");
										}
									}

									Refresh(true);
								}))));
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnMyProfile(GumpButton button)
		{
			Send(new PvPProfileOverviewGump(User, AutoPvP.EnsureProfile(User), Hide(true)));
		}

		protected override void CompileList(List<PvPProfile> list)
		{
			list.Clear();
			list.AddRange(AutoPvP.GetSortedProfiles(RankSortOrder, Season));
			base.CompileList(list);
		}

		public override string GetSearchKeyFor(PvPProfile key)
		{
			if (key != null && !key.Deleted)
			{
				return key.Owner.RawName;
			}

			return base.GetSearchKeyFor(key);
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
						if (Parent is PvPBattleListGump)
						{
							((PvPBattleListGump)Parent).Refresh(true);
						}
						else if (Parent is PvPBattleCategoryListGump)
						{
							((PvPBattleCategoryListGump)Parent).Refresh(true);
						}
						else
						{
							Send(new PvPBattleCategoryListGump(User, Hide(true), UseConfirmDialog));
						}
					}));

			layout.AddReplace(
				"label/header/title",
				() => AddLabelCropped(160, 15, 215, 20, GetTitleHue(), String.IsNullOrEmpty(Title) ? DefaultTitle : Title));
		}

		protected override void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, PvPProfile> range)
		{
			layout.Add(
				"background/footer/help",
				() =>
				{
					AddBackground(0, 75 + (range.Count * 30), 420, 130, 9270);
					AddImageTiled(10, 85 + (range.Count * 30), 400, 110, 2624);
				});

			layout.Add("html/footer/help", () => AddHtml(20, 85 + (range.Count * 30), 390, 110, GetHelpText(), false, false));

			base.CompileEntryLayout(layout, range);
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, PvPProfile entry)
		{
			base.CompileEntryLayout(layout, length, index, pIndex, yOffset, entry);

			layout.AddReplace(
				"label/list/entry/" + index,
				() =>
				{
					AddLabelCropped(65, 2 + yOffset, 160, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry));
					AddLabelCropped(
						225, 2 + yOffset, 150, 20, GetSortLabelHue(index, pIndex, entry), GetSortLabelText(index, pIndex, entry));
				});
		}

		protected virtual string GetHelpText()
		{
			return
				String.Format(
					"<BASEFONT COLOR=#{0:X6}>PvP profiles store all of your battle statistics.\nThey can be ranked in order of total points, wins or kills.\nDo you think you have what it takes to earn the number one rank on {1}?",
					DefaultHtmlColor.ToArgb(),
					ServerList.ServerName);
		}

		protected override int GetLabelHue(int index, int pageIndex, PvPProfile entry)
		{
			if (index < 3)
			{
				return HighlightHue;
			}

			if (entry != null)
			{
				return Notoriety.GetHue(Notoriety.Compute(User, entry.Owner));
			}

			return base.GetLabelHue(index, pageIndex, null);
		}

		protected override string GetLabelText(int index, int pageIndex, PvPProfile entry)
		{
			if (entry != null && entry.Owner != null)
			{
				if (RankSortOrder != PvPProfileRankOrder.None)
				{
					return String.Format("{0}: {1}", entry.GetRank(Season), entry.Owner.RawName);
				}

				return entry.Owner.RawName;
			}

			return base.GetLabelText(index, pageIndex, entry);
		}

		protected virtual string GetSortLabelText(int index, int pageIndex, PvPProfile entry)
		{
			if (entry != null)
			{
				string key = "Rank";
				long val = entry.GetRank(Season);

				switch (RankSortOrder)
				{
					case PvPProfileRankOrder.Points:
						{
							key = "Points";

							val = Season == null
									  ? entry.TotalPointsGained - entry.TotalPointsLost
									  : entry.History.EnsureEntry(Season).PointsGained - entry.History.EnsureEntry(Season).PointsLost;
						}
						break;
					case PvPProfileRankOrder.Kills:
						{
							key = "Kills";

							val = Season == null ? entry.TotalKills : entry.History.EnsureEntry(Season).Kills;
						}
						break;
					case PvPProfileRankOrder.Wins:
						{
							key = "Wins";

							val = Season == null ? entry.TotalWins : entry.History.EnsureEntry(Season).Wins;
						}
						break;
				}

				return String.Format("{0}: {1}", key, (val <= 0) ? "0" : val.ToString("#,#"));
			}

			return String.Empty;
		}

		protected virtual int GetSortLabelHue(int index, int pageIndex, PvPProfile entry)
		{
			if (entry != null)
			{
				switch (RankSortOrder)
				{
					case PvPProfileRankOrder.None:
						{
							return (entry.GetRank(Season) <= AutoPvP.CMOptions.Advanced.Seasons.TopListCount) ? HighlightHue : TextHue;
						}
					case PvPProfileRankOrder.Points:
						{
							if (Season == null)
							{
								return ((entry.TotalPointsGained - entry.TotalPointsLost) <= 0) ? ErrorHue : TextHue;
							}

							return ((entry.History.EnsureEntry(Season).PointsGained - entry.History.EnsureEntry(Season).PointsLost) <= 0)
									   ? ErrorHue
									   : TextHue;
						}
					case PvPProfileRankOrder.Kills:
						{
							if (Season == null)
							{
								return ((entry.TotalKills) <= 0) ? ErrorHue : TextHue;
							}

							return (entry.History.EnsureEntry(Season).Kills <= 0) ? ErrorHue : TextHue;
						}
					case PvPProfileRankOrder.Wins:
						{
							if (Season == null)
							{
								return ((entry.TotalWins) <= 0) ? ErrorHue : TextHue;
							}

							return (entry.History.EnsureEntry(Season).Wins <= 0) ? ErrorHue : TextHue;
						}
				}

				return TextHue;
			}

			return ErrorHue;
		}

		protected override void SelectEntry(GumpButton button, PvPProfile entry)
		{
			base.SelectEntry(button, entry);

			if (button == null || entry == null || entry.Deleted)
			{
				return;
			}

			Send(new PvPProfileOverviewGump(User, entry, Hide(true), UseConfirmDialog));
		}
	}
}