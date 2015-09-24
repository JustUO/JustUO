#region References
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
#endregion

namespace Server.Gumps.Compendium
{
	//               Label    Image  Tiled Image Alpha Area Item HTML TextButton Button 
	//+–––––––––––––+––––––––+––––––+–––––––––––+––––––––––+––––+––––+––––––––––+––––––+
	//+ Type            x       x         x          x       x    x       x        x    
	//+ Name            x       x         x          x       x    x       x        x
	//| X Offset    |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
	//| Y Offset    |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
	//| Z           |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |

	//| Text        |   x    |      |           |          |    | x  |    x     |      |

	//| Color       |   x    |  x   |           |          | x  |    |    x     |      |
	//| Graphics ID |        |  x   |     x     |          | x  |    |          |  x   |
	//| Width       |        |      |     x     |    x     |    | x  |          |      |
	//| Height      |        |      |     x     |    x     |    | x  |          |      |
	//| Scrollbar   |        |      |           |          |    | x  |          |      |
	//| Background  |        |      |           |          |    | x  |          |      |
	//| Font Size   |        |      |           |          |    |    |    x     |      |
	//| GumpID      |        |      |           |          |    |    |    x     |  x   |
	//| GraphicsID2 +        +      +           +          +    +    +          +  x   +
	//                  7       7         8          7       7    10      9        8    
	//
	public class HyperLinkElement : BaseCompendiumPageElement
	{
		public string GumpLink { get; set; }
		public string WebColor { get; set; }
		public string Text { get; set; }
		public bool Underlined { get; set; }
		public bool Italicized { get; set; }
		public bool Bold { get; set; }
		public FontHandling.FontSize FontSize { get; set; }

		public override object Clone()
		{
			var link = new HyperLinkElement
			{
				ElementType = ElementType.Clone() as string,
				Name = Name.Clone() as string,
				X = X,
				Y = Y,
				Z = Z,
				GumpLink = (string)GumpLink.Clone(),
				WebColor = (string)WebColor.Clone(),
				Text = (string)Text.Clone(),
				Underlined = Underlined,
				Italicized = Italicized,
				Bold = Bold,
				FontSize = FontSize
			};

			return link;
		}

		public static void Configure()
		{
			RegisterElement("HyperLinkElement", CreateLabelElement);
			CompendiumPageEditor.RegisterElementType(typeof(HyperLinkElement), CreateInstance, " GumpLink   ");
		}

		//factory method
		public static BaseCompendiumPageElement CreateLabelElement(XElement elementXml)
		{
			var elementToReturn = new HyperLinkElement();

			try
			{
				elementToReturn.Deserialize(elementXml);
			}
			catch
			{
				elementToReturn = null;
			}

			return elementToReturn;
		}

		public override void Render(CompendiumPageGump gump)
		{
			if (gump is CompendiumPreviewPageGump)
			{
				gump.AddHtmlLabel(X, Y, FontSize, Bold, Italicized, Underlined, WebColor, Text);
			}
			else
			{
				var hyperlinkId = gump.RegisterHyperlink(GumpLink);
				gump.AddHyperlink(
					new WebColoredHyperLink(
						new Point2D(X, Y),
						WebColor,
						Underlined,
						Italicized,
						Bold,
						gump.onHyperlinkClick,
						hyperlinkId,
						Text,
						FontSize));
			}
		}

		public override void GetElementPropertiesSnapshot(List<ElementProperty> list)
		{
			base.GetElementPropertiesSnapshot(list);

			list.Add(new ElementProperty("Gump Link", OnGumpLinkEntryUpdate, GumpLink, ElementProperty.InputType.TextEntry));
			list.Add(new ElementProperty("Web Color", OnWebColorEntryUpdate, WebColor, ElementProperty.InputType.TextEntry));
			list.Add(new ElementProperty("Text", OnTextEntryUpdate, Text, ElementProperty.InputType.TextEntry));

			list.Add(new ElementProperty("Font Size", null, GumpLink, ElementProperty.InputType.Blank));
			list.Add(
				new ElementProperty(
					"   Large",
					OnFontLargeRadioButtonUpdate,
					string.Empty,
					ElementProperty.InputType.RadioButton,
					FontSize == FontHandling.FontSize.Large));
			list.Add(
				new ElementProperty(
					"   Medium",
					OnFontMediumRadioButtonUpdate,
					string.Empty,
					ElementProperty.InputType.RadioButton,
					FontSize == FontHandling.FontSize.Medium));
			list.Add(
				new ElementProperty(
					"   Small",
					OnFontSmallRadioButtonUpdate,
					string.Empty,
					ElementProperty.InputType.RadioButton,
					FontSize == FontHandling.FontSize.Small));

			list.Add(
				new ElementProperty(
					"Underlined",
					OnUnderlinedCheckboxUpdate,
					string.Empty,
					ElementProperty.InputType.Checkbox,
					Underlined));
			list.Add(
				new ElementProperty(
					"Italicized",
					OnItalicizedCheckboxUpdate,
					string.Empty,
					ElementProperty.InputType.Checkbox,
					Italicized));
			list.Add(new ElementProperty("Bold", OnBoldCheckboxUpdate, string.Empty, ElementProperty.InputType.Checkbox, Bold));
		}

