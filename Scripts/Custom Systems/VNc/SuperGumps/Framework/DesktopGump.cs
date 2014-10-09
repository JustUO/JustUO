#region Header
//   Vorspire    _,-'/-'/  DesktopGump.cs
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
using System.Linq;

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.Targets;
#endregion

namespace VitaNex.SuperGumps
{
	public class DesktopGump : SuperGump
	{
		public static void Initialize()
		{
			CommandUtility.Register("ViewDesktop", AccessLevel.GameMaster, e => BeginDesktopTarget(e.Mobile as PlayerMobile));
		}

		private static readonly Dictionary<PlayerMobile, List<SuperGump>> _RestoreStates =
			new Dictionary<PlayerMobile, List<SuperGump>>();

		public static void BeginDesktopTarget(PlayerMobile pm)
		{
			if (pm != null && !pm.Deleted && pm.IsOnline())
			{
				pm.Target = new MobileSelectTarget<PlayerMobile>((m, t) => DisplayDesktop(pm, t), m => { });
			}
		}

		public static void DisplayDesktop(PlayerMobile viewer, PlayerMobile viewed)
		{
			if (viewer == null || viewed == null)
			{
				return;
			}

			if (viewer == viewed)
			{
				viewer.SendMessage(0x22, "You can't view your own desktop!");
				return;
			}

			if (!viewed.IsOnline())
			{
				viewer.SendMessage(0x22, "You can't view desktop of an off-line player!");
				return;
			}

			if (!_RestoreStates.ContainsKey(viewer))
			{
				_RestoreStates.Add(viewer, new List<SuperGump>());
			}
			else if (_RestoreStates[viewer] == null)
			{
				_RestoreStates[viewer] = new List<SuperGump>();
			}
			else
			{
				return;
			}

			_RestoreStates[viewer].AddRange(
				EnumerateInstances<SuperGump>(viewer, true)
					.Where(g => g != null && !g.IsDisposed && g.IsOpen && !g.Hidden && !(g is DesktopGump) && g.Hide(true) == g));

			Send(new DesktopGump(viewer, viewed));
		}

		public PlayerMobile Viewer { get { return User; } set { User = value; } }
		public PlayerMobile Viewed { get; set; }

		public SuperGump[] Sources { get; private set; }

		public DesktopGump(PlayerMobile viewer, PlayerMobile viewed)
			: base(viewer, null, 0, 0)
		{
			Viewed = viewed;
		}

