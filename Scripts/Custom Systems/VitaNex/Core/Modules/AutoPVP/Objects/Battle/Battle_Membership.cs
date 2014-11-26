#region Header
//   Vorspire    _,-'/-'/  Battle_Membership.cs
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
using Server.Regions;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public abstract partial class PvPBattle
	{
		[CommandProperty(AutoPvP.Access)]
		public virtual TimeSpan LogoutDelay { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual TimeSpan IdleThreshold { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool IdleKick { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool InviteWhileRunning { get; set; }

		public virtual bool InOtherBattle(PlayerMobile pm)
		{
			PvPBattle battle = AutoPvP.FindBattle(pm);

			return battle != null && battle != this && battle.IsParticipant(pm);
		}

		public bool IsParticipant(PlayerMobile pm)
		{
			PvPTeam team;
			return IsParticipant(pm, out team);
		}

		public bool IsParticipant(PlayerMobile pm, out PvPTeam team)
		{
			team = FindTeam(pm);
			return (team != null);
		}

		public virtual TimeSpan GetLogoutDelay(Mobile m)
		{
			return LogoutDelay;
		}

		public IEnumerable<PlayerMobile> GetParticipants()
		{
			return
				Teams.Where(team => team != null && !team.Deleted)
					 .SelectMany(team => team.Where(member => member != null && !member.Deleted));
		}

		public virtual bool CanSendInvites()
		{
			return !Hidden && (State == PvPBattleState.Preparing || (State == PvPBattleState.Running && InviteWhileRunning)) &&
				   CurrentCapacity < MaxCapacity;
		}

		public virtual bool CanSendInvite(PlayerMobile pm)
		{
			return pm != null && !pm.Deleted && pm.Alive && !pm.Region.IsPartOf<Jail>() && pm.DesignContext == null &&
				   IsOnline(pm) && !InCombat(pm) && IsQueued(pm) && !IsParticipant(pm) && !InOtherBattle(pm);
		}

		public virtual void SendInvites()
		{
			foreach (PlayerMobile pm in Queue.Keys)
			{
				var invites = SuperGump.GetInstances<PvPInviteGump>(pm);

				if (CanSendInvite(pm))
				{
					bool sendNew = invites.All(invite => !invite.IsOpen || invite.Battle != this);

					if (sendNew)
					{
						OnSendInvite(pm, new PvPInviteGump(pm, this));
					}
				}
				else
				{
					invites.Where(invite => invite.IsOpen && invite.Battle == this).ForEach(i => i.Close());
				}
			}
		}

		protected virtual void OnSendInvite(PlayerMobile pm, PvPInviteGump invite)
		{
			if (pm == null || pm.Deleted || !pm.Alive || invite == null || invite.IsDisposed)
			{
				return;
			}

			invite.Send();
			SendSound(pm, Options.Sounds.InviteSend);
		}

		public void AcceptInvite(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted || !pm.Alive)
			{
				return;
			}

			if (CanSendInvites() && CanSendInvite(pm))
			{
				OnInviteAccept(pm);
			}
			else
			{
				OnInviteRejected(pm);
			}
		}

		public void DeclineInvite(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				OnInviteDecline(pm);
			}
		}

		protected virtual void OnInviteAccept(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted || !IsOnline(pm))
			{
				return;
			}

			if (!IsQueued(pm))
			{
				pm.SendMessage("You are not queued for {0}", Name);
				return;
			}

			if (IsParticipant(pm))
			{
				pm.SendMessage("You are already participating in {0}", Name);
				return;
			}

			if (InOtherBattle(pm))
			{
				pm.SendMessage("You cannot join {0} while you are in another battle.", Name);
				return;
			}

			if (IsFull)
			{
				pm.SendMessage("The battle is full, you will be sent an invite if someone leaves.");
				return;
			}

			PvPTeam team = Queue[pm];

			if (team == null || team.Deleted) // Assume AutoAssign is true
			{
				if (Teams.Count == 1)
				{
					team = Teams[0];
				}
				else if (AutoAssign) // Make sure AutoAssign is true
				{
					team = GetAutoAssignTeam(pm);
				}
			}

			if (team == null || team.Deleted) // Fallback to most empty, or random team
			{
				team = GetMostEmptyTeam();

				if (team == null || team.Deleted)
				{
					team = GetRandomTeam();
				}
			}

			if (team == null || team.Deleted)
			{
				pm.SendMessage("The team you've chosen seems to have vanished in the void, sorry about that.");
				Queue.Remove(pm);
				return;
			}

			if (team.IsFull)
			{
				pm.SendMessage("The team you've chosen is full, you will be sent an invite if someone leaves.");
				return;
			}

			Queue.Remove(pm);
			team.AddMember(pm, true);
			SendSound(pm, Options.Sounds.InviteAccept);
		}

		protected virtual void OnInviteDecline(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			pm.SendMessage("You decide not to join {0}", Name);
			SendSound(pm, Options.Sounds.InviteCancel);

			if (IsQueued(pm))
			{
				Dequeue(pm);
			}
		}

		protected virtual void OnInviteRejected(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			pm.SendMessage("You can not join {0} at this time.", Name);
			SendSound(pm, Options.Sounds.InviteCancel);
		}

		public void Eject(Mobile m, bool teleport)
		{
			if (m == null || m.Deleted)
			{
				return;
			}

			if (m is PlayerMobile)
			{
				Eject((PlayerMobile)m, teleport);
			}
			else if (m is BaseCreature)
			{
				Eject((BaseCreature)m, teleport);
			}
		}

		public void Eject(PlayerMobile pm, bool teleport)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			PvPTeam team;

			if (IsParticipant(pm, out team))
			{
				if (State == PvPBattleState.Running || State == PvPBattleState.Ended)
				{
					EnsureStatistics(pm).Battles = 1;

					if (State == PvPBattleState.Running)
					{
						EnsureStatistics(pm).Losses = 1;

						int points = GetAwardPoints(team, pm);

						EnsureStatistics(pm).PointsLost += points;
						AutoPvP.EnsureProfile(pm).Points -= points;
					}
				}

				team.RemoveMember(pm, false);
			}
			else if (IsSpectator(pm))
			{
				RemoveSpectator(pm, false);
			}

			if (teleport)
			{
				Teleport(pm, Options.Locations.Eject, Options.Locations.Eject.Map);
			}
		}

		public void Eject(BaseCreature bc, bool teleportOrStable)
		{
			if (bc == null || bc.Deleted)
			{
				return;
			}

			if (!teleportOrStable)
			{
				bc.Delete();
				return;
			}

			if (bc.ControlMaster is PlayerMobile)
			{
				bc.Stable();
			}
			else
			{
				Teleport(bc, Options.Locations.Eject, Options.Locations.Eject.Map);
			}
		}

		protected virtual void OnEjected(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return;
			}

			if (m is PlayerMobile)
			{
				OnEjected((PlayerMobile)m);
			}
			else if (m is BaseCreature)
			{
				OnEjected((BaseCreature)m);
			}
		}

		protected virtual void OnEjected(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				pm.SendMessage("You have been ejected from the battle.");
			}
		}

		protected virtual void OnEjected(BaseCreature bc)
		{ }

		public virtual void InvalidateStray(Mobile m)
		{
			if (m == null || m.Deleted || State == PvPBattleState.Internal || Hidden)
			{
				return;
			}

			if (m is PlayerMobile)
			{
				InvalidateStrayPlayer((PlayerMobile)m);
			}
			else if (m is BaseCreature)
			{
				BaseCreature bc = (BaseCreature)m;

				if (bc.ControlMaster != null)
				{
					InvalidateStrayPet(bc);
				}
				else
				{
					InvalidateStraySpawn(bc);
				}
			}
		}

		public virtual void InvalidateStrayPlayer(PlayerMobile player)
		{
			if (player == null || player.Deleted || player.Region == null || State == PvPBattleState.Internal || Hidden)
			{
				return;
			}

			if (player.Region.IsPartOf(SpectateRegion))
			{
				if (IsParticipant(player))
				{
					Eject(player, false);
				}

				if (!IsSpectator(player))
				{
					AddSpectator(player, false);
				}
			}
			else if (player.Region.IsPartOf(BattleRegion))
			{
				if (IsParticipant(player))
				{
					if (IsSpectator(player))
					{
						RemoveSpectator(player, false);
					}

					if (IsQueued(player))
					{
						Queue.Remove(player);
					}

					if (player.Mounted && (DebugMode || player.AccessLevel < AccessLevel.Counselor))
					{
						if ((player.Mount is EtherealMount && !Options.Rules.CanMountEthereal) ||
							(player.Mount is BaseMount && !Options.Rules.CanMount))
						{
							player.SetMountBlock(BlockMountType.None, TimeSpan.FromSeconds(3), true);
						}
					}
				}
				else if (!IsSpectator(player))
				{
					if ((DebugMode || player.AccessLevel < AccessLevel.Counselor) && SpectateAllowed)
					{
						AddSpectator(player, true);
					}
					else if (player.AccessLevel < AccessLevel.Counselor)
					{
						Eject(player, true);
					}
				}
			}
		}

		public virtual void InvalidateStraySpawn(BaseCreature creature)
		{
			if (creature == null || creature.Deleted || creature.Region == null || State == PvPBattleState.Internal || Hidden)
			{
				return;
			}

			if (!AllowSpawn())
			{
				Eject(creature, creature.ControlMaster != null);
			}
		}

		public virtual void InvalidateStrayPet(BaseCreature pet)
		{
			if (pet == null || pet.Deleted || pet.Region == null || State == PvPBattleState.Internal || Hidden)
			{
				return;
			}

			if (!Options.Rules.AllowPets && (DebugMode || pet.ControlMaster.AccessLevel < AccessLevel.Counselor))
			{
				Eject(pet, pet.ControlMaster != null);
			}
		}

		public virtual void InvalidateGates()
		{
			InvalidateSpectateGate();
			InvalidateTeamGates();
		}

		public virtual void InvalidateSpectateGate()
		{
			if (!SpectateAllowed || SpectateRegion == null || State == PvPBattleState.Internal ||
				Options.Locations.SpectateGate.Internal || Options.Locations.SpectateGate.Zero)
			{
				if (Gate != null)
				{
					Gate.Delete();
					Gate = null;
				}

				return;
			}

			if (Gate == null || Gate.Deleted)
			{
				Gate = new PvPSpectatorGate(this);

				if (!Options.Locations.SpectateGate.MoveToWorld(Gate))
				{
					Gate.MoveToWorld(Options.Locations.SpectateGate, Options.Locations.SpectateGate);
				}
			}

			if (Gate.Battle == null)
			{
				Gate.Battle = this;
			}
		}

		public virtual void InvalidateTeamGates()
		{
			Teams.ForEach(t => t.InvalidateGate());
		}
	}
}