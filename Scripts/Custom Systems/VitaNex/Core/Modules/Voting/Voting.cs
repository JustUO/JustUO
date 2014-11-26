#region Header
//   Vorspire    _,-'/-'/  Voting.cs
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

namespace VitaNex.Modules.Voting
{
	public static partial class Voting
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static Type[] SiteTypes { get; private set; }

		public static VotingOptions CMOptions { get; private set; }

		public static BinaryDataStore<int, IVoteSite> VoteSites { get; private set; }
		public static BinaryDataStore<PlayerMobile, VoteProfile> Profiles { get; private set; }

		public static event Action<VoteRequestEventArgs> OnVoteRequest;

		public static List<VoteProfile> GetSortedProfiles(bool today = false)
		{
			var list = Profiles.Values.ToList();

			if (today)
			{
				list.Sort(InternalProfileSortToday);
			}
			else
			{
				list.Sort(InternalProfileSort);
			}

			return list;
		}

		private static int InternalProfileSort(VoteProfile a, VoteProfile b)
		{
			if (a == b)
			{
				return 0;
			}

			if (a == null)
			{
				return 1;
			}

			if (b == null)
			{
				return -1;
			}

			if (a.Deleted && b.Deleted)
			{
				return 0;
			}

			if (a.Deleted)
			{
				return 1;
			}

			if (b.Deleted)
			{
				return -1;
			}

			int aTotal = a.GetTokenTotal();
			int bTotal = b.GetTokenTotal();

			if (aTotal > bTotal)
			{
				return -1;
			}

			if (aTotal < bTotal)
			{
				return 1;
			}

			return 0;
		}

		private static int InternalProfileSortToday(VoteProfile a, VoteProfile b)
		{
			if (a == b)
			{
				return 0;
			}

			if (a == null)
			{
				return 1;
			}

			if (b == null)
			{
				return -1;
			}

			if (a.Deleted && b.Deleted)
			{
				return 0;
			}

			if (a.Deleted)
			{
				return 1;
			}

			if (b.Deleted)
			{
				return -1;
			}

			DateTime when = DateTime.UtcNow;

			int aTotal = a.GetTokenTotal(when);
			int bTotal = b.GetTokenTotal(when);

			if (aTotal > bTotal)
			{
				return -1;
			}

			if (aTotal < bTotal)
			{
				return 1;
			}

			return 0;
		}

		private static void InternalSiteSort()
		{
			if (VoteSites.Count < 2)
			{
				return;
			}

			var list = VoteSites.Values.ToList();
			VoteSites.Clear();
			list.Sort(InternalSiteSort);
			list.ForEach(s => VoteSites.Add(s.UID, s));
			list.Clear();
		}

		private static int InternalSiteSort(IVoteSite a, IVoteSite b)
		{
			if (a == b)
			{
				return 0;
			}

			if (a == null)
			{
				return 1;
			}

			if (b == null)
			{
				return -1;
			}

			if (!a.Valid && !b.Valid)
			{
				return 0;
			}

			if (!a.Valid)
			{
				return 1;
			}

			if (!b.Valid)
			{
				return -1;
			}

			if (a.Interval > b.Interval)
			{
				return 1;
			}

			if (a.Interval < b.Interval)
			{
				return -1;
			}

			return 0;
		}

		public static void InvalidateSites()
		{
			InternalSiteSort();
		}

		public static void Invoke(this VoteRequestEventArgs e)
		{
			if (OnVoteRequest != null && e != null)
			{
				OnVoteRequest(e);
			}
		}

		public static IVoteSite FindSite(int uid)
		{
			IVoteSite site;
			VoteSites.TryGetValue(uid, out site);
			return site;
		}

		public static void Remove(this IVoteSite site)
		{
			VoteSites.Remove(site.UID);

			site.Delete();

			Profiles.Values.ForEach(
				p => p.History.ForEach((t, h) => h.Where(e => e.VoteSite == site).ForEach(e => e.VoteSite = VoteSite.Empty)));
		}

		public static VoteProfile EnsureProfile(PlayerMobile m, bool replace = false)
		{
			if (!Profiles.ContainsKey(m))
			{
				Profiles.Add(m, new VoteProfile(m));
			}
			else if (replace || Profiles[m] == null || Profiles[m].Deleted)
			{
				Profiles[m] = new VoteProfile(m);
			}

			return Profiles[m];
		}

		public static void Remove(this VoteProfile profile)
		{
			Profiles.Remove(profile.Owner);
		}
	}
}