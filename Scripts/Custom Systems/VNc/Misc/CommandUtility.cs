#region Header
//   Vorspire    _,-'/-'/  CommandUtility.cs
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

using Server;
using Server.Commands;
#endregion

namespace VitaNex
{
	public static class CommandUtility
	{
		public static bool Register(string value, AccessLevel access, CommandEventHandler handler)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				return false;
			}

			if (CommandSystem.Entries.ContainsKey(value))
			{
				return Replace(value, access, handler, value);
			}

			CommandSystem.Register(value, access, handler);
			return true;
		}

		public static CommandEntry Unregister(string value)
		{
			CommandEntry handler = null;

			if (!String.IsNullOrWhiteSpace(value))
			{
				CommandSystem.Entries.TryGetValue(value, out handler);
			}

			return handler;
		}

		public static bool Replace(string value, AccessLevel access, CommandEventHandler handler, string newValue)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				if (String.IsNullOrWhiteSpace(newValue))
				{
					return false;
				}

				value = newValue;
			}

			if (handler == null)
			{
				if (!CommandSystem.Entries.ContainsKey(value))
				{
					return false;
				}

				handler = CommandSystem.Entries[value].Handler;
			}

			if (value != newValue)
			{
				if (String.IsNullOrWhiteSpace(newValue))
				{
					Unregister(value);
					return true;
				}

				value = newValue;
			}

			Unregister(value);
			CommandSystem.Register(value, access, handler);
			return true;
		}
	}
}