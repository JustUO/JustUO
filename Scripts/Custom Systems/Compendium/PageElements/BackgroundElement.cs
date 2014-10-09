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
    public class BackgroundElement : BaseCompendiumPageElement
    {
        public int Width { get; set; }
        public int Height { get; set; }
        public int GumpId { get; set; }

        public override object Clone()
        {
            BackgroundElement image = new BackgroundElement();

            image.ElementType = this.ElementType.Clone() as string;
            image.Name = this.Name.Clone() as string;
            image.X = this.X;
            image.Y = this.Y;
            image.Z = this.Z;
            image.GumpId = this.GumpId;
            image.Width = this.Width;
            image.Height = this.Height;

            return image;
        }

        public static void Configure()
        {
            BaseCompendiumPageElement.RegisterElement("BackgroundElement", CreateBackgroundElement);
            CompendiumPageEditor.RegisterElementType(typeof(BackgroundElement), CreateInstance, " Background ");
        }

        //factory method
        public static BaseCompendiumPageElement CreateBackgroundElement(XElement elementXml)
        {
            BackgroundElement elementToReturn = new BackgroundElement();

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
            gump.AddBackground(X, Y, Width, Height, GumpId);
        }

        public override List<ElementProperty> GetElementPropertiesSnapshot(List<ElementProperty> list = null)
        {
            list = base.GetElementPropertiesSnapshot();
            list.Add(new ElementProperty("Gump ID", OnGumpIdTextEntryUpdate, GumpId.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Width", OnWidthTextEntryUpdate, Width.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Height", OnHeightTextEntryUpdate, Height.ToString(), ElementProperty.InputType.TextEntry));

            return list;
        }

        public static BaseCompendiumPageElement CreateInstance()
        {
            BackgroundElement image = new BackgroundElement();
            image.GumpId = 9200;
            image.X = 0;
            image.Y = 0;
            image.Width = 100;
            image.Height = 100;
            image.Name = "new BackgroundImage";
            image.ElementType = "BackgroundElement";
            return image;
        }

        public void OnWidthTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    Width = Convert.ToInt32(entry.InitialText);
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
                    Height = Convert.ToInt32(entry.InitialText);
                }
                catch
                {
                }
            }
        }

        public void OnGumpIdTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    GumpId = Convert.ToInt32(entry.InitialText);
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

            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Width>", Width, "</Width>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Height>", Height, "</Height>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<GumpId>", GumpId, "</GumpId>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
        }

        public override void Deserialize(XElement xml)
        {
            try
            {
                base.Deserialize(xml);
                Width = Convert.ToInt32(xml.Descendants("Width").First().Value);
                Height = Convert.ToInt32(xml.Descendants("Height").First().Value);
                GumpId = Convert.ToInt32(xml.Descendants("GumpId").First().Value);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse BackgroundElement xml", e);
            }
        }
    }
}
