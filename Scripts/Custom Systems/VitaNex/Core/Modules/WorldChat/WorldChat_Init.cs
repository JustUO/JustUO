#region Header
//   Vorspire    _,-'/-'/  WorldChat_Init.cs
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

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.WorldChat
{
	[CoreModule("World Chat", "1.0.0.0", false, TaskPriority.Highest)]
	public static partial class WorldChat
	{
		static WorldChat()
		{
			CMOptions = new WorldChatOptions();

			ChannelTypes = typeof(WorldChatChannel).GetConstructableChildren();

			_Channels = new List<WorldChatChannel>();
			_PermaChannels = new WorldChatChannel[]
			{
				//
				new GlobalChatChannel
				{
					Token = "w",
					Available = true,
					AutoJoin = true
				},
				new FacetChatChannel
				{
					Token = "f",
					Available = true,
					AutoJoin = true
				},
				new TradeChatChannel
				{
					Token = "t",
					Available = true,
					AutoJoin = true
				},
				new LocalChatChannel
				{
					Token = "l",
					Available = true,
					AutoJoin = true
				}
			};
		}

		private static void CMConfig()
		{
			EventSink.Speech += OnSpeech;
			EventSink.Login += OnLogin;
			EventSink.Logout += OnLogout;

			CommandUtility.Register("ChatAdmin", Access, e => new WorldChatAdminGump(e.Mobile as PlayerMobile).Send());
		}

		private static void CMSave()
		{
			ChannelSaves.Serialize(SerializeChannels);
		}

		private static void CMLoad()
		{
			ChannelSaves.Deserialize(DeserializeChannels);
		}

		private static void CMEnabled()
		{
			EventSink.Speech += OnSpeech;
			EventSink.Login += OnLogin;
			EventSink.Logout += OnLogout;
		}

		private static void CMDisabled()
		{
			EventSink.Speech -= OnSpeech;
			EventSink.Login -= OnLogin;
			EventSink.Logout -= OnLogout;
		}

		private static void SerializeChannels(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.WriteBlockArray(_PermaChannels, (w, c) => c.Serialize(w));

			writer.WriteBlockList(
				_Channels,
				(w, c) => w.WriteType(
					c,
					t =>
					{
						if (t != null)
						{
							c.Serialize(w);
						}
					}));
		}

		private static void DeserializeChannels(GenericReader reader)
		{
			reader.GetVersion();

			int idx = 0;

			reader.ReadBlockArray(
				r =>
				{
					var c = _PermaChannels[idx++];

					if (c != null)
					{
						c.Deserialize(r);
					}

					return c;
				},
				_PermaChannels);

			reader.ReadBlockList(r => r.ReadTypeCreate<WorldChatChannel>(r), _Channels);
		}
	}
}