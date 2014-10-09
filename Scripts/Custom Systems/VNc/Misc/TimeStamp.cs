#region Header
//   Vorspire    _,-'/-'/  TimeStamp.cs
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
#endregion

namespace VitaNex
{
	public struct TimeStamp : IComparable<TimeStamp>, IEquatable<TimeStamp>
	{
		public static readonly DateTime Origin = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified);

		public static TimeStamp Zero { get { return Origin; } }
		public static TimeStamp Now { get { return DateTime.Now; } }
		public static TimeStamp UtcNow { get { return DateTime.UtcNow; } }

		public double Stamp { get; private set; }

		public long Ticks { get { return Value.Ticks; } set { Value = new DateTime(value); } }

		public DateTime Value { get { return Origin.AddSeconds(Stamp); } set { Stamp = (value - Origin).TotalSeconds; } }

		public DateTimeKind Kind { get; private set; }

		public TimeStamp(DateTimeKind kind)
			: this()
		{
			Kind = kind;
		}

		public TimeStamp(string stamp)
			: this(stamp, DateTimeKind.Unspecified)
		{ }

		public TimeStamp(string stamp, DateTimeKind kind)
			: this(Double.Parse(stamp), kind)
		{ }

		public TimeStamp(double stamp)
			: this(stamp, DateTimeKind.Unspecified)
		{ }

		public TimeStamp(double stamp, DateTimeKind kind)
			: this(kind)
		{
			Stamp = stamp;
		}

		public TimeStamp(long ticks)
			: this(ticks, DateTimeKind.Unspecified)
		{ }

		public TimeStamp(long ticks, DateTimeKind kind)
			: this(kind)
		{
			Value = new DateTime(ticks, kind);
		}

		public TimeStamp(DateTime date)
			: this(date.Kind)
		{
			Value = date;
		}

		public TimeStamp(GenericReader reader)
			: this(Origin)
		{
			Deserialize(reader);
		}

		public TimeStamp Add(TimeSpan ts)
		{
			return Stamp + ts.TotalSeconds;
		}

		public TimeStamp Subtract(TimeSpan ts)
		{
			return Stamp - ts.TotalSeconds;
		}

		public override int GetHashCode()
		{
			return Stamp.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (obj is TimeStamp)
			{
				return Equals((TimeStamp)obj);
			}

			return base.Equals(obj);
		}

		public bool Equals(TimeStamp t)
		{
			return Stamp.Equals(t.Stamp);
		}

		public int CompareTo(TimeStamp t)
		{
			return Stamp.CompareTo(t.Stamp);
		}

		public override string ToString()
		{
			return String.Format("{0:F2}", Stamp);
		}

		public void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 1:
					writer.WriteFlag(Kind);
					goto case 0;
				case 0:
					writer.Write(Stamp);
					break;
			}
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					Kind = reader.ReadFlag<DateTimeKind>();
					goto case 0;
				case 0:
					Stamp = reader.ReadDouble();
					break;
			}
		}

		public static bool operator !=(TimeStamp l, TimeStamp r)
		{
			return !(l == r);
		}

		public static bool operator ==(TimeStamp l, TimeStamp r)
		{
			return l.Stamp == r.Stamp;
		}

		public static bool operator >(TimeStamp l, TimeStamp r)
		{
			return l.Stamp > r.Stamp;
		}

		public static bool operator <(TimeStamp l, TimeStamp r)
		{
			return l.Stamp < r.Stamp;
		}

		public static bool operator >=(TimeStamp l, TimeStamp r)
		{
			return l.Stamp >= r.Stamp;
		}

		public static bool operator <=(TimeStamp l, TimeStamp r)
		{
			return l.Stamp <= r.Stamp;
		}

		public static implicit operator TimeStamp(double stamp)
		{
			return new TimeStamp(stamp);
		}

		public static implicit operator TimeStamp(long ticks)
		{
			return new TimeStamp(ticks);
		}

		public static implicit operator TimeStamp(TimeSpan time)
		{
			return new TimeStamp(time.Ticks);
		}

		public static implicit operator TimeStamp(DateTime date)
		{
			return new TimeStamp(date);
		}

		public static implicit operator double(TimeStamp stamp)
		{
			return stamp.Stamp;
		}
	}
}