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
	public class AlphaAreaElement : BaseCompendiumPageElement
	{
		public int Width { get; set; }
		public int Height { get; set; }

		public override object Clone()
		{
			var image = new AlphaAreaElement
			{
				ElementType = ElementType.Clone() as string,
				Name = Name.Clone() as string,
				X = X,
				Y = Y,
				Z = Z,
				Width = Width,
				Height = Height
			};

			return image;
		}

		public static void Configure()
		{
			RegisterElement("AlphaAreaElement", CreateAlphaAreaElement);
			CompendiumPageEditor.RegisterElementType(typeof(AlphaAreaElement), CreateInstance, " AlphaArea  ");
		}

		//factory method
		public static BaseCompendiumPageElement CreateAlphaAreaElement(XElement elementXml)
		{
			var elementToReturn = new AlphaAreaElement();

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
			gump.AddAlphaRegion(X, Y, Width, Height);
		}

		public override void GetElementPropertiesSnapshot(List<ElementProperty> list)
		{
			base.GetElementPropertiesSnapshot(list);

			list.Add(new ElementProperty("Width", OnWidthTextEntryUpdate, Width.ToString(), ElementProperty.InputType.TextEntry));
			list.Add(
				new ElementProperty("Height", OnHeightTextEntryUpdate, Height.ToString(), ElementProperty.InputType.TextEntry));
		}

		public static BaseCompendiumPageElement CreateInstance()
		{
			var image = new AlphaAreaElement
			{
				X = 0,
				Y = 0,
				Width = 100,
				Height = 100,
				Name = "new AlphaArea",
				ElementType = "AlphaAreaElement"
			};

			return image;
		}

		public void OnWidthTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Width = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
			}
		}

		public void OnHeightTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Height = Convert.ToInt32(entry.InitialText);
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

			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Width>", Width, "</Width>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Height>", Height, "</Height>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
		}

		public override void Deserialize(XElement xml)
		{
			try
			{
				base.Deserialize(xml);
				Width = Convert.ToInt32(xml.Descendants("Width").First().Value);
				Height = Convert.ToInt32(xml.Descendants("Height").First().Value);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to parse AlphaAreaElement xml", e);
			}
		}
	}
}