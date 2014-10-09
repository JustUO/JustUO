#region Header
//   Vorspire    _,-'/-'/  Dialog.cs
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
using System.Drawing;

using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class DialogGump : SuperGump
	{
		public static int Defaultwidth = 400;
		public static int DefaultHeight = 300;

		public static int DefaultIcon = 7000;
		public static string DefaultTitle = "Dialog";

		private int _Icon;

		public virtual Action<GumpButton> AcceptHandler { get; set; }
		public virtual Action<GumpButton> CancelHandler { get; set; }

		public virtual string Title { get; set; }
		public virtual bool HtmlBackground { get; set; }

		public virtual Color HtmlColor { get; set; }
		public virtual string Html { get; set; }

		public virtual int Icon { get { return _Icon; } set { _Icon = Math.Max(0, value); } }

		public virtual int Width { get; set; }
		public virtual int Height { get; set; }

		public DialogGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			string title = null,
			string html = null,
			int icon = -1,
			Action<GumpButton> onAccept = null,
			Action<GumpButton> onCancel = null)
			: base(user, parent, x, y)
		{
			Modal = true;
			CanDispose = false;
			HtmlBackground = false;
			HtmlColor = DefaultHtmlColor;

			Width = Defaultwidth;
			Height = DefaultHeight;

			Title = title ?? DefaultTitle;
			Html = html;
			Icon = (icon >= 0) ? icon : DefaultIcon;

			AcceptHandler = onAccept;
			CancelHandler = onCancel;
		}

		protected virtual void OnAccept(GumpButton button)
		{
			if (AcceptHandler != null)
			{
				AcceptHandler(button);
			}
		}

		protected virtual void OnCancel(GumpButton button)
		{
			if (CancelHandler != null)
			{
				CancelHandler(button);
			}
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			Width = Math.Max(300, Math.Min(1024, Width));
			Height = Math.Max(200, Math.Min(786, Height));

			layout.Add(
				"background/header/base",
				() =>
				{
					AddBackground(0, 0, Width, 50, 9270);
					AddImageTiled(10, 10, Width - 20, 30, 2624);
					//AddAlphaRegion(10, 10, Width - 20, 30);
				});

			layout.Add("label/header/title", () => AddLabelCropped(20, 15, Width - 40, 20, TextHue, Title));

			layout.Add(
				"background/body/base",
				() =>
				{
					AddBackground(0, 50, Width, Height - 50, 9270);
					AddImageTiled(10, 60, Width - 20, Height - 70, 2624);
					//AddAlphaRegion(10, 60, Width - 20, Height - 70);
				});

			layout.Add("image/body/icon", () => AddImage(20, 70, Icon));

			layout.Add(
				"html/body/info",
				() =>
				AddHtml(
					100,
					70,
					Width - 120,
					Height - 130,
					String.Format("<BIG><BASEFONT COLOR=#{0:X6}>{1}</BIG>", HtmlColor.ToArgb(), Html),
					HtmlBackground,
					true));

			layout.Add(
				"button/body/cancel",
				() =>
				{
					AddButton(Width - 90, Height - 45, 4018, 4019, OnCancel);
					AddTooltip(1006045);
				});

			layout.Add(
				"button/body/accept",
				() =>
				{
					AddButton(Width - 50, Height - 45, 4015, 4016, OnAccept);
					AddTooltip(1006044);
				});
		}
	}
}