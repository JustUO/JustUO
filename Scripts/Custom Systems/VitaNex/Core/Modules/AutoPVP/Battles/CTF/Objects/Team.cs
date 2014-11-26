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
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Server;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoPvP.Battles
{
	public class CTFTeam : PvPTeam
	{
		private static readonly TimeSpan _OneSecond = TimeSpan.FromSeconds(1.0);

		private CTFPodium _FlagPodium;
		private bool _SolidHueOverride = true;

		public Dictionary<PlayerMobile, int> Attackers { get; private set; }
		public Dictionary<PlayerMobile, int> Defenders { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public CTFBattle CTFBattle { get { return Battle as CTFBattle; } }

		[CommandProperty(AutoPvP.Access, true)]
		public CTFFlag Flag { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Caps { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool SolidHueOverride
		{
			get { return _SolidHueOverride; }
			set
			{
				_SolidHueOverride = value;
				InvalidateSolidHueOverride();
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual TimeSpan FlagRespawnDelay { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual CTFPodium FlagPodium
		{
			get { return _FlagPodium; }
			set
			{
				_FlagPodium = value;
				InvalidateFlagPodium();
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public override Point3D HomeBase
		{
			get { return base.HomeBase; }
			set
			{
				base.HomeBase = value;
				InvalidateFlagPodium();
			}
		}

		protected override void EnsureConstructDefaults()
		{
			base.EnsureConstructDefaults();

			Attackers = new Dictionary<PlayerMobile, int>();
			Defenders = new Dictionary<PlayerMobile, int>();
		}

		public CTFTeam(PvPBattle battle, string name = "Team", int minCapacity = 0, int maxCapacity = 1, int color = 12)
			: base(battle, name, minCapacity, maxCapacity, color)
		{
			Caps = 0;
			FlagRespawnDelay = TimeSpan.FromSeconds(10.0);
			RespawnOnDeath = true;
		}

		public CTFTeam(PvPBattle battle, GenericReader reader)
			: base(battle, reader)
		{ }

		protected override void OnMicroSync()
		{
			base.OnMicroSync();

			if (Flag == null || Flag.Deleted)
			{
				Flag = null;
				return;
			}

			if (Battle == null || Battle.Deleted || Battle.State == PvPBattleState.Internal || Battle.Hidden)
			{
				Flag.Delete();
				Flag = null;
				return;
			}

			Flag.InvalidateCarrier();

			if (Flag.IsAtPodium())
			{
				return;
			}

			Flag.UpdateDamageIncrease();
			Flag.CheckReset();
		}

		public override void Reset()
		{
			base.Reset();

			if (Flag != null)
			{
				Flag.Delete();
				Flag = null;
			}

			if (Attackers != null)
			{
				Attackers.Clear();
			}
			else
			{
				Attackers = new Dictionary<PlayerMobile, int>();
			}

			if (Defenders != null)
			{
				Defenders.Clear();
			}
			else
			{
				Defenders = new Dictionary<PlayerMobile, int>();
			}

			Caps = 0;
		}

		public virtual void InvalidateFlagPodium()
		{
			if (Deserializing || SpawnPoint == Point3D.Zero || Battle.Options.Locations.Map == null ||
				Battle.Options.Locations.Map == Map.Internal)
			{
				return;
			}

			if (FlagPodium == null || FlagPodium.Deleted)
			{
				FlagPodium = new CTFPodium(this);
			}
			else
			{
				FlagPodium.Hue = Color;
				FlagPodium.Name = Name;
			}

			FlagPodium.MoveToWorld(SpawnPoint, Battle.Options.Locations.Map);
		}

		public virtual void InvalidateSolidHueOverride()
		{
			if (!Deserializing)
			{
				ForEachMember(InvalidateSolidHueOverride);
			}
		}

		public virtual void InvalidateSolidHueOverride(PlayerMobile pm)
		{
			if (!Deserializing && pm != null && !pm.Deleted && IsMember(pm) && pm.Region == Battle.BattleRegion)
			{
				pm.SolidHueOverride = (Battle.State == PvPBattleState.Preparing || Battle.State == PvPBattleState.Running) &&
									  _SolidHueOverride
										  ? Color
										  : -1;
			}
		}

		public override void OnMemberAdded(PlayerMobile pm)
		{
			base.OnMemberAdded(pm);

			InvalidateSolidHueOverride(pm);
		}

		public override void OnMemberRemoved(PlayerMobile pm)
		{
			base.OnMemberRemoved(pm);

			if (Battle != null && !Battle.Deleted)
			{
				Battle.Teams.OfType<CTFTeam>()
					  .Where(t => t != this && t.Flag != null && !t.Flag.Deleted && t.Flag.Carrier == pm)
					  .ForEach(t => t.Flag.Carrier = null);
			}

			InvalidateSolidHueOverride(pm);
		}

		public override void OnBattlePreparing(DateTime when)
		{
			base.OnBattlePreparing(when);

			InvalidateSolidHueOverride();
		}

		public override void OnBattleStarted(DateTime when)
		{
			base.OnBattleStarted(when);

			if (Flag == null || Flag.Deleted)
			{
				RespawnFlag();
			}
			else
			{
				Flag.Reset();
			}

			InvalidateSolidHueOverride();
		}

		public override void OnBattleEnded(DateTime when)
		{
			base.OnBattleEnded(when);

			InvalidateSolidHueOverride();
		}

		public override void OnMemberDeath(PlayerMobile pm)
		{
			if (Battle != null && !Battle.Deleted)
			{
				Battle.Teams.OfType<CTFTeam>()
					  .Where(t => t != this && t.Flag != null && !t.Flag.Deleted && t.Flag.Carrier == pm)
					  .ForEach(t => t.Flag.Carrier = null);
			}

			InvalidateSolidHueOverride(pm);

			base.OnMemberDeath(pm);
		}

		public virtual void SpawnFlag()
		{
			if (Battle.State != PvPBattleState.Running)
			{
				if (Flag != null && !Flag.Deleted)
				{
					Flag.Delete();
				}

				Flag = null;
				return;
			}

			Flag = Flag != null && !Flag.Deleted ? Flag : new CTFFlag(this);
			Flag.Carrier = null;
			Flag.Reset();
		}

		public virtual void RespawnFlag()
		{
			Timer.DelayCall(FlagRespawnDelay, SpawnFlag);
		}

		public virtual void OnFlagDropped(PlayerMobile attacker, CTFTeam enemyTeam)
		{
			if (attacker == null || enemyTeam == null || Flag == null || Flag.Deleted || CTFBattle == null || CTFBattle.Deleted)
			{
				return;
			}

			Broadcast("[{0}]: {1} has dropped your flag!", enemyTeam.Name, attacker.RawName);
			CTFBattle.OnFlagDropped(Flag, attacker, enemyTeam);
		}

		public virtual void OnFlagCaptured(PlayerMobile attacker, CTFTeam enemyTeam)
		{
			if (attacker == null || enemyTeam == null || Flag == null || Flag.Deleted || CTFBattle == null || CTFBattle.Deleted)
			{
				return;
			}

			if (enemyTeam.Attackers.ContainsKey(attacker))
			{
				enemyTeam.Attackers[attacker]++;
			}
			else
			{
				enemyTeam.Attackers.Add(attacker, 1);
			}

			enemyTeam.Caps++;

			Broadcast("[{0}]: {1} has captured your flag!", enemyTeam.Name, attacker.RawName);
			CTFBattle.OnFlagCaptured(Flag, attacker, enemyTeam);

			RespawnFlag();
		}

		public virtual void OnFlagStolen(PlayerMobile attacker, CTFTeam enemyTeam)
		{
			if (attacker == null || enemyTeam == null || Flag == null || Flag.Deleted || CTFBattle == null || CTFBattle.Deleted)
			{
				return;
			}

			Broadcast("[{0}]: {1} has stolen your flag!", enemyTeam.Name, attacker.RawName);
			CTFBattle.OnFlagStolen(Flag, attacker, enemyTeam);
		}

		public virtual void OnFlagReturned(PlayerMobile defender)
		{
			if (defender == null || Flag == null || Flag.Deleted || CTFBattle == null || CTFBattle.Deleted)
			{
				return;
			}

			if (Defenders.ContainsKey(defender))
			{
				Defenders[defender]++;
			}
			else
			{
				Defenders.Add(defender, 1);
			}

			Broadcast("[{0}]: {1} has returned your flag!", Name, defender.RawName);
			CTFBattle.OnFlagReturned(Flag, defender);
		}

		protected override void OnDeleted()
		{
			if (Flag != null)
			{
				Flag.Delete();
				Flag = null;
			}

			if (FlagPodium != null)
			{
				FlagPodium.Delete();
				FlagPodium = null;
			}

			base.OnDeleted();
		}

		public override void GetHtmlStatistics(Mobile viewer, StringBuilder html)
		{
			base.GetHtmlStatistics(viewer, html);

			html.AppendLine("* Flags Captured: {0:#,0} / {1:#,0}", Caps, CTFBattle != null ? CTFBattle.CapsToWin : 0);
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(System.Drawing.Color.LawnGreen, false));
			html.AppendLine("Defenders: ");
			html.AppendLine();

			Defenders.OrderBy(kv => kv.Value)
					 .For(
						 (i, kv) =>
						 html.AppendLine(
							 "{0:#,0}: {1} ({2:#,0} Returns)".WrapUOHtmlColor(viewer.GetNotorietyColor(kv.Key), SuperGump.DefaultHtmlColor),
							 i + 1,
							 kv.Key.RawName,
							 kv.Value));
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(System.Drawing.Color.OrangeRed, false));
			html.AppendLine("Attackers: ");
			html.AppendLine();

			Attackers.OrderBy(kv => kv.Value)
					 .For(
						 (i, kv) =>
						 html.AppendLine(
							 "{0:#,0}: {1} ({2:#,0} Captures)".WrapUOHtmlColor(viewer.GetNotorietyColor(kv.Key), SuperGump.DefaultHtmlColor),
							 i + 1,
							 kv.Key.RawName,
							 kv.Value));
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Flag);
						writer.Write(FlagPodium);
						writer.Write(Caps);

						writer.Write(FlagRespawnDelay);
						writer.Write(SolidHueOverride);

						writer.WriteDictionary(
							Attackers,
							(m, c) =>
							{
								writer.Write(m);
								writer.Write(c);
							});

						writer.WriteDictionary(
							Defenders,
							(m, c) =>
							{
								writer.Write(m);
								writer.Write(c);
							});
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						Flag = reader.ReadItem<CTFFlag>();

						if (Flag != null)
						{
							Flag.Team = this;
						}

						FlagPodium = reader.ReadItem<CTFPodium>();

						if (FlagPodium != null)
						{
							FlagPodium.Team = this;
						}

						Caps = reader.ReadInt();

						FlagRespawnDelay = reader.ReadTimeSpan();
						SolidHueOverride = reader.ReadBool();

						reader.ReadDictionary(
							() => new KeyValuePair<PlayerMobile, int>(reader.ReadMobile<PlayerMobile>(), reader.ReadInt()), Attackers);

						reader.ReadDictionary(
							() => new KeyValuePair<PlayerMobile, int>(reader.ReadMobile<PlayerMobile>(), reader.ReadInt()), Defenders);
					}
					break;
			}
		}
	}
}