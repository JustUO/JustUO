#region Header
//   Vorspire    _,-'/-'/  TradeChatChannel.cs
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
using Server.Regions;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public sealed class TradeChatChannel : WorldChatChannel
	{
		public TradeChatChannel()
		{
			Name = "Trade";
			Summary = "Trade with others in major cities!";
			UserLimit = 500;
		}

		public TradeChatChannel(GenericReader reader)
			: base(reader)
		{ }

		public override string FormatMessage(PlayerMobile user, string text)
		{
			if (user.Region == null || !user.Region.IsPartOf(typeof(TownRegion)))
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
			return base.CanSee(user, message) && user.Region != null && user.Region.IsPartOf(typeof(TownRegion));
		}

		public override bool CanMessage(PlayerMobile user, string text, bool message = true)
		{
			if (user.Region == null || !user.Region.IsPartOf(typeof(TownRegion)))
			{
				if (message)
				{
					InternalMessage(user, "You must be in a major city to speak in the channel '{0}'", Name);
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