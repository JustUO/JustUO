#region Header
//   Vorspire    _,-'/-'/  Color555.cs
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
using System.Drawing;
#endregion

namespace VitaNex
{
	public struct Color555 : IEquatable<Color555>, IComparable<Color555>, IEquatable<ushort>, IComparable<ushort>
	{
		public static readonly Color555 MinValue = ushort.MinValue;
		public static readonly Color555 MaxValue = ushort.MaxValue;

		private readonly ushort _Value;

		public Color555(Color value)
		{
			//_Value = (ushort)((value.A >= 128 ? 0x8000 : 0x0000) | ((value.R & 0xF8) << 7) | ((value.G & 0xF8) << 2) | (value.B >> 3));
			uint c = (uint)value.ToArgb();
			_Value = (ushort)(((c >> 16) & 0x8000 | (c >> 9) & 0x7C00 | (c >> 6) & 0x03E0 | (c >> 3) & 0x1F));
		}

		public Color555(ushort value)
		{
			_Value = value;
		}

		public override int GetHashCode()
		{
			return _Value.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(obj, null) && obj is Color555
					   ? Equals((Color555)obj)
					   : obj is ushort ? Equals((ushort)obj) : base.Equals(obj);
		}

		public bool Equals(Color555 other)
		{
			return _Value.Equals(other._Value);
		}

		public bool Equals(ushort other)
		{
			return _Value.Equals(other);
		}

		public int CompareTo(Color555 other)
		{
			return _Value.CompareTo(other._Value);
		}

		public int CompareTo(ushort other)
		{
			return _Value.CompareTo(other);
		}

		public override string ToString()
		{
			return String.Format("{0}", _Value);
		}

		public string ToString(string format)
		{
			return String.Format(format, _Value);
		}

		public string ToString(IFormatProvider provider)
		{
			return String.Format(provider, "{0}", _Value);
		}

		public string ToString(string format, IFormatProvider provider)
		{
			return String.Format(provider, format, _Value);
		}

		public int ToArgb()
		{
			return ToColor().ToArgb();
		}

		public Color ToColor()
		{
			int a = _Value & 0x8000;
			int r = _Value & 0x7C00;
			int g = _Value & 0x03E0;
			int b = _Value & 0x1F;
			int rgb = (r << 9) | (g << 6) | (b << 3);

			return Color.FromArgb((a * 0x1FE00) | rgb | ((rgb >> 5) & 0x070707));
		}

		public static bool operator ==(Color555 l, Color555 r)
		{
			return l.Equals(r);
		}

		public static bool operator !=(Color555 l, Color555 r)
		{
			return !l.Equals(r);
		}

		public static implicit operator Color555(Color value)
		{
			return new Color555(value);
		}

		public static implicit operator Color555(ushort value)
		{
			return new Color555(value);
		}

		public static implicit operator ushort(Color555 value)
		{
			return value._Value;
		}
	}
}