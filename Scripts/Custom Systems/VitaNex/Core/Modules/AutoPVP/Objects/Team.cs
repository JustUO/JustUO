#region Header
//   Vorspire    _,-'/-'/  Team.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	[PropertyObject]
	public class PvPTeamStatistics
	{
		public PvPTeam Team { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalDamageTaken { get { return (Team != null && !Team.Deleted) ? Team.GetTotalDamageTaken() : 0; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalDamageDone { get { return (Team != null && !Team.Deleted) ? Team.GetTotalDamageDone() : 0; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalHealingTaken { get { return (Team != null && !Team.Deleted) ? Team.GetTotalHealingTaken() : 0; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalHealingDone { get { return (Team != null && !Team.Deleted) ? Team.GetTotalHealingDone() : 0; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalDeaths { get { return (Team != null && !Team.Deleted) ? Team.GetTotalDeaths() : 0; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalResurrections { get { return (Team != null && !Team.Deleted) ? Team.GetTotalResurrections() : 0; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalKills { get { return (Team != null && !Team.Deleted) ? Team.GetTotalKills() : 0; } }

		public PvPTeamStatistics(PvPTeam team)
		{
			Team = team;
		}

		public override string ToString()
		{
			return "View Statistics";
		}
	}

	[PropertyObject]
	public class PvPTeam : IEnumerable<PlayerMobile>
	{
		private string _Name;
		private int _Color;

		private int _MaxCapacity;
		private int _MinCapacity;

		private Point3D _HomeBase = Point3D.Zero;
		private Point3D _SpawnPoint = Point3D.Zero;

		public Dictionary<PlayerMobile, DateTime> Members { get; private set; }
		public Dictionary<PlayerMobile, DateTime> Dead { get; private set; }
		public List<PlayerMobile> Idlers { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public PvPSerial Serial { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public bool Initialized { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public bool Deleted { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public PvPBattle Battle { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPTeamGate Gate { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPTeamStatistics Statistics { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual string Name { get { return _Name; } set { _Name = value ?? String.Empty; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int MinCapacity { get { return _MinCapacity; } set { _MinCapacity = Math.Max(1, Math.Min(MaxCapacity, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int MaxCapacity { get { return _MaxCapacity; } set { _MaxCapacity = Math.Max(MinCapacity, value); } }

		[Hue, CommandProperty(AutoPvP.Access)]
		public virtual int Color { get { return _Color; } set { _Color = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual Point3D HomeBase
		{
			get { return _HomeBase; }
			set
			{
				if (Deserializing || (Battle.BattleRegion != null && Battle.BattleRegion.Contains(value)))
				{
					_HomeBase = value;
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual Point3D SpawnPoint
		{
			get { return _SpawnPoint; }
			set
			{
				if (Deserializing || (Battle.BattleRegion != null && Battle.BattleRegion.Contains(value)))
				{
					_SpawnPoint = value;
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual MapPoint GateLocation { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public bool KickOnDeath { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public bool RespawnOnStart { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public bool RespawnOnDeath { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public TimeSpan RespawnDelay { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public int Count { get { return Members.Count; } }

		[CommandProperty(AutoPvP.Access)]
		public bool IsFull { get { return Count >= MaxCapacity; } }

		public PlayerMobile this[int index]
		{
			get
			{
				if (index >= 0 && index <= Members.Count)
				{
					return Members.Keys.ElementAt(index);
				}

				return null;
			}
		}

		public DateTime? this[PlayerMobile pm]
		{
			get { return IsMember(pm) ? (DateTime?)Members[pm] : null; }
			set
			{
				if (IsMember(pm))
				{
					Members[pm] = value ?? DateTime.UtcNow;
				}
				else
				{
					Members.Add(pm, value ?? DateTime.UtcNow);
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual Map Map { get { return Battle.Map; } }

		protected bool Deserialized { get; private set; }
		protected bool Deserializing { get; private set; }

		protected virtual void EnsureConstructDefaults()
		{
			Idlers = new List<PlayerMobile>();
			Dead = new Dictionary<PlayerMobile, DateTime>();
			Members = new Dictionary<PlayerMobile, DateTime>();
		}

		private PvPTeam(bool deserializing)
		{
			Deserialized = deserializing;

			EnsureConstructDefaults();
		}

		public PvPTeam(PvPBattle battle, string name = "Incognito", int minCapacity = 1, int maxCapacity = 1, int color = 12)
			: this(false)
		{
			Serial = new PvPSerial();
			Battle = battle;
			Name = name;
			Color = color;

			GateLocation = MapPoint.Empty;

			KickOnDeath = true;
			RespawnOnStart = true;
			RespawnOnDeath = false;
			RespawnDelay = TimeSpan.FromSeconds(10.0);

			MinCapacity = minCapacity;
			MaxCapacity = maxCapacity;

			Statistics = new PvPTeamStatistics(this);

			Reset();
		}

		public PvPTeam(PvPBattle battle, GenericReader reader)
			: this(true)
		{
			Battle = battle;

			Deserializing = true;
			Deserialize(reader);
			Deserializing = false;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<PlayerMobile> GetEnumerator()
		{
			return Members.Keys.GetEnumerator();
		}

		public void ForEachMember(Action<PlayerMobile> action)
		{
			Members.Keys.ForEach(action);
		}

		public virtual bool CanRespawn(PlayerMobile member)
		{
			return member != null && !member.Deleted && RespawnOnDeath && IsMember(member) && Battle.IsOnline(member);
		}

		public virtual void Respawn(PlayerMobile member)
		{
			if (member == null || member.Deleted || !CanRespawn(member))
			{
				return;
			}

			if (member.Alive)
			{
				OnMemberResurrected(member);
			}

			Battle.RefreshStats(member, true, true);
			Battle.TeleportToSpawnPoint(this, member);
		}

		public bool Validate(Mobile viewer = null)
		{
			return Validate(viewer, new List<string>());
		}

		public virtual bool Validate(Mobile viewer, List<string> errors, bool pop = true)
		{
			if (Deleted)
			{
				errors.Add("This Team has been deleted.");
				return false;
			}

			if (Battle == null || Battle.Deleted)
			{
				errors.Add("This Team is unlinked.");
				return false;
			}

			if (String.IsNullOrWhiteSpace(Name))
			{
				errors.Add("Select a valid Name.");
				errors.Add("[Options] -> [Edit Options]");

				if (pop)
				{
					return false;
				}
			}

			if (HomeBase == Point3D.Zero)
			{
				errors.Add("Select a valid Home Base.");
				errors.Add("[Options] -> [Edit Options]");

				if (pop)
				{
					return false;
				}
			}
			else if (Battle.BattleRegion != null && !Battle.BattleRegion.Contains(HomeBase, Map))
			{
				errors.Add("Home Base must be within the Battle Region.");
				errors.Add("[Options] -> [Edit Options]");

				if (pop)
				{
					return false;
				}
			}

			if (SpawnPoint == Point3D.Zero)
			{
				errors.Add("Select a valid Spawn Point.");
				errors.Add("[Options] -> [Edit Options]");

				if (pop)
				{
					return false;
				}
			}
			else if (Battle.BattleRegion != null && !Battle.BattleRegion.Contains(SpawnPoint, Map))
			{
				errors.Add("Spawn Point must be within the Battle Region.");
				errors.Add("[Options] -> [Edit Options]");

				if (pop)
				{
					return false;
				}
			}

			return errors.Count <= 0;
		}

		public virtual bool IsReady()
		{
			return Count >= MinCapacity || Battle.IgnoreCapacity;
		}

		public virtual void Reset()
		{
			if (Dead != null)
			{
				Dead.Clear();
			}
			else
			{
				Dead = new Dictionary<PlayerMobile, DateTime>();
			}

			if (Idlers != null)
			{
				Idlers.Clear();
			}
			else
			{
				Idlers = new List<PlayerMobile>();
			}

			if (Members != null)
			{
				ForEachMember(pm => RemoveMember(pm, true));
				Members.Clear();
			}
			else
			{
				Members = new Dictionary<PlayerMobile, DateTime>();
			}
		}

		public virtual void UpdateActivity(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			Idlers.Remove(pm);

			if (IsMember(pm))
			{
				Members[pm] = DateTime.UtcNow;
			}
		}

		public virtual void AddMember(PlayerMobile pm, bool teleport)
		{
			if (IsMember(pm))
			{
				return;
			}

			Members.Add(pm, DateTime.UtcNow);

			if (teleport)
			{
				Battle.TeleportToHomeBase(this, pm);

				if (Battle.State == PvPBattleState.Running)
				{
					Timer.DelayCall(RespawnDelay, () => Battle.TeleportToSpawnPoint(this, pm));
				}
			}

			OnMemberAdded(pm);
		}

		public virtual void RemoveMember(PlayerMobile pm, bool teleport)
		{
			if (!IsMember(pm))
			{
				return;
			}

			Battle.TransferStatistics(pm);

			Dead.Remove(pm);
			Idlers.Remove(pm);

			Members.Remove(pm);
			OnMemberRemoved(pm);

			if (teleport)
			{
				Battle.TeleportToSpectateLocation(pm);
			}
		}

		public bool IsMember(PlayerMobile pm)
		{
			return Members != null && Members.ContainsKey(pm);
		}

		public bool IsDead(PlayerMobile pm)
		{
			return Dead != null && Dead.ContainsKey(pm);
		}

		public bool IsIdle(PlayerMobile pm)
		{
			return Idlers != null && Idlers.Contains(pm);
		}

		public virtual void OnMemberResurrected(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			Battle.OnTeamMemberResurrected(this, pm);
			Battle.OnAfterTeamMemberResurrected(this, pm);
		}

		public virtual void OnMemberDeath(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted)
			{
				return;
			}

			if (!IsDead(pm))
			{
				Dead.Add(pm, DateTime.UtcNow);
			}
			else
			{
				Dead[pm] = DateTime.UtcNow;
			}

			Battle.OnTeamMemberDeath(this, pm);
			Battle.OnAfterTeamMemberDeath(this, pm);
		}

		public virtual void OnMemberAdded(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				Battle.OnTeamMemberAdded(this, pm);
			}
		}

		public virtual void OnMemberRemoved(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted)
			{
				Battle.OnTeamMemberRemoved(this, pm);
			}
		}

		public virtual void PlaySound(int soundID)
		{
			ForEachMember(pm => PlaySound(pm, soundID));
		}

		public virtual void PlaySound(PlayerMobile pm, int soundID)
		{
			Battle.PlaySound(pm, soundID);
		}

		public virtual void SendSound(int soundID)
		{
			ForEachMember(pm => SendSound(pm, soundID));
		}

		public virtual void SendSound(PlayerMobile pm, int soundID)
		{
			Battle.SendSound(pm, soundID);
		}

		public virtual void Broadcast(string message, params object[] args)
		{
			ForEachMember(pm => pm.SendMessage(Battle.Options.Broadcasts.Local.MessageHue, message, args));
		}

		public long GetTotalKills()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).Kills) : 0;
		}

		public long GetTotalDeaths()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).Deaths) : 0;
		}

		public long GetTotalResurrections()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).Resurrections) : 0;
		}

		public long GetTotalDamageTaken()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).DamageTaken) : 0;
		}

		public long GetTotalDamageDone()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).DamageDone) : 0;
		}

		public long GetTotalHealingTaken()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).HealingTaken) : 0;
		}

		public long GetTotalHealingDone()
		{
			return Members != null ? Members.Keys.Sum(pm => Battle.EnsureStatistics(pm).HealingDone) : 0;
		}

		public void Delete()
		{
			if (Deleted)
			{
				return;
			}

			OnDeleted();

			Reset();

			if (Gate != null)
			{
				Gate.Delete();
				Gate = null;
			}

			MaxCapacity = 0;
			MaxCapacity = 0;
			Color = 0;
			Name = null;
			Members = null;
			Dead = null;
			Idlers = null;

			Battle.RemoveTeam(this);
			Battle = null;

			Deleted = true;
		}

		public void Init()
		{
			if (Initialized)
			{
				return;
			}

			OnInit();
			Battle.OnTeamInit(this);

			Initialized = true;
		}

		protected virtual void OnInit()
		{ }

		protected virtual void OnDeleted()
		{ }

		public virtual void OnBattleOpened(DateTime when)
		{ }

		public virtual void OnBattlePreparing(DateTime when)
		{ }

		public virtual void OnBattleStarted(DateTime when)
		{ }

		public virtual void OnBattleEnded(DateTime when)
		{ }

		public virtual void OnBattleCancelled(DateTime when)
		{ }

		public virtual void OnFrozen()
		{ }

		public virtual void OnFrozen(PlayerMobile pm)
		{ }

		public virtual void OnUnfrozen()
		{ }

		public virtual void OnUnfrozen(PlayerMobile pm)
		{ }

		public void Freeze()
		{
			ForEachMember(FreezeMember);
			OnFrozen();
			Battle.OnTeamFrozen(this);
		}

		public void Unfreeze()
		{
			ForEachMember(UnfreezeMember);
			OnUnfrozen();
			Battle.OnTeamUnfrozen(this);
		}

		public void FreezeMember(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted || pm.Frozen || !IsMember(pm))
			{
				return;
			}

			pm.Frozen = true;
			OnFrozen(pm);
			Battle.OnTeamMemberFrozen(this, pm);
		}

		public void UnfreezeMember(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted || !pm.Frozen || !IsMember(pm))
			{
				return;
			}

			pm.Frozen = false;
			OnUnfrozen(pm);
			Battle.OnTeamMemberUnfrozen(this, pm);
		}

		public void Sync()
		{
			OnSync();
			Battle.OnTeamSync(this);
		}

		protected virtual void OnSync()
		{ }

		public void MicroSync()
		{
			if (Battle.State == PvPBattleState.Running)
			{
				DateTime now = DateTime.UtcNow;

				foreach (var kvp in Members)
				{
					if (IsDead(kvp.Key))
					{
						Idlers.Remove(kvp.Key);
						continue;
					}

					if (kvp.Value + Battle.IdleThreshold < now && !IsIdle(kvp.Key))
					{
						Idlers.Add(kvp.Key);
					}
				}

				if (Battle.IdleKick)
				{
					Idlers.Not(IsDead).ForEach(pm => Battle.Eject(pm, true));
				}
			}

			OnMicroSync();

			Battle.OnTeamMicroSync(this);
		}

		protected virtual void OnMicroSync()
		{ }

		public virtual void InvalidateGate()
		{
			if (Battle.State == PvPBattleState.Internal || !Battle.QueueAllowed || GateLocation.Internal || GateLocation.Zero)
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
				Gate = new PvPTeamGate(this);

				if (GateLocation.MoveToWorld(Gate))
				{
					Gate.MoveToWorld(GateLocation, GateLocation);
				}
			}

			if (Gate.Team == null)
			{
				Gate.Team = this;
			}
		}

		public virtual string ToHtmlString(Mobile viewer = null, bool big = true)
		{
			var html = new StringBuilder();

			if (big)
			{
				html.Append("<BIG>");
			}

			GetHtmlString(viewer, html);

			if (big)
			{
				html.Append("</BIG>");
			}

			return html.ToString();
		}

		public virtual void GetHtmlStatistics(Mobile viewer, StringBuilder html)
		{
			html.AppendLine("* Kills: {0:#,0}", GetTotalKills());
			html.AppendLine("* Deaths: {0:#,0}", GetTotalDeaths());
			html.AppendLine("* Resurrections: {0:#,0}", GetTotalResurrections());
			html.AppendLine("* Damage Taken: {0:#,0}", GetTotalDamageTaken());
			html.AppendLine("* Damage Done: {0:#,0}", GetTotalDamageDone());
			html.AppendLine("* Healing Taken: {0:#,0}", GetTotalHealingTaken());
			html.AppendLine("* Healing Done: {0:#,0}", GetTotalHealingDone());
		}

		public virtual void GetHtmlString(Mobile viewer, StringBuilder html)
		{
			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));

			if (Deleted)
			{
				html.AppendLine(
					"This Team no longer exists.".WrapUOHtmlTag("BIG")
												 .WrapUOHtmlColor(System.Drawing.Color.OrangeRed, SuperGump.DefaultHtmlColor));
				return;
			}

			html.AppendLine("Team: {0} {1}".WrapUOHtmlTag("BIG"), Name, IsFull ? "(Full)" : String.Empty);
			html.AppendLine();

			int curCap = Count, maxCap = MaxCapacity;

			html.AppendLine("{0:#,0} invites available of {1:#,0} max.", maxCap - curCap, maxCap);
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(System.Drawing.Color.YellowGreen, false));

			html.AppendLine("Team Statistic Totals".WrapUOHtmlTag("BIG"));
			html.AppendLine();

			GetHtmlStatistics(viewer, html);
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(System.Drawing.Color.Cyan, false));

			html.AppendLine("Members: {0:#,0} / {1:#,0}".WrapUOHtmlTag("BIG"), Count, maxCap);
			html.AppendLine();

			Members.OrderBy(kv => kv.Value)
				   .For(
					   (i, kv) =>
					   html.AppendLine(
						   "{0:#,0}: {1}".WrapUOHtmlColor(viewer.GetNotorietyColor(kv.Key), SuperGump.DefaultHtmlColor),
						   i + 1,
						   kv.Key.RawName));
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(6);

			if (version > 4)
			{
				writer.WriteBlock(
					w =>
					{
						if (version > 5)
						{
							Serial.Serialize(w);
						}
						else
						{
							w.WriteType(Serial, t => Serial.Serialize(w));
						}
					});
			}

			switch (version)
			{
				case 6:
				case 5:
				case 4:
				case 3:
					writer.Write(RespawnOnStart);
					goto case 2;
				case 2:
					writer.Write(KickOnDeath);
					goto case 1;
				case 1:
					{
						GateLocation.Serialize(writer);
						writer.Write(Gate);
					}
					goto case 0;
				case 0:
					{
						writer.Write(Name);
						writer.Write(MinCapacity);
						writer.Write(MaxCapacity);
						writer.Write(Color);
						writer.Write(HomeBase);
						writer.Write(SpawnPoint);
						writer.Write(RespawnOnDeath);
						writer.Write(RespawnDelay);

						writer.WriteBlock(w => w.WriteType(Statistics));
					}
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			if (version > 4)
			{
				reader.ReadBlock(
					r =>
					{
						if (version > 5)
						{
							Serial = new PvPSerial(r);
						}
						else
						{
							Serial = r.ReadTypeCreate<PvPSerial>(r) ?? new PvPSerial(r);
							Serial = new PvPSerial();
						}
					});
			}
			else
			{
				Serial = new PvPSerial();
			}

			switch (version)
			{
				case 6:
				case 5:
				case 4:
				case 3:
					RespawnOnStart = reader.ReadBool();
					goto case 2;
				case 2:
					KickOnDeath = reader.ReadBool();
					goto case 1;
				case 1:
					{
						GateLocation = new MapPoint(reader);
						Gate = reader.ReadItem<PvPTeamGate>();
					}
					goto case 0;
				case 0:
					{
						Name = reader.ReadString();
						MinCapacity = reader.ReadInt();
						MaxCapacity = reader.ReadInt();
						Color = reader.ReadInt();
						HomeBase = reader.ReadPoint3D();
						SpawnPoint = reader.ReadPoint3D();

						RespawnOnDeath = reader.ReadBool();
						RespawnDelay = reader.ReadTimeSpan();

						reader.ReadBlock(r => Statistics = r.ReadTypeCreate<PvPTeamStatistics>(this) ?? new PvPTeamStatistics(this));
					}
					break;
			}

			if (version < 4)
			{
				RespawnOnStart = true;
			}

			if (version < 1)
			{
				GateLocation = MapPoint.Empty;
			}

			if (Gate != null)
			{
				Gate.Team = this;
			}

			if (Battle == null)
			{
				Delete();
			}
		}
	}
}