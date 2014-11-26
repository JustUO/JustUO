#region Header
//   Vorspire    _,-'/-'/  Operator.cs
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

namespace VitaNex.MySQL
{
	[Flags]
	public enum MySQLOperator
	{
		None = 0x00,
		Equal = 0x01,
		Not = 0x02,
		Lower = 0x04,
		Greater = 0x08,
		Like = 0x10,
		LowerOrEqual = Lower | Equal,
		GreaterOrEqual = Greater | Equal,
		NotEqual = Not | Equal
	}
}