		protected override void Compile()
		{
			base.Compile();

			Sources =
				EnumerateInstances<SuperGump>(Viewed).Where(g => g != null && !g.IsDisposed && !(g is DesktopGump)).ToArray();
			Sources.ForEach(Link);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			if (Sources == null)
			{
				return;
			}

			Sources.ForEach(
				source =>
				{
					if (source == null || source.IsDisposed || !source.Compiled || !source.IsOpen)
					{
						return;
					}

					source.Entries.For(
						(i, src) =>
						{
							if (src is GumpPage)
							{
								GumpPage e = (GumpPage)src;
								layout.Add(source.Serial + "/" + i + "/GumpPage", () => AddPage(e.Page));
							}
							else if (src is GumpTooltip)
							{
								GumpTooltip e = (GumpTooltip)src;
								layout.Add(source.Serial + "/" + i + "/GumpTooltip", () => AddTooltip(e.Number));
							}
							else if (src is GumpBackground)
							{
								GumpBackground e = (GumpBackground)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpBackground", () => AddBackground(e.X, e.Y, e.Width, e.Height, e.GumpID));
							}
							else if (src is GumpAlphaRegion)
							{
								GumpAlphaRegion e = (GumpAlphaRegion)src;
								layout.Add(source.Serial + "/" + i + "/GumpAlphaRegion", () => AddAlphaRegion(e.X, e.Y, e.Width, e.Height));
							}
							else if (src is GumpItem)
							{
								GumpItem e = (GumpItem)src;
								layout.Add(source.Serial + "/" + i + "/GumpItem", () => AddItem(e.X, e.Y, e.ItemID, e.Hue));
							}
							else if (src is GumpImage)
							{
								GumpImage e = (GumpImage)src;
								layout.Add(source.Serial + "/" + i + "/GumpImage", () => AddImage(e.X, e.Y, e.GumpID, e.Hue));
							}
							else if (src is GumpImageTiled)
							{
								GumpImageTiled e = (GumpImageTiled)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpImageTiled", () => AddImageTiled(e.X, e.Y, e.Width, e.Height, e.GumpID));
							}
							else if (src is GumpImageTileButton)
							{
								GumpImageTileButton e = (GumpImageTileButton)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpImageTileButton",
									() =>
									AddImageTiledButton(
										e.X, e.Y, e.NormalID, e.PressedID, e.ButtonID, e.Type, e.Param, e.ItemID, e.Hue, e.Width, e.Height));
							}
							else if (src is GumpLabel)
							{
								GumpLabel e = (GumpLabel)src;
								layout.Add(source.Serial + "/" + i + "/GumpLabel", () => AddLabel(e.X, e.Y, e.Hue, e.Text));
							}
							else if (src is GumpLabelCropped)
							{
								GumpLabelCropped e = (GumpLabelCropped)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpLabelCropped",
									() => AddLabelCropped(e.X, e.Y, e.Width, e.Height, e.Hue, e.Text));
							}
							else if (src is GumpHtml)
							{
								GumpHtml e = (GumpHtml)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpHtml",
									() => AddHtml(e.X, e.Y, e.Width, e.Height, e.Text, e.Background, e.Scrollbar));
							}
							else if (src is GumpHtmlLocalized)
							{
								GumpHtmlLocalized e = (GumpHtmlLocalized)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpHtmlLocalized",
									() => AddHtmlLocalized(e.X, e.Y, e.Width, e.Height, e.Number, e.Args, e.Color, e.Background, e.Scrollbar));
							}
							else if (src is GumpButton)
							{
								GumpButton e = (GumpButton)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpButton",
									() => AddButton(e.X, e.Y, e.NormalID, e.PressedID, e.ButtonID, source.Buttons.GetValue(e)));
							}
							else if (src is GumpCheck)
							{
								GumpCheck e = (GumpCheck)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpCheck",
									() => AddCheck(e.X, e.Y, e.InactiveID, e.ActiveID, e.SwitchID, e.InitialState, source.Switches.GetValue(e)));
							}
							else if (src is GumpRadio)
							{
								GumpRadio e = (GumpRadio)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpRadio",
									() => AddRadio(e.X, e.Y, e.InactiveID, e.ActiveID, e.SwitchID, e.InitialState, source.Radios.GetValue(e)));
							}
							else if (src is GumpTextEntry)
							{
								GumpTextEntry e = (GumpTextEntry)src;
								layout.Add(
									source.Serial + "/" + i + "/GumpTextEntry",
									() => AddTextEntry(e.X, e.Y, e.Width, e.Height, e.Hue, e.EntryID, e.InitialText, source.TextInputs.GetValue(e)));
							}
							else if (src is GumpTextEntryLimited)
							{
								GumpTextEntryLimited e = (GumpTextEntryLimited)src;
								var action = source.LimitedTextInputs.GetValue(e);

								layout.Add(
									source.Serial + "/" + i + "/GumpTextEntryLimited",
									() => AddTextEntryLimited(e.X, e.Y, e.Width, e.Height, e.Hue, e.EntryID, e.InitialText, e.Size, action));
							}
						});

					layout.Add(
						source.Serial + "/frame",
						() =>
						{
							AddImageTiled(source.X, source.Y, source.OuterWidth, 2, 11340); //top
							AddImageTiled(source.X + source.OuterWidth, source.Y, 2, source.OuterHeight, 11340); //right
							AddImageTiled(source.X, source.Y + source.OuterHeight, source.OuterWidth, 2, 11340); //bottom
							AddImageTiled(source.X, source.Y, 2, source.OuterHeight, 11340); //left
						});
				});
		}

		protected override void OnLinkSend(SuperGump link)
		{
			base.OnLinkSend(link);

			Refresh(true);
		}

		protected override void OnLinkRefreshed(SuperGump link)
		{
			base.OnLinkRefreshed(link);

			Refresh(true);
		}

		protected override void OnLinkHidden(SuperGump link)
		{
			base.OnLinkHidden(link);

			Refresh(true);
		}

		protected override void OnLinkClosed(SuperGump link)
		{
			base.OnLinkClosed(link);

			Refresh(true);
		}

		protected override void OnClosed(bool all)
		{
			Sources.ForEach(Unlink);

			if (_RestoreStates.ContainsKey(User))
			{
				if (_RestoreStates[User] != null)
				{
					_RestoreStates[User].AsEnumerable().ForEach(g => _RestoreStates[User].Remove(g.Refresh()));
				}

				_RestoreStates.Remove(User);
			}

			Sources.For(i => Sources[i] = null);
			Sources = new SuperGump[0];

			base.OnClosed(all);
		}
	}
}