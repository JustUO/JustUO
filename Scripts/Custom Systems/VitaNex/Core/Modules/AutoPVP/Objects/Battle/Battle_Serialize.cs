#region Header
//   Vorspire    _,-'/-'/  Battle_Serialize.cs
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
using Server.Items;

using VitaNex.Crypto;
using VitaNex.Schedules;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	[PropertyObject]
	public sealed class PvPSerial : CryptoHashCode
	{
		public static CryptoHashType Algorithm = CryptoHashType.MD5;

		[CommandProperty(AutoPvP.Access)]
		public override string Seed { get { return base.Seed; } }

		[CommandProperty(AutoPvP.Access)]
		public override string Value { get { return base.Value.Replace("-", String.Empty); } }

		public PvPSerial()
			: this(TimeStamp.UtcNow.ToString() + '+' + Utility.RandomDouble())
		{ }

		public PvPSerial(string seed)
			: base(Algorithm, seed)
		{ }

		public PvPSerial(GenericReader reader)
			: base(reader)
		{ }
	}

	public abstract partial class PvPBattle
	{
		[CommandProperty(AutoPvP.Access, true)]
		public PvPSerial Serial { get; private set; }

		protected bool Deserialized { get; private set; }
		protected bool Deserializing { get; private set; }

		private PvPBattle(bool deserializing)
		{
			Deserialized = deserializing;

			EnsureConstructDefaults();
		}

		public PvPBattle(GenericReader reader)
			: this(true)
		{
			Deserializing = true;

			Deserialize(reader);

			Deserializing = false;
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(7);

			if (version > 5)
			{
				writer.WriteBlock(
					w =>
					{
						if (version > 6)
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
				case 7:
				case 6:
				case 5:
					writer.Write(Hidden);
					goto case 4;
				case 4:
					writer.Write(FloorItemDelete);
					goto case 3;
				case 3:
				case 2:
					writer.Write(Gate);
					goto case 1;
				case 1:
					{
						writer.Write(Category);
						writer.Write(Ranked);
						writer.Write(InviteWhileRunning);
					}
					goto case 0;
				case 0:
					{
						if (version < 6)
						{
							writer.WriteBlock(w => w.WriteType(Serial, t => Serial.Serialize(w)));
						}

						writer.Write(DebugMode);
						writer.WriteFlag(State);
						writer.Write(Name);
						writer.Write(Description);
						writer.Write(AutoAssign);
						writer.Write(UseTeamColors);
						writer.Write(IgnoreCapacity);
						writer.Write(SubCommandPrefix);
						writer.Write(QueueAllowed);
						writer.Write(SpectateAllowed);
						writer.Write(KillPoints);
						writer.Write(PointsBase);
						writer.Write(PointsRankFactor);
						writer.Write(IdleKick);
						writer.Write(IdleThreshold);
						writer.WriteFlag(LastState);
						writer.Write(LastStateChange);
						writer.Write(LightLevel);
						writer.Write(LogoutDelay);
						writer.WriteItemList(Doors, true);

						writer.WriteBlock(w => w.WriteType(Options, t => Options.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(Schedule, t => Schedule.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(BattleRegion, t => BattleRegion.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(SpectateRegion, t => SpectateRegion.Serialize(w)));

						writer.WriteBlockList(Teams, (w, team) => w.WriteType(team, t => team.Serialize(w)));
					}
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			if (version > 5)
			{
				reader.ReadBlock(
					r =>
					{
						if (version > 6)
						{
							Serial = new PvPSerial(r);
						}
						else
						{
							Serial = r.ReadTypeCreate<PvPSerial>(r) ?? new PvPSerial(r);
						}
					});
			}

			switch (version)
			{
				case 7:
				case 6:
				case 5:
					Hidden = reader.ReadBool();
					goto case 4;
				case 4:
					FloorItemDelete = reader.ReadBool();
					goto case 3;
				case 3:
				case 2:
					{
						Gate = reader.ReadItem<PvPSpectatorGate>();

						if (Gate != null)
						{
							Gate.Battle = this;
						}
					}
					goto case 1;
				case 1:
					{
						Category = reader.ReadString();
						Ranked = reader.ReadBool();
						InviteWhileRunning = reader.ReadBool();
					}
					goto case 0;
				case 0:
					{
						if (version < 6)
						{
							reader.ReadBlock(r => Serial = r.ReadTypeCreate<PvPSerial>(r) ?? new PvPSerial(r));
						}

						DebugMode = reader.ReadBool();
						State = reader.ReadFlag<PvPBattleState>();
						Name = reader.ReadString();
						Description = reader.ReadString();
						AutoAssign = reader.ReadBool();
						UseTeamColors = reader.ReadBool();
						IgnoreCapacity = reader.ReadBool();
						SubCommandPrefix = reader.ReadChar();
						QueueAllowed = reader.ReadBool();
						SpectateAllowed = reader.ReadBool();
						KillPoints = version < 3 ? (reader.ReadBool() ? 1 : 0) : reader.ReadInt();
						PointsBase = reader.ReadInt();
						PointsRankFactor = reader.ReadDouble();
						IdleKick = reader.ReadBool();
						IdleThreshold = reader.ReadTimeSpan();
						LastState = reader.ReadFlag<PvPBattleState>();
						LastStateChange = reader.ReadDateTime();
						LightLevel = reader.ReadInt();
						LogoutDelay = reader.ReadTimeSpan();

						Doors.AddRange(reader.ReadStrongItemList<BaseDoor>());

						reader.ReadBlock(r => Options = r.ReadTypeCreate<PvPBattleOptions>(r) ?? new PvPBattleOptions(r));

						if (Schedule != null && Schedule.Running)
						{
							Schedule.Stop();
						}

						reader.ReadBlock(r => Schedule = r.ReadTypeCreate<Schedule>(r) ?? new Schedule("Battle " + Serial.Value, false));
						reader.ReadBlock(r => BattleRegion = r.ReadTypeCreate<PvPBattleRegion>(this, r) ?? new PvPBattleRegion(this, r));
						reader.ReadBlock(
							r => SpectateRegion = r.ReadTypeCreate<PvPSpectateRegion>(this, r) ?? new PvPSpectateRegion(this, r));

						reader.ReadBlockList(r => r.ReadTypeCreate<PvPTeam>(this, r) ?? new PvPTeam(this, r), Teams);
					}
					break;
			}
		}
	}
}