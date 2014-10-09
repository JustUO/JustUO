#region Header
//   Vorspire    _,-'/-'/  Updates_Init.cs
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

namespace VitaNex.Updates
{
	[CoreService("Updates")]
	public static partial class UpdateService
	{
		private static UpdateServiceOptions _CSOptions = new UpdateServiceOptions();

		public static UpdateServiceOptions CSOptions { get { return _CSOptions ?? (_CSOptions = new UpdateServiceOptions()); } }

		static UpdateService()
		{
			Staff = new List<PlayerMobile>();

			RemoteVersion = VersionInfo.DefaultVersion;

			VitaNexCore.RegisterINITHandler(
				"UPDATES_URL",
				value =>
				{
					Uri url;

					if (Uri.TryCreate(value, UriKind.Absolute, out url) || Uri.TryCreate(DefaultURL, UriKind.Absolute, out url))
					{
						URL = url;
					}
				});
		}

		private static void CSConfig()
		{
			CSOptions.Schedule.OnGlobalTick += OnScheduleTick;

			EventSink.Connected += e =>
			{
				if (e.Mobile is PlayerMobile && e.Mobile.Account.AccessLevel >= CSOptions.NotifyAccess)
				{
					Staff.Add((PlayerMobile)e.Mobile);
				}
			};

			EventSink.Disconnected += e =>
			{
				if (e.Mobile is PlayerMobile)
				{
					Staff.Remove((PlayerMobile)e.Mobile);
				}
			};
		}

		private static void CSInvoke()
		{
			RequestVersion();
		}

		private static void CSSave()
		{
			if (!CSOptions.Schedule.Enabled)
			{
				RequestVersion();
			}
		}
	}
}