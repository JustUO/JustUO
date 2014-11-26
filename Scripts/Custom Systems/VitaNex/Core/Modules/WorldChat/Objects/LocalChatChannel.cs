#region Header
//   Vorspire    _,-'/-'/  LocalChatChannel.cs
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
	public sealed class LocalChatChannel : WorldChatChannel
	{
		public LocalChatChannel()
		{
			Name = "Local";
			Summary = "Chat with others in your local area!";
			UserLimit = 500;
		}

		public LocalChatChannel(GenericReader reader)
			: base(reader)
		{ }

		public override string FormatMessage(PlayerMobile user, string text)
		{
			if (user.Region == null)
			{
				return base.FormatMessage(user, text);
			}

			return String.Format(
				"[{0}][{1}] [{2}{3}]: {4}",
				Name,
				user.Region.Name,
				WorldChat.CMOptions.AccessPrefixes[user.AccessLevel],
				user.RawName,
				text);
		}

		public override bool CanSee(PlayerMobile user, WorldChatMessage message)
		{
			return base.CanSee(user, message) && user.Map == message.Place.Map && user.Region != null &&
				   user.Region.Contains(message.Place.Location);
		}

		public override bool CanMessage(PlayerMobile user, string text, bool message = true)
		{
			if (user.Region == null)
			{
				if (message)
				{
					InternalMessage(user, "You must be in a significant region to speak in the channel '{0}'", Name);
				}

				return false;
			}

			return base.CanMessage(user, text, message);
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