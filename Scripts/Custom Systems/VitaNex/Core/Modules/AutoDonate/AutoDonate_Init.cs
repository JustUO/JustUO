#region Header
//   Vorspire    _,-'/-'/  AutoDonate_Init.cs
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
using Server.Accounting;
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	[CoreModule("Auto Donate", "1.0.0.0")]
	public static partial class AutoDonate
	{
		static AutoDonate()
		{
			Profiles.OnSerialize = SerializeProfiles;
			Profiles.OnDeserialize = DeserializeProfiles;
		}

		private static void CMConfig()
		{
			CommandUtility.Register("CheckDonate", AccessLevel.Player, e => CheckDonate(e.Mobile as PlayerMobile));
			CommandUtility.Register("DonateConfig", Access, e => CheckConfig(e.Mobile as PlayerMobile));
			CommandUtility.Register("DonateSync", Access, e => Sync());

			EventSink.Login += OnLogin;
			DonationEvents.OnTransDelivered += OnTransDelivered;
		}

		private static void CMEnabled()
		{
			CommandUtility.Register("CheckDonate", AccessLevel.Player, e => CheckDonate(e.Mobile as PlayerMobile));

			EventSink.Login += OnLogin;
			DonationEvents.OnTransDelivered += OnTransDelivered;
		}

		private static void CMDisabled()
		{
			CommandUtility.Unregister("CheckDonate");

			EventSink.Login -= OnLogin;
			DonationEvents.OnTransDelivered -= OnTransDelivered;
		}

		private static void CMSave()
		{
			Sync();
		}

		private static void CMLoad()
		{
			DataStoreResult result = Profiles.Import();
			CMOptions.ToConsole("Result: {0}", result.ToString());

			switch (result)
			{
				case DataStoreResult.Null:
				case DataStoreResult.Busy:
				case DataStoreResult.Error:
					{
						if (Profiles.HasErrors)
						{
							CMOptions.ToConsole("Profiles database has errors...");

							Profiles.Errors.ForEach(e => e.ToConsole(CMOptions.ModuleQuietMode, CMOptions.ModuleDebug));
						}
					}
					break;
				case DataStoreResult.OK:
					CMOptions.ToConsole("Profile count: {0:#,0}", Profiles.Count);
					break;
			}

			Sync();
		}

		private static bool SerializeProfiles(GenericWriter writer, IAccount key, DonationProfile val)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlock(
							w =>
							{
								w.Write(key);
								w.WriteType(val, t => val.Serialize(w));
							});
					}
					break;
			}

			return true;
		}

		private static Tuple<IAccount, DonationProfile> DeserializeProfiles(GenericReader reader)
		{
			IAccount key = null;
			DonationProfile val = null;

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlock(
							r =>
							{
								key = r.ReadAccount();
								val = r.ReadTypeCreate<DonationProfile>(r);
							});
					}
					break;
			}

			if (key == null)
			{
				if (val != null && val.Account != null)
				{
					key = val.Account;
				}
				else
				{
					return null;
				}
			}

			return new Tuple<IAccount, DonationProfile>(key, val);
		}
	}
}