#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] Serial.cs
// ************************************/
#endregion

#region References
using System;
using System.Globalization;
#endregion

namespace Server
{
	public struct Serial : IComparable, IComparable<Serial>, IEquatable<Serial>, IEquatable<Int32>
	{
		private readonly int m_Serial;

		public const int MobileMin = 0x00000000;
		public const int MobileMax = 0x3FFFFFFF;

		public const int ItemMin = 0x40000000;
		public const int ItemMax = 0x7FFFFFFE;
		
		public static readonly Serial MinusOne = new Serial(-1);
		public static readonly Serial Zero = new Serial(0);

		private static Serial m_LastMobile = MobileMin;
		private static Serial m_LastItem = ItemMin;

		public static Serial LastMobile { get { return m_LastMobile; } }
		public static Serial LastItem { get { return m_LastItem; } }

		public static Serial NewMobile
		{
			get
			{
				int lastMobile = m_LastMobile;

				while (m_LastMobile.m_Serial < MobileMax && ++lastMobile <= MobileMax)
				{
					if (!World.Items.ContainsKey(m_LastMobile = lastMobile))
					{
						break;
					}
				}

				return m_LastMobile;
			}
		}

		public static Serial NewItem
		{
			get
			{
				int lastItem = m_LastItem;

				while (m_LastItem.m_Serial < ItemMax && ++lastItem <= ItemMax)
				{
					if (!World.Items.ContainsKey(m_LastItem = lastItem))
					{
						break;
					}
				}

				return m_LastItem;
			}
		}

		private Serial(int serial)
		{
			m_Serial = serial;
		}

		public int Value { get { return m_Serial; } }

		public bool IsMobile { get { return m_Serial > MobileMin && m_Serial <= MobileMax; } }

		public bool IsItem { get { return m_Serial >= ItemMin && m_Serial <= ItemMax; } }

		public bool IsValid { get { return m_Serial > 0; } }

		public override int GetHashCode()
		{
			return m_Serial;
		}

		public int CompareTo(Serial other)
		{
			return m_Serial.CompareTo(other.m_Serial);
		}

		public int CompareTo(object other)
		{
			if (other is Serial)
			{
				return CompareTo((Serial)other);
			}
			
			if (other == null)
			{
				return -1;
			}

			throw new ArgumentException();
		}

		public bool Equals(Serial other)
		{
			return m_Serial == other.m_Serial;
		}

		public bool Equals(Int32 value)
		{
			return m_Serial == value;
		}

		public override bool Equals(object o)
		{
			return o is Serial && Equals((Serial)o);
		}

		public static bool operator ==(Serial l, Serial r)
		{
			return l.m_Serial == r.m_Serial;
		}

		public static bool operator !=(Serial l, Serial r)
		{
			return l.m_Serial != r.m_Serial;
		}

		public static bool operator >(Serial l, Serial r)
		{
			return l.m_Serial > r.m_Serial;
		}

		public static bool operator <(Serial l, Serial r)
		{
			return l.m_Serial < r.m_Serial;
		}

		public static bool operator >=(Serial l, Serial r)
		{
			return l.m_Serial >= r.m_Serial;
		}

		public static bool operator <=(Serial l, Serial r)
		{
			return l.m_Serial <= r.m_Serial;
		}

		public override string ToString()
		{
			return String.Format("0x{0:X8}", m_Serial);
		}

		public static implicit operator int(Serial serial)
		{
			return serial.m_Serial;
		}

		public static implicit operator Serial(int value)
		{
			return new Serial(value);
		}

		public static bool TryParse(string value, out Serial serial)
		{
			int val;

			if (Int32.TryParse(value, out val))
			{
				serial = val;
				return true;
			}

			serial = MinusOne;
			return false;
		}

		public static bool TryParse(string value, NumberStyles style, IFormatProvider format, out Serial serial)
		{
			int val;

			if (Int32.TryParse(value, style, format, out val))
			{
				serial = val;
				return true;
			}

			serial = MinusOne;
			return false;
		}

		public static Serial Parse(string value)
		{
			return new Serial(Int32.Parse(value));
		}

		public static Serial Parse(string value, NumberStyles style)
		{
			return new Serial(Int32.Parse(value, style));
		}
	}
}