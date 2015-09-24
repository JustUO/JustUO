#region References
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Web;
using System.Xml.Linq;
#endregion

namespace Server.Gumps.Compendium
{
	//               Label    Image  Tiled Image Alpha Area Item HTML TextButton Button 
	//+–––––––––––––+––––––––+––––––+–––––––––––+––––––––––+––––+––––+––––––––––+––––––+
	//| X Offset    |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
	//| Y Offset    |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
	//| Color       |   x    |  x   |           |          | x  |    |    x     |      |
	//| Graphics ID |        |  x   |     x     |          | x  |    |          |  x   |
	//| Width       |        |      |     x     |    x     |    | x  |          |      |
	//| Height      |        |      |     x     |    x     |    | x  |          |      |
	//| Text        |   x    |      |           |          |    | x  |    x     |      |
	//| Z           |   x    |  x   |     x     |    x     | x  | x  |    x     |  x   |
	//| Scrollbar   |        |      |           |          |    | x  |          |      |
	//| Background  |        |      |           |          |    | x  |          |      |
	//| Font Size   |        |      |           |          |    |    |    x     |      |
	//| GumpID      |        |      |           |          |    |    |    x     |  x   |
	//| GraphicsID2 +        +      +           +          +    +    +          +  x   +
	//+ Type            x       x         x          x       x    x       x        x    
	//                  6       6         7          6       6    9       8        7    
	//
	public class BaseCompendiumPageElement : ICloneable
	{
		public virtual object Clone()
		{
			var element = new BaseCompendiumPageElement
			{
				Name = Name.Clone() as string,
				ElementType = ElementType.Clone() as string,
				X = X,
				Y = Y,
				Z = Z
			};

			return element;
		}

		public delegate BaseCompendiumPageElement CreateCompendiumElementMethod(XElement xml);

		//element name, delegate to create class type
		private static readonly Dictionary<string, CreateCompendiumElementMethod> g_registeredElements =
			new Dictionary<string, CreateCompendiumElementMethod>();

		public static void RegisterElement(string elementName, CreateCompendiumElementMethod factoryMethod)
		{
			if (!g_registeredElements.ContainsKey(elementName))
			{
				g_registeredElements.Add(elementName, factoryMethod);
			}
			else
			{
				Console.WriteLine("Error Registering CompendiumPageElement.  That element name has been taken.");
			}
		}

		//pass in the element XML element
		public static BaseCompendiumPageElement CreateElement(XElement elementXml)
		{
			BaseCompendiumPageElement elementToReturn = null;

			var elementType = elementXml.Descendants("Type").First().Value;

			if (g_registeredElements.ContainsKey(elementType))
			{
				elementToReturn = g_registeredElements[elementType](elementXml);
			}
			else
			{
				Console.WriteLine("Cannot create Compendium Element ({0}), it was not registered.", elementType);
			}

			return elementToReturn;
		}

		public int X { get; set; }
		public int Y { get; set; }
		public double Z { get; set; }
		public string ElementType { get; set; }
		public string Name { get; set; }

		public virtual void Render(CompendiumPageGump gump)
		{ }

		public virtual void RenderOutline(Gump gump)
		{ }

		public virtual List<ElementProperty> GetElementPropertiesSnapshot()
		{
			var list = new List<ElementProperty>();

			GetElementPropertiesSnapshot(list);

			return list;
		}

		public virtual void GetElementPropertiesSnapshot(List<ElementProperty> list)
		{
			list.Add(new ElementProperty("Name", OnNameTextEntryUpdate, Name, ElementProperty.InputType.TextEntry));
			list.Add(new ElementProperty("Type", null, ElementType, ElementProperty.InputType.Label));
			list.Add(
				new ElementProperty("X Offset", OnXOffsetTextEntryUpdate, X.ToString(), ElementProperty.InputType.TextEntry));
			list.Add(
				new ElementProperty("Y Offset", OnYOffsetTextEntryUpdate, Y.ToString(), ElementProperty.InputType.TextEntry));
			list.Add(
				new ElementProperty(
					"Z",
					OnZTextEntryUpdate,
					Z.ToString(CultureInfo.InvariantCulture),
					ElementProperty.InputType.TextEntry));
		}

		public void OnNameTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Name = entry.InitialText;
				}
				catch
				{ }
			}
		}

		public void OnXOffsetTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					X = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
			}
		}

		public void OnYOffsetTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Y = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
			}
		}

		public void OnZTextEntryUpdate(GumpEntry gumpComponent, object param)
		{
			var entry = gumpComponent as GumpTextEntry;

			if (entry != null)
			{
				try
				{
					Z = Convert.ToInt32(entry.InitialText);
				}
				catch
				{ }
			}
		}

		public virtual void Serialize(ref string xml, int indentLevel)
		{
			var indent = "";
			for (var indentIdx = 0; indentIdx < indentLevel; ++indentIdx)
			{
				indent += " ";
			}

			xml += string.Format(
				"{0}{1}{2}{3}{4}",
				indent,
				"<ElementName>",
				HttpUtility.HtmlEncode(Name),
				"</ElementName>",
				Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Type>", ElementType, "</Type>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<X>", X, "</X>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Y>", Y, "</Y>", Environment.NewLine);
			xml += string.Format("{0}{1}{2}{3}{4}", indent, "<Z>", Z, "</Z>", Environment.NewLine);
		}

		public virtual void Deserialize(XElement xml)
		{
			try
			{
				ElementType = xml.Descendants("Type").First().Value;
				X = Convert.ToInt32(xml.Descendants("X").First().Value);
				Y = Convert.ToInt32(xml.Descendants("Y").First().Value);
				Z = Convert.ToDouble(xml.Descendants("Z").First().Value);
				Name = HttpUtility.HtmlDecode(xml.Descendants("ElementName").First().Value);
			}
			catch (Exception e)
			{
				throw new Exception("Failed to parse BaseCompendiumPageElement", e);
			}
		}
	}
}