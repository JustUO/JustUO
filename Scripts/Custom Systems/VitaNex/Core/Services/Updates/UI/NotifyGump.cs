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

namespace VitaNex.Updates
{
	public sealed class UpdatesNotifyGump : NotifyGump
	{
		private static void InitSettings(NotifySettings settings)
		{
			settings.Name = "Vita-Nex: Core Updates";
			settings.Access = UpdateService.CSOptions.NotifyAccess;
		}

		public UpdatesNotifyGump(PlayerMobile user, string html)
			: base(user, html)
		{ }
	}
}