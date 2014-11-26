#region Header
//   Vorspire    _,-'/-'/  LCD.cs
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

namespace VitaNex
{
	[Flags]
	public enum LCDLines : byte
	{
		None = 0x00,
		Top = 0x01,
		TopLeft = 0x02,
		TopRight = 0x04,
		Middle = 0x08,
		BottomLeft = 0x10,
		BottomRight = 0x20,
		Bottom = 0x40
	}

	public static class LCD
	{
		private static readonly LCDLines[] _NumericMatrix = new[]
		{
			LCDLines.Top | LCDLines.TopLeft | LCDLines.TopRight | LCDLines.BottomLeft | LCDLines.BottomRight | LCDLines.Bottom,
			LCDLines.TopLeft | LCDLines.BottomLeft,
			LCDLines.Top | LCDLines.TopRight | LCDLines.Middle | LCDLines.BottomLeft | LCDLines.Bottom,
			LCDLines.Top | LCDLines.TopRight | LCDLines.Middle | LCDLines.BottomRight | LCDLines.Bottom,
			LCDLines.TopLeft | LCDLines.TopRight | LCDLines.Middle | LCDLines.BottomRight,
			LCDLines.Top | LCDLines.TopLeft | LCDLines.Middle | LCDLines.BottomRight | LCDLines.Bottom,
			LCDLines.Top | LCDLines.TopLeft | LCDLines.Middle | LCDLines.BottomLeft | LCDLines.BottomRight | LCDLines.Bottom,
			LCDLines.Top | LCDLines.TopRight | LCDLines.BottomRight,
			LCDLines.Top | LCDLines.TopLeft | LCDLines.TopRight | LCDLines.Middle | LCDLines.BottomLeft | LCDLines.BottomRight |
			LCDLines.Bottom,
			LCDLines.Top | LCDLines.TopLeft | LCDLines.TopRight | LCDLines.Middle | LCDLines.BottomRight
		};

		public static LCDLines[] NumericMatrix { get { return _NumericMatrix; } }

		public static bool TryParse(int val, out LCDLines[] matrix)
		{
			string s = val.ToString(CultureInfo.InvariantCulture);
			matrix = new LCDLines[s.Length];

			bool success = false;

			for (int i = 0; i < s.Length; i++)
			{
				success = Int32.TryParse(s[i].ToString(CultureInfo.InvariantCulture), out val) && TryParse(val, out matrix[i]);

				if (success)
				{
					continue;
				}

				matrix = new LCDLines[0];
				break;
			}

			return success;
		}

		public static bool TryParse(int val, out LCDLines matrix)
		{
			if (val < 0 || val > 9)
			{
				matrix = LCDLines.None;
				return false;
			}

			matrix = _NumericMatrix[val];
			return true;
		}

		public static bool HasLines(int val, LCDLines lines)
		{
			if (val < 0 || val > 9)
			{
				return false;
			}

			LCDLines matrix;

			if (TryParse(val, out matrix))
			{
				return matrix.HasFlag(lines);
			}

			return false;
		}
	}
}