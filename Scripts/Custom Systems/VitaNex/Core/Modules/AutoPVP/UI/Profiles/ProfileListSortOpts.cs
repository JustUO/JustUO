#region Header
//   Vorspire    _,-'/-'/  ProfileListSortOpts.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPProfileListSortGump : MenuGump
	{
		public PvPProfileListSortGump(
			PlayerMobile user, PvPProfileListGump listGump, Gump parent = null, GumpButton clicked = null)
			: base(user, parent, clicked: clicked)
		{
			ListGump = listGump;
		}

		public PvPProfileListGump ListGump { get; set; }

		protected override void CompileOptions(MenuGumpOptions list)
		{
			if (ListGump != null)
			{
				if (ListGump.RankSortOrder != PvPProfileRankOrder.None)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"No Sorting",
							button =>
							{
								ListGump.RankSortOrder = PvPProfileRankOrder.None;
								ListGump.Refresh(true);
							}));
				}

				if (ListGump.RankSortOrder != PvPProfileRankOrder.Kills)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Kills",
							button =>
							{
								ListGump.RankSortOrder = PvPProfileRankOrder.Kills;
								ListGump.Refresh(true);
							}));
				}

				if (ListGump.RankSortOrder != PvPProfileRankOrder.Points)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Points",
							button =>
							{
								ListGump.RankSortOrder = PvPProfileRankOrder.Points;
								ListGump.Refresh(true);
							}));
				}

				if (ListGump.RankSortOrder != PvPProfileRankOrder.Wins)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Wins",
							button =>
							{
								ListGump.RankSortOrder = PvPProfileRankOrder.Wins;
								ListGump.Refresh(true);
							}));
				}
			}

			base.CompileOptions(list);
		}
	}
}