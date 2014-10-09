using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Server.Gumps;

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
            HyperLinkElement link = new HyperLinkElement();
            link.ElementType = this.ElementType.Clone() as string;
            link.Name = this.Name.Clone() as string;
            link.X = this.X;
            link.Y = this.Y;
            link.Z = this.Z;


            link.GumpLink = (string)this.GumpLink.Clone();
            link.WebColor = (string)this.WebColor.Clone();
            link.Text = (string)this.Text.Clone();
            link.Underlined = this.Underlined;
            link.Italicized = this.Italicized;
            link.Bold = this.Bold;
            link.FontSize = this.FontSize;
            return link;
        }

        public static void Configure()
        {
            BaseCompendiumPageElement.RegisterElement("HyperLinkElement", CreateLabelElement);
            CompendiumPageEditor.RegisterElementType(typeof(HyperLinkElement), CreateInstance, " GumpLink   ");
        }

        //factory method
        public static BaseCompendiumPageElement CreateLabelElement(XElement elementXml)
        {
            HyperLinkElement elementToReturn = new HyperLinkElement();

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
                int hyperlinkId = gump.RegisterHyperlink(GumpLink);
                gump.AddHyperlink(new WebColoredHyperLink(new Point2D(X, Y), WebColor, Underlined, Italicized, Bold, gump.onHyperlinkClick, hyperlinkId, Text, FontSize));
            }
        }

        public override List<ElementProperty> GetElementPropertiesSnapshot(List<ElementProperty> list = null)
        {
            list = base.GetElementPropertiesSnapshot();

            list.Add(new ElementProperty("Gump Link", OnGumpLinkEntryUpdate, GumpLink.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Web Color", OnWebColorEntryUpdate, WebColor.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Text", OnTextEntryUpdate, Text.ToString(), ElementProperty.InputType.TextEntry));

            list.Add(new ElementProperty("Font Size", null, GumpLink.ToString(), ElementProperty.InputType.Blank));
            list.Add(new ElementProperty("   Large", OnFontLargeRadioButtonUpdate, string.Empty, ElementProperty.InputType.RadioButton, FontSize == FontHandling.FontSize.Large ? true : false));
            list.Add(new ElementProperty("   Medium", OnFontMediumRadioButtonUpdate, string.Empty, ElementProperty.InputType.RadioButton, FontSize == FontHandling.FontSize.Medium ? true : false));
            list.Add(new ElementProperty("   Small", OnFontSmallRadioButtonUpdate, string.Empty, ElementProperty.InputType.RadioButton, FontSize == FontHandling.FontSize.Small ? true : false));

            list.Add(new ElementProperty("Underlined", OnUnderlinedCheckboxUpdate, string.Empty, ElementProperty.InputType.Checkbox, Underlined));
            list.Add(new ElementProperty("Italicized", OnItalicizedCheckboxUpdate, string.Empty, ElementProperty.InputType.Checkbox, Italicized));
            list.Add(new ElementProperty("Bold", OnBoldCheckboxUpdate, string.Empty, ElementProperty.InputType.Checkbox, Bold));

            return list;
        }

        public static BaseCompendiumPageElement CreateInstance()
        {
            HyperLinkElement link = new HyperLinkElement();
            link.Text = "";
            link.X = 0;
            link.Y = 0;
            link.Name = "new GumpLink";
            link.ElementType = "HyperLinkElement";

            link.GumpLink = "";
            link.Text = "Link";
            link.WebColor = "#FFFFFF";

            link.Underlined = false;
            link.Bold = false;
            link.Italicized = false;

            link.FontSize = FontHandling.FontSize.Medium;

            return link;
        }

        public void OnFontLargeRadioButtonUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpRadio radio = gumpComponent as GumpRadio;

            if (radio != null)
            {
                if (radio.InitialState)
                {
                    FontSize = FontHandling.FontSize.Large;
                }
            }
        }

        public void OnFontMediumRadioButtonUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpRadio radio = gumpComponent as GumpRadio;

            if (radio != null)
            {
                if (radio.InitialState)
                {
                    FontSize = FontHandling.FontSize.Medium;
                }
            }
        }

        public void OnFontSmallRadioButtonUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpRadio radio = gumpComponent as GumpRadio;

            if (radio != null)
            {
                if (radio.InitialState)
                {
                    FontSize = FontHandling.FontSize.Small;
                }
            }
        }

        public void OnGumpLinkEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    GumpLink =  entry.InitialText;
                }
                catch
                {
                }
            }
        }

        public void OnWebColorEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    WebColor = entry.InitialText;
                }
                catch
                {
                }
            }
        }

        public void OnTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    Text = entry.InitialText;
                }
                catch
                {
                }
            }
        }

        public void OnUnderlinedCheckboxUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpCheck check = gumpComponent as GumpCheck;

            if (check != null)
            {
                try
                {
                    Underlined = check.InitialState;
                }
                catch
                {
                }
            }
        }

        public void OnItalicizedCheckboxUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpCheck check = gumpComponent as GumpCheck;

            if (check != null)
            {
                try
                {
                    Italicized = check.InitialState;
                }
                catch
                {
                }
            }
        }

        public void OnBoldCheckboxUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpCheck check = gumpComponent as GumpCheck;

            if (check != null)
            {
                try
                {
                    Bold = check.InitialState;
                }
                catch
                {
                }
            }

        }

        public override void Serialize(ref string xml, int indentLevel)
        {
            string indent = "";
            for (int indentIdx = 0; indentIdx < indentLevel; ++indentIdx)
            {
                indent += " ";
            }

            xml += string.Format("{0}{1}{2}", indent, "<Element>", Environment.NewLine);

            base.Serialize(ref xml, indentLevel + 1);

            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<GumpLink>", System.Web.HttpUtility.HtmlEncode(GumpLink), "</GumpLink>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<WebColor>", System.Web.HttpUtility.HtmlEncode(WebColor), "</WebColor>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Text>", System.Web.HttpUtility.HtmlEncode(Text), "</Text>", Environment.NewLine);

            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Underlined>", Underlined.ToString(), "</Underlined>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Italicized>", Italicized.ToString(), "</Italicized>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Bold>", Bold.ToString(), "</Bold>", Environment.NewLine);

            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<FontSize>", (int)FontSize, "</FontSize>", Environment.NewLine);

            xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
        }

        public override void Deserialize(XElement xml)
        {
            try
            {
                base.Deserialize(xml);

                GumpLink = System.Web.HttpUtility.HtmlDecode(xml.Descendants("GumpLink").First().Value);
                WebColor = System.Web.HttpUtility.HtmlDecode(xml.Descendants("WebColor").First().Value);
                Text = System.Web.HttpUtility.HtmlDecode(xml.Descendants("Text").First().Value);

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
