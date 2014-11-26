#region Header
//   Vorspire    _,-'/-'/  LNG.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex
{
	public enum ClilocLNG
	{
		NULL,
		ENU,
		DEU,
		ESP,
		FRA,
		JPN,
		KOR,
		CHT
	}

	public static class ClilocLNGExt
	{
		public static string ToFullString(this ClilocLNG lng)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = Clilocs.DefaultLanguage;
			}

			switch (lng)
			{
				case ClilocLNG.ENU:
					return "English";
				case ClilocLNG.DEU:
					return "German";
				case ClilocLNG.ESP:
					return "Spanish";
				case ClilocLNG.FRA:
					return "French";
				case ClilocLNG.JPN:
					return "Japanese";
				case ClilocLNG.KOR:
					return "Korean";
				case ClilocLNG.CHT:
					return "Chinese";
			}

			return lng.ToString();
		}
	}
}