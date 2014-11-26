#region Header
//   Vorspire    _,-'/-'/  Profile.cs
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
using System.Drawing;
using System.Linq;
using System.Text;

using Server;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	[PropertyObject]
	public class PvPProfile : IEnumerable<PvPProfileHistoryEntry>
	{
		private PvPProfileHistory _History;
		private long _Points;

		[CommandProperty(AutoPvP.Access)]
		public bool Deleted { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public PlayerMobile Owner { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual long Points
		{
			get { return _Points; }
			set
			{
				if (value < 0)
				{
					value = 0;
				}

				if (value != _Points)
				{
					long oldVal = _Points;
					_Points = value;
					OnPointsChanged(oldVal);
				}
			}
		}

		public List<PvPBattle> Subscriptions { get; private set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalDamageTaken { get { return GetTotalDamageTaken(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalDamageDone { get { return GetTotalDamageDone(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalHealingTaken { get { return GetTotalHealingTaken(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalHealingDone { get { return GetTotalHealingDone(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalDeaths { get { return GetTotalDeaths(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalResurrections { get { return GetTotalResurrections(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalKills { get { return GetTotalKills(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalPointsGained { get { return GetTotalPointsGained(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalPointsLost { get { return GetTotalPointsLost(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalWins { get { return GetTotalWins(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalLosses { get { return GetTotalLosses(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual long TotalBattles { get { return GetTotalBattles(); } }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPProfileHistory History { get { return _History; } set { _History = value; } }

		[CommandProperty(AutoPvP.Access)]
		public PvPProfileHistoryEntry Statistics { get { return _History.EnsureEntry(); } }

		public PvPProfile(PlayerMobile owner)
		{
			Owner = owner;

			_History = new PvPProfileHistory(this);
			Subscriptions = new List<PvPBattle>();

			SubscribeAllBattles();
		}

		public PvPProfile(GenericReader reader)
		{
			Deserialize(reader);
		}

		public void SubscribeAllBattles()
		{
			foreach (PvPBattle battle in AutoPvP.GetBattles().Where(battle => !IsSubscribed(battle)))
			{
				Subscribe(battle);
			}
		}

		public bool IsSubscribed(PvPBattle battle)
		{
			return (battle != null && !battle.Deleted && Subscriptions.Contains(battle));
		}

		public void Subscribe(PvPBattle battle)
		{
			if (battle == null || battle.Deleted || battle.State == PvPBattleState.Internal || IsSubscribed(battle))
			{
				return;
			}

			Subscriptions.Add(battle);
		}

		public void Unsubscribe(PvPBattle battle)
		{
			if (battle == null || battle.Deleted || !IsSubscribed(battle))
			{
				return;
			}

			Subscriptions.Remove(battle);
		}

		public virtual void OnPointsChanged(long oldVal)
		{
			if (_Points > oldVal)
			{
				OnPointsGained(_Points - oldVal);
			}
			else if (_Points < oldVal)
			{
				OnPointsLost(oldVal - _Points);
			}
		}

		public virtual void OnPointsGained(long value)
		{
			Owner.SendMessage("You have gained {0} PvP Points.", value.ToString("#,#"));
			Statistics.PointsGained += value;
		}

		public virtual void OnPointsLost(long value)
		{
			Owner.SendMessage("You have lost {0} PvP Points.", value.ToString("#,#"));
			Statistics.PointsLost += value;
		}

		public int GetRank(PvPSeason season = null)
		{
			return AutoPvP.GetProfileRank(Owner, AutoPvP.CMOptions.Advanced.Profiles.RankingOrder, season);
		}

		public long GetTotalDamageTaken()
		{
			return _History.Values.Sum(entry => entry.DamageTaken);
		}

		public long GetTotalDamageDone()
		{
			return _History.Values.Sum(entry => entry.DamageDone);
		}

		public long GetTotalHealingTaken()
		{
			return _History.Values.Sum(entry => entry.HealingTaken);
		}

		public long GetTotalHealingDone()
		{
			return _History.Values.Sum(entry => entry.HealingDone);
		}

		public long GetTotalResurrections()
		{
			return _History.Values.Sum(entry => entry.Resurrections);
		}

		public long GetTotalDeaths()
		{
			return _History.Values.Sum(entry => entry.Deaths);
		}

		public long GetTotalKills()
		{
			return _History.Values.Sum(entry => entry.Kills);
		}

		public long GetTotalPointsGained()
		{
			return _History.Values.Sum(entry => entry.PointsGained);
		}

		public long GetTotalPointsLost()
		{
			return _History.Values.Sum(entry => entry.PointsLost);
		}

		public long GetTotalWins()
		{
			return _History.Values.Sum(entry => entry.Wins);
		}

		public long GetTotalLosses()
		{
			return _History.Values.Sum(entry => entry.Losses);
		}

		public long GetTotalBattles()
		{
			return _History.Values.Sum(entry => entry.Battles);
		}

		public Dictionary<string, long> GetMiscStatisticTotals()
		{
			var stats = new Dictionary<string, long>();

			foreach (var kvp in _History.Values.SelectMany(entry => entry.MiscStats))
			{
				if (stats.ContainsKey(kvp.Key))
				{
					stats[kvp.Key] += kvp.Value;
				}
				else
				{
					stats.Add(kvp.Key, kvp.Value);
				}
			}

			return stats;
		}

		public void Remove()
		{
			AutoPvP.RemoveProfile(this);
		}

		public void Delete()
		{
			Remove();
			_History = null;
			Deleted = true;
			OnDeleted();
		}

		public void OnDeleted()
		{ }

		public void OnRemoved()
		{ }

		public void Init()
		{ }

		public void Sync()
		{ }

		public string ToHtmlString(Mobile viewer = null, bool big = true)
		{
			StringBuilder html = new StringBuilder();

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

		public virtual void GetHtmlString(Mobile viewer, StringBuilder html)
		{
			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));

			if (Deleted)
			{
				html.Append("<B>This profile has been deleted.</B>");
				return;
			}

			html.AppendLine("<B>PvP Profile for {0}</B>", Owner.RawName);
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(Color.YellowGreen, false));
			html.AppendLine("<B>Statistics</B>");
			html.AppendLine();

			int oRank = GetRank(), sRank = GetRank(AutoPvP.CurrentSeason);

			html.AppendLine("* Season Rank: {0}", sRank > 0 ? sRank.ToString("#,0") : "Unranked");
			html.AppendLine("* Overall Rank: {0}", oRank > 0 ? oRank.ToString("#,0") : "Unranked");
			html.AppendLine("* Overall Points: {0}", _Points.ToString("#,0"));
			html.AppendLine();

			Statistics.GetHtmlString(viewer, html);

			html.Append("".WrapUOHtmlColor(Color.Cyan, false));
			html.AppendLine("<B>Statisctics For All Seasons:</B>");
			html.AppendLine();

			html.AppendLine("<B>Main Statistic Totals</B>");
			html.AppendLine();

			html.AppendLine("* Battles Attended: {0}", TotalBattles.ToString("#,0"));
			html.AppendLine("* Battles Won: {0}", TotalWins.ToString("#,0"));
			html.AppendLine("* Battles Lost: {0}", TotalLosses.ToString("#,0"));
			html.AppendLine("* Points Gained: {0}", TotalPointsGained.ToString("#,0"));
			html.AppendLine("* Points Lost: {0}", TotalPointsLost.ToString("#,0"));
			html.AppendLine("* Kills: {0}", TotalKills.ToString("#,0"));
			html.AppendLine("* Deaths: {0}", TotalDeaths.ToString("#,0"));
			html.AppendLine("* Resurrections: {0}", TotalResurrections.ToString("#,0"));
			html.AppendLine("* Damage Taken: {0}", TotalDamageTaken.ToString("#,0"));
			html.AppendLine("* Damage Done: {0}", TotalDamageDone.ToString("#,0"));
			html.AppendLine("* Healing Taken: {0}", TotalHealingTaken.ToString("#,0"));
			html.AppendLine("* Healing Done: {0}", TotalHealingDone.ToString("#,0"));
			html.AppendLine();

			html.Append("".WrapUOHtmlColor(Color.GreenYellow, false));
			html.AppendLine("<B>Misc Statistic Totals</B>");
			html.AppendLine();

			foreach (var kvp in GetMiscStatisticTotals())
			{
				html.AppendLine("* {0}: {1}", kvp.Key, kvp.Value.ToString("#,0"));
			}

			html.AppendLine();
			html.Append("".WrapUOHtmlColor(SuperGump.DefaultHtmlColor, false));
		}

		public IEnumerator<PvPProfileHistoryEntry> GetEnumerator()
		{
			return History.Values.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Deleted);
						writer.Write(Owner);
						writer.Write(_Points);

						writer.WriteBlock(w => w.WriteType(_History, t => _History.Serialize(w)));

						writer.WriteBlockList(Subscriptions, (w, b) => w.WriteType(b.Serial, t => b.Serial.Serialize(w)));
					}
					break;
			}
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Deleted = reader.ReadBool();
						Owner = reader.ReadMobile<PlayerMobile>();
						_Points = reader.ReadLong();

						reader.ReadBlock(r => _History = r.ReadTypeCreate<PvPProfileHistory>(this, r) ?? new PvPProfileHistory(this, r));

						Subscriptions = reader.ReadBlockList(
							r =>
							{
								PvPSerial serial = r.ReadTypeCreate<PvPSerial>(r) ?? new PvPSerial(r);
								return AutoPvP.FindBattleByID(serial);
							});
					}
					break;
			}
		}
	}
}