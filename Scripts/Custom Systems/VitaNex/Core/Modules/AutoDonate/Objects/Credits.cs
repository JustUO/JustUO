#region Header
//   Vorspire    _,-'/-'/  Credits.cs
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
using System.Globalization;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public struct DonationCredits
	{
		public const long MinValue = Int64.MinValue, MaxValue = Int64.MaxValue;

		public long Value { get; private set; }

		public DonationCredits(long val)
			: this()
		{
			Value = val;
		}

		public override int GetHashCode()
		{
			return Value.GetHashCode();
		}

		public bool Equals(DonationCredits other)
		{
			return Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) && (obj is DonationCredits && Equals((DonationCredits)obj));
		}

		public override string ToString()
		{
			return Value.ToString(CultureInfo.InvariantCulture);
		}

		public string ToString(string format)
		{
			return String.IsNullOrWhiteSpace(format) ? ToString() : Value.ToString(format);
		}

		#region OPERATORS
		public static bool operator ==(DonationCredits a, DonationCredits b)
		{
			return a.Value == b.Value;
		}

		public static bool operator !=(DonationCredits a, DonationCredits b)
		{
			return a.Value != b.Value;
		}

		public static bool operator <(DonationCredits a, DonationCredits b)
		{
			return a.Value < b.Value;
		}

		public static bool operator >(DonationCredits a, DonationCredits b)
		{
			return a.Value > b.Value;
		}

		public static DonationCredits operator -(DonationCredits a, DonationCredits b)
		{
			return new DonationCredits(a.Value - b.Value);
		}

		public static DonationCredits operator +(DonationCredits a, DonationCredits b)
		{
			return new DonationCredits(a.Value + b.Value);
		}

		public static DonationCredits operator *(DonationCredits a, DonationCredits b)
		{
			return new DonationCredits(a.Value * b.Value);
		}

		public static DonationCredits operator /(DonationCredits a, DonationCredits b)
		{
			return new DonationCredits(a.Value / b.Value);
		}

		public static bool operator ==(DonationCredits a, long b)
		{
			return (a.Value == b);
		}

		public static bool operator !=(DonationCredits a, long b)
		{
			return (a.Value != b);
		}

		public static bool operator <(DonationCredits a, long b)
		{
			return (a.Value < b);
		}

		public static bool operator >(DonationCredits a, long b)
		{
			return (a.Value > b);
		}

		public static DonationCredits operator -(DonationCredits a, long b)
		{
			return new DonationCredits(a.Value - b);
		}

		public static DonationCredits operator +(DonationCredits a, long b)
		{
			return new DonationCredits(a.Value + b);
		}

		public static DonationCredits operator *(DonationCredits a, long b)
		{
			return new DonationCredits(a.Value * b);
		}

		public static DonationCredits operator /(DonationCredits a, long b)
		{
			return new DonationCredits(a.Value / b);
		}

		public static bool operator ==(long a, DonationCredits b)
		{
			return (a == b.Value);
		}

		public static bool operator !=(long a, DonationCredits b)
		{
			return (a != b.Value);
		}

		public static bool operator <(long a, DonationCredits b)
		{
			return (a < b.Value);
		}

		public static bool operator >(long a, DonationCredits b)
		{
			return (a > b.Value);
		}

		public static long operator -(long a, DonationCredits b)
		{
			return a - b.Value;
		}

		public static long operator +(long a, DonationCredits b)
		{
			return a + b.Value;
		}

		public static long operator *(long a, DonationCredits b)
		{
			return a * b.Value;
		}

		public static long operator /(long a, DonationCredits b)
		{
			return a / b.Value;
		}

		public static implicit operator DonationCredits(long a)
		{
			return new DonationCredits(a);
		}

		public static implicit operator long(DonationCredits a)
		{
			return a.Value;
		}
		#endregion OPERATORS
	}
}