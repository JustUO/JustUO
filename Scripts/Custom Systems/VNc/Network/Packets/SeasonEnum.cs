#region Header
//   Vorspire    _,-'/-'/  SeasonEnum.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex.Network
{
	public enum Season
	{
		Spring = 0,
		Summer = 1,
		Autumn = 2,
		Winter = 3,
		Desolation = 4,
		Fall = Autumn,
	}

	public static class SeasonEnumExt
	{
		public static int GetID(this Season season)
		{
			return (int)season;
		}

		public static string GetName(this Season season)
		{
			return season.ToString();
		}
	}
}