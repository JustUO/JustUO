#region Header
//   Vorspire    _,-'/-'/  PlayerSelectGump.cs
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
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public class DonationGiftPlayerSelectGump : SuperGumpList<PlayerMobile>
	{
		public DonationGiftPlayerSelectGump(
			PlayerMobile user,
			DonationProfile profile,
			DonationTransaction trans,
			Gump parent = null,
			IEnumerable<PlayerMobile> list = null)
			: base(user, parent, list: list)
		{
			Profile = profile;
			Transaction = trans;
			EntriesPerPage = 8;
		}

		public DonationProfile Profile { get; set; }
		public DonationTransaction Transaction { get; set; }

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add("background/header", () => AddBackground(46, 25, 500, 50, 9270));

			layout.Add("background/body", () => AddBackground(45, 68, 500, 300, 9270));

			layout.Add("background/footer", () => AddBackground(45, 362, 500, 60, 9270));

			layout.Add("label/title", () => AddLabel(67, 39, 152, @"Select a player from the list."));

			if (Page > 0) //Display Previous Button
			{
				layout.Add(
					"button/prev",
					() =>
					{
						AddButton(63, 381, 4015, 4016, PreviousPage);
						AddTooltip(1011067);
					});
			}
			else
			{
				layout.Add("image/prev", () => AddImage(63, 381, 4014));
			}

			layout.Add(
				"label/pages",
				() => AddLabel(251, 381, 152, String.Format("Page {0:#,#} of {1:#,#} ({2:#,#})", Page + 1, PageCount, List.Count)));

			if (Page + 1 < PageCount) //Display Next Button
			{
				layout.Add(
					"button/next",
					() =>
					{
						AddButton(497, 381, 4006, 4007, NextPage);
						AddTooltip(1011066);
					});
			}
			else
			{
				layout.Add("image/next", () => AddImage(497, 381, 4005));
			}

			CompileEntryLayout(layout, GetListRange());
		}

		public virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, PlayerMobile> range)
		{
			int pageIndex = 0;
			foreach (var kvp in range)
			{
				PlayerMobile entry = kvp.Value;
				int index = kvp.Key, pIndex = pageIndex, yOffset = pIndex * 30;

				CompileEntryLayout(layout, range.Count, index, pIndex, yOffset, entry);
				pageIndex++;
			}
		}

		public virtual void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, PlayerMobile entry)
		{
			int picID = (int)StatusGem.Pending;

			if (entry == null || entry.Deleted)
			{
				layout.Add(
					"custom/entry/" + index,
					() =>
					{
						AddImage(65, 90 + (40 * pIndex), picID);
						AddLabel(140, 94 + (40 * pIndex), 57, "[Removed]");
						AddImageTiled(60, 120 + (40 * pIndex), 465, 10, 4201);
						AddImage(100, 94 + (40 * pIndex), 4010);
					});

				return;
			}

			if (entry.NetState != null && entry.NetState.Running)
			{
				picID = (int)StatusGem.Claimed;
			}
			else
			{
				picID = (int)StatusGem.Void;
			}

			int textColor = Notoriety.GetHue(Notoriety.Compute(User, entry));

			layout.Add(
				"custom/entry/" + index,
				() =>
				{
					AddImage(65, 90 + (40 * pIndex), picID);
					AddLabel(
						140,
						94 + (40 * pIndex),
						textColor,
						String.Format(
							"{0} [{1}] <{2}> ({3})",
							entry.RawName,
							entry.Name,
							(entry.Guild != null) ? entry.Guild.Abbreviation : String.Empty,
							entry.Location));

					AddImageTiled(60, 120 + (40 * pIndex), 465, 10, 4201);
					AddButton(100, 94 + (40 * pIndex), 4012, 4013, b => SelectEntry(entry));
				});
		}

		protected void SelectEntry(PlayerMobile entry)
		{
			if (entry == null)
			{
				Refresh();
				return;
			}

			DonationGiftGump dgg = Parent as DonationGiftGump;

			if (dgg == null)
			{
				Send(new DonationGiftGump(User, Hide(true), Profile, Transaction, entry));
			}
			else
			{
				dgg.To = entry;
				dgg.Refresh(true);
			}
		}

		private enum StatusGem
		{
			Pending = 10810,
			Claimed = 10830,
			Void = 10850
		}

		private enum TextColor
		{
			Pending = 92,
			Claimed = 57,
			Void = 42
		}
	}
}