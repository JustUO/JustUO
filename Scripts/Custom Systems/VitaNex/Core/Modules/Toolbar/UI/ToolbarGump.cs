#region Header
//   Vorspire    _,-'/-'/  ToolbarGump.cs
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

using Server.Gumps;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.Toolbar
{
	public class ToolbarGump : ListGump<ToolbarEntry>
	{
		public static string DefaultToolbarTitle = "Toolbar";

		public ToolbarGump(ToolbarState state, Color? headerColor = null, ToolbarTheme theme = ToolbarTheme.Default)
			: base(state.User, x: state.X, y: state.Y, title: DefaultToolbarTitle)
		{
			State = state;
			HeaderColor = headerColor ?? Color.DarkBlue;
			GlobalEdit = false;
			Theme = ToolbarThemes.GetTheme(theme);

			CanSearch = false;
			CanMove = false;
			CanDispose = false;
			CanClose = false;
			CanResize = false;
		}

		public ToolbarState State { get; protected set; }
		public bool GlobalEdit { get; protected set; }
		public ToolbarThemeBase Theme { get; set; }
		public Color HeaderColor { get; set; }

		public override bool Minimized
		{
			get { return State != null ? State.Minimized : base.Minimized; }
			set
			{
				if (State != null)
				{
					State.Minimized = value;
				}

				base.Minimized = value;
			}
		}

		public virtual bool CanGlobalEdit()
		{
			return (User != null && User.AccessLevel >= Toolbars.Access &&
					(Toolbars.DefaultEntries.User == User || Toolbars.DefaultEntries.User == null));
		}

		public virtual void BeginGlobalEdit()
		{
			if (CanGlobalEdit())
			{
				if (!GlobalEdit || State != Toolbars.DefaultEntries)
				{
					GlobalEdit = true;
					State = Toolbars.DefaultEntries;
					State.User = User;
				}

				Refresh(true);
			}
			else
			{
				EndGlobalEdit();
			}
		}

		public virtual void EndGlobalEdit()
		{
			if (State == Toolbars.DefaultEntries)
			{
				State.User = null;
				State = Toolbars.EnsureState(User);
			}

			GlobalEdit = false;
			Refresh(true);
		}

		protected virtual void ShowPositionSelect(GumpButton b)
		{
			Send(
				new OffsetSelectorGump(
					User,
					Refresh(true),
					Toolbars.GetOffset(User),
					(self, oldValue) =>
					{
						Toolbars.SetOffset(self.User, self.Value);
						X = self.Value.X;
						Y = self.Value.Y;
						Refresh(true);
					}));
		}

		public override SuperGump Refresh(bool recompile = false)
		{
			if (!CanGlobalEdit())
			{
				GlobalEdit = false;
			}

			return base.Refresh(recompile);
		}

		protected override void SelectEntry(GumpButton button, ToolbarEntry entry)
		{
			base.SelectEntry(button, entry);

			if (entry != null && entry.ValidateState(State))
			{
				entry.Invoke(State);
			}

			Refresh();
		}

		protected virtual void SelectEntryMenu(GumpButton button, Point loc, ToolbarEntry entry)
		{
			if (entry != null)
			{
				entry.Edit(this, loc, button);
			}
			else
			{
				MenuGumpOptions opts = new MenuGumpOptions();

				if (!GlobalEdit)
				{
					opts.AppendEntry(
						new ListGumpEntry(
							"Load Default",
							b =>
							Send(
								new ConfirmDialogGump(
									User,
									this,
									title: "Load Default",
									html: "Loading the default entry will overwrite your custom entry.\n\nDo you want to continue?",
									onAccept: db =>
									{
										ToolbarEntry def = Toolbars.DefaultEntries.GetContent(loc.X, loc.Y);

										State.SetContent(loc.X, loc.Y, def != null ? def.Clone() : null);

										Refresh(true);
									})),
							HighlightHue));
				}

				foreach (Type eType in Toolbars.EntryTypes)
				{
					string eName = "New " + eType.Name.Replace("Toolbar", String.Empty);

					Type type = eType;
					opts.AppendEntry(
						new ListGumpEntry(
							eName,
							b =>
							{
								State.SetContent(loc.X, loc.Y, CreateToolbarEntry(type));
								Refresh(true);
							},
							HighlightHue));
				}

				Send(new MenuGump(User, this, opts, button));
			}
		}

		protected virtual ToolbarEntry CreateToolbarEntry(Type type)
		{
			return type.CreateInstanceSafe<ToolbarEntry>();
		}

		protected override void Compile()
		{
			Theme = ToolbarThemes.GetTheme(State.Theme);
			base.Compile();
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.Clear();

			if (CanGlobalEdit())
			{
				if (GlobalEdit)
				{
					list.AppendEntry(new ListGumpEntry("End Global Edit", b => EndGlobalEdit(), ErrorHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Set Default Size",
							b =>
							Send(
								new InputDialogGump(
									User,
									this,
									title: "Set Default Size",
									html:
										"Set the global default size for all toolbars.\nFormat: Width,Height\n\nIf you shrink the size, any entires located beyond the new size will be lost.",
									input: String.Format("{0},{1}", Toolbars.CMOptions.DefaultWidth, Toolbars.CMOptions.DefaultHeight),
									callback: (cb, text) =>
									{
										int w = Toolbars.CMOptions.DefaultWidth, h = Toolbars.CMOptions.DefaultHeight;

										if (text.IndexOf(",", StringComparison.Ordinal) != -1)
										{
											var split = text.Split(',');

											if (!Int32.TryParse(split[0], out w))
											{
												w = Toolbars.CMOptions.DefaultWidth;
											}

											if (!Int32.TryParse(split[1], out h))
											{
												h = Toolbars.CMOptions.DefaultHeight;
											}
										}

										Toolbars.CMOptions.DefaultWidth = w;
										Toolbars.CMOptions.DefaultHeight = h;
										Refresh(true);
									})),
							HighlightHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Reset Global Entries",
							b =>
							Send(
								new ConfirmDialogGump(
									User,
									this,
									title: "Reset Global Entries",
									html:
										"Applying global defaults will copy the global toolbar to all existing toolbars.\nThis will overwrite any custom entries that exist.\n\nDo you want to continue?",
									onAccept: db =>
									{
										Toolbars.SetGlobalEntries();
										Refresh(true);
									})),
							HighlightHue));

					list.AppendEntry(
						new ListGumpEntry(
							"Reset Global Sizes",
							b =>
							Send(
								new ConfirmDialogGump(
									User,
									this,
									title: "Reset Global Sizes",
									html:
										"Applying global size will reset the size of all existing toolbars.\nAny entries located beyond the new size will be lost.\n\nDo you want to continue?",
									onAccept: db =>
									{
										Toolbars.SetGlobalSize();
										Refresh(true);
									})),
							HighlightHue));
				}
				else
				{
					list.AppendEntry(new ListGumpEntry("Begin Global Edit", b => BeginGlobalEdit(), HighlightHue));
				}
			}

			list.AppendEntry(
				new ListGumpEntry(
					"Load Defaults",
					b =>
					Send(
						new ConfirmDialogGump(
							User,
							this,
							title: "Load Defaults",
							html:
								"Loadng the defaults will overwrite any custom entries that exist in your toolbar.\n\nDo you want to continue?",
							onAccept: db =>
							{
								State.SetDefaultEntries();
								Refresh(true);
							})),
					HighlightHue));

			list.AppendEntry(
				new ListGumpEntry(
					"Set Position",
					b => Send(
						new OffsetSelectorGump(
							User,
							this,
							new Point(State.X, State.Y),
							(self, oldValue) =>
							{
								State.X = self.Value.X;
								State.Y = self.Value.Y;
								X = State.X;
								Y = State.Y;
								Refresh(true);
							})),
					HighlightHue));

			list.AppendEntry(
				new ListGumpEntry(
					"Set Size",
					b =>
					{
						string html =
							String.Format(
								"Set the size for your toolbar.\nFormat: Width,Height\nWidth Range: {0}\nHeight Range: {1}\n\nIf you shrink the size, any entires located beyond the new size will be lost.",
								String.Format("{0}-{1}", Toolbars.CMOptions.DefaultWidth, Toolbars.DefaultEntries.Width),
								String.Format("{0}-{1}", Toolbars.CMOptions.DefaultHeight, Toolbars.DefaultEntries.Height));

						Send(
							new InputDialogGump(
								User,
								this,
								title: "Set Size",
								html: html,
								input: String.Format("{0},{1}", State.Width, State.Height),
								callback: (cb, text) =>
								{
									int w = State.Width, h = State.Height;

									if (text.IndexOf(",", StringComparison.Ordinal) != -1)
									{
										var split = text.Split(',');

										if (split.Length >= 2)
										{
											if (Int32.TryParse(split[0], out w))
											{
												if (w < Toolbars.CMOptions.DefaultWidth)
												{
													w = Toolbars.CMOptions.DefaultWidth;
												}
												else if (!GlobalEdit && w > Toolbars.DefaultEntries.Width)
												{
													w = Toolbars.DefaultEntries.Width;
												}
											}
											else
											{
												w = State.Width;
											}

											if (Int32.TryParse(split[1], out h))
											{
												if (h < Toolbars.CMOptions.DefaultHeight)
												{
													h = Toolbars.CMOptions.DefaultHeight;
												}
												else if (!GlobalEdit && h > Toolbars.DefaultEntries.Height)
												{
													h = Toolbars.DefaultEntries.Height;
												}
											}
											else
											{
												h = State.Height;
											}
										}
									}

									State.Resize(w, h);
									Refresh(true);
								}));
					},
					HighlightHue));

			list.AppendEntry(
				new ListGumpEntry(
					"Set Theme",
					b =>
					{
						MenuGumpOptions opts = new MenuGumpOptions();
						var themes = Enum.GetValues(typeof(ToolbarTheme)).Cast<ToolbarTheme>();

						foreach (var themeID in themes)
						{
							if (State.Theme == themeID)
							{
								continue;
							}

							ToolbarTheme id = themeID;
							ToolbarThemeBase theme = ToolbarThemes.GetTheme(themeID);
							opts.AppendEntry(
								new ListGumpEntry(
									theme.Name,
									tb =>
									{
										State.Theme = id;
										Refresh(true);
									},
									HighlightHue));
						}

						Send(new MenuGump(User, this, opts, b));
					},
					HighlightHue));

			base.CompileMenuOptions(list);

			list.RemoveEntry("New Search");
			list.RemoveEntry("Clear Search");

			list.Replace("Refresh", new ListGumpEntry("Exit", Close));
		}

		protected override string GetLabelText(int index, int pageIndex, ToolbarEntry entry)
		{
			string label;

			if (entry != null)
			{
				Color labelColor = (entry.LabelColor ?? (entry.Highlight ? Theme.EntryLabelColorH : Theme.EntryLabelColorN));
				label = String.Format("<basefont color=#{0:X6}><center>{1}</center>", labelColor.ToArgb(), entry.GetDisplayLabel());
			}
			else
			{
				label = String.Format("<basefont color=#{0:X6}><center>{1}</center>", Theme.EntryLabelColorN.ToArgb(), "*Unused*");
			}

			return label;
		}

		protected override void CompileList(List<ToolbarEntry> list)
		{
			list.Clear();
			list.AddRange(State.GetCells());
			base.CompileList(list);
			EntriesPerPage = list.Count;
		}

		public override string GetSearchKeyFor(ToolbarEntry key)
		{
			return key != null
					   ? (!String.IsNullOrWhiteSpace(key.Label)
							  ? key.Label
							  : !String.IsNullOrWhiteSpace(key.Label) ? key.Label : key.Value)
					   : base.GetSearchKeyFor(null);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			layout.AddReplace(
				"button/header/minmax",
				() =>
				{
					if (Minimized)
					{
						AddButton(0, 0, 2328, 2329, Maximize);
						AddTooltip(3002086);
					}
					else
					{
						AddButton(0, 0, 2328, 2329, Minimize);
						AddTooltip(3002085);
					}
				});

			layout.Add("imagetiled/header/base", () => AddImageTiled(0, 0, 84, 56, Theme.TitleBackground));

			layout.AddReplace(
				"html/header/title",
				() =>
				AddHtml(
					0,
					0,
					84,
					56,
					String.Format(
						"<basefont color=#{0:X6}><center><big>{1}</big></center>",
						Theme.TitleLabelColor.ToArgb(),
						String.Format(
							"{0} {1}", String.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title, GlobalEdit ? "[GLOBAL]" : String.Empty)),
					false,
					false));

			layout.AddReplace(
				"button/header/options",
				() =>
				{
					AddButton(84, 0, Theme.EntryOptionsN, Theme.EntryOptionsP, ShowPositionSelect);
					AddButton(84, 28, Theme.EntryOptionsN, Theme.EntryOptionsP, ShowOptionMenu);
				});

			if (Minimized)
			{
				return;
			}

			int index = 0;

			State.ForEach(
				(x, y, entry) =>
				{
					int idx = index;
					Point loc = new Point(x, y);

					layout.Add(
						"button1/entry/" + idx, () => AddButton(110 + (loc.X * 130), (loc.Y * 28), 2445, 2445, b => SelectEntry(b, entry)));

					layout.Add(
						"imagetiled/entry/" + idx,
						() =>
						{
							AddImageTiled(
								106 + (loc.X * 130),
								(loc.Y * 28),
								112,
								28,
								(entry != null && entry.Highlight) ? Theme.EntryBackgroundH : Theme.EntryBackgroundN);
							AddImageTiled(106 + (loc.X * 130) + 112, (loc.Y * 28), 18, 28, Theme.EntrySeparator);
						});

					layout.Add(
						"html/entry/" + idx,
						() =>
						AddHtml(106 + (loc.X * 130) + 3, (loc.Y * 28) + 3, 112 - 6, 28 - 6, GetLabelText(idx, idx, entry), false, false));

					layout.Add(
						"button2/entry/" + idx,
						() => AddButton(
							106 + (loc.X * 130) + 112,
							(loc.Y * 28),
							Theme.EntryOptionsN,
							Theme.EntryOptionsP,
							b =>
							{
								Refresh();
								SelectEntryMenu(b, loc, entry);
							}));

					index++;
				});
		}
	}
}