#region Header
//   Vorspire    _,-'/-'/  UOFont.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
#endregion

namespace VitaNex.Text
{
	/// <summary>
	///     This is all guesswork. Maps the Ultima Online Font character dimensions.
	///     There are 11 Fonts in total, 4 to 10 don't have a practical application yet.
	///     0 to 3 are used by gumps, menus and chat, etc.
	///     Uncertain if client font setting can override them.
	/// </summary>
	public struct UOFont
	{
		/// <summary>
		///     Font 0: Big
		/// </summary>
		public static UOFont Font0 = new UOFont(
			0,
			new Dictionary<char, Size>
			{
				{' ', new Size(12, 12)},
				{'!', new Size(4, 12)},
				{'"', new Size(8, 8)},
				{'#', new Size(12, 12)},
				{'$', new Size(10, 12)},
				{'%', new Size(12, 12)},
				{'&', new Size(12, 12)},
				{'\'', new Size(4, 8)},
				{'(', new Size(8, 12)},
				{')', new Size(8, 12)},
				{'*', new Size(12, 12)},
				{'+', new Size(12, 12)},
				{',', new Size(4, 12)},
				{'-', new Size(8, 10)},
				{'.', new Size(4, 12)},
				{'/', new Size(12, 12)},
				{'0', new Size(12, 12)},
				{'1', new Size(8, 12)},
				{'2', new Size(12, 12)},
				{'3', new Size(12, 12)},
				{'4', new Size(12, 12)},
				{'5', new Size(12, 12)},
				{'6', new Size(12, 12)},
				{'7', new Size(12, 12)},
				{'8', new Size(12, 12)},
				{'9', new Size(12, 12)},
				{':', new Size(8, 12)},
				{';', new Size(8, 12)},
				{'<', new Size(10, 12)},
				{'=', new Size(8, 12)},
				{'>', new Size(10, 12)},
				{'?', new Size(10, 12)},
				{'@', new Size(12, 12)},
				{'A', new Size(12, 12)},
				{'B', new Size(12, 12)},
				{'C', new Size(12, 12)},
				{'D', new Size(12, 12)},
				{'E', new Size(12, 12)},
				{'F', new Size(12, 12)},
				{'G', new Size(12, 12)},
				{'H', new Size(12, 12)},
				{'I', new Size(8, 12)},
				{'J', new Size(12, 12)},
				{'K', new Size(12, 12)},
				{'L', new Size(12, 12)},
				{'M', new Size(14, 12)},
				{'N', new Size(12, 12)},
				{'O', new Size(12, 12)},
				{'P', new Size(12, 12)},
				{'Q', new Size(12, 12)},
				{'R', new Size(12, 12)},
				{'S', new Size(12, 12)},
				{'T', new Size(12, 12)},
				{'U', new Size(12, 12)},
				{'V', new Size(12, 12)},
				{'W', new Size(14, 12)},
				{'X', new Size(12, 12)},
				{'Y', new Size(12, 12)},
				{'Z', new Size(12, 12)},
				{'[', new Size(8, 12)},
				{'\\', new Size(12, 12)},
				{']', new Size(8, 12)},
				{'_', new Size(8, 12)},
				{'a', new Size(10, 10)},
				{'b', new Size(10, 10)},
				{'c', new Size(10, 10)},
				{'d', new Size(10, 10)},
				{'e', new Size(10, 10)},
				{'f', new Size(10, 10)},
				{'g', new Size(10, 12)},
				{'h', new Size(10, 10)},
				{'i', new Size(8, 10)},
				{'j', new Size(10, 12)},
				{'k', new Size(10, 10)},
				{'l', new Size(10, 10)},
				{'m', new Size(12, 10)},
				{'n', new Size(10, 10)},
				{'o', new Size(10, 10)},
				{'p', new Size(10, 12)},
				{'q', new Size(10, 12)},
				{'r', new Size(10, 10)},
				{'s', new Size(10, 10)},
				{'t', new Size(8, 10)},
				{'u', new Size(10, 10)},
				{'v', new Size(10, 10)},
				{'w', new Size(12, 10)},
				{'x', new Size(10, 10)},
				{'y', new Size(10, 12)},
				{'z', new Size(10, 10)},
			});

