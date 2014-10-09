#region Header
//   Vorspire    _,-'/-'/  Interfaces.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

namespace VitaNex.SuperGumps
{
	public interface ISuperGumpPages
	{
		int EntriesPerPage { get; set; }
		int PageCount { get; }
		int Page { get; set; }

		void PreviousPage();
		void NextPage();
	}
}