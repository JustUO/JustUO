#region Header
//   Vorspire    _,-'/-'/  PlayerNames.cs
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

using Server;
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex
{
	public static partial class PlayerNames
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static BinaryDataStore<string, List<PlayerMobile>> Registry { get; private set; }

		public static event Action<PlayerMobile> OnRegistered;

		private static void InvokeRegistered(PlayerMobile pm)
		{
			if (OnRegistered != null)
			{
				OnRegistered(pm);
			}
		}

		public static bool IsRegistered(PlayerMobile pm)
		{
			return pm != null && !String.IsNullOrWhiteSpace(FindRegisteredName(pm));
		}

		public static void Register(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			string rName = FindRegisteredName(pm);

			if (!String.IsNullOrWhiteSpace(rName) && rName != pm.RawName)
			{
				if (String.IsNullOrWhiteSpace(pm.RawName))
				{
					pm.RawName = rName;
					return;
				}

				Registry[rName].Remove(pm);

				if (Registry[rName].Count == 0)
				{
					Registry.Remove(rName);
				}
			}

			if (!Registry.ContainsKey(pm.RawName))
			{
				Registry.Add(
					pm.RawName,
					new List<PlayerMobile>(1)
					{
						pm
					});
				InvokeRegistered(pm);
			}
			else if (!Registry[pm.RawName].Contains(pm))
			{
				Registry[pm.RawName].Add(pm);
				InvokeRegistered(pm);
			}
		}

		public static bool HasNameChanged(PlayerMobile pm)
		{
			return pm != null && !pm.Deleted && pm.RawName != FindRegisteredName(pm);
		}

		public static string FindRegisteredName(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return String.Empty;
			}

			foreach (var kvp in Registry.Where(kvp => kvp.Value.Any(rpm => rpm == pm)))
			{
				return kvp.Key;
			}

			return String.Empty;
		}

		public static PlayerMobile[] FindPlayers(string name)
		{
			return !String.IsNullOrWhiteSpace(name) && Registry.ContainsKey(name) && Registry[name] != null
					   ? Registry[name].ToArray()
					   : new PlayerMobile[0];
		}
	}
}