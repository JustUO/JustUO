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

namespace Server.Mobiles
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

		public static void FixMap(this PlayerMobile m, MapPoint mp)
		{
			FixMap(m, mp.Map, mp.Location);
		}

		public static void FixMap(this PlayerMobile m, Map def, Point3D loc)
		{
			if (m == null || m.Deleted || def == null || def == Map.Internal || loc == Point3D.Zero)
			{
				return;
			}

			if (m.LogoutMap == null || m.LogoutMap == Map.Internal)
			{
				m.LogoutLocation = loc.ToPoint3D();
				m.LogoutMap = def;
			}

			if (m.Map != null)
			{
				return;
			}

			if (IsOnline(m))
			{
				m.MoveToWorld(loc, def);
				BaseCreature.TeleportPets(m, loc, def);
			}
			else
			{
				m.Location = loc;
				m.Internalize();
				m.AutoStablePets();
			}
		}
	}
}