#region Header
//   Vorspire    _,-'/-'/  Schedules_Init.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;

using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Schedules
{
	[CoreService("Schedules", "1.0.0.0", TaskPriority.High)]
	public static partial class Schedules
	{
		static Schedules()
		{
			Registry = new Dictionary<string, Schedule>();

			ScheduleTimes.Config();
		}

		private static void CSConfig()
		{
			CommandUtility.Register(
				"Schedules",
				Access,
				e =>
				{
					if (e != null && e.Mobile != null && !e.Mobile.Deleted && e.Mobile is PlayerMobile)
					{
						SuperGump.Send(new ScheduleListGump((PlayerMobile)e.Mobile));
					}
				});
		}
	}
}