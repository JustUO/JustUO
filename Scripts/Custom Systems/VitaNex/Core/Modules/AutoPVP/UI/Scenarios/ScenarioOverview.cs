#region Header
//   Vorspire    _,-'/-'/  ScenarioOverview.cs
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

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPScenarioOverviewGump : HtmlPanelGump<PvPScenario>
	{
		public PvPScenarioOverviewGump(PlayerMobile user, PvPScenario scenario, Gump parent = null, bool useConfirm = true)
			: base(user, parent, emptyText: "No scenario selected.", title: "PvP Scenario Overview", selected: scenario)
		{
			UseConfirmDialog = useConfirm;
		}

		public bool UseConfirmDialog { get; set; }

		protected override void Compile()
		{
			base.Compile();

			if (Selected == null)
			{
				return;
			}

			Html = Selected.ToHtmlString(User);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Selected != null)
			{
				if (User.AccessLevel >= AutoPvP.Access)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"Create Battle",
							b =>
							{
								if (UseConfirmDialog)
								{
									Send(
										new ConfirmDialogGump(
											User,
											this,
											title: "Create New Battle?",
											html: "This action will create a new battle from the selected scenario.\nDo you want to continue?",
											onAccept: OnConfirmCreateBattle));
								}
								else
								{
									OnConfirmCreateBattle(b);
								}
							},
							HighlightHue));
				}
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmCreateBattle(GumpButton button)
		{
			if (Selected == null)
			{
				Close();
				return;
			}

			PvPBattle battle = AutoPvP.CreateBattle(Selected);

			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						Refresh(true),
						title: "View New Battle?",
						html: "Your new battle has been created.\nDo you want to view it now?",
						onAccept: b => Send(new PvPBattleOverviewGump(User, Hide(true), battle))));
			}
			else
			{
				Send(new PvPBattleOverviewGump(User, Hide(true), battle));
			}
		}
	}
}