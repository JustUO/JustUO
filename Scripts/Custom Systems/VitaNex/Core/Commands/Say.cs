#region Header
//   Vorspire    _,-'/-'/  Say.cs
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
using Server.Network;

using VitaNex.Targets;
#endregion

namespace VitaNex.Commands
{
	public static class SayCommand
	{
		public static void Initialize()
		{
			CommandUtility.Register(
				"Say",
				AccessLevel.GameMaster,
				e =>
				{
					if (e == null || e.Mobile == null)
					{
						return;
					}

					if (String.IsNullOrWhiteSpace(e.ArgString))
					{
						e.Mobile.SendMessage(0x22, "Speech must be at least 1 character and not all white-space.");
						e.Mobile.SendMessage(0x22, "Usage: {0}{1} <speech>", CommandSystem.Prefix, "Say");
						return;
					}

					Say(e.Mobile, e.ArgString);
				});
		}

		public static void Say(Mobile m, string speech)
		{
			if (m == null || m.Deleted || !(m is PlayerMobile))
			{
				return;
			}

			GenericSelectTarget<IPoint3D>.Begin(
				m,
				(from, target) =>
				{
					if (target == null)
					{
						return;
					}

					if (target is Item)
					{
						var item = (Item)target;
						item.PublicOverheadMessage(MessageType.Regular, m.SpeechHue, true, speech);
						return;
					}

					if (target is Mobile)
					{
						var mobile = (Mobile)target;
						mobile.Say(speech);
						return;
					}

					from.SendMessage("Invalid Target.");
				},
				from => { });
		}
	}
}