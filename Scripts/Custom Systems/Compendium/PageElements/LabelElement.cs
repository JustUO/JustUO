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
	public class LabelElement : BaseCompendiumPageElement
	{
		public int Hue { get; set; }
		public string Text { get; set; }

		public override object Clone()
		{
			var label = new LabelElement
			{
				ElementType = ElementType.Clone() as string,
				Name = Name.Clone() as string,
				X = X,
				Y = Y,
				Z = Z,
				Text = (string)Text.Clone(),
				Hue = Hue
			};

			return label;
		}

		public static void Configure()
		{
			RegisterElement("LabelElement", CreateLabelElement);
			CompendiumPageEditor.RegisterElementType(typeof(LabelElement), CreateInstance, " Label     ");
		}

		//factory method
		public static BaseCompendiumPageElement CreateLabelElement(XElement elementXml)
		{
			var elementToReturn = new LabelElement();

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
			gump.AddLabel(X, Y, Hue, Text);
		}

		public override void GetElementPropertiesSnapshot(List<ElementProperty> list)
		{
			base.GetElementPropertiesSnapshot(list);

			list.Add(new ElementProperty("Hue", OnHueTextEntryUpdate, Hue.ToString(), ElementProperty.InputType.TextEntry));
			list.Add(new ElementProperty("Text", OnTextIdTextEntryUpdate, Text, ElementProperty.InputType.TextEntry));
		}

		public static BaseCompendiumPageElement CreateInstance()
		{
			var label = new LabelElement
			{
				Text = "",
				X = 0,
				Y = 0,
				Hue = 0,
				Name = "new Label",
				ElementType = "LabelElement"
			};
			return label;
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

		public void OnTextIdTextEntryUpdate(GumpEntry gumpComponent, object param)
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
			xml += string.Format(
				"{0}{1}{2}{3}{4}",
				indent,
				"<Text>",
				HttpUtility.HtmlEncode(Text),
				"</Text>",
				Environment.NewLine);
			xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
		}

		public override void Deserialize(XElement xml)
		{
			try
			{
				base.Deserialize(xml);
				Hue = Convert.ToInt32(xml.Descendants("Hue").First().Value);
				Text = HttpUtility.HtmlDecode(xml.Descendants("Text").First().Value);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to parse LabelElement xml", e);
			}
		}
	}
}