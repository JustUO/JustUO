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
    public class ItemElement : BaseCompendiumPageElement
    {
        public int Hue { get; set; }
        public int ItemId { get; set; }

        public override object Clone()
        {
            ItemElement item = new ItemElement();

            item.ElementType = this.ElementType.Clone() as string;
            item.Name = this.Name.Clone() as string;
            item.X = this.X;
            item.Y = this.Y;
            item.Z = this.Z;
            item.ItemId = this.ItemId;
            item.Hue = this.Hue;

            return item;
        }

        public static void Configure()
        {
            BaseCompendiumPageElement.RegisterElement("ItemElement", CreateItemElement);
            CompendiumPageEditor.RegisterElementType(typeof(ItemElement), CreateInstance, " Item     ");
        }

        //factory method
        public static BaseCompendiumPageElement CreateItemElement(XElement elementXml)
        {
            ItemElement elementToReturn = new ItemElement();

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
            gump.AddItem(X, Y, ItemId, Hue); 
        }

        public override List<ElementProperty> GetElementPropertiesSnapshot(List<ElementProperty> list = null)
        {
            list = base.GetElementPropertiesSnapshot();
            list.Add(new ElementProperty("Hue", OnHueTextEntryUpdate, Hue.ToString(), ElementProperty.InputType.TextEntry));
            list.Add(new ElementProperty("Item ID", OnItemIdTextEntryUpdate, ItemId.ToString(), ElementProperty.InputType.TextEntry));

            return list;
        }

        public static BaseCompendiumPageElement CreateInstance()
        {
            ItemElement item = new ItemElement();
            item.ItemId = 1;
            item.X = 0;
            item.Y = 0;
            item.Hue = 0;
            item.Name = "new Item";
            item.ElementType = "ItemElement";
            return item;
        }

        public void OnHueTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    Hue = Convert.ToInt32(entry.InitialText);
                }
                catch
                {
                }
            }
        }

        public void OnItemIdTextEntryUpdate(IGumpComponent gumpComponent, object param)
        {
            GumpTextEntry entry = gumpComponent as GumpTextEntry;

            if (entry != null)
            {
                try
                {
                    ItemId = Convert.ToInt32(entry.InitialText);
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

            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Hue>", Hue, "</Hue>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}{3}{4}", indent, "<ItemId>", ItemId, "</ItemId>", Environment.NewLine);
            xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
        }

        public override void Deserialize(XElement xml)
        {
            try
            {
                base.Deserialize(xml);
                Hue = Convert.ToInt32(xml.Descendants("Hue").First().Value);
                ItemId = Convert.ToInt32(xml.Descendants("ItemId").First().Value);
            }
            catch (Exception e)
            {
                throw new Exception("Failed to parse ItemElement xml", e);
            }
        }
    }
}
