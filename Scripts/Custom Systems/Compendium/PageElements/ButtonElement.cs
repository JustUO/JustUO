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
	public class ButtonElement : BaseCompendiumPageElement
	{
		public string GumpLink { get; set; }
		public int NormalId { get; set; }
		public int PressedId { get; set; }

		public override object Clone()
		{
			var link = new ButtonElement
			{
				ElementType = ElementType.Clone() as string,
				Name = Name.Clone() as string,
				X = X,
				Y = Y,
				Z = Z,
				GumpLink = (string)GumpLink.Clone(),
				NormalId = NormalId,
				PressedId = PressedId
			};

			return link;
		}

		public static void Configure()
		{
			RegisterElement("ButtonElement", CreateLabelElement);
			CompendiumPageEditor.RegisterElementType(typeof(ButtonElement), CreateInstance, " GumpButton   ");
		}

		//factory method
		public static BaseCompendiumPageElement CreateLabelElement(XElement elementXml)
		{
			var elementToReturn = new ButtonElement();

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
				var hyperlinkId = gump.RegisterHyperlink(GumpLink);
				gump.AddButton(X, Y, NormalId, PressedId, b => gump.onHyperlinkClick(b, hyperlinkId));
			}
		}

		public override void GetElementPropertiesSnapshot(List<ElementProperty> list)
		{
			base.GetElementPropertiesSnapshot(list);

			list.Add(new ElementProperty("Gump Link", OnGumpLinkEntryUpdate, GumpLink, ElementProperty.InputType.TextEntry));
			list.Add(
				new ElementProperty(
					"Normal ID",
					OnNormalIdTextEntryUpdate,
					NormalId.ToString(),
					ElementProperty.InputType.TextEntry));
			list.Add(
				new ElementProperty(
					"Pressed ID",
					OnPressedIdTextEntryUpdate,
					PressedId.ToString(),
					ElementProperty.InputType.TextEntry));
		}

		public static BaseCompendiumPageElement CreateInstance()
		{
			var link = new ButtonElement
			{
				X = 0,
				Y = 0,
				Name = "new Button",
				ElementType = "ButtonElement",
				GumpLink = "",
				NormalId = 247,
				PressedId = 248
			};

			return link;
		}

		public void OnNormalIdTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					NormalId = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
			}
		}

		public void OnPressedIdTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					PressedId = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
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
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<PressedId>", PressedId, "</PressedId>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<NormalId>", NormalId, "</NormalId>", Environment.NewLine);

			xml += string.Format("{0}{1}{2}", indent, "</Element>", Environment.NewLine);
		}

		public override void Deserialize(XElement xml)
		{
			try
			{
				base.Deserialize(xml);

				GumpLink = HttpUtility.HtmlDecode(xml.Descendants("GumpLink").First().Value);

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