		/// <summary>
		///     Font 1: Small
		/// </summary>
		public static UOFont Font1 = new UOFont(
			1,
			new Dictionary<char, Size>
			{
				{' ', new Size(8, 8)},
				{'!', new Size(2, 8)},
				{'"', new Size(4, 8)},
				{'#', new Size(8, 8)},
				{'$', new Size(8, 8)},
				{'%', new Size(8, 8)},
				{'&', new Size(8, 8)},
				{'\'', new Size(2, 2)},
				{'(', new Size(2, 8)},
				{')', new Size(2, 8)},
				{'*', new Size(6, 8)},
				{'+', new Size(6, 8)},
				{',', new Size(2, 8)},
				{'-', new Size(2, 8)},
				{'.', new Size(2, 8)},
				{'/', new Size(8, 8)},
				{'0', new Size(8, 8)},
				{'1', new Size(4, 8)},
				{'2', new Size(8, 8)},
				{'3', new Size(8, 8)},
				{'4', new Size(8, 8)},
				{'5', new Size(8, 8)},
				{'6', new Size(8, 8)},
				{'7', new Size(8, 8)},
				{'8', new Size(8, 8)},
				{'9', new Size(8, 8)},
				{':', new Size(4, 8)},
				{';', new Size(4, 8)},
				{'<', new Size(8, 8)},
				{'=', new Size(4, 6)},
				{'>', new Size(8, 8)},
				{'?', new Size(8, 8)},
				{'@', new Size(8, 8)},
				{'A', new Size(8, 8)},
				{'B', new Size(8, 8)},
				{'C', new Size(8, 8)},
				{'D', new Size(8, 8)},
				{'E', new Size(8, 8)},
				{'F', new Size(8, 8)},
				{'G', new Size(8, 8)},
				{'H', new Size(8, 8)},
				{'I', new Size(4, 8)},
				{'J', new Size(8, 8)},
				{'K', new Size(8, 8)},
				{'L', new Size(8, 8)},
				{'M', new Size(10, 8)},
				{'N', new Size(8, 8)},
				{'O', new Size(8, 8)},
				{'P', new Size(8, 8)},
				{'Q', new Size(8, 8)},
				{'R', new Size(8, 8)},
				{'S', new Size(8, 8)},
				{'T', new Size(8, 8)},
				{'U', new Size(8, 8)},
				{'V', new Size(8, 8)},
				{'W', new Size(10, 8)},
				{'X', new Size(8, 8)},
				{'Y', new Size(8, 8)},
				{'Z', new Size(8, 8)},
				{'[', new Size(4, 8)},
				{'\\', new Size(8, 8)},
				{']', new Size(4, 8)},
				{'_', new Size(8, 8)},
				{'a', new Size(6, 8)},
				{'b', new Size(6, 8)},
				{'c', new Size(6, 8)},
				{'d', new Size(6, 8)},
				{'e', new Size(6, 8)},
				{'f', new Size(6, 8)},
				{'g', new Size(6, 10)},
				{'h', new Size(6, 8)},
				{'i', new Size(4, 8)},
				{'j', new Size(6, 10)},
				{'k', new Size(6, 8)},
				{'l', new Size(6, 8)},
				{'m', new Size(8, 10)},
				{'n', new Size(6, 8)},
				{'o', new Size(6, 8)},
				{'p', new Size(6, 10)},
				{'q', new Size(6, 10)},
				{'r', new Size(6, 8)},
				{'s', new Size(6, 8)},
				{'t', new Size(4, 8)},
				{'u', new Size(6, 8)},
				{'v', new Size(6, 8)},
				{'w', new Size(8, 10)},
				{'x', new Size(6, 8)},
				{'y', new Size(6, 10)},
				{'z', new Size(6, 8)},
			});

		/// <summary>
		///     Font 2: Roughly the same as Font 0
		/// </summary>
		public static UOFont Font2 = new UOFont(2, Font0.Chars);

		/// <summary>
		///     Font 3: Roughly the same as Font 0
		/// </summary>
		public static UOFont Font3 = new UOFont(3, Font0.Chars);

		public byte ID { get; private set; }
		public Dictionary<char, Size> Chars { get; private set; }

		public int MaxCharWidth { get; private set; }
		public int MaxCharHeight { get; private set; }

		public int CharSpacing { get; set; }
		public int LineSpacing { get; set; }

		public UOFont(byte id, IDictionary<char, Size> chars)
			: this()
		{
			ID = id;
			Chars = new Dictionary<char, Size>(chars);

			MaxCharWidth = Chars.Values.Max(s => s.Width);
			MaxCharHeight = Chars.Values.Max(s => s.Height);

			CharSpacing = 1;
			LineSpacing = 4;
		}

		public int GetWidth(string value)
		{
			return GetSize(value).Width;
		}

		public int GetHeight(string value)
		{
			return GetSize(value).Height;
		}

		public Size GetSize(string value)
		{
			var lines = value.Split('\n');

			if (lines.Length == 0)
			{
				lines = new[] {value};
			}

			Size s;
			int w = 0;
			int h = lines.Length * (MaxCharHeight + LineSpacing);

			foreach (string line in lines)
			{
				int lw = 0;

				foreach (char c in line)
				{
					if (c == '\t')
					{
						lw += (CharSpacing + Chars[' '].Width) * 4;
						continue;
					}

					if (!Chars.TryGetValue(c, out s))
					{
						lw += (CharSpacing + Chars[' '].Width);
						continue;
					}

					lw += (CharSpacing + s.Width);
				}

				if (lw > w)
				{
					w = lw;
				}
			}

			return new Size(w, h);
		}

		public override string ToString()
		{
			return String.Join(String.Empty, Chars.Keys);
		}
	}
}