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
using Server;
using Server.Commands;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.MOTD
{
	public sealed class MOTDOptions : CoreModuleOptions
	{
		private string _ConfigCommand;
		private string _PopupCommand;

		public MOTDOptions()
			: base(typeof(MOTD))
		{
			ConfigCommand = "MOTDConfig";
			PopupCommand = "MOTD";
		}

		public MOTDOptions(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(MOTD.Access)]
		public string ConfigCommand { get { return _ConfigCommand; } set { CommandUtility.Replace(_ConfigCommand, MOTD.Access, HandleConfigCommand, (_ConfigCommand = value)); } }

		[CommandProperty(MOTD.Access)]
		public string PopupCommand { get { return _PopupCommand; } set { CommandUtility.Replace(_PopupCommand, AccessLevel.Player, HandlePopupCommand, (_PopupCommand = value)); } }

		[CommandProperty(MOTD.Access)]
		public bool LoginPopup { get; set; }

		public void HandleConfigCommand(CommandEventArgs e)
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm != null && !pm.Deleted && pm.Alive)
			{
				SuperGump.Send(new MOTDMessageListGump(pm));
			}
		}

		public void HandlePopupCommand(CommandEventArgs e)
		{
			PlayerMobile pm = e.Mobile as PlayerMobile;

			if (pm != null && !pm.Deleted)
			{
				MOTD.SendPopUpTo(pm);
			}
		}

		public override void Clear()
		{
			base.Clear();

			ConfigCommand = null;
			PopupCommand = null;
		}

		public override void Reset()
		{
			base.Reset();

			ConfigCommand = "MOTDConfig";
			PopupCommand = "MOTD";
		}

		public override string ToString()
		{
			return "MOTD Config";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(ConfigCommand);
						writer.Write(PopupCommand);
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
				case 0:
					{
						ConfigCommand = reader.ReadString();
						PopupCommand = reader.ReadString();
					}
					break;
			}
		}
	}
}