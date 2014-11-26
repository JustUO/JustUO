#region Header
//   Vorspire    _,-'/-'/  Info.cs
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
using System.Linq;
#endregion

namespace VitaNex
{
	public sealed class ClilocInfo
	{
		public ClilocLNG Language { get; private set; }

		public int Index { get; private set; }
		public string Text { get; private set; }

		public int Count { get; private set; }
		public string Format { get; private set; }

		public bool HasArgs { get { return Count > 0 && !String.IsNullOrWhiteSpace(Format); } }

		public ClilocInfo(ClilocLNG lng, int index, string text)
		{
			Language = lng;
			Index = index;
			Text = text;

			InvalidateArgs();
		}

		public void InvalidateArgs()
		{
			if (String.IsNullOrWhiteSpace(Text))
			{
				Count = 0;
				Text = Format = String.Empty;
				return;
			}

			Format = Clilocs.VarPattern.Replace(Text, e => "{" + (Count++) + "}");

			/*
			Console.WriteLine("{0}: {1}", Index, Format);
			
			Count = Clilocs.VarPattern.Matches(Text).OfType<Match>().Count(m => m.Success);

			string[] format = new string[Count];

			format.SetAll(i => "{" + i + "}");

			Format = String.Join("\t", format);
			*/
		}

		public override string ToString()
		{
			return Text;
		}

		public string ToString(string args)
		{
			if (!HasArgs || String.IsNullOrWhiteSpace(args))
			{
				return ToString();
			}

			var buffer = new object[Count];
			var split = args.Split('\t');

			buffer.SetAll(i => (i < split.Length ? split[i] ?? String.Empty : String.Empty) + (Count > 1 ? "\t" : String.Empty));

			if (split.Length > buffer.Length)
			{
				buffer[buffer.Length - 1] += String.Join(String.Empty, split.Skip(buffer.Length).Take(split.Length - buffer.Length));
			}

			return String.Format(Format, buffer);
		}

		public string ToString(object[] args)
		{
			if (!HasArgs || args == null || args.Length == 0)
			{
				return ToString();
			}

			var buffer = new object[Count];

			buffer.SetAll(i => (i < args.Length ? args[i] ?? String.Empty : String.Empty) + (Count > 1 ? "\t" : String.Empty));

			if (args.Length > buffer.Length)
			{
				buffer[buffer.Length - 1] += String.Join(String.Empty, args.Skip(buffer.Length).Take(args.Length - buffer.Length));
			}

			return String.Format(Format, buffer);
		}
	}
}