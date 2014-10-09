#region Header
//   Vorspire    _,-'/-'/  GmlReader.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.IO;
using System.Xml;

using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps.Gml
{
	public interface IGmlReader : IDisposable
	{
		Stream Stream { get; }

		GumpPage ReadPage();
		GumpGroup ReadGroup();
		GumpTooltip ReadTooltip();
		GumpAlphaRegion ReadAlphaRegion();
		GumpBackground ReadBackground();
		GumpImage ReadImage();
		GumpImageTiled ReadImageTiled();
		GumpImageTileButton ReadImageTiledButton();
		GumpItem ReadItem();
		GumpLabel ReadLabel();
		GumpLabelCropped ReadLabelCropped();
		GumpHtml ReadHtml();
		GumpHtmlLocalized ReadHtmlLocalized();
		GumpButton ReadButton();
		GumpCheck ReadCheck();
		GumpRadio ReadRadio();
		GumpTextEntry ReadTextEntry();
		GumpTextEntryLimited ReadTextEntryLimited();
	}

	public class GmlReader : IGmlReader
	{
		private readonly XmlDocument _Document;

		/*private XmlElement _LastNode;
		private XmlElement _CurrentNode;
		private XmlElement _CurrentPageNode;
		private XmlElement _CurrentGroupNode;*/

		public Stream Stream { get; private set; }

		public GmlReader(Stream stream)
		{
			_Document = new XmlDocument();

			Stream = stream;

			_Document.Load(Stream);
		}

		public virtual GumpPage ReadPage()
		{
			return null;
		}

		public virtual GumpGroup ReadGroup()
		{
			return null;
		}

		public virtual GumpTooltip ReadTooltip()
		{
			return null;
		}

		public virtual GumpAlphaRegion ReadAlphaRegion()
		{
			return null;
		}

		public virtual GumpBackground ReadBackground()
		{
			return null;
		}

		public virtual GumpImage ReadImage()
		{
			return null;
		}

		public virtual GumpImageTiled ReadImageTiled()
		{
			return null;
		}

		public virtual GumpImageTileButton ReadImageTiledButton()
		{
			return null;
		}

		public virtual GumpItem ReadItem()
		{
			return null;
		}

		public virtual GumpLabel ReadLabel()
		{
			return null;
		}

		public virtual GumpLabelCropped ReadLabelCropped()
		{
			return null;
		}

		public virtual GumpHtml ReadHtml()
		{
			return null;
		}

		public virtual GumpHtmlLocalized ReadHtmlLocalized()
		{
			return null;
		}

		public virtual GumpButton ReadButton()
		{
			return null;
		}

		public virtual GumpCheck ReadCheck()
		{
			return null;
		}

		public virtual GumpRadio ReadRadio()
		{
			return null;
		}

		public virtual GumpTextEntry ReadTextEntry()
		{
			return null;
		}

		public virtual GumpTextEntryLimited ReadTextEntryLimited()
		{
			return null;
		}

		public virtual void Dispose()
		{ }
	}
}