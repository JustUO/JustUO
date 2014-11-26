#region Header
//   Vorspire    _,-'/-'/  AccountExt.cs
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

using Server.Accounting;
#endregion

namespace Server
{
	public static class AccountExtUtility
	{
		public static Mobile[] GetMobiles(this IAccount acc)
		{
			return GetMobiles(acc, null);
		}

		public static Mobile[] GetMobiles(this IAccount acc, Func<Mobile, bool> predicate)
		{
			return FindMobiles(acc, predicate).ToArray();
		}

		public static IEnumerable<Mobile> FindMobiles(this IAccount acc)
		{
			return FindMobiles(acc, null);
		}

		public static IEnumerable<Mobile> FindMobiles(this IAccount acc, Func<Mobile, bool> predicate)
		{
			if (acc == null)
			{
				yield break;
			}

			for (int i = 0; i < acc.Length; i++)
			{
				if (acc[i] != null && (predicate == null || predicate(acc[i])))
				{
					yield return acc[i];
				}
			}
		}

		public static TMob[] GetMobiles<TMob>(this IAccount acc) where TMob : Mobile
		{
			return GetMobiles<TMob>(acc, null);
		}

		public static TMob[] GetMobiles<TMob>(this IAccount acc, Func<TMob, bool> predicate) where TMob : Mobile
		{
			return FindMobiles(acc, predicate).ToArray();
		}

		public static IEnumerable<TMob> FindMobiles<TMob>(this IAccount acc) where TMob : Mobile
		{
			return FindMobiles<TMob>(acc, null);
		}

		public static IEnumerable<TMob> FindMobiles<TMob>(this IAccount acc, Func<TMob, bool> predicate) where TMob : Mobile
		{
			if (acc == null)
			{
				yield break;
			}

			for (int i = 0; i < acc.Length; i++)
			{
				if (acc[i] is TMob && (predicate == null || predicate((TMob)acc[i])))
				{
					yield return (TMob)acc[i];
				}
			}
		}

		public static Account[] GetSharedAccounts(this IAccount acc)
		{
			return GetSharedAccounts(acc as Account);
		}

		public static Account[] GetSharedAccounts(this Account acc)
		{
			return FindSharedAccounts(acc).ToArray();
		}

		public static IEnumerable<Account> FindSharedAccounts(this IAccount acc)
		{
			return FindSharedAccounts(acc as Account);
		}

		public static IEnumerable<Account> FindSharedAccounts(this Account acc)
		{
			if (acc == null)
			{
				yield break;
			}

			foreach (Account a in
				Accounts.GetAccounts().AsParallel().OfType<Account>().Where(a => IsSharedWith(acc, a)))
			{
				yield return a;
			}
		}

		public static bool IsSharedWith(this IAccount acc, IAccount a)
		{
			return IsSharedWith(acc as Account, a as Account);
		}

		public static bool IsSharedWith(this Account acc, Account a)
		{
			return acc != null && a != null && acc != a && acc.LoginIPs.Any(a.LoginIPs.Contains);
		}
	}
}