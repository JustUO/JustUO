#region Header
//   Vorspire    _,-'/-'/  Sandbox_Init.cs
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
using System.Linq;
#endregion

namespace VitaNex.Sandbox
{
	[CoreService("Sandbox", "1.1.0", TaskPriority.Lowest)]
	public static partial class Sandbox
	{
		private static readonly Type _TypeOfISandboxTest = typeof(ISandboxTest);

		private static void CSConfig()
		{
			foreach (var test in
				_TypeOfISandboxTest.GetConstructableChildren()
								   .Select(t => t.CreateInstanceSafe<ISandboxTest>())
								   .Where(test => test != null))
			{
				SafeInvoke(test.EntryPoint, test);
			}
		}
	}
}