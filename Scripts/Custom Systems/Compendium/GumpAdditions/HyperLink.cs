#region References
using System;
using System.Linq;
using System.Reflection;

using VitaNex.SuperGumps;
#endregion

namespace Server.Gumps
{
	public static class GumpExtensions
	{
		public static void AddHtmlLabel(
			this Gump gump,
			int x,
			int y,
			FontHandling.FontSize size,
			bool bold,
			bool italicized,
			bool underlined,
			string webColor,
			string text)
		{
			gump.AddHtmlTextRectangle(
				x,
				y,
				size,
				bold,
				italicized,
				underlined,
				webColor,
				text,
				FontHandling.CalculateTextLengthInPixels(text, size, italicized, bold),
				FontHandling.FONT_LINE_HEIGHT);
		}

		public static void AddHtmlTextRectangle(
			this Gump gump,
			int x,
			int y,
			FontHandling.FontSize size,
			bool bold,
			bool italicized,
			bool underlined,
			string webColor,
			string text,
			int width,
			int height)
		{
			var displayMarkup = text;

			if (bold)
			{
				displayMarkup = string.Format("<B>{0}</B>", displayMarkup);
			}

			if (italicized)
			{
				displayMarkup = string.Format("<I>{0}</I>", displayMarkup);
			}

			if (underlined)
			{
				displayMarkup = string.Format("<U>{0}</U>", displayMarkup);
			}

			displayMarkup = string.Format("<BASEFONT COLOR={0} SIZE={1} >{2}</BASEFONT>", webColor, (int)size, displayMarkup);

			gump.AddHtml(x, y, width, height, displayMarkup, false, false);
		}

		private static MethodInfo m_AssignIdMethodInfo;

		private static MethodInfo AssignIdMethodInfo
		{
			get
			{
				if (m_AssignIdMethodInfo == null)
				{
					try
					{
						m_AssignIdMethodInfo = typeof(GumpEntry).GetMethod(
							"AssignID",
							BindingFlags.Static | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
					}
					catch
					{ }
				}

				return m_AssignIdMethodInfo;
			}
		}

		public static void AddGumpEntryFirst(this SuperGump gump, GumpEntry g)
		{
			if (g != null)
			{
				if (g.Parent == null)
				{
					g.Parent = gump;
				}

				gump.Entries.Remove(g);

				if (AssignIdMethodInfo != null)
				{
					AssignIdMethodInfo.Invoke(g, new object[] {});
				}

				gump.Entries.Insert(0, g);
				gump.Invalidate();
			}
		}

		public static void MoveLastAddedToBack(this SuperGump gump)
		{
			var btn = gump.Entries.Last();
			gump.Entries.Remove(btn);
			gump.Entries.Insert(0, btn);
		}

		public static void AddHyperlink(this SuperGump gump, HyperLink link)
		{
			link.AddHyperLinkBackingButton(gump);
			link.AddText(gump);
		}
	}

	public class HyperLink
	{
		public const int HYPERLINK_UNDERLINE_HEIGHT_IN_PIXELS = 1;
		public static readonly int[] HYPERLINK_BACK_GUMP_WIDTHS = {170, 109, 84, 68, 16, 9, 8, 6, 3};
		public static readonly int[] HYPERLINK_BACK_GUMP_IDS = {2487, 2481, 2467, 2463, 87, 5009, 5136, 5113, 9103};

		public virtual int HyperLinkUnderlineGumpId { get { return 2627; } }

		public Point2D Location { get; set; }
		public int LabelWidthInPixels { get; set; }
		public int Hue { get; set; }
		public bool Underlined { get; set; }
		public bool Italicized { get; set; }
		public bool Bold { get; set; }
		public Action<GumpButton, int> Callback { get; set; }
		public int CallbackParam { get; set; }
		public FontHandling.FontSize Size { get; set; }
		private string m_displayText;

		public string DisplayText
		{
			get { return m_displayText; }
			set
			{
				m_displayText = value;
				LabelWidthInPixels = FontHandling.CalculateTextLengthInPixels(DisplayText, Size, Italicized, Bold);
			}
		}

		public HyperLink(
			Point2D location,
			int hue,
			bool underlined,
			bool italicized,
			bool bold,
			Action<GumpButton, int> callback,
			int callbackParam,
			string text,
			FontHandling.FontSize size = FontHandling.FontSize.Medium)
		{
			Location = location;
			Hue = hue;
			Underlined = underlined;
			Italicized = italicized;
			Bold = bold;
			Callback = callback;
			CallbackParam = callbackParam;
			Size = size;
			DisplayText = text;
		}
		
		public virtual void AddHyperLinkBackingButton(SuperGump gump)
		{
			var displayTextLengthInPixels = LabelWidthInPixels;

			var x = Location.X;
			var y = Location.Y + 1;

			for (var gumpIdx = 0; gumpIdx < HYPERLINK_BACK_GUMP_WIDTHS.Length; ++gumpIdx)
			{
				var gumpWidth = HYPERLINK_BACK_GUMP_WIDTHS[gumpIdx];
				var gumpId = HYPERLINK_BACK_GUMP_IDS[gumpIdx];
				var continueLooping = true;

				while (displayTextLengthInPixels - gumpWidth >= 0)
				{
					gump.AddButton(x, y, gumpId, gumpId, b => Callback(b, CallbackParam));
					gump.MoveLastAddedToBack();
					displayTextLengthInPixels -= gumpWidth;

					if (displayTextLengthInPixels >= 0 && displayTextLengthInPixels - gumpWidth < 0)
					{
						gump.AddButton(x + displayTextLengthInPixels, y, gumpId, gumpId, b => Callback(b, CallbackParam));
						gump.MoveLastAddedToBack();

						x += displayTextLengthInPixels;
						displayTextLengthInPixels = 0;
						continueLooping = false;
					}

					x += gumpWidth;
				}

				if (!continueLooping)
				{
					break;
				}
			}
		}

		public virtual void AddText(Gump gump)
		{
			gump.AddLabel(Location.X, Location.Y, Hue, DisplayText);
			if (Underlined)
			{
				gump.AddImageTiled(
					Location.X,
					Location.Y + FontHandling.FONT_LINE_HEIGHT - 1,
					LabelWidthInPixels,
					HYPERLINK_UNDERLINE_HEIGHT_IN_PIXELS,
					HyperLinkUnderlineGumpId); //Top Divisor Image
			}
		}
	}

	public class WebColoredHyperLink : HyperLink
	{
		public WebColoredHyperLink(
			Point2D location,
			string htmlColor,
			bool underlined,
			bool italicized,
			bool bold,
			Action<GumpButton, int> callback,
			int callbackParam,
			string text,
			FontHandling.FontSize size = FontHandling.FontSize.Medium)
			: base(location, 0, underlined, italicized, bold, callback, callbackParam, text, size)
		{
			HtmlColor = htmlColor;
		}

		public string HtmlColor { get; set; }

		public override void AddText(Gump gump)
		{
			gump.AddHtmlTextRectangle(
				Location.X,
				Location.Y,
				Size,
				Bold,
				Italicized,
				Underlined,
				HtmlColor,
				DisplayText,
				LabelWidthInPixels,
				FontHandling.FONT_LINE_HEIGHT);
		}
	}
}