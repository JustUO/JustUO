#region Header
//   Vorspire    _,-'/-'/  ProfileHistoryEntry.cs
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
using System.Text;

using Server;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPProfileHistoryEntry : PropertyObject, IEquatable<PvPProfileHistoryEntry>
	{
		private long _Battles;
		private long _DamageDone;
		private long _DamageTaken;
		private long _Deaths;
		private long _HealingDone;
		private long _HealingTaken;
		private long _Kills;
		private long _Losses;
		private long _PointsGained;
		private long _PointsLost;
		private long _Resurrections;
		private long _Wins;

		private Dictionary<string, long> _MiscStats = new Dictionary<string, long>();

		[CommandProperty(AutoPvP.Access)]
		public PvPSerial UID { get; private set; }

		[CommandProperty(AutoPvP.Access, true)]
		public int Season { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual long DamageTaken { get { return _DamageTaken; } set { _DamageTaken = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long DamageDone { get { return _DamageDone; } set { _DamageDone = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long HealingTaken { get { return _HealingTaken; } set { _HealingTaken = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long HealingDone { get { return _HealingDone; } set { _HealingDone = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Deaths { get { return _Deaths; } set { _Deaths = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Resurrections { get { return _Resurrections; } set { _Resurrections = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Kills { get { return _Kills; } set { _Kills = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long PointsGained { get { return _PointsGained; } set { _PointsGained = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long PointsLost { get { return _PointsLost; } set { _PointsLost = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Wins { get { return _Wins; } set { _Wins = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Losses { get { return _Losses; } set { _Losses = Math.Max(0, value); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Battles { get { return _Battles; } set { _Battles = Math.Max(0, value); } }

		public virtual Dictionary<string, long> MiscStats { get { return _MiscStats; } }

		public virtual long this[string stat]
		{
			get { return String.IsNullOrWhiteSpace(stat) || !_MiscStats.ContainsKey(stat) ? 0 : _MiscStats[stat]; }
			set
			{
				if (String.IsNullOrWhiteSpace(stat))
				{
					return;
				}

				if (_MiscStats.ContainsKey(stat))
				{
					if (value >= 0)
					{
						_MiscStats[stat] = value;
					}
					else
					{
						_MiscStats.Remove(stat);
					}
				}
				else if (value >= 0)
				{
					_MiscStats.Add(stat, value);
				}
			}
		}

		public PvPProfileHistoryEntry(int season)
		{
			UID = new PvPSerial(TimeStamp.UtcNow + "~" + Utility.RandomMinMax(0, Int32.MaxValue));
			Season = season;
		}

		public PvPProfileHistoryEntry(GenericReader reader)
		{
			Deserialize(reader);
		}

		public override void Clear()
		{
			_DamageTaken = 0;
			_DamageDone = 0;
			_HealingTaken = 0;
			_HealingDone = 0;
			_Kills = 0;
			_Deaths = 0;
			_Resurrections = 0;
			_PointsGained = 0;
			_PointsLost = 0;
			_Wins = 0;
			_Losses = 0;
			_Battles = 0;
			_MiscStats.Clear();
		}

		public override void Reset()
		{
			_DamageTaken = 0;
			_DamageDone = 0;
			_HealingTaken = 0;
			_HealingDone = 0;
			_Kills = 0;
			_Deaths = 0;
			_Resurrections = 0;
			_PointsGained = 0;
			_PointsLost = 0;
			_Wins = 0;
			_Losses = 0;
			_Battles = 0;
			_MiscStats.Clear();
		}

		public virtual long GetMiscStat(string stat)
		{
			return this[stat];
		}

		public virtual void SetMiscStat(string stat, long value)
		{
			this[stat] = value;
		}

		public string ToHtmlString(bool big)
		{
			return ToHtmlString(null, big);
		}

		public string ToHtmlString(Mobile viewer, bool big)
		{
			StringBuilder sb = new StringBuilder();

			if (big)
			{
				sb.Append("<BIG>");
			}

			GetHtmlString(viewer, sb);

			if (big)
			{
				sb.Append("</BIG>");
			}

			return sb.ToString();
		}

		public virtual void GetHtmlString(Mobile viewer, StringBuilder html)
		{
			html.Append("".WrapUOHtmlColor(Color.Cyan, false));
			html.AppendLine("<B>Statistics For Season: {0}</B>", Season.ToString("#,0"));
			html.AppendLine();

			html.AppendLine("<B>Statistics:</B>");
			html.AppendLine();

			html.AppendLine("* Battles Attended: {0}", _Battles.ToString("#,0"));
			html.AppendLine("* Battles Won: {0}", _Wins.ToString("#,0"));
			html.AppendLine("* Battles Lost: {0}", _Losses.ToString("#,0"));
			html.AppendLine("* Points Gained: {0}", _PointsGained.ToString("#,0"));
			html.AppendLine("* Points Lost: {0}", _PointsLost.ToString("#,0"));
			html.AppendLine("* Kills: {0}", _Kills.ToString("#,0"));
			html.AppendLine("* Deaths: {0}", _Deaths.ToString("#,0"));
			html.AppendLine("* Resurrections: {0}", _Resurrections.ToString("#,0"));
			html.AppendLine("* Damage Taken: {0}", _DamageTaken.ToString("#,0"));
			html.AppendLine("* Damage Done: {0}", _DamageDone.ToString("#,0"));
			html.AppendLine("* Healing Taken: {0}", _HealingTaken.ToString("#,0"));
			html.AppendLine("* Healing Done: {0}", _HealingDone.ToString("#,0"));
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(Color.GreenYellow, false));
			html.AppendLine("Misc Statistics");

			foreach (var kvp in _MiscStats)
			{
				html.AppendLine("* {0}: {1}", kvp.Key, kvp.Value.ToString("#,0"));
			}

			html.AppendLine();
			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));
		}

		public override bool Equals(object obj)
		{
			return obj is PvPProfileHistoryEntry ? Equals((PvPProfileHistoryEntry)obj) : base.Equals(obj);
		}

		public virtual bool Equals(PvPProfileHistoryEntry other)
		{
			return !ReferenceEquals(other, null) && UID.Equals(other.UID);
		}

		public override sealed int GetHashCode()
		{
			return UID.GetHashCode();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			UID.Serialize(writer);

			switch (version)
			{
				case 1:
				case 0:
					{
						writer.Write(Season);
						writer.Write(_DamageTaken);
						writer.Write(_DamageDone);
						writer.Write(_HealingTaken);
						writer.Write(_HealingDone);
						writer.Write(_Kills);
						writer.Write(_Deaths);
						writer.Write(_Resurrections);
						writer.Write(_PointsGained);
						writer.Write(_PointsLost);
						writer.Write(_Wins);
						writer.Write(_Losses);
						writer.Write(_Battles);

						writer.WriteBlockDictionary(
							_MiscStats,
							(w, k, v) =>
							{
								w.Write(k);
								w.Write(v);
							});
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			UID = version > 0
					  ? new PvPSerial(reader)
					  : new PvPSerial(TimeStamp.UtcNow + "~" + Utility.RandomMinMax(0, Int32.MaxValue));

			switch (version)
			{
				case 1:
				case 0:
					{
						Season = reader.ReadInt();
						_DamageTaken = reader.ReadLong();
						_DamageDone = reader.ReadLong();
						_HealingTaken = reader.ReadLong();
						_HealingDone = reader.ReadLong();
						_Kills = reader.ReadLong();
						_Deaths = reader.ReadLong();
						_Resurrections = reader.ReadLong();
						_PointsGained = reader.ReadLong();
						_PointsLost = reader.ReadLong();
						_Wins = reader.ReadLong();
						_Losses = reader.ReadLong();
						_Battles = reader.ReadLong();

						_MiscStats = reader.ReadBlockDictionary(
							r =>
							{
								string k = r.ReadString();
								long v = r.ReadLong();
								return new KeyValuePair<string, long>(k, v);
							});
					}
					break;
			}
		}

		public static bool operator ==(PvPProfileHistoryEntry left, PvPProfileHistoryEntry right)
		{
			return ReferenceEquals(left, null) ? ReferenceEquals(right, null) : left.Equals(right);
		}

		public static bool operator !=(PvPProfileHistoryEntry left, PvPProfileHistoryEntry right)
		{
			return ReferenceEquals(left, null) ? !ReferenceEquals(right, null) : !left.Equals(right);
		}
	}
}