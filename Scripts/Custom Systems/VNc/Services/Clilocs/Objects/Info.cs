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
using System.Text.RegularExpressions;
#endregion

namespace VitaNex
{
	public sealed class ClilocInfo
	{
		public ClilocInfo(ClilocLNG lng, int index, string text)
		{
			Language = lng;
			Index = index;
			Text = text;
		}

		public ClilocLNG Language { get; private set; }
		public int Index { get; private set; }
		public string Text { get; private set; }

		public override string ToString()
		{
			return Text;
		}

		public string ToString(string args)
		{
			if (String.IsNullOrWhiteSpace(args))
			{
				return ToString();
			}

			return ToString(args.Contains("\t") ? Regex.Split(args, "\t") : new[] {args});
		}

		public string ToString(string[] args)
		{
			if (args == null || args.Length == 0)
			{
				return ToString();
			}

			string text = Text;
			Match match = Clilocs.VarPattern.Match(text);

			while (match.Success)
			{
				int i = Int32.Parse(match.Groups[2].Value);

				if (args.Length < i)
				{
					text = null;
					break;
				}

				text = text.Replace(match.Groups[1].Value, args[i - 1]);
				match = match.NextMatch();
			}

			return text != null ? text.Trim() : String.Empty;
		}
	}
}