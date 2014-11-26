namespace System
{
	public static class NumericExtUtility
	{
		private static string GetOrdinalSuffix(ulong value)
		{
			ulong ones = value % 10;
			int tens = (int)Math.Floor(value / 10.0) % 10;

			string suff;

			if (tens == 1)
			{
				suff = "th";
			}
			else
			{
				switch (ones)
				{
					case 1:
						suff = "st";
						break;
					case 2:
						suff = "nd";
						break;
					case 3:
						suff = "rd";
						break;
					default:
						suff = "th";
						break;
				}
			}

			return suff;
		}

		private static string GetOrdinalSuffix(long value)
		{
			return GetOrdinalSuffix((ulong)Math.Abs(value));
		}
		
		public static string ToOrdinalString(this sbyte value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this byte value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this short value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this ushort value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this int value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this uint value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this long value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}

		public static string ToOrdinalString(this ulong value, string format = "#,0")
		{
			return value.ToString(format) + GetOrdinalSuffix(value);
		}
	}
}