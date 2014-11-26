#region Header
//   Vorspire    _,-'/-'/  GiftGump.cs
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
using System.Linq;
using System.Text;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public class DonationGiftGump : SuperGump
	{
		public DonationGiftGump(
			PlayerMobile user, Gump parent, DonationProfile profile, DonationTransaction trans, PlayerMobile to = null)
			: base(user, parent)
		{
			Profile = profile;
			Transaction = trans;
			To = to;
		}

		private string InternalName { get; set; }
		private string InternalMessage { get; set; }

		public DonationProfile Profile { get; set; }
		public DonationTransaction Transaction { get; set; }
		public PlayerMobile To { get; set; }

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

			layout.Add("label/to", () => AddLabel(70, 134, textColor, "Gift To:"));

			layout.Add(
				"textentry/to",
				() =>
				{
					AddBackground(180, 135, 330, 25, 9350);
					AddTextEntryLimited(
						190,
						138,
						310,
						25,
						textColor,
						0,
						(To != null) ? To.RawName : String.IsNullOrWhiteSpace(InternalName) ? "" : InternalName,
						40,
						(input, text) => { InternalName = text; });
				});

			layout.Add("label/message", () => AddLabel(70, 174, textColor, "Message:"));

			layout.Add(
				"textentry/message",
				() =>
				{
					AddBackground(180, 175, 330, 25, 9350);
					AddTextEntryLimited(
						190,
						178,
						310,
						25,
						textColor,
						String.IsNullOrWhiteSpace(InternalMessage)
							? User.RawName + @" has sent you " + Transaction.Credit.Value.ToString("N") + " Credits as a gift!"
							: InternalMessage,
						255,
						(input, text) => { InternalMessage = text; });
				});

			layout.Add(
				"html/desc",
				() =>
				{
					StringBuilder html = new StringBuilder();

					html.AppendLine("Information:");
					html.AppendLine(
						"When you gift a transaction to another player, they will instantly be credited with the value of this transaction.");

					AddHtml(67, 218, 454, 126, html.ToString(), false, true);
				});

			layout.Add("label/status", () => AddLabel(111, 382, textColor, statusDesc));

			if (Transaction.State == DonationTransactionState.Processed)
			{
				layout.Add("button/send", () => AddButton(449, 384, 12002, 12000, OnGiftSend));
			}
		}

		protected void OnBack(GumpButton b)
		{
			Close();
		}

		protected void OnGiftSend(GumpButton b)
		{
			if (Transaction.State != DonationTransactionState.Processed)
			{
				return;
			}

			if (To != null && !To.Deleted && Profile.Claim(Transaction, User, To, InternalMessage))
			{
				return;
			}

			if (String.IsNullOrWhiteSpace(InternalName))
			{
				Refresh();
				return;
			}

			var mobiles = PlayerNames.FindPlayers(InternalName).OrderByNatural(m => m.RawName).ToList();

			if (mobiles.Count == 0)
			{
				return;
			}

			if (mobiles.Count == 1)
			{
				To = mobiles[0];
				Refresh(true);
			}
			else
			{
				Send(new DonationGiftPlayerSelectGump(User, Profile, Transaction, Hide(true), mobiles));
			}

			mobiles.Free(true);
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