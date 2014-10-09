using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Linq;

using Server.Items;
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
    public class HtmlElement : BaseCompendiumPageElement
    {
        public string Text { get; set; }
        public UInt32 Width { get; set; }
        public UInt32 Height { get; set; }
        public bool Scrollbar { get; set; }
        public bool Background { get; set; }

        public override object Clone()
        {
            HtmlElement html = new HtmlElement();

            html.ElementType = this.ElementType.Clone() as string;
            html.Name = this.Name.Clone() as string;
            html.X = this.X;
            html.Y = this.Y;
            html.Z = this.Z;

            html.Text = (string)this.Text.Clone();
            html.Width = this.Width;
            html.Height = this.Height;
            html.Scrollbar = this.Scrollbar;
            html.Background = this.Background;

            return html;
        }

        public static void Configure()
        {
            BaseCompendiumPageElement.RegisterElement("HtmlElement", CreateHtmlElement);
            CompendiumPageEditor.RegisterElementType(typeof(HtmlElement), CreateInstance, " Html      ");
        }

        //factory method
        public static BaseCompendiumPageElement CreateHtmlElement(XElement elementXml)
        {
            HtmlElement elementToReturn = new HtmlElement();

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
            gump.AddHtml(X, Y, (int)Width, (int)Height, Text, Background, Scrollbar);
        }

        public override void RenderOutline(Gump gump)
        {
            if (!Background)
            {
                gump.AddImageTiled(X, Y,               (int)Width, 1, 2621);
                gump.AddImageTiled(X, Y + (int)Height, (int)Width, 1, 2621);

                gump.AddImageTiled(X,              Y, 1, (int)Height, 2623);
                gump.AddImageTiled(X + (int)Width, Y, 1, (int)Height, 2623); 
            }
        }

        public override List<ElementProperty> GetElementPropertiesSnapshot(List<ElementProperty> list = null)
        {
            list = base.GetElementPropertiesSnapshot();
            list.Add(new ElementProperty("Text", null, Text, ElementProperty.InputType.Book, false, onUpdateTextCallback));
            list.Add(new ElementProperty("Width", OnWidthTextEntryUpdate, Width.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Height", OnHeightTextEntryUpdate, Height.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Background", OnBackgroundCheckboxUpdate, Text.ToString(), ElementProperty.InputType.Checkbox, Background));
            list.Add(new ElementProperty("Scrollbar", OnScrollbarCheckboxUpdate, Text.ToString(), ElementProperty.InputType.Checkbox, Scrollbar));

            return list;
        }

        public static BaseCompendiumPageElement CreateInstance()
        {
            HtmlElement html = new HtmlElement();
            html.Text = "";
            html.X = 0;
            html.Y = 0;
            html.Width = 100;
            html.Height = 40;
            html.Background = false;
            html.Scrollbar = false;
            html.Name = "new Html";
            html.ElementType = "HtmlElement";
            return html;
        }

        public void onUpdateTextCallback(string s)
        {
            Text = (string)s.Clone();
        }

        public void OnBackgroundCheckboxUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpCheck check = gumpComponent as GumpCheck;

            if (check != null)
            {
                try
                {
                    Background = check.InitialState;
                }
                catch
                {
                }
            }
        }

        public void OnScrollbarCheckboxUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpCheck check = gumpComponent as GumpCheck;

            if (check != null)
            {
                try
                {
                    Scrollbar = check.InitialState;
                }
                catch
                {
                }
            }
        }

        public void OnWidthTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    Width = Convert.ToUInt32(entry.InitialText);
                }
                catch
                {
                }
            }
        }

        public void OnHeightTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    Height = Convert.ToUInt32(entry.InitialText);
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

            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Text>", System.Web.HttpUtility.HtmlEncode(Text), "</Text>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Width>", Width, "</Width>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Height>", Height, "</Height>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Background>", Background, "</Background>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Scrollbar>", Scrollbar, "</Scrollbar>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
        }

        public override void Deserialize(XElement xml)
        {
            try
            {
                base.Deserialize(xml);
                Text = System.Web.HttpUtility.HtmlDecode(xml.Descendants("Text").First().Value);
                Width = Convert.ToUInt32(xml.Descendants("Width").First().Value);
                Height = Convert.ToUInt32(xml.Descendants("Height").First().Value);
                Background = Convert.ToBoolean(xml.Descendants("Background").First().Value);
                Scrollbar = Convert.ToBoolean(xml.Descendants("Scrollbar").First().Value);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse HtmlElement xml", e);
            }
        }
    }
}
