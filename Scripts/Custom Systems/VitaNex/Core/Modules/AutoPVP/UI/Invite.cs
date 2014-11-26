#region Header
//   Vorspire    _,-'/-'/  Invite.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPInviteGump : ConfirmDialogGump
	{
		public PvPInviteGump(PlayerMobile user, PvPBattle battle, Gump parent = null)
			: base(user, parent, title: "Call To Arms")
		{
			Battle = battle;
		}

		public PvPBattle Battle { get; set; }

		protected override void Compile()
		{
			base.Compile();

			Html = Battle != null ? Battle.ToHtmlString(viewer: User) : "Battle does not exist.";
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"textentry/body/question",
				() => AddLabelCropped(25, Height - 45, Width - 80, 20, HighlightHue, "Accept or Decline?"));
		}

		protected override void OnAccept(GumpButton button)
		{
			base.OnAccept(button);

			if (Battle != null && !Battle.Deleted)
			{
				Battle.AcceptInvite(User);
			}

			Close(true);
		}

		protected override void OnCancel(GumpButton button)
		{
			base.OnCancel(button);

			if (Battle != null && !Battle.Deleted)
			{
				Battle.DeclineInvite(User);
			}

			Close(true);
		}
	}
}