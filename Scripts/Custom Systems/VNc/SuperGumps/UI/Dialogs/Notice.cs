#region Header
//   Vorspire    _,-'/-'/  Notice.cs
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

using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class NoticeDialogGump : DialogGump
	{
		public NoticeDialogGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			string title = null,
			string html = null,
			int icon = 7004,
			Action<GumpButton> onAccept = null,
			Action<GumpButton> onCancel = null)
			: base(user, parent, x, y, title, html, icon, onAccept, onCancel)
		{ }

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Remove("button/body/cancel");
		}
	}
}