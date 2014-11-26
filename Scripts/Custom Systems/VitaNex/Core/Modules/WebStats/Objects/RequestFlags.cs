#region Header
//   Vorspire    _,-'/-'/  RequestFlags.cs
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
#endregion

namespace VitaNex.Modules.WebStats
{
	[Flags]
	public enum WebStatsRequestFlags
	{
		None = 0x00,
		Stats = 0x01,
		Guilds = 0x02,
		Players = 0x04,
		PlayerStats = 0x08 | Players,
		PlayerSkills = 0x10 | Players,
		PlayerEquip = 0x20 | Players,
		Server = 0x40,
		All = Stats | Guilds | Players | PlayerStats | PlayerSkills | PlayerEquip | Server
	}
}