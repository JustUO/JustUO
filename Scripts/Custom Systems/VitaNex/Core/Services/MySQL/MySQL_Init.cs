#region Header
//   Vorspire    _,-'/-'/  MySQL_Init.cs
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
#endregion

namespace VitaNex.MySQL
{
	[CoreService("MySQL", "1.0.0.0", TaskPriority.Highest)]
	public static partial class MySQL
	{
		static MySQL()
		{
			CSOptions = new MySQLOptions();
			Connections = new List<MySQLConnection>();
		}

		private static void CSConfig()
		{
			ConnectionPoller = PollTimer.CreateInstance(
				TimeSpan.FromSeconds(15),
				() => Connections.Where(c => !c.Connected).ForEach(Disconnected),
				() => Connections.Count > 0);
		}
	}
}