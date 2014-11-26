#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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

using Server;
using Server.Commands;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.Toolbar
{
	public sealed class ToolbarsOptions : CoreModuleOptions
	{
		private int _DefaultHeight;
		private int _DefaultWidth;
		private string _PopupCommand;
		private string _PositionCommand;

		[CommandProperty(Toolbars.Access)]
		public int DefaultWidth { get { return _DefaultWidth; } set { _DefaultWidth = Math.Max(1, value); } }

		[CommandProperty(Toolbars.Access)]
		public int DefaultHeight { get { return _DefaultHeight; } set { _DefaultHeight = Math.Max(1, value); } }

		[CommandProperty(Toolbars.Access)]
		public string PositionCommand { get { return _PositionCommand; } set { CommandUtility.Replace(_PositionCommand, AccessLevel.Player, HandlePositionCommand, (_PositionCommand = value)); } }

		[CommandProperty(Toolbars.Access)]
		public string PopupCommand { get { return _PopupCommand; } set { CommandUtility.Replace(_PopupCommand, AccessLevel.Player, HandlePopupCommand, (_PopupCommand = value)); } }

		[CommandProperty(Toolbars.Access)]
		public bool LoginPopup { get; set; }

		[CommandProperty(Toolbars.Access)]
		public AccessLevel Access { get; set; }

		public ToolbarsOptions()
			: base(typeof(Toolbars))
		{
			DefaultWidth = 6;
			DefaultHeight = 4;
			PositionCommand = "ToolbarPos";
			PopupCommand = "Toolbar";
			LoginPopup = false;
			Access = Toolbars.Access;
		}

		public ToolbarsOptions(GenericReader reader)
			: base(reader)
		{ }

		public void HandlePositionCommand(CommandEventArgs e)
		{
			PlayerMobile user = e.Mobile as PlayerMobile;

			if (user == null || user.Deleted || user.NetState == null || !ModuleEnabled)
			{
				return;
			}

			if (user.AccessLevel < Access)
			{
				if (user.AccessLevel > AccessLevel.Player)
				{
					user.SendMessage("You do not have access to that command.");
				}

				return;
			}

			SuperGump tb = Toolbars.EnsureState(user).GetToolbarGump();
			SuperGump.Send(
				new OffsetSelectorGump(
					user,
					tb.Refresh(true),
					Toolbars.GetOffset(user),
					(self, oldValue) =>
					{
						Toolbars.SetOffset(self.User, self.Value);
						tb.X = self.Value.X;
						tb.Y = self.Value.Y;
						tb.Refresh(true);
					}));
		}

		public void HandlePopupCommand(CommandEventArgs e)
		{
			PlayerMobile user = e.Mobile as PlayerMobile;

			if (user == null || user.Deleted || user.NetState == null || !ModuleEnabled)
			{
				return;
			}

			if (user.AccessLevel < Access)
			{
				if (user.AccessLevel > AccessLevel.Player)
				{
					user.SendMessage("You do not have access to that command.");
				}

				return;
			}

			SuperGump.Send(Toolbars.EnsureState(user).GetToolbarGump());
		}

		public override void Clear()
		{
			base.Clear();

			DefaultWidth = 1;
			DefaultHeight = 1;
			PositionCommand = null;
			PopupCommand = null;
			LoginPopup = false;
			Access = Toolbars.Access;
		}

		public override void Reset()
		{
			base.Reset();

			DefaultWidth = 6;
			DefaultHeight = 4;
			PositionCommand = "ToolbarPos";
			PopupCommand = "Toolbar";
			LoginPopup = false;
			Access = Toolbars.Access;
		}

		public override string ToString()
		{
			return "Toolbars Config";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					{
						writer.WriteFlag(Access);
						writer.Write(LoginPopup);
					}
					goto case 0;
				case 0:
					{
						writer.Write(DefaultWidth);
						writer.Write(DefaultHeight);
						writer.Write(PositionCommand);
						writer.Write(PopupCommand);

						writer.WriteBlock(Toolbars.DefaultEntries.Serialize);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 1:
					{
						Access = reader.ReadFlag<AccessLevel>();
						LoginPopup = reader.ReadBool();
					}
					goto case 0;
				case 0:
					{
						DefaultWidth = reader.ReadInt();
						DefaultHeight = reader.ReadInt();
						PositionCommand = reader.ReadString();
						PopupCommand = reader.ReadString();

						reader.ReadBlock(Toolbars.DefaultEntries.Deserialize);
					}
					break;
			}

			if (version < 1)
			{
				Access = Toolbars.Access;
			}
		}
	}
}