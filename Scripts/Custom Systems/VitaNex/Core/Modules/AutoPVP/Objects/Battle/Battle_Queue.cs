#region Header
//   Vorspire    _,-'/-'/  Battle_Queue.cs
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
using Server.Engines.PartySystem;
using Server.Mobiles;
using Server.Regions;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract partial class PvPBattle
	{
		public Dictionary<PlayerMobile, PvPTeam> Queue { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool QueueAllowed { get; set; }

		public virtual Dictionary<PlayerMobile, PvPTeam> GetQueued()
		{
			return new Dictionary<PlayerMobile, PvPTeam>(Queue);
		}

		public virtual IEnumerable<PlayerMobile> GetQueued(PvPTeam team)
		{
			if (team == null || team.Deleted)
			{
				yield break;
			}

			foreach (var kvp in Queue.Where(kvp => kvp.Value == team))
			{
				yield return kvp.Key;
			}
		}

		public virtual void EnqueueParty(PlayerMobile pm, Party party, PvPTeam team = null)
		{
			if (pm != null && !pm.Deleted && party != null && party.Active && party.Leader == pm)
			{
				party.Members.Where(pmi => pmi != null && pmi.Mobile != null && !pmi.Mobile.Deleted && pmi.Mobile != pm)
					 .Select(pmi => (PlayerMobile)pmi.Mobile)
					 .ForEach(m => Enqueue(m, team, false));
			}
		}

		public virtual void Enqueue(PlayerMobile pm, PvPTeam team = null, bool party = true)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			if (!CanQueue(pm))
			{
				OnQueueReject(pm);
				return;
			}

			if (team != null && team.Deleted)
			{
				team = null;
			}

			if (!IsQueued(pm))
			{
				Queue.Add(pm, team);
				OnQueueJoin(pm, team);
			}
			else
			{
				Queue[pm] = team;
				OnQueueUpdate(pm, team);
			}

			if (party)
			{
				EnqueueParty(pm, Party.Get(pm), team);
			}
		}

		public virtual void Dequeue(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted && Queue.Remove(pm))
			{
				OnQueueLeave(pm);
			}
		}

		public virtual bool CanQueue(PlayerMobile pm)
		{
			return QueueAllowed && State != PvPBattleState.Internal && pm != null && !pm.Deleted && pm.Alive &&
				   (pm.Region == null || !pm.Region.IsPartOf<Jail>()) && IsOnline(pm) && !InCombat(pm) && !IsQueued(pm) &&
				   !InOtherBattle(pm);
		}

		public virtual bool IsQueued(PlayerMobile pm)
		{
			return pm != null && Queue.ContainsKey(pm);
		}

		protected virtual void OnQueueJoin(PlayerMobile pm, PvPTeam team)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			pm.SendMessage(
				"You have joined the queue for {0}{1}", Name, team != null ? ", your team is " + team.Name : String.Empty);
			SendSound(pm, Options.Sounds.QueueJoin);
		}

		protected virtual void OnQueueUpdate(PlayerMobile pm, PvPTeam team)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			pm.SendMessage(
				"Your queue status for {0} has changed{1}", Name, team != null ? ", your team is " + team.Name : String.Empty);
			SendSound(pm, Options.Sounds.QueueJoin);
		}

		protected virtual void OnQueueLeave(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted || IsParticipant(pm))
			{
				return;
			}

			pm.SendMessage("You have left the queue for {0}", Name);
			SendSound(pm, Options.Sounds.QueueLeave);
		}

		protected virtual void OnQueueReject(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				pm.SendMessage(
					IsQueued(pm) ? "You are already in the queue for {0}" : "You can not queue for {0} at this time.", Name);
			}
		}
	}
}