#region Header
//   Vorspire    _,-'/-'/  DeceitBraziers_Init.cs
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

namespace VitaNex.Modules
{
	[CoreModule("Deceit Braziers", "1.0.0.0", false, TaskPriority.Lowest)]
	public static partial class DeceitBraziers
	{
		static DeceitBraziers()
		{
			Locations = new List<MapPoint>();
			Spawns = new List<Type>();

			CMOptions = new CoreModuleOptions(typeof(DeceitBraziers));

			Registry = new BinaryDataStore<DeceitBrazier, MapPoint>(VitaNexCore.SavesDirectory + "/DeceitBraziers/", "Braziers")
			{
				Async = true,
				OnSerialize = Serialize,
				OnDeserialize = Deserialize
			};
		}

		private static void CMConfig()
		{
			CommandUtility.Register("DeceitBraziers", Access, OnSystemCommand);

			RegisterLocation(Map.Trammel, 5175, 615, 0);
			RegisterLocation(Map.Felucca, 5175, 615, 0);

			RegisterSpawns(typeof(Rat), typeof(Cat), typeof(Dog), typeof(Rabbit), typeof(Sheep), typeof(Cow));
			RegisterSpawns(typeof(Zombie), typeof(Mummy), typeof(Wraith), typeof(Spectre), typeof(Lich), typeof(LichLord));
			RegisterSpawns(
				typeof(Skeleton),
				typeof(SkeletalMage),
				typeof(SkeletalKnight),
				typeof(HellCat),
				typeof(HellHound),
				typeof(HellSteed));
			RegisterSpawns(
				typeof(Wisp), typeof(DarkWisp), typeof(ShadowWisp), typeof(EnergyVortex), typeof(BladeSpirits), typeof(HeadlessOne));
			RegisterSpawns(typeof(Ettin), typeof(Cyclops), typeof(Gazer), typeof(ElderGazer), typeof(Ogre), typeof(OgreLord));
			RegisterSpawns(
				typeof(Mongbat),
				typeof(StrongMongbat),
				typeof(GreaterMongbat),
				typeof(GrizzlyBear),
				typeof(BlackBear),
				typeof(EnragedBlackBear));
			RegisterSpawns(
				typeof(Harpy), typeof(StoneHarpy), typeof(Balron), typeof(Daemon), typeof(Dragon), typeof(GreaterDragon));
			RegisterSpawns(
				typeof(Wyvern), typeof(WhiteWyrm), typeof(ShadowWyrm), typeof(Drake), typeof(SkeletalDragon), typeof(Troll));
		}

		private static void CMEnabled()
		{
			CommandUtility.Register("DeceitBraziers", Access, OnSystemCommand);
			SpawnBraziers();
		}

		private static void CMDisabled()
		{
			CommandUtility.Unregister("DeceitBraziers");
			DespawnBraziers();
		}

		private static void CMInvoke()
		{
			if (Registry.Count != Locations.Count)
			{
				SpawnBraziers(true);
			}
		}

		private static void CMSave()
		{
			DataStoreResult result = Registry.Export();
			CMOptions.ToConsole("{0} entries saved, {1}.", Registry.Count > 0 ? Registry.Count.ToString("#,#") : "0", result);
		}

		private static void CMLoad()
		{
			DataStoreResult result = Registry.Import();
			CMOptions.ToConsole("{0} entries loaded, {1}.", Registry.Count > 0 ? Registry.Count.ToString("#,#") : "0", result);

			SpawnBraziers();
		}

		private static bool Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlockDictionary(
							Registry,
							(w, b, t) =>
							{
								w.Write(b);
								w.Write(t.Location);
								w.Write(t.Map);
							});
					}
					break;
			}

			return true;
		}

		private static bool Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							r =>
							{
								DeceitBrazier b = r.ReadItem<DeceitBrazier>();
								Point3D p = r.ReadPoint3D();
								Map m = r.ReadMap();
								return new KeyValuePair<DeceitBrazier, MapPoint>(b, new MapPoint(m, p));
							},
							Registry);
					}
					break;
			}

			return true;
		}
	}
}