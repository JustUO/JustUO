#region Header
//   Vorspire    _,-'/-'/  ProfileGump.cs
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

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public class DonationProfileGump : SuperGumpList<DonationTransaction>
	{
		public DonationProfileGump(PlayerMobile user, DonationProfile profile, Gump parent = null)
			: base(user, parent)
		{
			Profile = profile;
			EntriesPerPage = 8;
		}

		public DonationProfile Profile { get; set; }

		protected override void CompileList(List<DonationTransaction> list)
		{
			List.Clear();
			List.AddRange(Profile.Transactions.Values);
			base.CompileList(list);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add("background/header", () => AddBackground(46, 25, 500, 50, 9270));

			layout.Add("background/body", () => AddBackground(45, 68, 500, 300, 9270));

			layout.Add("background/footer", () => AddBackground(45, 362, 500, 60, 9270));

			layout.Add("label/title", () => AddLabel(67, 39, 152, @"Donation Profile for " + Profile.Account.Username));

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

		public virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, DonationTransaction> range)
		{
			int pageIndex = 0;
			foreach (var kvp in range)
			{
				DonationTransaction entry = kvp.Value;
				int index = kvp.Key, pIndex = pageIndex, yOffset = pIndex * 30;

				CompileEntryLayout(layout, range.Count, index, pIndex, yOffset, entry);
				pageIndex++;
			}
		}

		public virtual void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, DonationTransaction entry)
		{
			int picID = (int)StatusGem.Pending, textColor = (int)TextColor.Pending;

			if (entry == null || entry.Account == null)
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

			switch (entry.State)
			{
				case DonationTransactionState.Void:
					{
						picID = (int)StatusGem.Void;
						textColor = (int)TextColor.Void;
					}
					break;
				case DonationTransactionState.Pending:
					{
						picID = (int)StatusGem.Pending;
						textColor = (int)TextColor.Pending;
					}
					break;
				case DonationTransactionState.Processed:
					{
						picID = (int)StatusGem.Pending;
						textColor = (int)TextColor.Pending;
					}
					break;
				case DonationTransactionState.Claimed:
					{
						picID = (int)StatusGem.Claimed;
						textColor = (int)TextColor.Claimed;
					}
					break;
			}

			layout.Add(
				"custom/entry/" + index,
				() =>
				{
					AddImage(65, 90 + (40 * pIndex), picID);
					AddLabel(
						140,
						94 + (40 * pIndex),
						textColor,
						String.Format("{0} [{1}] ({2:#,#} Credits)", entry.ID, entry.Time.Value.ToShortDateString(), entry.Credit.Value));
					AddImageTiled(60, 120 + (40 * pIndex), 465, 10, 4201);
					AddButton(100, 94 + (40 * pIndex), 4012, 4013, b => SelectEntry(entry));
				});
		}

		protected void SelectEntry(DonationTransaction entry)
		{
			if (entry == null || entry.Account == null)
			{
				Refresh();
				return;
			}

			Send(new DonationTransactionOverviewGump(User, Hide(true), Profile, entry));
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