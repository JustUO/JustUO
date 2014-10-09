#region Header
//   Vorspire    _,-'/-'/  Error.cs
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
	public class ErrorDialogGump : DialogGump
	{
		public virtual Exception Error { get; set; }

		public ErrorDialogGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			string title = null,
			Exception error = null,
			int icon = 7019,
			Action<GumpButton> onAccept = null,
			Action<GumpButton> onCancel = null)
			: base(user, parent, x, y, title, String.Empty, icon, onAccept, onCancel)
		{
			Error = error;
		}

		protected override void Compile()
		{
			Html = Error != null ? Error.ToString() : String.Empty;

			base.Compile();
		}
	}
}