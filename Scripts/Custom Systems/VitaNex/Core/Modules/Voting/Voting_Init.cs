#region Header
//   Vorspire    _,-'/-'/  Voting_Init.cs
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

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.Voting
{
	[CoreModule("Voting", "1.0.0.0")]
	public static partial class Voting
	{
		static Voting()
		{
			SiteTypes = typeof(IVoteSite).GetConstructableChildren();

			CMOptions = new VotingOptions();

			VoteSites = new BinaryDataStore<int, IVoteSite>(VitaNexCore.SavesDirectory + "/Voting", "Sites")
			{
				OnSerialize = SerializeVoteSites,
				OnDeserialize = DeserializeVoteSites
			};

			Profiles = new BinaryDataStore<PlayerMobile, VoteProfile>(VitaNexCore.SavesDirectory + "/Voting", "Profiles")
			{
				Async = true,
				OnSerialize = SerializeProfiles,
				OnDeserialize = DeserializeProfiles
			};
		}

		private static void CMConfig()
		{ }

		private static void CMEnabled()
		{ }

		private static void CMDisabled()
		{ }

		private static void CMInvoke()
		{
			if (VoteSites.Count != 0)
			{
				return;
			}

			var sites = new List<IVoteSite>();

			SiteTypes.ForEach(
				type =>
				{
					var site = type.CreateInstanceSafe<IVoteSite>();

					if (site == null)
					{
						return;
					}

					if (site.Name == "Vita-Nex")
					{
						site.Enabled = true;
					}

					sites.Add(site);
					CMOptions.ToConsole(
						"Created site ({0}) '{1}', '{2}'", site.GetType().Name, site.Name, site.Enabled ? "Enabled" : "Disabled");
				});

			sites.ForEach(s => VoteSites.AddOrReplace(s.UID, s));

			InternalSiteSort();
		}

		private static void CMSave()
		{
			DataStoreResult result = VitaNexCore.TryCatchGet(VoteSites.Export, CMOptions.ToConsole);
			CMOptions.ToConsole("{0} sites saved, {1}", VoteSites.Count.ToString("#,0"), result);

			result = VitaNexCore.TryCatchGet(Profiles.Export, CMOptions.ToConsole);
			CMOptions.ToConsole("{0} profiles saved, {1}", Profiles.Count.ToString("#,0"), result);
		}

		private static void CMLoad()
		{
			DataStoreResult result = VitaNexCore.TryCatchGet(VoteSites.Import, CMOptions.ToConsole);
			CMOptions.ToConsole("{0} sites loaded, {1}.", VoteSites.Count.ToString("#,0"), result);

			result = VitaNexCore.TryCatchGet(Profiles.Import, CMOptions.ToConsole);
			CMOptions.ToConsole("{0} profiles loaded, {1}.", Profiles.Count.ToString("#,0"), result);
		}

		private static bool SerializeVoteSites(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.WriteBlockDictionary(VoteSites, (w, k, v) => w.WriteType(v, t => v.Serialize(w)));
					break;
			}

			return true;
		}

		private static bool DeserializeVoteSites(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							r =>
							{
								var v = r.ReadTypeCreate<IVoteSite>(r);
								return new KeyValuePair<int, IVoteSite>(v.UID, v);
							},
							VoteSites);
					}
					break;
			}

			return true;
		}

		private static bool SerializeProfiles(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlockDictionary(
							Profiles,
							(w, k, v) =>
							{
								w.Write(k);
								v.Serialize(w);
							});
					}
					break;
			}

			return true;
		}

		private static bool DeserializeProfiles(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							r =>
							{
								var k = r.ReadMobile<PlayerMobile>();
								var v = new VoteProfile(r);
								return new KeyValuePair<PlayerMobile, VoteProfile>(k, v);
							},
							Profiles);
					}
					break;
			}

			return true;
		}
	}
}