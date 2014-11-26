#region Header
//   Vorspire    _,-'/-'/  CTF.cs
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

using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
	public class CTFBattle : PvPBattle
	{
		private PollTimer _FlagEffectTimer;

		[CommandProperty(AutoPvP.Access)]
		public virtual int CapsToWin { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual double FlagDamageInc { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual double FlagDamageIncMax { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int FlagCapturePoints { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int FlagReturnPoints { get; set; }

		public CTFBattle()
		{
			Name = "Capture The Flag";
			Category = "Capture The Flag";
			Description =
				"Capture the enemy flag and return it to your podium to score points!\nDefend your flag from the enemy, you can only capture their flag when your flag is on your podium.";

			CapsToWin = 5;

			AddTeam(NameList.RandomName("daemon"), 1, 1, 0x22);
			AddTeam(NameList.RandomName("daemon"), 1, 1, 0x55);

			Schedule.Info.Months = ScheduleMonths.All;
			Schedule.Info.Days = ScheduleDays.All;
			Schedule.Info.Times = ScheduleTimes.EveryQuarterHour;

			Options.Timing.PreparePeriod = TimeSpan.FromMinutes(5.0);
			Options.Timing.RunningPeriod = TimeSpan.FromMinutes(15.0);
			Options.Timing.EndedPeriod = TimeSpan.FromMinutes(5.0);

			Options.Rules.AllowBeneficial = true;
			Options.Rules.AllowHarmful = true;
			Options.Rules.AllowHousing = false;
			Options.Rules.AllowPets = false;
			Options.Rules.AllowSpawn = false;
			Options.Rules.AllowSpeech = true;
			Options.Rules.CanBeDamaged = true;
			Options.Rules.CanDamageEnemyTeam = true;
			Options.Rules.CanDamageOwnTeam = false;
			Options.Rules.CanDie = false;
			Options.Rules.CanHeal = true;
			Options.Rules.CanHealEnemyTeam = false;
			Options.Rules.CanHealOwnTeam = true;
			Options.Rules.CanMount = false;
			Options.Rules.CanResurrect = true;
			Options.Rules.CanUseStuckMenu = false;
		}

		public CTFBattle(GenericReader reader)
			: base(reader)
		{
			CapsToWin = 5;
		}

		public override bool Validate(Mobile viewer, List<string> errors, bool pop = true)
		{
			if (!base.Validate(viewer, errors, pop) && pop)
			{
				return false;
			}

			if (CapsToWin < 1)
			{
				errors.Add("CapsToWin value must be equal to or greater than 1.");
				errors.Add("[Options] -> [Edit Options]");
			}

			if (Teams.Any(t => !(t is CTFTeam)))
			{
				errors.Add("One or more teams are not of the CTFTeam type.");
				errors.Add("[Options] -> [View Teams]");

				if (pop)
				{
					return false;
				}
			}

			return true;
		}

		protected override void OnInit()
		{
			_FlagEffectTimer = PollTimer.FromMilliseconds(
				100.0,
				() => Teams.OfType<CTFTeam>().Where(t => t != null && t.Flag != null).ForEach(t => t.Flag.InvalidateCarryEffect()),
				() => !Deleted && !Hidden && State == PvPBattleState.Running && Teams.Count > 0);

			base.OnInit();
		}

		protected override void RegisterSubCommands()
		{
			base.RegisterSubCommands();

			RegisterSubCommand(
				"scores",
				state =>
				{
					if (state == null || state.Mobile == null || state.Mobile.Deleted)
					{
						return false;
					}

					PlayerMobile pm = state.Mobile;

					if (pm == null || !IsParticipant(pm))
					{
						return false;
					}

					foreach (CTFTeam team in Teams.OfType<CTFTeam>())
					{
						pm.SendMessage(team.Color, "[{0}]: {1:#,0} / {2:#,0} flag captures.", team.Name, team.Caps, CapsToWin);
					}

					return true;
				},
				"Displays the current scores for each team.",
				"",
				AccessLevel.Player);

			RegisterSubCommandAlias("scores");
		}

		public override bool CanEndBattle()
		{
			return base.CanEndBattle() ||
				   (State == PvPBattleState.Running && Teams.OfType<CTFTeam>().Any(t => t.Caps >= CapsToWin));
		}

		public override int CompareTeamRank(PvPTeam a, PvPTeam b)
		{
			return CompareTeamRank(a as CTFTeam, b as CTFTeam);
		}

		public virtual int CompareTeamRank(CTFTeam a, CTFTeam b)
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

			if (a.Deleted)
			{
				return 1;
			}

			if (b.Deleted)
			{
				return -1;
			}

			if (a.Caps > b.Caps)
			{
				return -1;
			}

			if (a.Caps < b.Caps)
			{
				return 1;
			}

			return 0;
		}

		public override bool IsWinningTeam(PvPTeam team)
		{
			var t = team as CTFTeam;

			if (t == null || t.Deleted)
			{
				return false;
			}

			return t.Caps >= CapsToWin;
		}

		public override void OnTeamWin(PvPTeam team)
		{
			WorldBroadcast("Team {0} has won {1}!", team.Name, Name);

			base.OnTeamWin(team);
		}

		public override bool AddTeam(string name, int minCapacity, int capacity, int color)
		{
			return AddTeam(new CTFTeam(this, name, minCapacity, capacity, color));
		}

		public override bool AddTeam(PvPTeam team)
		{
			return team != null && !team.Deleted &&
				   (team is CTFTeam ? base.AddTeam(team) : AddTeam(team.Name, team.MinCapacity, team.MinCapacity, team.Color));
		}

		public virtual void OnFlagDropped(CTFFlag flag, PlayerMobile attacker, CTFTeam enemyTeam)
		{
			EnsureStatistics(attacker)["Flags Dropped"]++;

			PlaySound(746);

			LocalBroadcast("[{0}]: {1} has dropped the flag of team {2}!", enemyTeam.Name, attacker.Name, flag.Team.Name);
		}

		public virtual void OnFlagCaptured(CTFFlag flag, PlayerMobile attacker, CTFTeam enemyTeam)
		{
			EnsureStatistics(attacker)["Flags Captured"]++;

			if (FlagCapturePoints > 0)
			{
				AwardPoints(attacker, FlagCapturePoints);
			}

			PlaySound(747);

			LocalBroadcast("[{0}]: {1} has captured the flag of team {2}!", enemyTeam.Name, attacker.Name, flag.Team.Name);
			LocalBroadcast("Team {0} now has {1:#,0} / {2:#,0} flag captures!", enemyTeam.Name, enemyTeam.Caps, CapsToWin);
		}

		public virtual void OnFlagStolen(CTFFlag flag, PlayerMobile attacker, CTFTeam enemyTeam)
		{
			EnsureStatistics(attacker)["Flags Stolen"]++;

			PlaySound(748);

			LocalBroadcast("[{0}]: {1} has stolen the flag of team {2}!", enemyTeam.Name, attacker.Name, flag.Team.Name);
		}

		public virtual void OnFlagReturned(CTFFlag flag, PlayerMobile defender)
		{
			EnsureStatistics(defender)["Flags Returned"]++;

			if (FlagReturnPoints > 0)
			{
				AwardPoints(defender, FlagReturnPoints);
			}

			PlaySound(749);

			LocalBroadcast("[{0}]: {1} has returned the flag of team {0}!", flag.Team.Name, defender.Name);
		}

		public virtual void OnFlagTimeout(CTFFlag flag)
		{
			PlaySound(749);

			LocalBroadcast("[{0}]: Flag has been returned to the base!", flag.Team.Name);
		}

		protected override void OnDamageAccept(Mobile from, Mobile damaged, ref int damage)
		{
			base.OnDamageAccept(from, damaged, ref damage);

			var flag = damaged.Backpack.FindItemByType<CTFFlag>();

			if (flag != null)
			{
				damage += (int)(damage * flag.DamageInc);
			}
		}

		protected override void OnDeleted()
		{
			if (_FlagEffectTimer != null)
			{
				_FlagEffectTimer.Stop();
				_FlagEffectTimer = null;
			}

			base.OnDeleted();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(2);

			switch (version)
			{
				case 2:
					{
						writer.Write(FlagDamageInc);
						writer.Write(FlagDamageIncMax);
					}
					goto case 1;
				case 1:
					{
						writer.Write(FlagCapturePoints);
						writer.Write(FlagReturnPoints);
					}
					goto case 0;
				case 0:
					writer.Write(CapsToWin);
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 2:
					{
						FlagDamageInc = reader.ReadDouble();
						FlagDamageIncMax = reader.ReadDouble();
					}
					goto case 1;
				case 1:
					{
						FlagCapturePoints = reader.ReadInt();
						FlagReturnPoints = reader.ReadInt();
					}
					goto case 0;
				case 0:
					{
						CapsToWin = reader.ReadInt();

						Type type = typeof(CTFTeam);

						Teams.Where(t => !t.GetType().IsEqualOrChildOf(type)).ForEach(
							t =>
							{
								Teams.Remove(t);
								AddTeam(t);
							});
					}
					break;
			}
		}
	}
}