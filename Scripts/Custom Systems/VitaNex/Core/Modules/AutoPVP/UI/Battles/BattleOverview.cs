#region Header
//   Vorspire    _,-'/-'/  BattleOverview.cs
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
using System.Linq;
using System.Text;

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.Schedules;
using VitaNex.SuperGumps.UI;
using VitaNex.Text;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPBattleOverviewGump : HtmlPanelGump<PvPBattle>
	{
		public PvPBattleOverviewGump(PlayerMobile user, Gump parent = null, PvPBattle battle = null, bool useConfirm = true)
			: base(user, parent, emptyText: "No battle selected.", title: "PvP Battle Overview", selected: battle)
		{
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
			//AutoRefresh = true;
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
					Html += "*This battle has failed validation*\n\n".WrapUOHtmlTag("BIG").WrapUOHtmlColor(Color.OrangeRed, false);
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
								User.SendGump(
									new PropertiesGump(User, Selected)
									{
										X = b.X,
										Y = b.Y
									});
							},
							HighlightHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Edit Advanced Options",
							b =>
							{
								Minimize();
								User.SendGump(
									new PropertiesGump(User, Selected.Options)
									{
										X = b.X,
										Y = b.Y
									});
							},
							HighlightHue));

					if (Selected.State == PvPBattleState.Internal)
					{
						list.AppendEntry(
							new ListGumpEntry(
								"Edit Spectate Region",
								b =>
								{
									if (Selected.SpectateRegion == null)
									{
										Selected.SpectateRegion = RegionExtUtility.Create<PvPSpectateRegion>(Selected);
									}

									Send(new PvPSpectateBoundsGump(User, Selected, Hide(true)));
								},
								HighlightHue));

						list.AppendEntry(
							new ListGumpEntry(
								"Edit Battle Region",
								b =>
								{
									if (Selected.BattleRegion == null)
									{
										Selected.BattleRegion = RegionExtUtility.Create<PvPBattleRegion>(Selected);
									}

									Send(new PvPBattleBoundsGump(User, Selected, Hide(true)));
								},
								HighlightHue));
					}

					list.AppendEntry(
						new ListGumpEntry(
							"Edit Doors", b => Send(new PvPDoorListGump(User, Selected, Hide(true), UseConfirmDialog)), HighlightHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Edit Description",
							b =>
							Send(
								new TextInputPanelGump<PvPBattle>(
									User,
									Hide(true),
									title: "Battle Description (HTML/BBC Supported)",
									input: Selected.Description,
									limit: 1000,
									callback: s =>
									{
										s = s.ParseBBCode();

										if (!String.IsNullOrWhiteSpace(s))
										{
											Selected.Description = s;
										}

										Refresh(true);
									})),
							HighlightHue));
				}

				list.AppendEntry(
					new ListGumpEntry(
						"View Schedule",
						b => Send(new ScheduleOverviewGump(User, Selected.Schedule, Hide(true))),
						(User.AccessLevel >= AutoPvP.Access) ? HighlightHue : TextHue));

				list.AppendEntry(
					new ListGumpEntry(
						"View Teams",
						b => Send(new PvPTeamListGump(User, Selected, Hide(true))),
						(User.AccessLevel >= AutoPvP.Access) ? HighlightHue : TextHue));

				if (User.AccessLevel >= AutoPvP.Access)
				{
					list.AppendEntry(
						new ListGumpEntry(
							"View Rules/Restrictions",
							b =>
							{
								MenuGumpOptions opts = new MenuGumpOptions();

								opts.AppendEntry(
									new ListGumpEntry(
										"Inherit Rules/Restrictions",
										b2 =>
										{
											MenuGumpOptions opts2 = new MenuGumpOptions();

											AutoPvP.Battles.Values.Where(ba => ba != Selected)
												   .ForEach(
													   ba => opts2.AppendEntry(
														   new ListGumpEntry(
															   ba.Name,
															   () =>
															   {
																   var rulesA = Selected.Options.Rules;
																   var rulesB = ba.Options.Rules;

																   rulesA.AllowBeneficial = rulesB.AllowBeneficial;
																   rulesA.AllowHarmful = rulesB.AllowHarmful;
																   rulesA.AllowHousing = rulesB.AllowHousing;
																   rulesA.AllowPets = rulesB.AllowPets;
																   rulesA.AllowSpawn = rulesB.AllowSpawn;
																   rulesA.AllowSpeech = rulesB.AllowSpeech;
																   rulesA.CanBeDamaged = rulesB.CanBeDamaged;
																   rulesA.CanDamageEnemyTeam = rulesB.CanDamageEnemyTeam;
																   rulesA.CanDamageOwnTeam = rulesB.CanDamageOwnTeam;
																   rulesA.CanDie = rulesB.CanDie;
																   rulesA.CanHeal = rulesB.CanHeal;
																   rulesA.CanHealEnemyTeam = rulesB.CanHealEnemyTeam;
																   rulesA.CanHealOwnTeam = rulesB.CanHealOwnTeam;
																   rulesA.CanMount = rulesB.CanMount;
																   rulesA.CanMoveThrough = rulesB.CanMoveThrough;
																   rulesA.CanMountEthereal = rulesB.CanMountEthereal;
																   rulesA.CanResurrect = rulesB.CanResurrect;
																   rulesA.CanUseStuckMenu = rulesB.CanUseStuckMenu;

																   Selected.Options.Restrictions.Items.List =
																	   new Dictionary<Type, bool>(ba.Options.Restrictions.Items.List);

																   Selected.Options.Restrictions.Pets.List =
																	   new Dictionary<Type, bool>(ba.Options.Restrictions.Pets.List);

																   Selected.Options.Restrictions.Spells.List =
																	   new Dictionary<Type, bool>(ba.Options.Restrictions.Spells.List);

																   Selected.Options.Restrictions.Skills.List =
																	   new Dictionary<int, bool>(ba.Options.Restrictions.Skills.List);

																   Refresh(true);
															   })));

											Send(new MenuGump(User, this, opts2, b));
										}));

								opts.AppendEntry(
									new ListGumpEntry(
										"Rules",
										mb =>
										{
											Refresh();

											PropertiesGump g = new PropertiesGump(User, Selected.Options.Rules)
											{
												X = mb.X,
												Y = mb.Y
											};
											User.SendGump(g);
										}));

								opts.AppendEntry(
									new ListGumpEntry(
										"Items", mb => Send(new PvPRestrictItemsListGump(User, Selected.Options.Restrictions.Items, Hide(true)))));

								opts.AppendEntry(
									new ListGumpEntry(
										"Pets", mb => Send(new PvPRestrictPetsListGump(User, Selected.Options.Restrictions.Pets, Hide(true)))));

								opts.AppendEntry(
									new ListGumpEntry(
										"Skills", mb => Send(new PvPRestrictSkillsListGump(User, Selected.Options.Restrictions.Skills, Hide(true)))));

								opts.AppendEntry(
									new ListGumpEntry(
										"Spells", mb => Send(new PvPRestrictSpellsListGump(User, Selected.Options.Restrictions.Spells, Hide(true)))));

								Send(new MenuGump(User, this, opts, b));
							},
							(User.AccessLevel >= AutoPvP.Access) ? HighlightHue : TextHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Reset Statistics",
							b =>
							{
								if (UseConfirmDialog)
								{
									Send(
										new ConfirmDialogGump(
											User,
											this,
											title: "Reset Battle Statistics?",
											html:
												"All data associated with the battle statistics will be transferred to player profiles then cleared.\nThis action can not be reversed!\nDo you want to continue?",
											onAccept: OnConfirmResetStatistics));
								}
								else
								{
									OnConfirmResetStatistics(b);
								}
							},
							HighlightHue));

					if (Selected.State == PvPBattleState.Internal)
					{
						if (Selected.Validate(User))
						{
							list.AppendEntry(
								new ListGumpEntry(
									"Publish",
									b =>
									{
										Selected.State = PvPBattleState.Queueing;
										Refresh(true);
									},
									HighlightHue));
						}
					}
					else
					{
						list.AppendEntry(
							new ListGumpEntry(
								"Internalize",
								b =>
								{
									Selected.State = PvPBattleState.Internal;
									Refresh(true);
								},
								HighlightHue));

						if (!Selected.Hidden)
						{
							if (Selected.Validate(User))
							{
								list.AppendEntry(
									new ListGumpEntry(
										"Hide",
										b =>
										{
											Selected.Hidden = true;
											Refresh(true);
										},
										HighlightHue));
							}
						}
						else
						{
							list.AppendEntry(
								new ListGumpEntry(
									"Unhide",
									b =>
									{
										Selected.Hidden = false;
										Refresh(true);
									},
									HighlightHue));
						}
					}

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
											title: "Delete Battle?",
											html:
												"All data associated with this battle will be deleted.\nThis action can not be reversed!\nDo you want to continue?",
											onAccept: OnConfirmDeleteBattle));
								}
								else
								{
									OnConfirmDeleteBattle(b);
								}
							},
							HighlightHue));
				}

				list.AppendEntry(
					new ListGumpEntry(
						"Command List",
						b =>
						{
							StringBuilder html = new StringBuilder();
							Selected.GetHtmlCommandList(User, html);
							new HtmlPanelGump<PvPBattle>(User, this, title: "Command List", html: html.ToString(), selected: Selected).Send();
						}));

				PvPProfile profile = AutoPvP.EnsureProfile(User);

				if (profile != null && !profile.Deleted)
				{
					if (profile.IsSubscribed(Selected))
					{
						list.AppendEntry(
							new ListGumpEntry(
								"Unsubscribe",
								b =>
								{
									profile.Unsubscribe(Selected);
									User.SendMessage("You have unsubscribed from {0} notifications.", Selected.Name);
									Refresh(true);
								}));
					}
					else
					{
						list.AppendEntry(
							new ListGumpEntry(
								"Subscribe",
								b =>
								{
									if (UseConfirmDialog)
									{
										Send(
											new ConfirmDialogGump(
												User,
												this,
												title: "Subscriptions",
												html:
													"Subscribing to a battle allows you to see its world broadcast notifications.\n\nDo you want to subscribe to " +
													Selected.Name + "?",
												onAccept: OnConfirmSubscribe));
									}
									else
									{
										OnConfirmSubscribe(b);
									}
								}));
					}
				}

				if (Selected.IsParticipant(User))
				{
					list.AppendEntry(new ListGumpEntry("Quit & Leave", b => Selected.Eject(User, true)));
				}
				else
				{
					if (Selected.IsQueued(User))
					{
						list.AppendEntry(new ListGumpEntry("Leave Queue", b => Selected.Dequeue(User)));
					}
					else if (Selected.CanQueue(User))
					{
						list.AppendEntry(new ListGumpEntry("Join Queue", b => Selected.Enqueue(User)));
					}

					if (Selected.IsSpectator(User))
					{
						list.AppendEntry(new ListGumpEntry("Leave Spectators", b => Selected.RemoveSpectator(User, true)));
					}
					else if (Selected.CanSpectate(User))
					{
						list.AppendEntry(new ListGumpEntry("Join Spectators", b => Selected.AddSpectator(User, true)));
					}
				}
			}

			base.CompileMenuOptions(list);
		}

		protected virtual void OnConfirmSubscribe(GumpButton button)
		{
			if (Selected == null || Selected.Deleted)
			{
				Close();
				return;
			}

			PvPProfile profile = AutoPvP.EnsureProfile(User);

			if (profile != null && !profile.Deleted)
			{
				profile.Subscribe(Selected);
				User.SendMessage("You have subscribed to {0} notifications.", Selected.Name);
				Refresh(true);
			}
		}

		protected virtual void OnConfirmResetStatistics(GumpButton button)
		{
			if (Selected == null || Selected.Deleted)
			{
				Close();
				return;
			}

			Selected.TransferStatistics();
			Selected.Statistics.Clear();
			Refresh(true);
		}

		protected virtual void OnConfirmDeleteBattle(GumpButton button)
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