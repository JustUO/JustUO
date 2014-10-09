#region Header
//   Vorspire    _,-'/-'/  Confirm.cs
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
	public class ConfirmDialogGump : DialogGump
	{
		public virtual bool Confirmed { get; set; }

		public ConfirmDialogGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			string title = null,
			string html = null,
			int icon = 7022,
			Action<GumpButton> onAccept = null,
			Action<GumpButton> onCancel = null)
			: base(user, parent, x, y, title, html, icon, onAccept, onCancel)
		{ }

		protected override void OnAccept(GumpButton button)
		{
			Confirmed = true;
			base.OnAccept(button);
		}

		protected override void OnCancel(GumpButton button)
		{
			Confirmed = false;
			base.OnCancel(button);
		}
	}
}