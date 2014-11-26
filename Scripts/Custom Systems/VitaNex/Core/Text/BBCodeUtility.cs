#region Header
//   Vorspire    _,-'/-'/  BBCodeUtility.cs
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
using System.Text.RegularExpressions;
#endregion

namespace VitaNex.Text
{
	public static class BBCodeUtility
	{
		/*
		(Description)		(Syntax)						(Args)				(Examples)
		Line Break:			[br]												[br]
		URL:				[url] link [/url]									[url]http://www.google.com[/url]
		URL Labeled:		[url=arg] text [/url]			hyperlink			[url=http://www.google.com]Google Search[/url]
		Center Align:		[center] text [/center]								[center]Hello World[/center]
		Left Align:			[left] text [/left]									[left]Hello World[/left]
		Right Align:		[right] text [/right]								[right]Hello World[/right]
		Small Font:			[small] text [/small]								[small]Hello World[/small]
		Big Font:			[big] text [/big]									[big]Hello World[/big]
		Bold:				[b] text [/b]										[b]Hello World[/b]
		Italic:				[i] text [/i]										[i]Hello World[/i]
		Underline:			[u] text [/u]										[u]Hello World[/u]
		Strikeout:			[s] text [/s]										[s]Hello World[/s]
		Font Size:			[size=arg] text [/size]			int 1 - 4			[size=4]Hello World[/color]
		Font Color:			[color=arg] text [/color]		hex	x6				[color=#FFFFFF]Hello World[/color]
		**********:			*************************		named color			[color=white]Hello World[/color]
		*/

		public static RegexOptions DefaultRegexOptions = RegexOptions.IgnoreCase | RegexOptions.Singleline;

		public static readonly Regex RegexLineBreak = new Regex(@"\[br\]", DefaultRegexOptions),
									 RegexCenterText = new Regex(@"\[center\](.+?)\[\/center\]", DefaultRegexOptions),
									 RegexLeftText = new Regex(@"\[left\](.+?)\[\/left\]", DefaultRegexOptions),
									 RegexRightText = new Regex(@"\[right\](.+?)\[\/right\]", DefaultRegexOptions),
									 RegexSmallText = new Regex(@"\[small\](.+?)\[\/small\]", DefaultRegexOptions),
									 RegexBigText = new Regex(@"\[big\](.+?)\[\/big\]", DefaultRegexOptions),
									 RegexBoldText = new Regex(@"\[b\](.+?)\[\/b\]", DefaultRegexOptions),
									 RegexItalicText = new Regex(@"\[i\](.+?)\[\/i\]", DefaultRegexOptions),
									 RegexUnderlineText = new Regex(@"\[u\](.+?)\[\/u\]", DefaultRegexOptions),
									 RegexStrikeOutText = new Regex(@"\[s\](.+?)\[\/s\]", DefaultRegexOptions),
									 RegexUrl = new Regex(@"\[url\](.+?)\[\/url\]", DefaultRegexOptions),
									 RegexUrlAnchored = new Regex(@"\[url=([^\]]+)\]([^\]]+)\[\/url\]", DefaultRegexOptions),
									 RegexColorAnchored = new Regex(@"\[color=([^\]]+)\]([^\]]+)\[\/color\]", DefaultRegexOptions),
									 RegexSizeAnchored = new Regex(@"\[size=([^\]]+)\]([^\]]+)\[\/size\]", DefaultRegexOptions),
									 RegexImage = new Regex(@"\[img\](.+?)\[\/img\]", DefaultRegexOptions),
									 RegexImageAnchored = new Regex(@"\[img=([^\]]+)\]([^\]]+)\[\/img\]", DefaultRegexOptions),
									 RegexStripMisc = new Regex(@"\[([^\]]+)\]([^\]]+)\[\/[^\]]+\]", DefaultRegexOptions);

		public static string ParseBBCode(
			this string input, Color? defaultColor = null, int defaultSize = 2, bool imgAsLink = true, bool stripMisc = false)
		{
			if (String.IsNullOrWhiteSpace(input))
			{
				return input ?? String.Empty;
			}

			input = RegexLineBreak.Replace(input, "<BR>");
			input = RegexCenterText.Replace(input, "<CENTER>$1</CENTER>");
			input = RegexLeftText.Replace(input, "<LEFT>$1</LEFT>");
			input = RegexRightText.Replace(input, "<RIGHT>$1</RIGHT>");
			input = RegexSmallText.Replace(input, "<SMALL>$1</SMALL>");
			input = RegexBigText.Replace(input, "<BIG>$1</BIG>");
			input = RegexBoldText.Replace(input, "<B>$1</B>");
			input = RegexItalicText.Replace(input, "<I>$1</I>");
			input = RegexUnderlineText.Replace(input, "<U>$1</U>");
			input = RegexStrikeOutText.Replace(input, "<S>$1</S>");

			input = RegexUrl.Replace(input, "<A HREF=\"$1\">$1</A>");
			input = RegexUrlAnchored.Replace(input, "<A HREF=\"$1\">$2</A>");

			if (imgAsLink)
			{
				input = RegexImage.Replace(input, "<A HREF=\"$1\">$1</A>");
				input = RegexImageAnchored.Replace(input, "<A HREF=\"$1\">$2</A>");
			}

			input = RegexSizeAnchored.Replace(input, "<BASEFONT SIZE=$1>$2<BASEFONT SIZE=" + defaultSize + ">");

			if (defaultColor != null)
			{
				input = RegexColorAnchored.Replace(
					input, "<BASEFONT COLOR=$1>$2<BASEFONT COLOR=#" + defaultColor.Value.ToArgb().ToString("X") + ">");
			}
			else
			{
				input = RegexColorAnchored.Replace(input, "<BASEFONT COLOR=$1>$2");
			}

			if (stripMisc)
			{
				input = RegexStripMisc.Replace(input, "($1) $2");
			}

			return input;
		}
	}
}