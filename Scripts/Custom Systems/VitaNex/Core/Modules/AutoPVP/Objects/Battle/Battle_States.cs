#region Header
//   Vorspire    _,-'/-'/  Battle_States.cs
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
using System.Linq;

using Server;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public enum PvPBattleState
	{
		Internal,
		Queueing,
		Preparing,
		Running,
		Ended
	}

	public abstract partial class PvPBattle
	{
		private bool _StateTransition;

		private PvPBattleState _State = PvPBattleState.Internal;

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleState State
		{
			get { return _State; }
			set
			{
				if (_State == value)
				{
					return;
				}

				PvPBattleState oldState = _State;
				_State = value;

				if (!Deserializing)
				{
					OnStateChanged(oldState);
				}
			}
		}

		[CommandProperty(AutoPvP.Access, true)]
		public virtual PvPBattleState LastState { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public virtual DateTime LastStateChange { get; private set; }

		public void InvalidateState()
		{
			switch (State)
			{
				case PvPBattleState.Queueing:
					{
						if (Schedule != null && Schedule.Enabled && Schedule.NextGlobalTick != null)
						{
							return;
						}

						if (CanPrepareBattle())
						{
							State = PvPBattleState.Preparing;
						}
					}
					break;
				case PvPBattleState.Preparing:
					{
						if (CanStartBattle())
						{
							State = PvPBattleState.Running;
						}
					}
					break;
				case PvPBattleState.Running:
					{
						if (CanEndBattle())
						{
							State = PvPBattleState.Ended;
						}
					}
					break;
				case PvPBattleState.Ended:
					{
						if (CanOpenBattle())
						{
							State = PvPBattleState.Queueing;
						}
					}
					break;
			}
		}

		protected virtual void OnStateChanged(PvPBattleState oldState)
		{
			_StateTransition = true;

			DateTime now = DateTime.UtcNow;
			LastState = oldState;

			if (LastState == PvPBattleState.Internal)
			{
				InvalidateRegions();
			}

			switch (State)
			{
				case PvPBattleState.Internal:
					OnBattleInternalized(now);
					break;
				case PvPBattleState.Queueing:
					OnBattleOpened(now);
					break;
				case PvPBattleState.Preparing:
					OnBattlePreparing(now);
					break;
				case PvPBattleState.Running:
					OnBattleStarted(now);
					break;
				case PvPBattleState.Ended:
					OnBattleEnded(now);
					break;
			}

			LastStateChange = now;

			_StateTransition = false;
		}

		public virtual bool CanOpenBattle()
		{
			return (State == PvPBattleState.Internal || State == PvPBattleState.Ended) && !Hidden &&
				   DateTime.UtcNow >= Options.Timing.EndedWhen + Options.Timing.EndedPeriod;
		}

		public virtual bool CanPrepareBattle()
		{
			return State == PvPBattleState.Queueing && !Hidden;
		}

		public virtual bool CanStartBattle()
		{
			return State == PvPBattleState.Preparing && !Hidden &&
				   DateTime.UtcNow >= Options.Timing.PreparedWhen + Options.Timing.PreparePeriod;
		}

		public virtual bool CanEndBattle()
		{
			return State == PvPBattleState.Running &&
				   (Hidden || CurrentCapacity <= 1 || DateTime.UtcNow >= Options.Timing.StartedWhen + Options.Timing.RunningPeriod ||
					(Teams.Count > 1 && GetAliveTeams().Count() <= 1));
		}

		protected virtual void OnBattleInternalized(DateTime when)
		{
			Options.Timing.SetAllTimes(when);

			if (LastState == PvPBattleState.Running)
			{
				OnBattleCancelled(when);
			}

			Reset();

			AutoPvP.Profiles.Values.Where(p => p != null && !p.Deleted && p.IsSubscribed(this)).ForEach(p => p.Unsubscribe(this));
		}

		protected virtual void OnBattleOpened(DateTime when)
		{
			Options.Timing.SetTime(PvPBattleState.Queueing, when);

			Hidden = false;

			LocalBroadcast("The battle is queueing volunteers!");
			SendGlobalSound(Options.Sounds.BattleOpened);

			Reset();

			Teams.Where(t => t != null && !t.Deleted).ForEach(team => team.OnBattleOpened(when));

			if (LastState == PvPBattleState.Internal)
			{
				AutoPvP.Profiles.Values.Where(p => p != null && !p.Deleted && !p.IsSubscribed(this)).ForEach(p => p.Subscribe(this));
			}
		}

		protected virtual void OnBattlePreparing(DateTime when)
		{
			Options.Timing.SetTime(PvPBattleState.Preparing, when);

			Hidden = false;

			LocalBroadcast("The battle is iminent!");
			SendGlobalSound(Options.Sounds.BattlePreparing);

			Teams.Where(t => t != null && !t.Deleted).ForEach(
				t =>
				{
					t.OnBattlePreparing(when);
					t.ForEachMember(pm => TeleportToHomeBase(t, pm));
				});
		}

		protected virtual void OnBattleStarted(DateTime when)
		{
			Options.Timing.SetTime(PvPBattleState.Running, when);

			Hidden = false;

			bool check = Teams.TrueForAll(t => t != null && !t.Deleted && t.IsReady());

			if (!check)
			{
				LocalBroadcast("There were not enough players to start the battle.");
				OnBattleCancelled(when);
				return;
			}

			LocalBroadcast("The battle has begun!");
			SendGlobalSound(Options.Sounds.BattleStarted);

			OpendDoors(true);

			Teams.Where(t => t != null && !t.Deleted).ForEach(
				t =>
				{
					t.OnBattleStarted(when);

					if (t.RespawnOnStart)
					{
						t.ForEachMember(pm => TeleportToSpawnPoint(t, pm));
					}
				});
		}

		protected virtual void OnBattleEnded(DateTime when)
		{
			Options.Timing.SetTime(PvPBattleState.Ended, when);

			Hidden = false;

			if (Options.Broadcasts.World.EndNotify)
			{
				WorldBroadcast("The battle for {0} has ended.", Name);
			}

			LocalBroadcast("The battle has ended!");
			SendGlobalSound(Options.Sounds.BattleEnded);

			CloseDoors(true);

			var winningTeams = GetWinningTeams();

			Teams.Where(t => t != null && !t.Deleted).ForEach(
				t =>
				{
					t.OnBattleEnded(when);

					if (winningTeams.Contains(t))
					{
						TeamWinEject(t);
					}
					else
					{
						TeamLoseEject(t);
					}
				});
		}

		protected virtual void OnBattleCancelled(DateTime when)
		{
			Options.Timing.SetTime(PvPBattleState.Ended, when);

			Hidden = false;

			if (Options.Broadcasts.World.EndNotify)
			{
				WorldBroadcast("The battle for {0} has been cancelled.", Name);
			}

			LocalBroadcast("The battle has been cancelled!");
			SendGlobalSound(Options.Sounds.BattleCanceled);

			CloseDoors(true);

			Teams.Where(t => t != null && !t.Deleted).ForEach(
				t =>
				{
					t.OnBattleCancelled(when);

					t.ForEachMember(
						pm =>
						{
							GiveLoserReward(pm);

							t.RemoveMember(pm, true);
						});
				});
		}

		public virtual TimeSpan GetStateTimeLeft()
		{
			return GetStateTimeLeft(DateTime.UtcNow);
		}

		public virtual TimeSpan GetStateTimeLeft(DateTime when)
		{
			switch (State)
			{
				case PvPBattleState.Queueing:
					{
						if (Schedule != null && Schedule.Enabled && Schedule.NextGlobalTick != null)
						{
							return Schedule.NextGlobalTick.Value - when;
						}
					}
					break;
				case PvPBattleState.Preparing:
					return (Options.Timing.PreparedWhen + Options.Timing.PreparePeriod) - when;
				case PvPBattleState.Running:
					return (Options.Timing.StartedWhen + Options.Timing.RunningPeriod) - when;
				case PvPBattleState.Ended:
					return (Options.Timing.EndedWhen + Options.Timing.EndedPeriod) - when;
			}

			return TimeSpan.Zero;
		}
	}
}