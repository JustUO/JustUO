#region Header
//   Vorspire    _,-'/-'/  FacetChatChannel.cs
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

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public sealed class FacetChatChannel : WorldChatChannel
	{
		public FacetChatChannel()
		{
			Name = "Facet";
			Summary = "Chat with others in your local facet!";
			UserLimit = 500;
		}

		public FacetChatChannel(GenericReader reader)
			: base(reader)
		{ }

		public override string FormatMessage(PlayerMobile user, string text)
		{
			if (user.Map == null)
			{
				return base.FormatMessage(user, text);
			}

			return String.Format(
				"[{0}][{1}] [{2}{3}]: {4}",
				Name,
				user.Map.Name,
				WorldChat.CMOptions.AccessPrefixes[user.AccessLevel],
				user.RawName,
				text);
		}

		public override bool CanSee(PlayerMobile user, WorldChatMessage message)
		{
			return base.CanSee(user, message) && user.Map == message.Place.Map;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}