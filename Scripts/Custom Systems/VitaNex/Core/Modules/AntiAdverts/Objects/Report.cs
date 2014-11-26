#region Header
//   Vorspire    _,-'/-'/  Report.cs
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

using Server.Mobiles;
#endregion

namespace Server.Misc
{
	public sealed class AntiAdvertsReport : IEquatable<AntiAdvertsReport>
	{
		public DateTime Date { get; private set; }
		public PlayerMobile Mobile { get; private set; }

		public string Report { get; set; }
		public string Speech { get; set; }
		public bool Jailed { get; set; }
		public bool Banned { get; set; }
		public bool Viewed { get; set; }

		public AntiAdvertsReport(
			DateTime date, PlayerMobile m, string report, string speech, bool jailed, bool banned, bool viewed = false)
		{
			Date = date;
			Mobile = m;
			Speech = speech;
			Report = report;
			Viewed = viewed;
			Jailed = jailed;
			Banned = banned;
		}

		public AntiAdvertsReport(GenericReader reader)
		{
			Deserialize(reader);
		}

		public override string ToString()
		{
			return String.Format("[{0}] {1}: {2}", Date.ToSimpleString("t@h:m@ m-d"), Mobile.RawName, Report);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = (Mobile != null ? Mobile.Serial.Value : 0);
				hashCode = (hashCode * 397) ^ Date.GetHashCode();
				hashCode = (hashCode * 397) ^ (Speech != null ? Speech.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ (Report != null ? Report.GetHashCode() : 0);
				hashCode = (hashCode * 397) ^ Jailed.GetHashCode();
				hashCode = (hashCode * 397) ^ Banned.GetHashCode();
				return hashCode;
			}
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) && (obj is AntiAdvertsReport && Equals((AntiAdvertsReport)obj));
		}

		public bool Equals(AntiAdvertsReport other)
		{
			return !ReferenceEquals(other, null) && Equals(Mobile, other.Mobile) && Equals(Date, other.Date) &&
				   Equals(Jailed, other.Jailed) && Equals(Banned, other.Banned) && String.Equals(Speech, other.Speech) &&
				   String.Equals(Report, other.Report);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.Write(Date);
			writer.Write(Mobile);
			writer.Write(Speech);
			writer.Write(Report);
			writer.Write(Viewed);
			writer.Write(Jailed);
			writer.Write(Banned);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.GetVersion();

			Date = reader.ReadDateTime();
			Mobile = reader.ReadMobile<PlayerMobile>();
			Speech = reader.ReadString();
			Report = reader.ReadString();
			Viewed = reader.ReadBool();
			Jailed = reader.ReadBool();
			Banned = reader.ReadBool();
		}

		public static bool operator ==(AntiAdvertsReport left, AntiAdvertsReport right)
		{
			if (ReferenceEquals(left, null))
			{
				return ReferenceEquals(right, null);
			}

			return left.Equals(right);
		}

		public static bool operator !=(AntiAdvertsReport left, AntiAdvertsReport right)
		{
			if (ReferenceEquals(left, null))
			{
				return !ReferenceEquals(right, null);
			}

			return !left.Equals(right);
		}
	}
}