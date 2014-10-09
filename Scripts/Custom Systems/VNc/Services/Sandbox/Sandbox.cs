#region Header
//   Vorspire    _,-'/-'/  Sandbox.cs
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

namespace VitaNex.Sandbox
{
	public static partial class Sandbox
	{
		private static CoreServiceOptions _CSOptions = new CoreServiceOptions(typeof(Sandbox));
		public static CoreServiceOptions CSOptions { get { return _CSOptions ?? (_CSOptions = new CoreServiceOptions(typeof(Sandbox))); } }

		public static void SafeInvoke(Action func, ISandboxTest test)
		{
			if (func == null || test == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					func();
					test.OnSuccess();
				},
				test.OnException);
		}
	}
}