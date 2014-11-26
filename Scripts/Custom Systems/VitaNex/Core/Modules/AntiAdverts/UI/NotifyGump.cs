#region Header
//   Vorspire    _,-'/-'/  NotifyGump.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Mobiles;

using VitaNex.Notify;
#endregion

namespace Server.Misc
{
	public sealed class AntiAdvertNotifyGump : NotifyGump
	{
		private static void InitSettings(NotifySettings settings)
		{
			settings.Name = "Advertising Reports";
			settings.CanIgnore = true;
			settings.Access = AntiAdverts.Access;
		}

		public AntiAdvertNotifyGump(PlayerMobile user, string html)
			: base(user, html)
		{ }
	}
}