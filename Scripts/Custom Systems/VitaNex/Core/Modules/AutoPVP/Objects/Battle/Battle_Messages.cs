#region Header
//   Vorspire    _,-'/-'/  Battle_Messages.cs
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
using Server.Commands;
using Server.Mobiles;
using Server.Network;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public enum PvPBattleWarning
	{
		Starting,
		Ending,
		WBStarting,
		WBEnding
	}

	public abstract partial class PvPBattle
	{
		public virtual IEnumerable<PlayerMobile> GetLocalBroadcastList()
		{
			if (BattleRegion != null)
			{
				foreach (PlayerMobile pm in BattleRegion.GetMobiles().OfType<PlayerMobile>().Where(pm => pm.IsOnline()))
				{
					yield return pm;
				}
			}

			if (SpectateRegion != null)
			{
				foreach (PlayerMobile pm in SpectateRegion.GetMobiles().OfType<PlayerMobile>().Where(pm => pm.IsOnline()))
				{
					yield return pm;
				}
			}
		}

		public virtual IEnumerable<PlayerMobile> GetWorldBroadcastList()
		{
			return
				NetState.Instances.Where(state => state != null)
						.Select(state => state.Mobile as PlayerMobile)
						.Where(pm => pm != null && pm.IsOnline() && AutoPvP.EnsureProfile(pm).IsSubscribed(this));
		}

		public virtual void LocalBroadcast(string message, params object[] args)
		{
			PvPTeam team;

			foreach (PlayerMobile pm in GetLocalBroadcastList().Where(pm => pm != null && !pm.Deleted))
			{
				pm.SendMessage(IsParticipant(pm, out team) ? team.Color : Options.Broadcasts.Local.MessageHue, message, args);
			}
		}

		public virtual void WorldBroadcast(string message, params object[] args)
		{
			switch (Options.Broadcasts.World.Mode)
			{
				case PvPBattleWorldBroadcastMode.Notify:
					{
						string text = String.Format(message, args);

						foreach (PlayerMobile pm in GetWorldBroadcastList())
						{
							pm.SendNotification(text, true, 0.5, 10.0);
						}
					}
					break;

				case PvPBattleWorldBroadcastMode.Broadcast:
					{
						string text = String.Format(message, args);
						Packet p = new AsciiMessage(
							Server.Serial.MinusOne, -1, MessageType.Regular, Options.Broadcasts.World.MessageHue, 3, "System", text);

						p.Acquire();

						foreach (PlayerMobile pm in GetWorldBroadcastList())
						{
							pm.Send(p);
						}

						p.Release();
						NetState.FlushAll();
					}
					break;

				case PvPBattleWorldBroadcastMode.TownCrier:
					{
						foreach (TownCrier tc in TownCrier.Instances)
						{
							tc.PublicOverheadMessage(
								MessageType.Yell, Options.Broadcasts.World.MessageHue, true, String.Format(message, args));
						}
					}
					break;
			}
		}

		protected virtual void BroadcastStateHandler()
		{
			if (Hidden)
			{
				return;
			}

			PvPBattleState state = State;
			TimeSpan timeLeft = GetStateTimeLeft(DateTime.UtcNow).Add(TimeSpan.FromSeconds(1.0));

			if (timeLeft <= TimeSpan.Zero)
			{
				return;
			}

			switch (state)
			{
				case PvPBattleState.Ended:
					BroadcastOpenMessage(timeLeft);
					break;
				case PvPBattleState.Preparing:
					BroadcastStartMessage(timeLeft);
					break;
				case PvPBattleState.Running:
					BroadcastEndMessage(timeLeft);
					break;
			}
		}

		protected virtual void BroadcastOpenMessage(TimeSpan timeLeft)
		{
			if (timeLeft.Minutes > 5 || timeLeft.Minutes == 0 || timeLeft.Seconds != 0)
			{
				return;
			}

			string msg = String.Format("{0} {1}!", timeLeft.Minutes, timeLeft.Minutes != 1 ? "minutes" : "minute");

			if (String.IsNullOrWhiteSpace(msg))
			{
				return;
			}

			if (Options.Broadcasts.Local.OpenNotify)
			{
				LocalBroadcast("The battle will open in {0}", msg);
			}

			if (!Options.Broadcasts.World.OpenNotify)
			{
				return;
			}

			string cmd = QueueAllowed
							 ? String.Format(
								 "Type {0}{1} to join the queue!", CommandSystem.Prefix, AutoPvP.CMOptions.Advanced.Commands.BattlesCommand)
							 : String.Empty;
			WorldBroadcast("{0} will open in {1}! {2}", Name, msg, cmd);
		}

		protected virtual void BroadcastStartMessage(TimeSpan timeLeft)
		{
			if ((timeLeft.Minutes == 0 && timeLeft.Seconds > 10) || timeLeft.Minutes > 5)
			{
				return;
			}

			string msg = String.Empty;

			if (timeLeft.Minutes > 0)
			{
				if (timeLeft.Seconds == 0)
				{
					msg = String.Format("{0} {1}!", timeLeft.Minutes, timeLeft.Minutes != 1 ? "minutes" : "minute");
				}
			}
			else if (timeLeft.Seconds > 0)
			{
				msg = String.Format("{0} {1}!", timeLeft.Seconds, timeLeft.Seconds != 1 ? "seconds" : "second");
			}

			if (String.IsNullOrWhiteSpace(msg))
			{
				return;
			}

			if (Options.Broadcasts.Local.StartNotify)
			{
				LocalBroadcast("The battle will start in {0}", msg);
			}

			if (!Options.Broadcasts.World.StartNotify || timeLeft.Minutes <= 0)
			{
				return;
			}

			string cmd = QueueAllowed
							 ? String.Format(
								 "Type {0}{1} to join the battle!", CommandSystem.Prefix, AutoPvP.CMOptions.Advanced.Commands.BattlesCommand)
							 : String.Empty;
			WorldBroadcast("{0} will start in {1}! {2}", Name, msg, cmd);
		}

		protected virtual void BroadcastEndMessage(TimeSpan timeLeft)
		{
			if ((timeLeft.Minutes == 0 && timeLeft.Seconds > 10) || timeLeft.Minutes > 5)
			{
				return;
			}

			string msg = String.Empty;

			if (timeLeft.Minutes > 0)
			{
				if (timeLeft.Seconds == 0)
				{
					msg = String.Format("{0} {1}!", timeLeft.Minutes, timeLeft.Minutes != 1 ? "minutes" : "minute");
				}
			}
			else if (timeLeft.Seconds > 0)
			{
				msg = String.Format("{0} {1}!", timeLeft.Seconds, timeLeft.Seconds != 1 ? "seconds" : "second");
			}

			if (String.IsNullOrWhiteSpace(msg))
			{
				return;
			}

			if (Options.Broadcasts.Local.EndNotify)
			{
				LocalBroadcast("The battle will end in {1}", _Name, msg);
			}

			if (Options.Broadcasts.World.EndNotify && timeLeft.Minutes > 0)
			{
				WorldBroadcast("The battle {0} will end in {1}", Name, msg);
			}
		}
	}
}