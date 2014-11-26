#region Header
//   Vorspire    _,-'/-'/  TransactionGump.cs
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
using System.Text;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public class DonationTransactionOverviewGump : SuperGump
	{
		public DonationTransactionOverviewGump(
			PlayerMobile user, Gump parent, DonationProfile profile, DonationTransaction trans)
			: base(user, parent)
		{
			Profile = profile;
			Transaction = trans;
		}

		public DonationProfile Profile { get; set; }
		public DonationTransaction Transaction { get; set; }

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add("background/header", () => AddBackground(46, 25, 500, 50, 9270));

			layout.Add("background/body", () => AddBackground(45, 68, 500, 300, 9270));

			layout.Add("background/footer", () => AddBackground(45, 362, 500, 60, 9270));

			layout.Add(
				"imagetiled/seps",
				() =>
				{
					AddImageTiled(60, 120, 465, 10, 4201);
					AddImageTiled(60, 160, 465, 10, 4201);
					AddImageTiled(60, 200, 465, 10, 4201);
				});

			layout.Add("label/title", () => AddLabel(67, 39, 152, "Gift Transaction Overview"));

			layout.Add(
				"button/back",
				() =>
				{
					AddButton(63, 381, 4015, 4016, OnBack);
					AddTooltip(3010002);
				});

			int picID = (int)StatusGem.Void;
			int textColor = (int)TextColor.Void;
			string statusDesc = "This transaction is Unknown", tag = Transaction.State.ToString();

			switch (Transaction.State)
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
						statusDesc = "This transaction is Pending confirmation.";
					}
					break;
				case DonationTransactionState.Processed:
					{
						picID = (int)StatusGem.Pending;
						textColor = (int)TextColor.Pending;
						statusDesc = "This transaction has been Processed.";
					}
					break;
				case DonationTransactionState.Claimed:
					{
						picID = (int)StatusGem.Claimed;
						textColor = (int)TextColor.Claimed;
						statusDesc = "This transaction has been Claimed.";
					}
					break;
			}

			layout.Add("image/gem", () => AddImage(65, 90, picID));

			layout.Add("label/tag", () => AddLabel(107, 94, textColor, tag));

			layout.Add("label/id", () => AddLabel(239, 94, textColor, String.Format("ID: {0}", Transaction.ID)));

			layout.Add(
				"label/date",
				() => AddLabel(239, 134, textColor, String.Format("Date: {0}", Transaction.Time.Value.ToShortDateString())));

			layout.Add(
				"label/account",
				() =>
				AddLabel(
					70,
					134,
					textColor,
					String.Format("Account: {0}", (Transaction.Account == null ? "No Account" : Transaction.Account.Username))));

			layout.Add(
				"label/cost",
				() =>
				AddLabel(
					70,
					174,
					textColor,
					String.Format(
						"Total: {0}{1:N} ({2})", AutoDonate.CMOptions.MoneySymbol, Transaction.Total, AutoDonate.CMOptions.MoneyAbbr)));

			layout.Add(
				"label/credits", () => AddLabel(239, 174, textColor, String.Format("Credits: {0:#,#}", Transaction.Credit.Value)));

			layout.Add(
				"html/details",
				() =>
				{
					StringBuilder html = new StringBuilder();

					html.AppendLine("Delivery Details:");

					if (Transaction.IsGift)
					{
						html.AppendLine("");
						html.AppendFormat("Gifted to {0} from {1}", Transaction.DeliverTo.RawName, Transaction.DeliverFrom.RawName);
					}

					html.AppendLine("");
					html.AppendFormat("Received: {0}", Transaction.DeliveryTime.Value.ToShortDateString());

					html.AppendLine("");

					html.AppendLine("Notes:");
					html.AppendLine(Transaction.Notes);
					html.AppendLine("");

					html.AppendLine("Extra Information:");
					html.AppendLine(Transaction.Extra);
					html.AppendLine("");

					AddHtml(67, 218, 454, 126, html.ToString(), false, true);
				});

			layout.Add("label/status", () => AddLabel(111, 382, textColor, statusDesc));

			if (Transaction.State == DonationTransactionState.Processed)
			{
				if (AutoDonate.CMOptions.GiftingEnabled)
				{
					layout.Add("button/gift", () => AddButton(410, 381, 4029, 4030, OnGift));
				}

				layout.Add("button/claim", () => AddButton(449, 384, 12002, 12000, OnClaim));
			}
		}

		protected void OnBack(GumpButton b)
		{
			Close();
		}

		protected void OnGift(GumpButton b)
		{
			Send(new DonationGiftGump(User, Hide(true), Profile, Transaction));
		}

		protected void OnClaim(GumpButton b)
		{
			Profile.Claim(Transaction, User, User);
			Refresh(true);
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