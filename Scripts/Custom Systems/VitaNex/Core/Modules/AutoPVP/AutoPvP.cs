#region Header
//   Vorspire    _,-'/-'/  AutoPvP.cs
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
using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public delegate void SeasonChangedHandler(PvPSeason newSeason, PvPSeason oldSeason);

	public static partial class AutoPvP
	{
		public const AccessLevel Access = AccessLevel.Seer;

		public static ScheduleInfo DefaultSeasonSchedule = new ScheduleInfo(
			ScheduleMonths.All, ScheduleDays.Monday, ScheduleTimes.Midnight);

		public static AutoPvPOptions CMOptions { get; private set; }

		public static BinaryDataStore<int, PvPSeason> Seasons { get; private set; }
		public static BinaryDataStore<PlayerMobile, PvPProfile> Profiles { get; private set; }
		public static BinaryDirectoryDataStore<PvPSerial, PvPBattle> Battles { get; private set; }

		public static Type[] BattleTypes { get; set; }
		public static PvPScenario[] Scenarios { get; set; }

		public static Schedule SeasonSchedule { get; set; }

		public static PvPSeason CurrentSeason { get { return EnsureSeason(CMOptions.Advanced.Seasons.CurrentSeason); } }
		public static DateTime NextSeasonTime { get { return SeasonSchedule.NextGlobalTick ?? DateTime.UtcNow; } }

		private static DateTime LastSort { get; set; }
		private static TimeSpan CacheDelay { get; set; }
		private static List<PvPProfile> CachedSort { get; set; }

		public static event Action<PvPSeason> OnSeasonChanged;

		public static int SkippedSeasonTicks = 0;

		public static void ChangeSeason(Schedule schedule)
		{
			if (CMOptions.Advanced.Seasons.SkippedTicks < CMOptions.Advanced.Seasons.SkipTicks)
			{
				++CMOptions.Advanced.Seasons.SkippedTicks;
				return;
			}

			SkippedSeasonTicks = 0;

			PvPSeason old = CurrentSeason;
			EnsureSeason(++CMOptions.Advanced.Seasons.CurrentSeason).Start();

			old.End();
			SeasonChanged(old);
		}

		public static void SeasonChanged(PvPSeason old)
		{
			var profiles = GetSortedProfiles(old);
			var winners = new List<PvPProfile>(CMOptions.Advanced.Seasons.TopListCount);
			var losers = new List<PvPProfile>(CMOptions.Advanced.Seasons.RunnersUpCount);

			for (int idx = 0; idx < profiles.Count; idx++)
			{
				if (idx < CMOptions.Advanced.Seasons.TopListCount)
				{
					winners.Add(profiles[idx]);
				}
				else if (idx < CMOptions.Advanced.Seasons.TopListCount + CMOptions.Advanced.Seasons.RunnersUpCount)
				{
					losers.Add(profiles[idx]);
				}
				else
				{
					break;
				}
			}

			winners.ForEach(profile => IssueWinnerRewards(old, profile));

			losers.ForEach(profile => IssueLoserRewards(old, profile));

			if (OnSeasonChanged != null)
			{
				OnSeasonChanged(old);
			}
		}

		public static void IssueWinnerRewards(this PvPSeason season, PvPProfile profile)
		{
			if (!season.Winners.ContainsKey(profile.Owner))
			{
				season.Winners.Add(profile.Owner, new List<Item>());
			}
			else if (season.Winners[profile.Owner] == null)
			{
				season.Winners[profile.Owner] = new List<Item>();
			}

			int rank = profile.GetRank(season);

			CMOptions.Advanced.Seasons.Rewards.Winner.GiveReward(profile.Owner).ForEach(
				reward =>
				{
					season.Winners[profile.Owner].Add(reward);

					reward.Name = String.Format("{0} (Season {1} - Rank {2})", reward.ResolveName(profile.Owner), season.Number, rank);
				});
		}

		public static void IssueLoserRewards(this PvPSeason season, PvPProfile profile)
		{
			if (!season.Losers.ContainsKey(profile.Owner))
			{
				season.Losers.Add(profile.Owner, new List<Item>());
			}
			else if (season.Losers[profile.Owner] == null)
			{
				season.Losers[profile.Owner] = new List<Item>();
			}

			int rank = profile.GetRank(season);

			CMOptions.Advanced.Seasons.Rewards.Loser.GiveReward(profile.Owner).ForEach(
				reward =>
				{
					season.Losers[profile.Owner].Add(reward);

					reward.Name = String.Format("{0} (Season {1} - Rank {2})", reward.ResolveName(profile.Owner), season.Number, rank);
				});
		}

		public static PvPSeason EnsureSeason(int num, bool replace = false)
		{
			if (!Seasons.ContainsKey(num))
			{
				Seasons.Add(num, new PvPSeason(num));
			}
			else if (replace || Seasons[num] == null)
			{
				Seasons[num] = new PvPSeason(num);
			}

			return Seasons[num];
		}

		public static PvPProfile EnsureProfile(PlayerMobile pm, bool replace = false)
		{
			if (!Profiles.ContainsKey(pm))
			{
				Profiles.Add(pm, new PvPProfile(pm));
			}
			else if (replace || Profiles[pm] == null || Profiles[pm].Deleted)
			{
				Profiles[pm] = new PvPProfile(pm);
			}

			return Profiles[pm];
		}

		public static int GetProfileRank(PlayerMobile pm, PvPProfileRankOrder order, PvPSeason season = null)
		{
			if (pm == null || pm.Deleted || order == PvPProfileRankOrder.None)
			{
				return -1;
			}

			var profiles = GetSortedProfiles(order, season);

			for (int rank = 0; rank < profiles.Count; rank++)
			{
				if (profiles[rank].Owner == pm)
				{
					return rank + 1;
				}
			}

			EnsureProfile(pm);
			return profiles.Count + 1;
		}

		public static List<PvPProfile> GetSortedProfiles(PvPSeason season = null)
		{
			return GetSortedProfiles(CMOptions.Advanced.Profiles.RankingOrder, season);
		}

		public static List<PvPProfile> GetSortedProfiles(List<PvPProfile> profiles, PvPSeason season = null)
		{
			return GetSortedProfiles(CMOptions.Advanced.Profiles.RankingOrder, profiles, season);
		}

		public static List<PvPProfile> GetSortedProfiles(PvPProfileRankOrder order, PvPSeason season = null)
		{
			return GetSortedProfiles(order, null, season);
		}

		public static List<PvPProfile> GetSortedProfiles(
			PvPProfileRankOrder order, List<PvPProfile> profiles, PvPSeason season = null)
		{
			if (profiles == null || profiles.Count == 0)
			{
				profiles = new List<PvPProfile>(Profiles.Values);
			}

			switch (order)
			{
				case PvPProfileRankOrder.Points:
					{
						profiles.Sort(
							(a, b) =>
							{
								if (a == b)
								{
									return 0;
								}

								if (b == null)
								{
									return -1;
								}

								if (a == null)
								{
									return 1;
								}

								if (a.Deleted && b.Deleted)
								{
									return 0;
								}

								if (b.Deleted)
								{
									return -1;
								}

								if (a.Deleted)
								{
									return 1;
								}

								long aTotal;
								long bTotal;

								if (season == null)
								{
									aTotal = a.TotalPointsGained - a.TotalPointsLost;
									bTotal = b.TotalPointsGained - b.TotalPointsLost;
								}
								else
								{
									aTotal = a.History.EnsureEntry(season).PointsGained - a.History.EnsureEntry(season).PointsLost;
									bTotal = b.History.EnsureEntry(season).PointsGained - b.History.EnsureEntry(season).PointsLost;
								}

								if (aTotal > bTotal)
								{
									return -1;
								}

								if (aTotal < bTotal)
								{
									return 1;
								}

								return 0;
							});
					}
					break;

				case PvPProfileRankOrder.Wins:
					{
						profiles.Sort(
							(a, b) =>
							{
								if (a == b)
								{
									return 0;
								}

								if (b == null)
								{
									return -1;
								}

								if (a == null)
								{
									return 1;
								}

								if (a.Deleted && b.Deleted)
								{
									return 0;
								}

								if (b.Deleted)
								{
									return -1;
								}

								if (a.Deleted)
								{
									return 1;
								}

								long aTotal;
								long bTotal;

								if (season == null)
								{
									aTotal = a.TotalWins;
									bTotal = b.TotalWins;
								}
								else
								{
									aTotal = a.History.EnsureEntry(season).Wins;
									bTotal = b.History.EnsureEntry(season).Wins;
								}

								if (aTotal > bTotal)
								{
									return -1;
								}

								if (aTotal < bTotal)
								{
									return 1;
								}

								return 0;
							});
					}
					break;

				case PvPProfileRankOrder.Kills:
					{
						profiles.Sort(
							(a, b) =>
							{
								if (a == b)
								{
									return 0;
								}

								if (b == null)
								{
									return -1;
								}

								if (a == null)
								{
									return 1;
								}

								if (a.Deleted && b.Deleted)
								{
									return 0;
								}

								if (b.Deleted)
								{
									return -1;
								}

								if (a.Deleted)
								{
									return 1;
								}

								long aTotal;
								long bTotal;

								if (season == null)
								{
									aTotal = a.TotalKills;
									bTotal = b.TotalKills;
								}
								else
								{
									aTotal = a.History.EnsureEntry(season).Kills;
									bTotal = b.History.EnsureEntry(season).Kills;
								}

								if (aTotal > bTotal)
								{
									return -1;
								}

								if (aTotal < bTotal)
								{
									return 1;
								}

								return 0;
							});
					}
					break;
			}

			return profiles;
		}

		public static PvPBattle FindBattleByID(PvPSerial serial)
		{
			return Battles.Where(kvp => kvp.Key.Equals(serial)).Select(kvp => kvp.Value).FirstOrDefault();
		}

		public static IPvPBattle FindBattleIByID(PvPSerial serial)
		{
			return Battles.Where(kvp => kvp.Key.Equals(serial)).Select(kvp => kvp.Value).FirstOrDefault();
		}

		public static PvPBattle FindBattle(PlayerMobile pm)
		{
			return FindBattle<PvPBattle>(pm);
		}

		public static TBattle FindBattle<TBattle>(PlayerMobile pm) where TBattle : PvPBattle, IPvPBattle
		{
			TBattle battle;

			if (IsParticipant(pm, out battle) || IsSpectator(pm, out battle))
			{
				return battle;
			}

			return null;
		}

		public static IPvPBattle FindBattleI(PlayerMobile pm)
		{
			return FindBattleI<IPvPBattle>(pm);
		}

		public static TIBattle FindBattleI<TIBattle>(PlayerMobile pm) where TIBattle : IPvPBattle
		{
			TIBattle battle;

			if (IsParticipantI(pm, out battle) || IsSpectatorI(pm, out battle))
			{
				return battle;
			}

			return default(TIBattle);
		}

		public static bool IsSpectator(PlayerMobile pm)
		{
			PvPBattle battle;
			return IsSpectator(pm, out battle);
		}

		public static bool IsSpectator(PlayerMobile pm, out PvPBattle battle)
		{
			return IsSpectator<PvPBattle>(pm, out battle);
		}

		public static bool IsSpectator<TBattle>(PlayerMobile pm, out TBattle battle) where TBattle : PvPBattle
		{
			battle = null;

			return pm != null && GetSpectators<TBattle>().TryGetValue(pm, out battle) && battle != null;
		}

		public static Dictionary<PlayerMobile, PvPBattle> GetSpectators()
		{
			return GetSpectators<PvPBattle>();
		}

		public static void GetSpectators(Dictionary<PlayerMobile, PvPBattle> spectators)
		{
			GetSpectators<PvPBattle>(spectators);
		}

		public static Dictionary<PlayerMobile, TBattle> GetSpectators<TBattle>() where TBattle : PvPBattle
		{
			var spectators = new Dictionary<PlayerMobile, TBattle>();
			GetSpectators(spectators);
			return spectators;
		}

		public static void GetSpectators<TBattle>(Dictionary<PlayerMobile, TBattle> spectators) where TBattle : PvPBattle
		{
			foreach (var battle in Battles.Values.OfType<TBattle>())
			{
				foreach (var pm in battle.Spectators)
				{
					if (!spectators.ContainsKey(pm))
					{
						spectators.Add(pm, battle);
					}
					else
					{
						spectators[pm] = battle;
					}
				}
			}
		}

		public static bool IsSpectatorI<TIBattle>(PlayerMobile pm, out TIBattle battle) where TIBattle : IPvPBattle
		{
			battle = default(TIBattle);

			return pm != null && GetSpectatorsI<TIBattle>().TryGetValue(pm, out battle) && battle != null;
		}

		public static Dictionary<PlayerMobile, IPvPBattle> GetSpectatorsI()
		{
			return GetSpectatorsI<IPvPBattle>();
		}

		public static void GetSpectatorsI(Dictionary<PlayerMobile, IPvPBattle> spectators)
		{
			GetSpectatorsI<IPvPBattle>(spectators);
		}

		public static Dictionary<PlayerMobile, TIBattle> GetSpectatorsI<TIBattle>() where TIBattle : IPvPBattle
		{
			var spectators = new Dictionary<PlayerMobile, TIBattle>();
			GetSpectatorsI(spectators);
			return spectators;
		}

		public static void GetSpectatorsI<TIBattle>(Dictionary<PlayerMobile, TIBattle> spectators) where TIBattle : IPvPBattle
		{
			foreach (var battle in Battles.Values.OfType<TIBattle>())
			{
				foreach (var pm in battle.Spectators)
				{
					if (!spectators.ContainsKey(pm))
					{
						spectators.Add(pm, battle);
					}
					else
					{
						spectators[pm] = battle;
					}
				}
			}
		}

		public static bool IsParticipant(PlayerMobile pm)
		{
			PvPBattle battle;
			return IsParticipant(pm, out battle);
		}

		public static bool IsParticipant(PlayerMobile pm, out PvPBattle battle)
		{
			return IsParticipant<PvPBattle>(pm, out battle);
		}

		public static bool IsParticipant<TBattle>(PlayerMobile pm, out TBattle battle) where TBattle : PvPBattle
		{
			battle = null;

			return pm != null && GetParticipants<TBattle>().TryGetValue(pm, out battle) && battle != null;
		}

		public static Dictionary<PlayerMobile, PvPBattle> GetParticipants()
		{
			return GetParticipants<PvPBattle>();
		}

		public static void GetParticipants(Dictionary<PlayerMobile, PvPBattle> participants)
		{
			GetParticipants<PvPBattle>(participants);
		}

		public static Dictionary<PlayerMobile, TBattle> GetParticipants<TBattle>() where TBattle : PvPBattle
		{
			var participants = new Dictionary<PlayerMobile, TBattle>();
			GetParticipants(participants);
			return participants;
		}

		public static void GetParticipants<TBattle>(Dictionary<PlayerMobile, TBattle> participants) where TBattle : PvPBattle
		{
			foreach (var battle in Battles.Values.OfType<TBattle>())
			{
				foreach (var pm in battle.GetParticipants())
				{
					if (!participants.ContainsKey(pm))
					{
						participants.Add(pm, battle);
					}
					else
					{
						participants[pm] = battle;
					}
				}
			}
		}

		public static bool IsParticipantI(PlayerMobile pm, out IPvPBattle battle)
		{
			return IsParticipantI<IPvPBattle>(pm, out battle);
		}

		public static bool IsParticipantI<TIBattle>(PlayerMobile pm, out TIBattle battle) where TIBattle : IPvPBattle
		{
			battle = default(TIBattle);

			return pm != null && GetParticipantsI<TIBattle>().TryGetValue(pm, out battle) && battle != null;
		}

		public static Dictionary<PlayerMobile, IPvPBattle> GetParticipantsI()
		{
			return GetParticipantsI<IPvPBattle>();
		}

		public static void GetParticipantsI(Dictionary<PlayerMobile, IPvPBattle> participants)
		{
			GetParticipantsI<IPvPBattle>(participants);
		}

		public static Dictionary<PlayerMobile, TIBattle> GetParticipantsI<TIBattle>() where TIBattle : IPvPBattle
		{
			var participants = new Dictionary<PlayerMobile, TIBattle>();
			GetParticipantsI(participants);
			return participants;
		}

		public static void GetParticipantsI<TIBattle>(Dictionary<PlayerMobile, TIBattle> participants)
			where TIBattle : IPvPBattle
		{
			foreach (var battle in Battles.Values.OfType<TIBattle>())
			{
				var b = battle as PvPBattle;

				if (b == null)
				{
					continue;
				}

				foreach (var pm in b.GetParticipants())
				{
					if (!participants.ContainsKey(pm))
					{
						participants.Add(pm, battle);
					}
					else
					{
						participants[pm] = battle;
					}
				}
			}
		}

		public static List<PvPBattle> GetBattles(params PvPBattleState[] states)
		{
			return GetBattles<PvPBattle>(states);
		}

		public static List<TBattle> GetBattles<TBattle>(params PvPBattleState[] states) where TBattle : PvPBattle
		{
			if (states == null || states.Length == 0)
			{
				return Battles.Values.OfType<TBattle>().ToList();
			}

			return Battles.Values.OfType<TBattle>().Where(battle => states.Contains(battle.State)).ToList();
		}

		public static List<TIBattle> GetBattlesI<TIBattle>(params PvPBattleState[] states) where TIBattle : IPvPBattle
		{
			if (states == null || states.Length == 0)
			{
				return Battles.Values.OfType<TIBattle>().ToList();
			}

			return Battles.Values.OfType<TIBattle>().Where(battle => states.Contains(battle.State)).ToList();
		}

		public static void DeleteAllBattles()
		{
			Battles.Values.Where(b => b != null && !b.Deleted).ForEach(b => b.Delete());
		}

		public static void InternalizeAllBattles()
		{
			Battles.Values.Where(b => b != null && !b.Deleted).ForEach(b => b.State = PvPBattleState.Internal);
		}

		public static PvPBattle CreateBattle(PvPScenario scenario)
		{
			if (scenario == null)
			{
				return null;
			}

			PvPBattle battle = scenario.CreateBattle();
			Battles.Add(battle.Serial, battle);
			battle.Init();

			return battle;
		}

		public static bool RemoveBattle(PvPBattle battle)
		{
			return battle != null && !battle.Deleted && Battles.ContainsKey(battle.Serial) && Battles.Remove(battle.Serial);
		}

		public static void RemoveProfile(PvPProfile profile)
		{
			if (!Profiles.ContainsKey(profile.Owner))
			{
				return;
			}

			Profiles.Remove(profile.Owner);
			profile.OnRemoved();
		}
	}
}