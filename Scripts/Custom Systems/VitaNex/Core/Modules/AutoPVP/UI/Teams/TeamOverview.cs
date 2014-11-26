#region Header
//   Vorspire    _,-'/-'/  TeamOverview.cs
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

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPTeamOverviewGump : HtmlPanelGump<PvPTeam>
	{
		public PvPTeamOverviewGump(PlayerMobile user, PvPTeam battle, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "No team selected.", title: "PvP Team Overview", selected: battle)
		{
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
		}

		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			base.Compile();

			if (Selected == null || Selected.Deleted)
			{
				return;
			}

			Html = String.Empty;

			if (User.AccessLevel >= AutoPvP.Access)
			{
				var errors = new List<string>();

				if (!Selected.Validate(User, errors))
				{
					Html += "*This team has failed validation*\n\n".WrapUOHtmlTag("BIG").WrapUOHtmlColor(Color.OrangeRed, false);
					Html += String.Join("\n", errors).WrapUOHtmlColor(Color.Yellow);
					Html += "\n\n";
				}
			}

			Html += Selected.ToHtmlString(User);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (Selected != null && !Selected.Deleted)
			{
				if (User.AccessLevel >= AutoPvP.Access)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Edit Options",
							b =>
							{
								Minimize();
								User.SendGump(new PropertiesGump(User, Selected));
							},
							HighlightHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Delete",
							b =>
							{
								if (UseConfirmDialog)
								{
									Send(
										new ConfirmDialogGump(
											User,
											this,
											title: "Delete Team?",
											html:
												"All data associated with this team will be deleted.\nThis action can not be reversed!\nDo you want to continue?",
											onAccept: OnConfirmDeleteTeam));
								}
								else
								{
									OnConfirmDeleteTeam(b);
								}
							},
							HighlightHue));
				}

				PvPTeam team;

				if (Selected.Battle.IsParticipant(User, out team))
				{
					if (team == Selected)
					{
						list.AppendEntry(new ListGumpEntry("Quit & Leave", b => Selected.Battle.Eject(User, true)));
					}
				}
				else
				{
					if (Selected.Battle.IsQueued(User))
					{
						if (Selected.Battle.Queue[User] == Selected)
						{
							list.AppendEntry(new ListGumpEntry("Leave Queue", b => Selected.Battle.Dequeue(User)));
						}
					}
					else if (!Selected.Battle.AutoAssign && Selected.Battle.CanQueue(User))
					{
						list.AppendEntry(new ListGumpEntry("Join Queue", b => Selected.Battle.Enqueue(User, Selected)));
					}
				}
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmDeleteTeam(GumpButton button)
		{
			if (Selected == null || Selected.Deleted)
			{
				Close();
				return;
			}

			Selected.Delete();
			Close();
		}
	}
}