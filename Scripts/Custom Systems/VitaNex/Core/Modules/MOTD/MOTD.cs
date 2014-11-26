#region Header
//   Vorspire    _,-'/-'/  MOTD.cs
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

using Server;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.MOTD
{
	public static partial class MOTD
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static MOTDOptions CMOptions { get; private set; }

		public static XmlDataStore<string, MOTDMessage> Messages { get; private set; }

		public static event Func<PlayerMobile, MOTDMessage, bool> OnPopUpRequest;

		private static bool HandlePopupRequest(PlayerMobile user, MOTDMessage message)
		{
			if (user == null || user.Deleted)
			{
				return false;
			}

			if (message == null)
			{
				user.SendMessage(0x22, "No message to display.");
				return false;
			}

			SuperGump.Send(new MOTDMessageOverviewGump(user, selected: message));
			return true;
		}

		private static void OnLogin(LoginEventArgs e)
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm == null || pm.Deleted || !pm.Alive)
			{
				return;
			}

			if (Messages.Count > 0 && CMOptions.LoginPopup)
			{
				Timer.DelayCall(TimeSpan.FromSeconds(1.337), () => SendPopUpTo(pm));
			}
		}

		public static bool SendPopUpTo(PlayerMobile user)
		{
			if (user == null || user.Deleted)
			{
				return false;
			}

			return OnPopUpRequest != null
					   ? OnPopUpRequest(user, GetMostRecentMessage())
					   : HandlePopupRequest(user, GetMostRecentMessage());
		}

		public static MOTDMessage GetMostRecentMessage()
		{
			return GetSortedMessages().FirstOrDefault(m => m.Published);
		}

		public static List<MOTDMessage> GetSortedMessages()
		{
			return Messages.Values.OrderByDescending(m => m.Date.Stamp).ToList();
		}
	}
}