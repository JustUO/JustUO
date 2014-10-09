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
    public class ButtonElement : BaseCompendiumPageElement
    {
        public string GumpLink { get; set; }
        public int NormalId { get; set; }
        public int PressedId { get; set; }

        public override object Clone()
        {
            ButtonElement link = new ButtonElement();
            link.ElementType = this.ElementType.Clone() as string;
            link.Name = this.Name.Clone() as string;
            link.X = this.X;
            link.Y = this.Y;
            link.Z = this.Z;
            link.GumpLink = (string)this.GumpLink.Clone();
            link.NormalId = this.NormalId;
            link.PressedId = this.PressedId;
            return link;
        }

        public static void Configure()
        {
            BaseCompendiumPageElement.RegisterElement("ButtonElement", CreateLabelElement);
            CompendiumPageEditor.RegisterElementType(typeof(ButtonElement), CreateInstance, " GumpButton   ");
        }

        //factory method
        public static BaseCompendiumPageElement CreateLabelElement(XElement elementXml)
        {
            ButtonElement elementToReturn = new ButtonElement();

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
                gump.AddImage(X, Y, NormalId);
            }
            else
            {
                int hyperlinkId = gump.RegisterHyperlink(GumpLink);
                gump.AddButton(X, Y, NormalId, PressedId, GumpButtonType.Reply, gump.onHyperlinkClick, hyperlinkId);
            }
        }

        public override List<ElementProperty> GetElementPropertiesSnapshot(List<ElementProperty> list = null)
        {
            list = base.GetElementPropertiesSnapshot();

            list.Add(new ElementProperty("Gump Link", OnGumpLinkEntryUpdate, GumpLink.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Normal ID", OnNormalIdTextEntryUpdate, NormalId.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Pressed ID", OnPressedIdTextEntryUpdate, PressedId.ToString(), ElementProperty.InputType.TextEntry));


            return list;
        }

        public static BaseCompendiumPageElement CreateInstance()
        {
            ButtonElement link = new ButtonElement();
            link.X = 0;
            link.Y = 0;
            link.Name = "new Button";
            link.ElementType = "ButtonElement";

            link.GumpLink = "";
            link.NormalId = 247;
            link.PressedId = 248;

            return link;
        }

        public void OnNormalIdTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    NormalId = Convert.ToInt32(entry.InitialText);
                }
                catch
                {
                }
            }
        }

        public void OnPressedIdTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    PressedId = Convert.ToInt32(entry.InitialText);
                }
                catch
                {
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
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<PressedId>", PressedId, "</PressedId>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<NormalId>", NormalId, "</NormalId>", Environment.NewLine);

            xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
        }

        public override void Deserialize(XElement xml)
        {
            try
            {
                base.Deserialize(xml);

                GumpLink = System.Web.HttpUtility.HtmlDecode(xml.Descendants("GumpLink").First().Value);

                try
                {
                    PressedId = Convert.ToInt32(xml.Descendants("PressedId").First().Value);
                    NormalId = Convert.ToInt32(xml.Descendants("NormalId").First().Value);
                }
                catch (Exception e)
                {
                    Console.WriteLine("An error occured while loading Button element");
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
