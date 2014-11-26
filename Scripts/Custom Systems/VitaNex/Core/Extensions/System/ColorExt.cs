#region Header
//   Vorspire    _,-'/-'/  ColorExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Drawing;

using VitaNex;
#endregion

namespace System
{
	public static class ColorExtUtility
	{
		public static Color ToColor(this KnownColor color)
		{
			return Color.FromKnownColor(color);
		}

		public static Color555 ToColor555(this Color value)
		{
			return value;
		}

		public static ushort ToArgb555(this Color value)
		{
			return ToColor555(value);
		}

		public static Color Interpolate(this Color source, Color target, double percent)
		{
			percent = Math.Max(0.0, Math.Min(1.0, percent));

			var r = (int)(source.R + (target.R - source.R) * percent);
			var g = (int)(source.G + (target.G - source.G) * percent);
			var b = (int)(source.B + (target.B - source.B) * percent);

			return Color.FromArgb(255, r, g, b);
		}
	}
}