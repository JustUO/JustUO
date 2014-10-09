#region Header
//   Vorspire    _,-'/-'/  StringExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Drawing;
using System.Text;
using System.Text.RegularExpressions;

using Server;

using VitaNex;
using VitaNex.Text;
#endregion

namespace System
{
	public static class StringExtUtility
	{
		private static readonly Regex _SpaceWordsRegex = new Regex(@"((?<=\p{Ll})\p{Lu})|((?!\A)\p{Lu}(?>\p{Ll}))");
		private static readonly Graphics _Graphics = Graphics.FromImage(new Bitmap(1, 1));

		public static Size ComputeSize(
			this string str, SystemFont font, float emSize = 12, FontStyle style = FontStyle.Regular)
		{
			return ComputeSize(str ?? String.Empty, font.ToFont(emSize, style));
		}

		public static Size ComputeSize(this string str, Font font)
		{
			return _Graphics.MeasureString(str ?? String.Empty, font).ToSize();
		}

		public static int ComputeWidth(
			this string str, SystemFont font, float emSize = 12, FontStyle style = FontStyle.Regular)
		{
			return ComputeWidth(str ?? String.Empty, font.ToFont(emSize, style));
		}

		public static int ComputeWidth(this string str, Font font)
		{
			return (int)_Graphics.MeasureString(str ?? String.Empty, font).Width;
		}

		public static int ComputeHeight(
			this string str, SystemFont font, float emSize = 12, FontStyle style = FontStyle.Regular)
		{
			return ComputeHeight(str ?? String.Empty, font.ToFont(emSize, style));
		}

		public static int ComputeHeight(this string str, Font font)
		{
			return (int)_Graphics.MeasureString(str ?? String.Empty, font).Height;
		}

		public static Size ComputeSize(this string str, UOFont font)
		{
			return font.GetSize(str ?? String.Empty);
		}

		public static int ComputeWidth(this string str, UOFont font)
		{
			return font.GetWidth(str ?? String.Empty);
		}

		public static int ComputeHeight(this string str, UOFont font)
		{
			return font.GetHeight(str ?? String.Empty);
		}

		public static string ToLowerWords(this string str)
		{
			if (String.IsNullOrWhiteSpace(str))
			{
				return str ?? String.Empty;
			}

			var split = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

			split.For((i, s) => { split[i] = Char.ToLower(s[0]) + ((s.Length > 1) ? s.Substring(1) : String.Empty); });

			return String.Join(" ", split);
		}

		public static string ToUpperWords(this string str)
		{
			if (String.IsNullOrWhiteSpace(str))
			{
				return str ?? String.Empty;
			}

			var split = str.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

			split.For((i, s) => { split[i] = Char.ToUpper(s[0]) + ((s.Length > 1) ? s.Substring(1) : String.Empty); });

			return String.Join(" ", split);
		}

		public static string SpaceWords(this string str)
		{
			if (String.IsNullOrWhiteSpace(str))
			{
				return str ?? String.Empty;
			}

			str = str.Replace('_', ' ');
			str = String.Join(" ", str.Split(" ".ToCharArray(), StringSplitOptions.RemoveEmptyEntries));

			return _SpaceWordsRegex.Replace(str, " $0");
		}

		public static string WrapUOHtmlTag(this string str, string tag, params KeyValueString[] args)
		{
			if (String.IsNullOrWhiteSpace(tag))
			{
				return str ?? String.Empty;
			}

			tag = tag.ToLower();

			if (args == null || args.Length == 0)
			{
				return String.Format("<{0}>{1}</{0}>", tag, str ?? String.Empty);
			}

			var kvc = new string[args.GetLength(0)];

			for (int i = 0; i < kvc.Length; i++)
			{
				kvc[i] = String.Format("{0}={1}", args[i].Key, args[i].Value);
			}

			return String.Format("<{0} {1}>{2}</{0}>", tag, String.Join(" ", kvc), str ?? String.Empty);
		}

		public static string WrapUOHtmlColor(this string str, Color color, Color reset, bool close = true)
		{
			return close
					   ? String.Format(
						   "<basefont color=#{0:X6}>{2}<basefont color=#{1:X6}>", color.ToArgb(), reset.ToArgb(), str ?? String.Empty)
					   : String.Format("<basefont color=#{0:X6}>{1}", color.ToArgb(), str ?? String.Empty);
		}

		public static string WrapUOHtmlColor(this string str, Color color, bool close = true)
		{
			return WrapUOHtmlColor(str ?? String.Empty, color, Color.White, close);
		}

		public static string WrapUOHtmlColor(this string str, Color555 color, Color555 reset, bool close = true)
		{
			return WrapUOHtmlColor(str ?? String.Empty, color.ToColor(), reset.ToColor(), close);
		}

		public static string WrapUOHtmlColor(this string str, Color555 color, bool close = true)
		{
			return WrapUOHtmlColor(str ?? String.Empty, color.ToColor(), Color.White, close);
		}

		public static void AppendLine(this StringBuilder sb, string format, params object[] args)
		{
			sb.AppendLine(String.Format(format ?? String.Empty, args));
		}
	}

	[PropertyObject]
	public struct KeyValueString : IEquatable<KeyValuePair<string, string>>, IEquatable<KeyValueString>
	{
		public string Key { get; private set; }
		public string Value { get; set; }

		public KeyValueString(KeyValueString kvs)
			: this(kvs.Key, kvs.Value)
		{ }

		public KeyValueString(KeyValuePair<string, string> kvp)
			: this(kvp.Key, kvp.Value)
		{ }

		public KeyValueString(string key, string value)
			: this()
		{
			Key = key;
			Value = value;
		}

		public KeyValueString(GenericReader reader)
			: this()
		{
			Deserialize(reader);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return ((Key != null ? Key.GetHashCode() : 0) * 397) ^ (Value != null ? Value.GetHashCode() : 0);
			}
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) &&
				   ((obj is KeyValueString && Equals((KeyValueString)obj)) ||
					(obj is KeyValuePair<string, string> && Equals((KeyValuePair<string, string>)obj)));
		}

		public bool Equals(KeyValueString other)
		{
			return string.Equals(Key, other.Key) && string.Equals(Value, other.Value);
		}

		public bool Equals(KeyValuePair<string, string> other)
		{
			return String.Equals(Value, other.Value) && String.Equals(Key, other.Key);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.Write(Key);
			writer.Write(Value);
		}

		public void Deserialize(GenericReader reader)
		{
			reader.GetVersion();

			Key = reader.ReadString();
			Value = reader.ReadString();
		}

		public static bool operator ==(KeyValueString l, KeyValueString r)
		{
			return Equals(l, r);
		}

		public static bool operator !=(KeyValueString l, KeyValueString r)
		{
			return !Equals(l, r);
		}

		public static implicit operator KeyValuePair<string, string>(KeyValueString kv)
		{
			return new KeyValuePair<string, string>(kv.Key, kv.Value);
		}

		public static implicit operator KeyValueString(KeyValuePair<string, string> kv)
		{
			return new KeyValueString(kv.Key, kv.Value);
		}
	}
}