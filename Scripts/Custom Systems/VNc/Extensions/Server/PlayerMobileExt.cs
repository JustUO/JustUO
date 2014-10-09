#region Header
//   Vorspire    _,-'/-'/  PlayerMobileExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Mobiles;
#endregion

namespace Server
{
	public static class PlayerMobileExtUtility
	{
		public static bool IsOnline(this PlayerMobile player)
		{
			return player != null && player.NetState != null && player.NetState.Socket != null && player.NetState.Running;
		}

		public static ClientVersion GetClientVersion(this PlayerMobile player)
		{
			return IsOnline(player) ? player.NetState.Version : new ClientVersion(null);
		}

		public static ClientType GetClientType(this PlayerMobile player)
		{
			return IsOnline(player) ? player.NetState.Version.Type : ClientType.Regular;
		}

		public static bool HasClient(this PlayerMobile player, ClientType type)
		{
			return IsOnline(player) && player.NetState.Version.Type == type;
		}
	}
}