		public static BaseCompendiumPageElement CreateInstance()
		{
			var link = new HyperLinkElement
			{
				Text = "",
				X = 0,
				Y = 0,
				Name = "new GumpLink",
				ElementType = "HyperLinkElement",
				GumpLink = ""
			};

			link.Text = "Link";
			link.WebColor = "#FFFFFF";

			link.Underlined = false;
			link.Bold = false;
			link.Italicized = false;

			link.FontSize = FontHandling.FontSize.Medium;

			return link;
		}

		public void OnFontLargeRadioButtonUpdate(GumpEntry gumpComponent, object param)
		{
			var radio = gumpComponent as GumpRadio;

			if (radio != null)
			{
				if (radio.InitialState)
				{
					FontSize = FontHandling.FontSize.Large;
				}
			}
		}

		public void OnFontMediumRadioButtonUpdate(GumpEntry gumpComponent, object param)
		{
			var radio = gumpComponent as GumpRadio;

			if (radio != null)
			{
				if (radio.InitialState)
				{
					FontSize = FontHandling.FontSize.Medium;
				}
			}
		}

		public void OnFontSmallRadioButtonUpdate(GumpEntry gumpComponent, object param)
		{
			var radio = gumpComponent as GumpRadio;

			if (radio != null)
			{
				if (radio.InitialState)
				{
					FontSize = FontHandling.FontSize.Small;
				}
			}
		}

		public void OnGumpLinkEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					GumpLink = entry.InitialText;
				}
				catch
				{ }
			}
		}

		public void OnWebColorEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					WebColor = entry.InitialText;
				}
				catch
				{ }
			}
		}

		public void OnTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Text = entry.InitialText;
				}
				catch
				{ }
			}
		}

		public void OnUnderlinedCheckboxUpdate(GumpEntry gumpComponent, object param)
		{
			var check = gumpComponent as GumpCheck;

			if (check != null)
			{
				try
				{
					Underlined = check.InitialState;
				}
				catch
				{ }
			}
		}

		public void OnItalicizedCheckboxUpdate(GumpEntry gumpComponent, object param)
		{
			var check = gumpComponent as GumpCheck;

			if (check != null)
			{
				try
				{
					Italicized = check.InitialState;
				}
				catch
				{ }
			}
		}

		public void OnBoldCheckboxUpdate(GumpEntry gumpComponent, object param)
		{
			var check = gumpComponent as GumpCheck;

			if (check != null)
			{
				try
				{
					Bold = check.InitialState;
				}
				catch
				{ }
			}
		}

		public override void Serialize(ref string xml, int indentLevel)
		{
			var indent = "";
			for (var indentIdx = 0; indentIdx < indentLevel; ++indentIdx)
			{
				indent += " ";
			}

			xml += string.Format("{0}{1}{2}", indent, "<Element>", Environment.NewLine);

			base.Serialize(ref xml, indentLevel + 1);

			xml += string.Format(
				"{0}{1}{2}{3}{4}",
				indent,
				"<GumpLink>",
				HttpUtility.HtmlEncode(GumpLink),
				"</GumpLink>",
				Environment.NewLine);
			xml += string.Format(
				"{0}{1}{2}{3}{4}",
				indent,
				"<WebColor>",
				HttpUtility.HtmlEncode(WebColor),
				"</WebColor>",
				Environment.NewLine);
			xml += string.Format(
				"{0}{1}{2}{3}{4}",
				indent,
				"<Text>",
				HttpUtility.HtmlEncode(Text),
				"</Text>",
				Environment.NewLine);

			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Underlined>", Underlined, "</Underlined>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Italicized>", Italicized, "</Italicized>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Bold>", Bold, "</Bold>", Environment.NewLine);

			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<FontSize>", (int)FontSize, "</FontSize>", Environment.NewLine);

			xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
		}

		public override void Deserialize(XElement xml)
		{
			try
			{
				base.Deserialize(xml);

				GumpLink = HttpUtility.HtmlDecode(xml.Descendants("GumpLink").First().Value);
				WebColor = HttpUtility.HtmlDecode(xml.Descendants("WebColor").First().Value);
				Text = HttpUtility.HtmlDecode(xml.Descendants("Text").First().Value);

				try
				{
					Underlined = Convert.ToBoolean(xml.Descendants("Underlined").First().Value);
					Italicized = Convert.ToBoolean(xml.Descendants("Italicized").First().Value);
					Bold = Convert.ToBoolean(xml.Descendants("Bold").First().Value);

					FontSize = (FontHandling.FontSize)Convert.ToInt32(xml.Descendants("FontSize").First().Value);
				}
				catch (Exception e)
				{
					Console.WriteLine("An error occured while loading Hyperlink element");
					Console.WriteLine(e);
				}
			}
			catch (Exception e)
			{
				throw new Exception("Failed to parse LabelElement xml", e);
			}
		}
	}
}