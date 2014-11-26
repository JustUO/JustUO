#region Header
//   Vorspire    _,-'/-'/  ToolbarThemes.cs
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
#endregion

namespace VitaNex.Modules.Toolbar
{
	public enum ToolbarTheme
	{
		Default = 0,
		Paper,
		Stone
	}

	public static class ToolbarThemes
	{
		public static ToolbarThemeBase GetTheme(ToolbarTheme theme)
		{
			switch (theme)
			{
				case ToolbarTheme.Paper:
					return ToolbarThemePaper.Instance;
				case ToolbarTheme.Stone:
					return ToolbarThemeStone.Instance;
				default:
					return ToolbarThemeDefault.Instance;
			}
		}
	}

	public abstract class ToolbarThemeBase
	{
		public abstract ToolbarTheme ThemeID { get; }

		public abstract string Name { get; }

		public abstract Color TitleLabelColor { get; }
		public abstract int TitleBackground { get; }

		public abstract int EntrySeparator { get; }
		public abstract int EntryBackgroundN { get; }
		public abstract Color EntryLabelColorN { get; }
		public abstract int EntryBackgroundH { get; }
		public abstract Color EntryLabelColorH { get; }

		public abstract int EntryOptionsN { get; }
		public abstract int EntryOptionsP { get; }
	}

	public sealed class ToolbarThemeDefault : ToolbarThemeBase
	{
		private static readonly ToolbarThemeDefault _Instance = new ToolbarThemeDefault();

		public static ToolbarThemeDefault Instance { get { return _Instance; } }
		public override ToolbarTheme ThemeID { get { return ToolbarTheme.Default; } }
		public override string Name { get { return "Default"; } }

		public override int TitleBackground { get { return 9274; } }
		public override Color TitleLabelColor { get { return Color.Gold; } }
		public override int EntrySeparator { get { return 9790; } }
		public override int EntryBackgroundN { get { return 9274; } }
		public override Color EntryLabelColorN { get { return Color.Gold; } }
		public override int EntryBackgroundH { get { return 9204; } }
		public override Color EntryLabelColorH { get { return Color.LightBlue; } }
		public override int EntryOptionsN { get { return 9791; } }
		public override int EntryOptionsP { get { return 9790; } }
	}

	public sealed class ToolbarThemePaper : ToolbarThemeBase
	{
		private static readonly ToolbarThemePaper _Instance = new ToolbarThemePaper();

		public static ToolbarThemePaper Instance { get { return _Instance; } }
		public override ToolbarTheme ThemeID { get { return ToolbarTheme.Paper; } }
		public override string Name { get { return "Paper"; } }

		public override int TitleBackground { get { return 9394; } }
		public override Color TitleLabelColor { get { return Color.DarkSlateGray; } }
		public override int EntrySeparator { get { return 11340; } }
		public override int EntryBackgroundN { get { return 9394; } }
		public override Color EntryLabelColorN { get { return Color.DarkSlateGray; } }
		public override int EntryBackgroundH { get { return 9384; } }
		public override Color EntryLabelColorH { get { return Color.Chocolate; } }
		public override int EntryOptionsN { get { return 11350; } }
		public override int EntryOptionsP { get { return 11340; } }
	}

	public sealed class ToolbarThemeStone : ToolbarThemeBase
	{
		private static readonly ToolbarThemeStone _Instance = new ToolbarThemeStone();

		public static ToolbarThemeStone Instance { get { return _Instance; } }
		public override ToolbarTheme ThemeID { get { return ToolbarTheme.Stone; } }
		public override string Name { get { return "Stone"; } }

		public override int TitleBackground { get { return 5124; } }
		public override Color TitleLabelColor { get { return Color.GhostWhite; } }
		public override int EntrySeparator { get { return 11340; } }
		public override int EntryBackgroundN { get { return 5124; } }
		public override Color EntryLabelColorN { get { return Color.GhostWhite; } }
		public override int EntryBackgroundH { get { return 9204; } }
		public override Color EntryLabelColorH { get { return Color.Cyan; } }
		public override int EntryOptionsN { get { return 11374; } }
		public override int EntryOptionsP { get { return 11340; } }
	}
}