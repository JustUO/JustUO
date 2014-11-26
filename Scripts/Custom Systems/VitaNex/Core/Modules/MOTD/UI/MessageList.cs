#region Header
//   Vorspire    _,-'/-'/  MessageList.cs
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
using System.Linq;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.Crypto;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.MOTD
{
	public class MOTDMessageListGump : ListGump<MOTDMessage>
	{
		private InputDialogGump _NewMessageInput;

		public bool UseConfirmDialog { get; set; }

		public MOTDMessageListGump(PlayerMobile user, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "There are no messages to display.", title: "MOTD Messages")
		{
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
			CanMove = false;
			CanResize = false;
		}

		protected override string GetLabelText(int index, int pageIndex, MOTDMessage entry)
		{
			return entry != null ? entry.ToString() : base.GetLabelText(index, pageIndex, null);
		}

		protected override int GetLabelHue(int index, int pageIndex, MOTDMessage entry)
		{
			return entry != null ? entry.Published ? HighlightHue : ErrorHue : base.GetLabelHue(index, pageIndex, null);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (User.AccessLevel >= MOTD.Access)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Login Popup [" + (MOTD.CMOptions.LoginPopup ? "Enabled" : "Disabled") + "]",
						button =>
						{
							MOTD.CMOptions.LoginPopup = !MOTD.CMOptions.LoginPopup;
							Refresh(true);
						},
						(MOTD.CMOptions.LoginPopup ? HighlightHue : ErrorHue)));

				list.AppendEntry(
					new ListGumpEntry(
						"Load Messages",
						button =>
						{
							MOTD.Messages.Import();
							Refresh(true);
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"Save Messages",
						button =>
						{
							MOTD.Messages.Export();
							Refresh(true);
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"Delete All",
						button =>
						{
							if (UseConfirmDialog)
							{
								Send(
									new ConfirmDialogGump(
										User,
										this,
										title: "Delete All Messages?",
										html:
											"All messages in the MOTD database will be deleted, erasing all data associated with them.\nThis action can not be reversed.\n\nDo you want to continue?",
										onAccept: subButton =>
										{
											DeleteAllMessages();
											Refresh(true);
										}));
							}
							else
							{
								DeleteAllMessages();
								Refresh(true);
							}
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"New Message",
						button =>
						{
							_NewMessageInput = new InputDialogGump(
								User,
								this,
								title: "Create New Message?",
								html: "This action will create a new MOTD message with the selected title.\nDo you want to continue?",
								callback: OnConfirmCreateMessage);
							Send(_NewMessageInput);
						},
						HighlightHue));
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmCreateMessage(GumpButton button, string title)
		{
			TimeStamp date = TimeStamp.UtcNow;
			string author = User.RawName;

			const string content = "Content - BBCode supported:<br>[B]B, U, I, BIG, SMALL, URL, COLOR[/B]";

			string uid = CryptoGenerator.GenString(CryptoHashType.MD5, String.Format("{0}", date.Stamp)).Replace("-", "");
			Selected = new MOTDMessage(uid, date, title, content, author);

			if (MOTD.Messages.ContainsKey(uid))
			{
				MOTD.Messages[uid] = Selected;
			}
			else
			{
				MOTD.Messages.Add(uid, Selected);
			}

			MOTD.Messages.Export();

			Refresh(true);

			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						this,
						title: "View Message?",
						html: "Your message has been created.\nDo you want to view it now?",
						onAccept: subButton => Send(new MOTDMessageOverviewGump(User, this, Selected))));
			}
			else
			{
				Send(new MOTDMessageOverviewGump(User, this, Selected));
			}
		}

		private void DeleteAllMessages()
		{
			foreach (MOTDMessage message in List.Where(message => message != null))
			{
				message.Delete();
			}

			MOTD.Messages.Clear();
			MOTD.Messages.Export();
		}

		protected override void CompileList(List<MOTDMessage> list)
		{
			list.Clear();

			list.AddRange(
				MOTD.GetSortedMessages().Where(message => message != null && (message.Published || User.AccessLevel >= MOTD.Access)));

			base.CompileList(list);
		}

		protected override void SelectEntry(GumpButton button, MOTDMessage entry)
		{
			base.SelectEntry(button, entry);

			if (button != null && entry != null)
			{
				Send(new MOTDMessageOverviewGump(User, this, entry, UseConfirmDialog));
			}
		}
	}
}