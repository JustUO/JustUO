#region Header
//   Vorspire    _,-'/-'/  DeceitBraziers.cs
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
using System.Linq;

using Server;
using Server.Commands;
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules
{
	public static partial class DeceitBraziers
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static readonly Type _TypeOfPlayer = typeof(PlayerMobile);

		public static Type TypeOfSpawn = typeof(Mobile);
		public static Type[] TypeOfSpawnArgs = {};

		public static CoreModuleOptions CMOptions { get; private set; }

		public static BinaryDataStore<DeceitBrazier, MapPoint> Registry { get; private set; }

		public static List<MapPoint> Locations { get; private set; }
		public static List<Type> Spawns { get; private set; }

		public static void RegisterSpawns(params Type[] types)
		{
			if (types == null || types.Length == 0)
			{
				return;
			}

			types.Where(
				t =>
				(t != null && !Spawns.Contains(t) && !t.IsEqualOrChildOf(_TypeOfPlayer) &&
				 t.IsConstructableFrom(TypeOfSpawn, TypeOfSpawnArgs))).ToList().ForEach(Spawns.Add);
		}

		public static void RegisterLocation(Map map, Point3D p)
		{
			RegisterLocation(map, p.X, p.Y, p.Z);
		}

		public static void RegisterLocation(Map map, int x, int y, int z)
		{
			RegisterLocation(new MapPoint(map, x, y, z));
		}

		public static void RegisterLocation(MapPoint mp)
		{
			if (!mp.Internal && !Locations.Contains(mp))
			{
				Locations.Add(mp);
			}
		}

		public static void UnregisterLocation(Map map, Point3D p)
		{
			UnregisterLocation(map, p.X, p.Y, p.Z);
		}

		public static void UnregisterLocation(Map map, int x, int y, int z)
		{
			UnregisterLocation(new MapPoint(map, x, y, z));
		}

		private static void UnregisterLocation(MapPoint mp)
		{
			if (Locations.Contains(mp))
			{
				Locations.Remove(mp);
			}
		}

		private static void OnSystemCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null || e.Mobile.Deleted)
			{
				return;
			}

			if (e.Arguments == null || e.Arguments.Length == 0)
			{
				e.Mobile.SendMessage(0x22, "Usage: {0}{1} [spawn | despawn]", CommandSystem.Prefix, e.Command);
				return;
			}

			switch (e.Arguments[0].ToLower())
			{
				case "spawn":
					{
						SpawnBraziers();
						e.Mobile.SendMessage(0x55, "Deceit Braziers have been spawned.");
					}
					break;
				case "despawn":
					{
						DespawnBraziers();
						e.Mobile.SendMessage(0x55, "Deceit Braziers have been despawned.");
					}
					break;
				default:
					e.Mobile.SendMessage(0x22, "Usage: {0}{1} [spawn | despawn]", CommandSystem.Prefix, e.Command);
					break;
			}
		}

		public static Type GetRandomSpawn(this DeceitBrazier b)
		{
			return Spawns.GetRandom();
		}

		public static void DespawnBraziers()
		{
			foreach (var kvp in Registry)
			{
				kvp.Key.Delete();
			}

			Registry.Clear();
		}

		public static void SpawnBraziers(bool despawn = false)
		{
			if (!CMOptions.ModuleEnabled)
			{
				return;
			}

			if (despawn)
			{
				DespawnBraziers();
			}

			Locations.ForEach(
				p =>
				{
					DeceitBrazier b = FindBrazier(p);

					if (b != null)
					{
						if (!b.Deleted)
						{
							return;
						}

						Registry.Remove(b);
					}

					b = new DeceitBrazier();

					p.GetItemsInRange(p.Map, 0).ForEach(
						i =>
						{
							if (i != null && !i.Deleted && !i.Movable && i.Visible && i.RootParent == null)
							{
								i.Delete();
							}
						});

					b.MoveToWorld(p, p.Map);
					Registry.Add(b, p);
				});

			Registry.Keys.Where(b => (b != null && !b.Deleted && Locations.TrueForAll(p => !p.Equals(b)))).ToList().ForEach(
				b =>
				{
					b.Delete();
					Registry.Remove(b);
				});
		}

		public static DeceitBrazier FindBrazier(MapPoint p)
		{
			return FindBrazier(p.Location, p.Map);
		}

		public static DeceitBrazier FindBrazier(Point3D p, Map m)
		{
			return (Registry.Where(c => c.Value == p && c.Value.Map == m).Select(c => c.Key)).FirstOrDefault();
		}
	}
}