#region Header
//   Vorspire    _,-'/-'/  Battle.cs
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
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;

using Server;
using Server.Items;
using Server.Mobiles;
using Server.Spells.Fifth;
using Server.Spells.Seventh;
using Server.Targeting;

using VitaNex.Schedules;
using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public interface IPvPBattle
	{
		Dictionary<string, PvPBattleCommandInfo> SubCommandHandlers { get; }
		Dictionary<PlayerMobile, PvPProfileHistoryEntry> Statistics { get; }
		Dictionary<PlayerMobile, PvPTeam> Queue { get; }

		List<PlayerMobile> Spectators { get; }
		List<BaseDoor> Doors { get; }
		List<PvPTeam> Teams { get; }

		[CommandProperty(AutoPvP.Access, true)]
		PvPSerial Serial { get; }

		[CommandProperty(AutoPvP.Access, true)]
		bool Initialized { get; }

		[CommandProperty(AutoPvP.Access, true)]
		bool Deleted { get; }

		[CommandProperty(AutoPvP.Access, true)]
		bool IsFull { get; }

		[CommandProperty(AutoPvP.Access, true)]
		int MinCapacity { get; }

		[CommandProperty(AutoPvP.Access, true)]
		int MaxCapacity { get; }

		[CommandProperty(AutoPvP.Access, true)]
		int CurrentCapacity { get; }

		[CommandProperty(AutoPvP.Access, true)]
		PvPBattleState LastState { get; }

		[CommandProperty(AutoPvP.Access, true)]
		DateTime LastStateChange { get; }

		[CommandProperty(AutoPvP.Access, true)]
		string BattleRegionName { get; }

		[CommandProperty(AutoPvP.Access, true)]
		string SpectateRegionName { get; }

		[CommandProperty(AutoPvP.Access)]
		PvPBattleRegion BattleRegion { get; set; }

		[CommandProperty(AutoPvP.Access)]
		PvPSpectateRegion SpectateRegion { get; set; }

		[CommandProperty(AutoPvP.Access)]
		PvPBattleState State { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool DebugMode { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool Hidden { get; set; }

		[CommandProperty(AutoPvP.Access)]
		Schedule Schedule { get; set; }

		[CommandProperty(AutoPvP.Access)]
		PvPBattleOptions Options { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool Ranked { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool AutoAssign { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool UseTeamColors { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool IgnoreCapacity { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool FloorItemDelete { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool InviteWhileRunning { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool SpectateAllowed { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool QueueAllowed { get; set; }

		[CommandProperty(AutoPvP.Access)]
		bool IdleKick { get; set; }

		[CommandProperty(AutoPvP.Access)]
		string Name { get; set; }

		[CommandProperty(AutoPvP.Access)]
		string Description { get; set; }

		[CommandProperty(AutoPvP.Access)]
		string Category { get; set; }

		[CommandProperty(AutoPvP.Access)]
		char SubCommandPrefix { get; set; }

		[CommandProperty(AutoPvP.Access)]
		int LightLevel { get; set; }

		[CommandProperty(AutoPvP.Access)]
		int KillPoints { get; set; }

		[CommandProperty(AutoPvP.Access)]
		int PointsBase { get; set; }

		[CommandProperty(AutoPvP.Access)]
		double PointsRankFactor { get; set; }

		[CommandProperty(AutoPvP.Access)]
		PvPSpectatorGate Gate { get; set; }

		[CommandProperty(AutoPvP.Access)]
		TimeSpan LogoutDelay { get; set; }

		[CommandProperty(AutoPvP.Access)]
		TimeSpan IdleThreshold { get; set; }

		void Init();
		void Sync();
		void MicroSync();
		void Reset();
		void Delete();

		void Serialize(GenericWriter writer);
		void Deserialize(GenericReader reader);
	}

	[PropertyObject]
	public abstract partial class PvPBattle : IPvPBattle, IEquatable<PvPBattle>
	{
		private static readonly FieldInfo _DoorTimerField = ResolveDoorTimerField();

		private static FieldInfo ResolveDoorTimerField()
		{
			Type t = typeof(BaseDoor);

			return t.GetField("m_Timer", BindingFlags.Instance | BindingFlags.NonPublic) ??
				   t.GetField("_Timer", BindingFlags.Instance | BindingFlags.NonPublic);
		}

		private string _Name;
		private PvPBattleOptions _Options;

		private int _CoreTicks;
		private PollTimer _CoreTimer;

		[CommandProperty(AutoPvP.Access, true)]
		public bool Initialized { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public bool Deleted { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool DebugMode { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Hidden { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual Schedule Schedule { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleOptions Options { get { return _Options; } set { _Options = value ?? new PvPBattleOptions(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual string Name
		{
			get { return _Name; }
			set
			{
				_Name = value;

				if (!Deserializing)
				{
					InvalidateRegions();
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual string Category { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual string Description { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPSpectatorGate Gate { get; set; }

		public List<BaseDoor> Doors { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual Map Map { get { return Options.Locations.Map; } set { Options.Locations.Map = value; } }

		protected virtual void EnsureConstructDefaults()
		{
			Queue = new Dictionary<PlayerMobile, PvPTeam>();
			SubCommandHandlers = new Dictionary<string, PvPBattleCommandInfo>();
			Statistics = new Dictionary<PlayerMobile, PvPProfileHistoryEntry>();
			StatisticsCache = new Dictionary<PlayerMobile, PvPProfileHistoryEntry>();

			Spectators = new List<PlayerMobile>();
			Teams = new List<PvPTeam>();
			Doors = new List<BaseDoor>();
		}

		public PvPBattle()
			: this(false)
		{
			Serial = new PvPSerial();
			Options = new PvPBattleOptions();
			Schedule = new Schedule("PvP Battle " + Serial, false);

			AutoAssign = true;
			UseTeamColors = true;

			Name = "PvP Battle";
			Description = "PvP Battle";
			Category = "Misc";

			IdleKick = true;
			IdleThreshold = TimeSpan.FromSeconds(30.0);

			PointsBase = 1;
			KillPoints = 1;

			QueueAllowed = true;
			SpectateAllowed = false;
		}

		public void Init()
		{
			if (Initialized)
			{
				return;
			}

			InvalidateRegions();
			RegisterSubCommands();

			BattleNotoriety.RegisterNotorietyHandler(this, NotorietyHandler);
			BattleNotoriety.RegisterAllowBeneficialHandler(this, AllowBeneficial);
			BattleNotoriety.RegisterAllowHarmfulHandler(this, AllowHarmful);

			EventSink.Shutdown += ServerShutdownHandler;
			EventSink.Logout += LogoutHandler;
			EventSink.Login += LoginHandler;

			_CoreTimer = PollTimer.FromSeconds(1.0, OnCoreTick, () => Initialized && !_StateTransition);

			Schedule.OnGlobalTick += OnScheduleTick;

			Teams.Where(team => team != null && !team.Deleted).ForEach(team => team.Init());

			OnInit();

			Initialized = true;

			if (!Validate())
			{
				State = PvPBattleState.Internal;
			}
		}

		protected virtual void OnCoreTick()
		{
			++_CoreTicks;

			MicroSync();
			InvalidateState();

			if (State == PvPBattleState.Internal || Hidden)
			{
				return;
			}

			BroadcastStateHandler();
			SuddenDeathHandler();

			if (CanUseWeather())
			{
				WeatherCycle();
			}

			if (_CoreTicks % 5 == 0 && State != PvPBattleState.Internal)
			{
				GetParticipants().Where(pm => !IsOnline(pm)).ForEach(pm => Eject(pm, true));

				if (CanSendInvites())
				{
					SendInvites();
				}
			}

			if (_CoreTicks % 10 == 0)
			{
				Sync();
			}
		}

		protected virtual void OnScheduleTick(Schedule schedule)
		{
			if (!World.Loading && !World.Saving && CanPrepareBattle())
			{
				State = PvPBattleState.Preparing;
			}
		}

		protected virtual void ServerShutdownHandler(ShutdownEventArgs e)
		{
			Reset();
		}

		protected virtual void LogoutHandler(LogoutEventArgs e)
		{
			if (e == null || e.Mobile == null || e.Mobile.Deleted || e.Mobile.Region == null ||
				(!e.Mobile.Region.IsPartOf(BattleRegion) && !e.Mobile.Region.IsPartOf(SpectateRegion)))
			{
				return;
			}

			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm == null)
			{
				return;
			}

			if (IsQueued(pm))
			{
				Dequeue(pm);
			}

			if (IsParticipant(pm) || IsSpectator(pm))
			{
				Eject(pm, true);
			}
		}

		protected virtual void LoginHandler(LoginEventArgs e)
		{
			if (e != null && e.Mobile != null && !e.Mobile.Deleted && e.Mobile.Region != null &&
				(e.Mobile.Region.IsPartOf(BattleRegion) || e.Mobile.Region.IsPartOf(SpectateRegion)))
			{
				InvalidateStray(e.Mobile);
			}
		}

		public void Sync()
		{
			GetParticipants()
				.Where(pm => pm != null && !pm.Deleted)
				.Select(pm => AutoPvP.EnsureProfile(pm))
				.ForEach(p => p.Sync());

			Teams.Where(team => team != null && !team.Deleted).ForEach(team => team.Sync());

			if (Schedule != null && Schedule.Enabled)
			{
				Schedule.InvalidateNextTick(DateTime.UtcNow);
			}

			OnSync();
		}

		public void MicroSync()
		{
			if (!Validate())
			{
				State = PvPBattleState.Internal;
				return;
			}

			if (BattleRegion != null)
			{
				BattleRegion.MicroSync();
			}

			if (SpectateRegion != null)
			{
				SpectateRegion.MicroSync();
			}

			Teams.Where(team => team != null && !team.Deleted).ForEach(team => team.MicroSync());

			OnMicroSync();
		}

		public void Reset()
		{
			OnReset();

			Teams.ForEach(ResetTeam);

			Statistics.Clear();
			StatisticsCache.Clear();
		}

		public void Delete()
		{
			if (Deleted)
			{
				return;
			}

			Reset();

			BattleNotoriety.NameHandlers.Remove(this);
			BattleNotoriety.BeneficialHandlers.Remove(this);
			BattleNotoriety.HarmfulHandlers.Remove(this);

			EventSink.Shutdown -= ServerShutdownHandler;
			EventSink.Logout -= LogoutHandler;
			EventSink.Login -= LoginHandler;

			if (_CoreTimer != null)
			{
				_CoreTimer.Dispose();
				_CoreTimer = null;
			}

			_CoreTicks = 0;

			if (Gate != null)
			{
				Gate.Delete();
				Gate = null;
			}

			Teams.Where(t => t != null && !t.Deleted).ForEach(t => t.Delete());

			if (Schedule != null)
			{
				Schedule.Stop();
				Schedule.Enabled = false;
				Schedule.OnGlobalTick -= OnScheduleTick;
				Schedule = null;
			}

			if (_BattleRegion != null)
			{
				_BattleRegion.ClearPreview();
				_BattleRegion.Unregister();
				_BattleRegion = null;
			}

			if (_SpectateRegion != null)
			{
				_SpectateRegion.ClearPreview();
				_SpectateRegion.Unregister();
				_SpectateRegion = null;
			}

			if (_Options != null)
			{
				_Options.Clear();
			}

			OnDeleted();

			if (AutoPvP.RemoveBattle(this))
			{
				OnRemoved();
			}

			Deleted = true;
		}

		public virtual void ToggleDoors(bool secure)
		{
			Doors.RemoveAll(door => door == null || door.Deleted || door.Map != Map);

			Doors.Where(d => (d.Open && CanCloseDoor(d)) || (!d.Open && CanOpenDoor(d))).ForEach(
				door =>
				{
					door.Open = !door.Open;
					door.Locked = secure;

					if (_DoorTimerField == null)
					{
						return;
					}

					Timer t = _DoorTimerField.GetValue(door) as Timer;

					if (t != null)
					{
						t.Stop();
					}
				});
		}

		public virtual void OpendDoors(bool secure)
		{
			Doors.RemoveAll(door => door == null || door.Deleted || door.Map != Map);

			Doors.Where(d => !d.Open && CanOpenDoor(d)).ForEach(
				door =>
				{
					door.Open = true;
					door.Locked = secure;

					if (_DoorTimerField == null)
					{
						return;
					}

					Timer t = _DoorTimerField.GetValue(door) as Timer;

					if (t != null)
					{
						t.Stop();
					}
				});
		}

		public virtual void CloseDoors(bool secure)
		{
			Doors.RemoveAll(door => door == null || door.Deleted || door.Map != Map);

			Doors.Where(d => d.Open && CanCloseDoor(d)).ForEach(
				door =>
				{
					door.Open = false;
					door.Locked = secure;

					if (_DoorTimerField == null)
					{
						return;
					}

					Timer t = _DoorTimerField.GetValue(door) as Timer;

					if (t != null)
					{
						t.Stop();
					}
				});
		}

		public virtual bool CanOpenDoor(BaseDoor door)
		{
			return door != null && !door.Deleted;
		}

		public virtual bool CanCloseDoor(BaseDoor door)
		{
			return door != null && !door.Deleted && door.CanClose();
		}

		public virtual bool CheckSuddenDeath()
		{
			if (!Options.SuddenDeath.Enabled || State != PvPBattleState.Running || Hidden)
			{
				return false;
			}

			if (Teams.Count > 1 && GetAliveTeams().Count() > 1)
			{
				return CurrentCapacity <= Options.SuddenDeath.CapacityRequired;
			}

			return CurrentCapacity > 1 && CurrentCapacity <= Options.SuddenDeath.CapacityRequired;
		}

		protected virtual void SuddenDeathHandler()
		{
			if (!CheckSuddenDeath())
			{
				if (Options.SuddenDeath.Active)
				{
					Options.SuddenDeath.End();
					OnSuddenDeathEnd();
				}
			}
			else if (!Options.SuddenDeath.Active)
			{
				Options.SuddenDeath.Start();
				OnSuddenDeathStart();
			}
		}

		protected virtual void OnSuddenDeathStart()
		{
			LocalBroadcast("Sudden death! Prepare for the worst!");
		}

		protected virtual void OnSuddenDeathEnd()
		{
			LocalBroadcast("Sudden death has ended.");
		}

		public bool IsOnline(PlayerMobile pm)
		{
			return pm != null && !pm.Deleted && pm.NetState != null && pm.NetState.Running;
		}

		public bool InCombat(PlayerMobile pm)
		{
			return pm != null && pm.InCombat();
		}

		protected virtual void OnReset()
		{ }

		protected virtual void OnDeleted()
		{ }

		protected virtual void OnInit()
		{ }

		protected virtual void OnSync()
		{ }

		protected virtual void OnMicroSync()
		{
			if (Validate())
			{
				InvalidateGates();
			}
		}

		protected virtual void OnRemoved()
		{ }

		protected virtual void OnRewarded(PlayerMobile pm, IEntity reward)
		{
			if (pm != null && !pm.Deleted && reward != null)
			{
				pm.SendMessage("You have been rewarded for your efforts.");
			}
		}

		protected virtual void OnWin(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			var entry = EnsureStatistics(pm);

			entry.Battles = 1;
			entry.Wins = 1;

			GiveWinnerReward(pm);

			AwardPoints(pm);
		}

		protected virtual void OnLose(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			var entry = EnsureStatistics(pm);

			entry.Battles = 1;
			entry.Losses = 1;

			GiveLoserReward(pm);

			RevokePoints(pm);
		}

		public virtual void GiveWinnerReward(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				Options.Rewards.Winner.GiveReward(pm).ForEach(reward => OnRewarded(pm, reward));
			}
		}

		public virtual void GiveLoserReward(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				Options.Rewards.Loser.GiveReward(pm).ForEach(reward => OnRewarded(pm, reward));
			}
		}

		public virtual void SetRestrictedPets(IDictionary<Type, bool> list)
		{
			foreach (var kvp in list)
			{
				Options.Restrictions.Pets.SetRestricted(kvp.Key, kvp.Value);
			}
		}

		public virtual void SetRestrictedItems(IDictionary<Type, bool> list)
		{
			foreach (var kvp in list)
			{
				Options.Restrictions.Pets.SetRestricted(kvp.Key, kvp.Value);
			}
		}

		public virtual void SetRestrictedSpells(IDictionary<Type, bool> list)
		{
			foreach (var kvp in list)
			{
				Options.Restrictions.Spells.SetRestricted(kvp.Key, kvp.Value);
			}
		}

		public virtual void SetRestrictedSkills(IDictionary<SkillName, bool> list)
		{
			foreach (var kvp in list)
			{
				Options.Restrictions.Skills.SetRestricted(kvp.Key, kvp.Value);
			}
		}

		public virtual void RefreshStats(PlayerMobile pm)
		{
			RefreshStats(pm, true, true);
		}

		public virtual void RefreshStats(PlayerMobile pm, bool negate, bool resurrect)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			if (negate)
			{
				Negate(pm);
			}

			if (!pm.Alive && resurrect)
			{
				pm.Resurrect();
			}

			if (!pm.Alive)
			{
				return;
			}

			pm.Hits = pm.HitsMax;
			pm.Stam = pm.StamMax;
			pm.Mana = pm.ManaMax;
		}

		public virtual void Negate(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return;
			}

			if (m.Frozen)
			{
				m.Frozen = false;
			}

			if (m.Paralyzed)
			{
				m.Paralyzed = false;
			}

			if (m.Poisoned)
			{
				m.CurePoison(m);
			}

			if (BleedAttack.IsBleeding(m))
			{
				BleedAttack.EndBleed(m, true);
			}

			if (MortalStrike.IsWounded(m))
			{
				MortalStrike.EndWound(m);
			}

			PolymorphSpell.StopTimer(m);
			IncognitoSpell.StopTimer(m);
			DisguiseTimers.RemoveTimer(m);

			m.EndAction(typeof(PolymorphSpell));
			m.EndAction(typeof(IncognitoSpell));

			MeerMage.StopEffect(m, false);

			if (DebugMode || m.AccessLevel <= AccessLevel.Counselor)
			{
				m.RevealingAction();
				m.DisruptiveAction();
			}

			if (m.Target != null)
			{
				m.Target.Cancel(m, TargetCancelType.Overriden);
			}

			m.Spell = null;

			if (m.Combatant != null)
			{
				if (m.Combatant.Combatant != null && m.Combatant.Combatant == m)
				{
					m.Combatant.Combatant = null;
					m.Combatant.Warmode = false;
				}

				m.Combatant = null;
			}

			if (m.Aggressed != null)
			{
				m.Aggressed.Clear();
			}

			if (m.Aggressors != null)
			{
				m.Aggressors.Clear();
			}

			m.Warmode = false;
			m.Criminal = false;
			m.Delta(MobileDelta.Noto);
		}

		public virtual bool CheckAccessibility(Item item, Mobile from)
		{
			return true;
		}

		public virtual bool OnDecay(Item item)
		{
			return true;
		}

		public virtual void SpellDamageScalar(Mobile caster, Mobile target, ref double damage)
		{ }

		public virtual void GetHtmlString(Mobile viewer, StringBuilder html, bool preview = false)
		{
			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));

			if (Deleted)
			{
				html.AppendLine(
					"This battle no longer exists.".WrapUOHtmlTag("B")
												   .WrapUOHtmlTag("BIG")
												   .WrapUOHtmlColor(Color.OrangeRed, SuperGump.DefaultHtmlColor));
				return;
			}

			html.AppendLine("{0} ({1})".WrapUOHtmlTag("BIG"), Name, Ranked ? "Ranked" : "Unranked");
			html.AppendLine(
				"State: {0}".WrapUOHtmlTag("BIG").WrapUOHtmlColor(Color.SkyBlue, SuperGump.DefaultHtmlColor),
				State.ToString().SpaceWords());

			int curCap = CurrentCapacity, minCap = MinCapacity, maxCap = MaxCapacity;

			if (!preview)
			{
				TimeSpan timeLeft = GetStateTimeLeft(DateTime.UtcNow);

				if (timeLeft >= TimeSpan.Zero && State != PvPBattleState.Internal)
				{
					html.AppendLine(
						"Time Left: {0}".WrapUOHtmlTag("BIG")
										.WrapUOHtmlColor(timeLeft <= TimeSpan.Zero ? Color.OrangeRed : Color.LawnGreen, SuperGump.DefaultHtmlColor),
						timeLeft.ToSimpleString("h:m:s"));
					html.AppendLine();
				}

				if (State == PvPBattleState.Preparing && !CanStartBattle() && !IgnoreCapacity && curCap < minCap)
				{
					html.AppendLine(
						"{0} more players are required to start the battle.".WrapUOHtmlColor(Color.OrangeRed, SuperGump.DefaultHtmlColor),
						minCap - curCap);
					html.AppendLine();
				}
			}

			if (!String.IsNullOrWhiteSpace(Description))
			{
				html.AppendLine(Description.WrapUOHtmlTag("BIG").WrapUOHtmlColor(Color.SkyBlue, SuperGump.DefaultHtmlColor));
				html.AppendLine();
			}

			if (Schedule != null && Schedule.Enabled)
			{
				html.AppendLine(
					Schedule.NextGlobalTick != null
						? "This battle is scheduled.".WrapUOHtmlColor(Color.LawnGreen, SuperGump.DefaultHtmlColor)
						: "This battle is scheduled, but has no future dates.".WrapUOHtmlColor(
							Color.OrangeRed, SuperGump.DefaultHtmlColor));
				html.AppendLine();
			}
			else
			{
				html.AppendLine("This battle is automatic.".WrapUOHtmlColor(Color.LawnGreen, SuperGump.DefaultHtmlColor));
				html.AppendLine();
			}

			if (!preview)
			{
				html.Append("".WrapUOHtmlColor(Color.YellowGreen, false));

				html.AppendLine("This battle takes place in {0}.", Map.Name);
				html.AppendLine();

				html.AppendLine("{0:#,0} players in the queue.", Queue.Count);
				html.AppendLine("{0:#,0} players in {1:#,0} teams attending.", curCap, Teams.Count);
				html.AppendLine("{0:#,0} invites available of {1:#,0} max.", maxCap - curCap, maxCap);
				html.AppendLine();

				html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));
			}

			GetHtmlCommandList(viewer, html, preview);
		}

		public virtual void GetHtmlCommandList(Mobile viewer, StringBuilder html, bool preview = false)
		{
			if (SubCommandHandlers.Count <= 0)
			{
				return;
			}

			html.Append("".WrapUOHtmlColor(Color.White, false));

			html.AppendLine("Commands".WrapUOHtmlTag("BIG"));
			html.AppendLine("(For use in Battle or Spectate regions)".WrapUOHtmlTag("BIG"));
			html.AppendLine();

			foreach (var i in
				SubCommandHandlers.Values.Where(i => viewer == null || viewer.AccessLevel >= i.Access)
								  .OrderByNatural(i => i.Command))
			{
				html.AppendLine(
					"{0}{1}{2}".WrapUOHtmlTag("BIG"),
					SubCommandPrefix,
					i.Command,
					!String.IsNullOrWhiteSpace(i.Usage) ? " " + i.Usage : String.Empty);

				if (String.IsNullOrWhiteSpace(i.Description))
				{
					html.AppendLine();
					continue;
				}

				html.AppendLine(i.Description.WrapUOHtmlColor(Color.SkyBlue, Color.White));
				html.AppendLine();
			}

			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));
		}

		public override int GetHashCode()
		{
			return Serial.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return obj is PvPBattle && Equals((PvPBattle)obj);
		}

		public bool Equals(PvPBattle other)
		{
			return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || Serial.Equals(other.Serial));
		}

		public override string ToString()
		{
			return Name ?? "PvP Battle";
		}

		public string ToHtmlString(Mobile viewer = null, bool preview = false)
		{
			StringBuilder html = new StringBuilder();

			GetHtmlString(viewer, html, preview);

			return html.ToString();
		}

		public static bool operator ==(PvPBattle left, PvPBattle right)
		{
			return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
		}

		public static bool operator !=(PvPBattle left, PvPBattle right)
		{
			return ReferenceEquals(null, left) ? !ReferenceEquals(null, right) : !left.Equals(right);
		}
	}
}