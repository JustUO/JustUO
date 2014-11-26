#region Header
//   Vorspire    _,-'/-'/  Battle_Teams.cs
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
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract partial class PvPBattle
	{
		public List<PvPTeam> Teams { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool AutoAssign { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool UseTeamColors { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool IgnoreCapacity { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int MinCapacity { get { return GetMinCapacity(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int MaxCapacity { get { return GetMaxCapacity(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int CurrentCapacity { get { return GetCurrentCapacity(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool IsFull { get { return (CurrentCapacity >= MaxCapacity); } }

		public int GetMinCapacity()
		{
			return Teams.Sum(team => team.MinCapacity);
		}

		public int GetMaxCapacity()
		{
			return Teams.Sum(team => team.MaxCapacity);
		}

		public int GetCurrentCapacity()
		{
			return Teams.Sum(team => team.Count);
		}

		public virtual bool AddTeam(string name, int capacity, int color)
		{
			return AddTeam(name, 0, capacity, color);
		}

		public virtual bool AddTeam(string name, int minCapacity, int capacity, int color)
		{
			return AddTeam(new PvPTeam(this, name, minCapacity, capacity, color));
		}

		public virtual bool AddTeam(PvPTeam team)
		{
			if (!ContainsTeam(team))
			{
				Teams.Add(team);
				OnTeamAdded(team);
				return true;
			}

			return false;
		}

		public bool RemoveTeam(PvPTeam team)
		{
			if (ContainsTeam(team) && Teams.Remove(team))
			{
				OnTeamRemoved(team);
				return true;
			}

			return false;
		}

		public bool ContainsTeam(PvPTeam team)
		{
			return Teams.Contains(team);
		}

		public void ResetTeam(PvPTeam team)
		{
			if (team != null && !team.Deleted)
			{
				team.Reset();
			}
		}

		public virtual bool CanDamageOwnTeam(PlayerMobile damager, PlayerMobile target)
		{
			return Options.Rules.CanDamageOwnTeam && State == PvPBattleState.Running;
		}

		public virtual bool CanDamageEnemyTeam(PlayerMobile damager, PlayerMobile target)
		{
			return Options.Rules.CanDamageEnemyTeam && State == PvPBattleState.Running;
		}

		public virtual bool CanHealOwnTeam(PlayerMobile healer, PlayerMobile target)
		{
			return Options.Rules.CanHealOwnTeam && (State == PvPBattleState.Preparing || State == PvPBattleState.Running);
		}

		public virtual bool CanHealEnemyTeam(PlayerMobile healer, PlayerMobile target)
		{
			return Options.Rules.CanHealEnemyTeam && (State == PvPBattleState.Preparing || State == PvPBattleState.Running);
		}

		public virtual IEnumerable<PvPTeam> GetWinningTeams()
		{
			return GetTeamsRanked().Where(team => team != null && !team.Deleted && IsWinningTeam(team));
		}

		public virtual IEnumerable<PvPTeam> GetLosingTeams()
		{
			return GetTeamsRanked().Where(team => team != null && !team.Deleted && !IsWinningTeam(team));
		}

		public virtual IEnumerable<PvPTeam> GetTeamsRanked()
		{
			var teams = new List<PvPTeam>(Teams);

			teams.Sort(CompareTeamRank);

			return teams;
		}

		public virtual int CompareTeamRank(PvPTeam a, PvPTeam b)
		{
			int result = 0;

			if (a.CompareNull(b, ref result))
			{
				return result;
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

			int retVal = a.Statistics.TotalKills.CompareTo(b.Statistics.TotalKills);

			return retVal < 0 ? 1 : (retVal > 0 ? -1 : retVal);
		}

		public virtual IEnumerable<PvPTeam> GetAliveTeams()
		{
			return Teams.Where(team => team.Count > 0 && (team.RespawnOnDeath || team.Dead.Count < team.Count));
		}

		public virtual bool IsWinningTeam(PvPTeam team)
		{
			return GetTeamRank(team) == 0;
		}

		public virtual int GetTeamRank(PvPTeam team)
		{
			return GetTeamsRanked().IndexOf(team);
		}

		public virtual PvPTeam GetMostEmptyTeam()
		{
			return Teams.Where(t => t != null && !t.Deleted).OrderBy(t => t.Count).FirstOrDefault();
		}

		public virtual PvPTeam GetRandomTeam()
		{
			return Teams.GetRandom();
		}

		public virtual PvPTeam GetAutoAssignTeam(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return null;
			}

			PvPTeam team;

			if (IsParticipant(pm, out team) && team != null && !team.Deleted)
			{
				return team;
			}

			return Teams.OrderBy(GetAssignPriority).FirstOrDefault();
		}

		public virtual double GetAssignPriority(PvPTeam team)
		{
			double weight;

			CalculateAssignPriority(team, out weight);

			return weight;
		}

		protected virtual void CalculateAssignPriority(PvPTeam team, out double weight)
		{
			if (team == null || team.Deleted)
			{
				weight = Double.MaxValue;
				return;
			}

			if (team.Count <= 0)
			{
				weight = Double.MinValue;
				return;
			}

			weight = team.Sum(
				m =>
				{
					unchecked
					{
						return (double)(m.SkillsTotal + m.Str + m.Dex + m.Int + m.HitsMax + m.StamMax + m.ManaMax);
					}
				});
		}

		public PvPTeam FindTeam(PlayerMobile pm)
		{
			return FindTeam<PvPTeam>(pm);
		}

		public TTeam FindTeam<TTeam>(PlayerMobile pm) where TTeam : PvPTeam
		{
			return Teams.OfType<TTeam>().FirstOrDefault(t => t.IsMember(pm));
		}

		public virtual void TeamRespawn(PvPTeam team)
		{
			if (team != null && !team.Deleted)
			{
				team.ForEachMember(team.Respawn);
			}
		}

		public virtual void TeamWinEject(PvPTeam team)
		{
			if (team == null || team.Deleted)
			{
				return;
			}

			OnTeamWin(team);
			team.ForEachMember(pm => team.RemoveMember(pm, true));
		}

		public virtual void TeamLoseEject(PvPTeam team)
		{
			if (team == null || team.Deleted)
			{
				return;
			}

			OnTeamLose(team);
			team.ForEachMember(member => team.RemoveMember(member, true));
		}

		public virtual void OnTeamInit(PvPTeam team)
		{ }

		public virtual void OnTeamSync(PvPTeam team)
		{ }

		public virtual void OnTeamMicroSync(PvPTeam team)
		{ }

		public virtual void OnTeamAdded(PvPTeam team)
		{ }

		public virtual void OnTeamRemoved(PvPTeam team)
		{ }

		public virtual void OnTeamFrozen(PvPTeam team)
		{ }

		public virtual void OnTeamUnfrozen(PvPTeam team)
		{ }

		public virtual void OnTeamMemberFrozen(PvPTeam team, PlayerMobile pm)
		{ }

		public virtual void OnTeamMemberUnfrozen(PvPTeam team, PlayerMobile pm)
		{ }

		public virtual void OnTeamMemberDeath(PvPTeam team, PlayerMobile pm)
		{
			LocalBroadcast("{0} has died.", pm.RawName);

			EnsureStatistics(pm).Deaths++;

			if (KillPoints > 0)
			{
				RevokePoints(pm, KillPoints);
			}

			PlayerMobile pk = pm.FindMostRecentDamager(false) as PlayerMobile;

			if (pk != null && !pk.Deleted && IsParticipant(pk))
			{
				pm.LastKiller = pk;
				EnsureStatistics(pk).Kills++;

				if (KillPoints > 0 && pk.IsOnline() && pm.IsOnline() && pk.NetState.Address != pm.NetState.Address)
				{
					AwardPoints(pk, KillPoints);
				}
			}

			TeleportToHomeBase(team, pm);
		}

		public virtual void OnAfterTeamMemberDeath(PvPTeam team, PlayerMobile pm)
		{
			RefreshStats(pm, true, true);

			if (team.KickOnDeath)
			{
				OnLose(pm);
				team.RemoveMember(pm, true);
				return;
			}

			if (team.RespawnOnDeath)
			{
				Timer.DelayCall(team.RespawnDelay, () => team.Respawn(pm));
			}
		}

		public virtual void OnTeamMemberResurrected(PvPTeam team, PlayerMobile pm)
		{
			EnsureStatistics(pm).Resurrections++;
		}

		public virtual void OnAfterTeamMemberResurrected(PvPTeam team, PlayerMobile pm)
		{
			team.Dead.Remove(pm);
		}

		public virtual void OnTeamMemberAdded(PvPTeam team, PlayerMobile pm)
		{
			team.Broadcast("{0} has joined the battle.", pm.RawName);

			if (UseTeamColors)
			{
				pm.SolidHueOverride = team.Color;
			}
		}

		public virtual void OnTeamMemberRemoved(PvPTeam team, PlayerMobile pm)
		{
			team.Broadcast("{0} has left the battle.", pm.RawName);
			pm.SolidHueOverride = -1;
		}

		public virtual void OnTeamWin(PvPTeam team)
		{
			team.ForEachMember(RefreshStats);
			team.ForEachMember(OnWin);
		}

		public virtual void OnTeamLose(PvPTeam team)
		{
			team.ForEachMember(RefreshStats);
			team.ForEachMember(OnLose);
		}
	}
}