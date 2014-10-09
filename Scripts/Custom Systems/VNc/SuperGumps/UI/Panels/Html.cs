#region Header
//   Vorspire    _,-'/-'/  Html.cs
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
using System.Collections.Generic;
using System.Drawing;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.Text;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class HtmlPanelGump<T> : PanelGump<T>
	{
		public virtual string Html { get; set; }
		public virtual Color HtmlColor { get; set; }
		public virtual bool HtmlBackground { get; set; }

		public HtmlPanelGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			int width = 420,
			int height = 420,
			string html = null,
			string emptyText = null,
			string title = null,
			IEnumerable<ListGumpEntry> opts = null,
			T selected = default(T))
			: base(user, parent, x, y, width, height, emptyText, title, opts, selected)
		{
			HtmlColor = DefaultHtmlColor;
			HtmlBackground = false;

			Html = html ?? String.Empty;
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			if (!Minimized)
			{
				layout.Add(
					"html/body/base", () => AddHtml(15, 65, Width - 30, Height - 30, Html.ParseBBCode(HtmlColor), HtmlBackground, true));
			}
		}
	}
}