#region Header
//   Vorspire    _,-'/-'/  SectorExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace Server
{
	public static class SectorExtUtility
	{
		public static bool Contains(this Sector s, Point3D p)
		{
			return s.RegionRects.Contains(p);
		}
	}
}