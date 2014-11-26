#region Header
//   Vorspire    _,-'/-'/  ExceptionExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.IO;

using Server;

using VitaNex;
#endregion

namespace System
{
	public static class ExceptionExtUtility
	{
		public static void ToConsole(this Exception e, bool simple = false, bool log = false)
		{
			if (e == null)
			{
				return;
			}

			lock (VitaNexCore.ConsoleLock)
			{
				Utility.PushColor(ConsoleColor.DarkRed);
				Console.WriteLine(simple ? e.Message : e.ToString());
				Utility.PopColor();
			}

			if (log)
			{
				Log(e);
			}
		}

		public static void Log(this Exception e, FileInfo file = null)
		{
			if (e == null)
			{
				return;
			}

			file = file ?? VitaNexCore.LogFile;

			string now = String.Format("***ERROR LOG [{0}]***", DateTime.Now.ToSimpleString("t@h:m:s@ Z"));

			lock (VitaNexCore.IOLock)
			{
				file.AppendText(false, String.Empty, now, e.Message, e.ToString(), e.HelpLink, String.Empty);
			}
		}
	}
}