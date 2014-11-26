#region Header
//   Vorspire    _,-'/-'/  Admin.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2013  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public sealed class WorldChatAdminGump : ListGump<WorldChatChannel>
	{
		public WorldChatAdminGump(PlayerMobile user, Gump parent = null)
			: base(user, parent, emptyText: "There are no channels to display.", title: "World Chat Control Panel")
		{
			ForceRecompile = true;
		}

		public override string GetSearchKeyFor(WorldChatChannel key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override int GetLabelHue(int index, int pageIndex, WorldChatChannel entry)
		{
			return entry != null ? (entry.Available ? HighlightHue : ErrorHue) : base.GetLabelHue(index, pageIndex, null);
		}

		protected override string GetLabelText(int index, int pageIndex, WorldChatChannel entry)
		{
			return entry != null
					   ? String.Format("{0}{1}", entry.Permanent ? "[Permanent] " : String.Empty, entry.Name)
					   : base.GetLabelText(index, pageIndex, null);
		}

		protected override void CompileList(List<WorldChatChannel> list)
		{
			list.Clear();
			list.AddRange(WorldChat.AllChannels);
			base.CompileList(list);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (User.AccessLevel >= WorldChat.Access)
			{
				list.AppendEntry(new ListGumpEntry("System Options", OpenConfig, HighlightHue));
				list.AppendEntry(new ListGumpEntry("Add Channel", AddChannel, HighlightHue));
			}

			base.CompileMenuOptions(list);
		}

		protected override void SelectEntry(GumpButton button, WorldChatChannel entry)
		{
			base.SelectEntry(button, entry);

			MenuGumpOptions opts = new MenuGumpOptions();

			if (User.AccessLevel >= WorldChat.Access)
			{
				opts.AppendEntry(
					new ListGumpEntry(
						"Options",
						b =>
						{
							Refresh();

							PropertiesGump pg = new PropertiesGump(User, Selected)
							{
								X = b.X,
								Y = b.Y
							};
							User.SendGump(pg);
						},
						HighlightHue));

				opts.AppendEntry(
					new ListGumpEntry(
						entry.Available ? "Disable" : "Enable",
						b1 =>
						{
							entry.Available = !entry.Available;
							Refresh(recompile: true);
						},
						entry.Available ? ErrorHue : HighlightHue));

				if (!entry.Permanent)
				{
					opts.AppendEntry(new ListGumpEntry("Delete", DeleteChannel, ErrorHue));
				}

				opts.AppendEntry(new ListGumpEntry("Cancel", b => { }));
			}

			Send(new MenuGump(User, Refresh(), opts, button));
		}

		private void DeleteChannel()
		{
			if (Selected == null)
			{
				return;
			}

			Send(
				new ConfirmDialogGump(User, Refresh())
				{
					Title = "Delete Channel?",
					Html =
						"All data associated with this channel will be deleted.\nThis action can not be reversed!\nDo you want to continue?",
					AcceptHandler = OnDeleteChannelConfirm
				});
		}

		private void OnDeleteChannelConfirm(GumpButton button)
		{
			if (Selected != null && !Selected.Permanent)
			{
				Selected.Clear();
				Selected.Users.Clear();
				WorldChat.Channels.Remove(Selected);
			}

			Refresh();
		}

		private void AddChannel(GumpButton btn)
		{
			MenuGumpOptions opts = new MenuGumpOptions();

			WorldChat.ChannelTypes.ForEach(
				t => opts.AppendEntry(new ListGumpEntry(t.Name.Replace("ChatChannel", " Channel"), b => OnAddChannel(t))));

			Refresh();
			Send(new MenuGump(User, btn.Parent, opts, btn));
		}

		private void OnAddChannel(Type t)
		{
			var c = VitaNexCore.TryCatchGet(() => t.CreateInstance<WorldChatChannel>());

			if (c != null)
			{
				WorldChat.Channels.Add(c);
			}

			Refresh(true);
		}

		private void OpenConfig(GumpButton btn)
		{
			Minimize();

			PropertiesGump p = new PropertiesGump(User, WorldChat.CMOptions)
			{
				X = X + btn.X,
				Y = Y + btn.Y
			};

			User.SendGump(p);
		}
	}
}