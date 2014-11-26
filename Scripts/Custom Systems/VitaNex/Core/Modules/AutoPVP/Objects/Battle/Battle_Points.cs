#region Header
//   Vorspire    _,-'/-'/  Battle_Points.cs
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
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract partial class PvPBattle
	{
		[CommandProperty(AutoPvP.Access)]
		public virtual int KillPoints { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int PointsBase { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual double PointsRankFactor { get; set; }

		public virtual int GetAwardPoints(PvPTeam team, PlayerMobile pm)
		{
			return (int)Math.Floor(PointsBase + (PointsRankFactor * (Teams.Count - GetTeamRank(team))));
		}

		public virtual void AwardPoints(PlayerMobile pm)
		{
			PvPTeam team;

			if (IsParticipant(pm, out team))
			{
				AwardPoints(pm, GetAwardPoints(team, pm));
			}
		}

		public virtual void AwardPoints(PlayerMobile pm, int points)
		{
			if (!IsParticipant(pm))
			{
				return;
			}

			EnsureStatistics(pm).PointsGained += points;
			AutoPvP.EnsureProfile(pm).Points += points;
		}

		public virtual void RevokePoints(PlayerMobile pm)
		{
			PvPTeam team;

			if (IsParticipant(pm, out team))
			{
				RevokePoints(pm, GetAwardPoints(team, pm));
			}
		}

		public virtual void RevokePoints(PlayerMobile pm, int points)
		{
			if (!IsParticipant(pm))
			{
				return;
			}

			EnsureStatistics(pm).PointsLost += points;
			AutoPvP.EnsureProfile(pm).Points -= points;
		}

		public virtual void AwardTeamPoints(PvPTeam team)
		{
			if (team != null)
			{
				team.ForEachMember(
					pm =>
					{
						int points = GetAwardPoints(team, pm);

						EnsureStatistics(pm).PointsGained += points;
						AutoPvP.EnsureProfile(pm).Points += points;
					});
			}
		}

		public virtual void AwardTeamPoints(PvPTeam team, int points)
		{
			if (team != null)
			{
				team.ForEachMember(
					pm =>
					{
						EnsureStatistics(pm).PointsGained += points;
						AutoPvP.EnsureProfile(pm).Points += points;
					});
			}
		}

		public virtual void RevokeTeamPoints(PvPTeam team)
		{
			if (team != null)
			{
				team.ForEachMember(
					pm =>
					{
						int points = GetAwardPoints(team, pm);
						EnsureStatistics(pm).PointsLost += points;
						AutoPvP.EnsureProfile(pm).Points -= points;
					});
			}
		}

		public virtual void RevokeTeamPoints(PvPTeam team, int points)
		{
			if (team != null)
			{
				team.ForEachMember(
					pm =>
					{
						EnsureStatistics(pm).PointsLost += points;
						AutoPvP.EnsureProfile(pm).Points -= points;
					});
			}
		}
	}
}