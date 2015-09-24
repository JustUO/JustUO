#region References
using System;
using System.Collections.Generic;
using System.Linq;
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
	public class ItemElement : BaseCompendiumPageElement
	{
		public int Hue { get; set; }
		public int ItemId { get; set; }

		public override object Clone()
		{
			var item = new ItemElement
			{
				ElementType = ElementType.Clone() as string,
				Name = Name.Clone() as string,
				X = X,
				Y = Y,
				Z = Z,
				ItemId = ItemId,
				Hue = Hue
			};
			
			return item;
		}

		public static void Configure()
		{
			RegisterElement("ItemElement", CreateItemElement);
			CompendiumPageEditor.RegisterElementType(typeof(ItemElement), CreateInstance, " Item     ");
		}

		//factory method
		public static BaseCompendiumPageElement CreateItemElement(XElement elementXml)
		{
			var elementToReturn = new ItemElement();

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

		public override void GetElementPropertiesSnapshot(List<ElementProperty> list)
		{
			base.GetElementPropertiesSnapshot(list);

			list.Add(new ElementProperty("Hue", OnHueTextEntryUpdate, Hue.ToString(), ElementProperty.InputType.TextEntry));
			list.Add(
				new ElementProperty("Item ID", OnItemIdTextEntryUpdate, ItemId.ToString(), ElementProperty.InputType.TextEntry));
		}

		public static BaseCompendiumPageElement CreateInstance()
		{
			var item = new ItemElement
			{
				ItemId = 1,
				X = 0,
				Y = 0,
				Hue = 0,
				Name = "new Item",
				ElementType = "ItemElement"
			};
			return item;
		}

		public void OnHueTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Hue = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
			}
		}

		public void OnItemIdTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					ItemId = Convert.ToInt32(entry.InitialText);
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