#region Header
//   Vorspire    _,-'/-'/  RangeExt.cs
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

using Server.Items;
using Server.Mobiles;
#endregion

namespace Server
{
	public class ScanRangeResult
	{
		/// <summary>
		///     Gets a value representing the center IPoint3D of the current range function query.
		/// </summary>
		public IPoint3D QueryCenter { get; protected set; }

		/// <summary>
		///     Gets a value representing the current Map used by the current range function query.
		/// </summary>
		public Map QueryMap { get; protected set; }

		/// <summary>
		///     Gets a value representing the min range of the current range function query.
		/// </summary>
		public int QueryRangeMin { get; protected set; }

		/// <summary>
		///     Gets a value representing the min range of the current range function query.
		/// </summary>
		public int QueryRangeMax { get; protected set; }

		/// <summary>
		///     Gets a value representing the current distance from 'QueryCenter'.
		/// </summary>
		public int Distance { get; protected set; }

		/// <summary>
		///     Gets a value representing the current location Point3D of the current range function query.
		/// </summary>
		public Point3D Current { get; protected set; }

		/// <summary>
		///     Gets a value representing if the current ZipRangeResult instance should be excluded in the current range function query.
		/// </summary>
		public bool Excluded { get; protected set; }

		public ScanRangeResult(IPoint3D center, Map map, Point3D current, int distance, int range)
			: this(center, map, current, distance, 0, range)
		{ }

		public ScanRangeResult(IPoint3D center, Map map, Point3D current, int distance, int minRange, int maxRange)
		{
			QueryCenter = center;
			QueryMap = map;
			Distance = Math.Max(0, distance);
			Current = current;
			QueryRangeMin = Math.Min(minRange, maxRange);
			QueryRangeMax = Math.Max(minRange, maxRange);
			Excluded = false;
		}

		/// <summary>
		///     Excludes the current ZipRangeResult instance from the current range function query.
		/// </summary>
		public void Exclude()
		{
			Excluded = true;
		}
	}

	public static class RangeExtUtility
	{
		/// <summary>
		///     Iterates through a Point3D collection representing all locations within 'range' range of 'center' on the given 'map'.
		/// </summary>
		public static void ScanRange(
			this IPoint2D center, Map map, int range, Predicate<ScanRangeResult> handler, bool avgZ = true)
		{
			ScanRange(center.ToPoint3D(), map, range, handler, avgZ);
		}

		/// <summary>
		///     Iterates through a Point3D collection representing all locations within 'range' range of 'center' on the given 'map'.
		/// </summary>
		public static void ScanRange(
			this IPoint3D center, Map map, int range, Predicate<ScanRangeResult> handler, bool avgZ = true)
		{
			if (center == null || map == null || map == Map.Internal || handler == null)
			{
				return;
			}

			range = Math.Abs(range);

			bool die = false;

			for (int x = -range; x <= range; x++)
			{
				for (int y = -range; y <= range; y++)
				{
					int distance = (int)Math.Sqrt(x * x + y * y);

					if (distance <= range)
					{
						var p = center.Clone3D(x, y);

						p.Z = avgZ ? p.GetAverageZ(map) : center.Z;

						die = handler(new ScanRangeResult(center, map, p, distance, range));
					}

					if (die)
					{
						break;
					}
				}

				if (die)
				{
					break;
				}
			}
		}

		/// <summary>
		///     Iterates through a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		/// </summary>
		public static void ScanRange(
			this IPoint2D center, Map map, int min, int max, Predicate<ScanRangeResult> handler, bool avgZ = true)
		{
			ScanRange(center.ToPoint3D(), map, min, max, handler, avgZ);
		}

		/// <summary>
		///     Iterates through a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		/// </summary>
		public static void ScanRange(
			this IPoint3D center, Map map, int min, int max, Predicate<ScanRangeResult> handler, bool avgZ = true)
		{
			if (map == null || map == Map.Internal)
			{
				return;
			}

			int ml = Math.Abs(Math.Min(min, max));
			int mr = Math.Abs(Math.Max(min, max));

			min = ml;
			max = mr;

			bool die = false;

			for (int x = -max; x <= max; x++)
			{
				for (int y = -max; y <= max; y++)
				{
					var distance = (int)Math.Sqrt(x * x + y * y);

					if (distance >= min && distance <= max)
					{
						var p = center.Clone3D(x, y);

						p.Z = avgZ ? p.GetAverageZ(map) : center.Z;

						die = handler(new ScanRangeResult(center, map, p, distance, min, max));
					}

					if (die)
					{
						break;
					}
				}

				if (die)
				{
					break;
				}
			}
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		///     The first dimension represents the distance from center, the second dimension is the collection of points at that distance.
		/// </summary>
		public static Point3D[][] ScanRangeGet(
			this IPoint2D center, Map map, int range, Predicate<ScanRangeResult> handler = null, bool avgZ = true)
		{
			return ScanRangeGet(center.ToPoint3D(), map, range, handler, avgZ);
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'range' of 'center' on the given 'map'.
		///     The first dimension represents the distance from center, the second dimension is the collection of points at that distance.
		/// </summary>
		public static Point3D[][] ScanRangeGet(
			this IPoint3D center, Map map, int range, Predicate<ScanRangeResult> handler = null, bool avgZ = true)
		{
			if (center == null || map == null || map == Map.Internal)
			{
				return new Point3D[0][];
			}

			var points = new List<Point3D>[range + 1];

			points.SetAll(
				i =>
				{
					var oc = Math.PI * Math.Sqrt(i);
					var nc = Math.PI * Math.Sqrt(i + 1);

					return new List<Point3D>((int)Math.Ceiling(nc - oc));
				});

			ScanRange(
				center,
				map,
				range,
				result =>
				{
					bool die = false;

					if (handler != null)
					{
						die = handler(result);
					}

					if (!result.Excluded)
					{
						points[result.Distance].Add(result.Current);
					}

					return die;
				},
				avgZ);

			var arr = points.ToMultiArray();

			points.Free(true);

			return arr;
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		///     The first dimension represents the distance from center, the second dimension is the collection of points at that distance.
		/// </summary>
		public static Point3D[][] ScanRangeGet(
			this IPoint2D center, Map map, int min, int max, Predicate<ScanRangeResult> handler = null, bool avgZ = true)
		{
			return ScanRangeGet(center.ToPoint3D(), map, min, max, handler, avgZ);
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		///     The first dimension represents the distance from center, the second dimension is the collection of points at that distance.
		/// </summary>
		public static Point3D[][] ScanRangeGet(
			this IPoint3D center, Map map, int min, int max, Predicate<ScanRangeResult> handler = null, bool avgZ = true)
		{
			if (center == null || map == null || map == Map.Internal)
			{
				return new Point3D[0][];
			}

			var points = new List<Point3D>[max + 1];

			points.SetAll(
				i =>
				{
					var oc = Math.PI * Math.Sqrt(i);
					var nc = Math.PI * Math.Sqrt(i + 1);

					return new List<Point3D>((int)Math.Ceiling(nc - oc));
				});

			ScanRange(
				center,
				map,
				min,
				max,
				result =>
				{
					bool die = false;

					if (handler != null)
					{
						die = handler(result);
					}

					if (!result.Excluded)
					{
						points[result.Distance].Add(result.Current);
					}

					return die;
				},
				avgZ);

			var arr = points.ToMultiArray();

			points.Free(true);

			return arr;
		}

		/// <summary>
		///     Determines if the given 'target' is within 'range' of 'source'.
		/// </summary>
		public static bool InRange2D(this IPoint2D source, IPoint2D target, int range)
		{
			if (source == null || target == null)
			{
				return false;
			}

			if (source == target)
			{
				return true;
			}

			range = Math.Abs(range);

			int x = source.X - target.X;
			int y = source.Y - target.Y;

			return Math.Sqrt(x * x + y * y) <= range;
		}

		/// <summary>
		///     Determines if the given 'target' is within 'range' of 'source', taking Z-axis into consideration.
		///     'floor' is used as a lower offset for the current Z-axis being checked.
		///     'roof' is used as an upper offset for the current Z-axis being checked.
		/// </summary>
		public static bool InRange3D(this IPoint3D source, IPoint3D target, int range, int floor, int roof)
		{
			if (source == null || target == null)
			{
				return false;
			}

			if (source == target)
			{
				return true;
			}

			int f = Math.Min(floor, roof);
			int r = Math.Max(floor, roof);

			floor = f;
			roof = r;

			return InRange2D(source, target, range) && target.Z >= source.Z + floor && target.Z <= source.Z + roof;
		}

		/// <summary>
		///     Gets a BaseMulti collection representing all BaseMultis that are within 'range' of 'center' on the given 'map'
		/// </summary>
		public static List<BaseMulti> GetMultisInRange(this IPoint2D center, Map map, int range)
		{
			return GetMultisInRange(center.ToPoint3D(), map, range);
		}

		/// <summary>
		///     Gets a BaseMulti collection representing all BaseMultis that are within 'range' of 'center' on the given 'map'
		/// </summary>
		public static List<BaseMulti> GetMultisInRange(this IPoint3D center, Map map, int range)
		{
			if (center == null || map == null)
			{
				return new List<BaseMulti>();
			}

			return
				map.GetSector(center).Multis.Where(v => v != null && (v.Contains(center) || v.InRange2D(center, range))).ToList();
		}

		/// <summary>
		///     Gets a StaticTile collection representing all StaticTiles that are within 'range' of 'center' on the given 'map'
		/// </summary>
		public static List<StaticTile> GetStaticTilesInRange(this IPoint3D center, Map map, int range)
		{
			if (center == null || map == null)
			{
				return new List<StaticTile>();
			}

			range = Math.Abs(range);

			var tiles = new List<StaticTile>((int)Math.Ceiling(Math.PI * Math.Sqrt(range)));

			ScanRange(
				center,
				map,
				range,
				r =>
				{
					if (!r.Excluded)
					{
						tiles.AddRange(map.GetStaticTiles(r.Current));
					}

					return false;
				},
				false);

			tiles.Free(false);

			return tiles;
		}

		/// <summary>
		///     Gets a LandTile collection representing all LandTiles that are within 'range' of 'center' on the given 'map'
		/// </summary>
		public static List<LandTile> GetLandTilesInRange(this IPoint3D center, Map map, int range)
		{
			if (center == null || map == null)
			{
				return new List<LandTile>();
			}

			range = Math.Abs(range);

			var tiles = new List<LandTile>((int)Math.Ceiling(Math.PI * Math.Sqrt(range)));

			ScanRange(
				center,
				map,
				range,
				r =>
				{
					if (!r.Excluded)
					{
						tiles.Add(map.GetLandTile(r.Current));
					}

					return false;
				},
				false);

			tiles.Free(false);

			return tiles;
		}

		/// <summary>
		///     Gets an ISpawner collection representing all ISpawners that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<ISpawner> GetSpawnersInRange(this IPoint3D center, Map map, int range)
		{
			if (center == null || map == null || map == Map.Internal)
			{
				return new List<ISpawner>();
			}

			range = Math.Abs(range);

			var ipe = map.GetObjectsInRange(center.ToPoint3D(), range);
			var list = ipe.OfType<ISpawner>().ToList();

			ipe.Free();

			return list;
		}

		/// <summary>
		///     Gets a collection of all objects of the given Type 'T' that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<T> GetEntitiesInRange<T>(this IPoint2D center, Map map, int range) where T : IEntity
		{
			return GetEntitiesInRange<T>(center.ToPoint3D(), map, range);
		}

		/// <summary>
		///     Gets a collection of all objects of the given Type 'T' that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<T> GetEntitiesInRange<T>(this IPoint3D center, Map map, int range) where T : IEntity
		{
			if (center == null || map == null || map == Map.Internal)
			{
				return new List<T>();
			}

			range = Math.Abs(range);

			var ipe = map.GetObjectsInRange(center.ToPoint3D(), range);
			var list = ipe.OfType<T>().ToList();

			ipe.Free();

			return list;
		}

		/// <summary>
		///     Gets a collection of all Mobiles that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<Mobile> GetMobilesInRange(this IPoint2D center, Map map, int range)
		{
			return GetEntitiesInRange<Mobile>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all Mobiles that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<Mobile> GetMobilesInRange(this IPoint3D center, Map map, int range)
		{
			return GetEntitiesInRange<Mobile>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all BaseVendors that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<BaseVendor> GetVendorsInRange(this IPoint2D center, Map map, int range)
		{
			return GetEntitiesInRange<BaseVendor>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all BaseVendors that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<BaseVendor> GetVendorsInRange(this IPoint3D center, Map map, int range)
		{
			return GetEntitiesInRange<BaseVendor>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all PlayerMobiles that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<PlayerMobile> GetPlayersInRange(this IPoint2D center, Map map, int range)
		{
			return GetEntitiesInRange<PlayerMobile>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all PlayerMobiles that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<PlayerMobile> GetPlayersInRange(this IPoint3D center, Map map, int range)
		{
			return GetEntitiesInRange<PlayerMobile>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all BaseCreatures that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<BaseCreature> GetCreaturesInRange(this IPoint2D center, Map map, int range)
		{
			return GetEntitiesInRange<BaseCreature>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all BaseCreatures that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<BaseCreature> GetCreaturesInRange(this IPoint3D center, Map map, int range)
		{
			return GetEntitiesInRange<BaseCreature>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all Items that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<Item> GetItemsInRange(this IPoint2D center, Map map, int range)
		{
			return GetEntitiesInRange<Item>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all Items that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<Item> GetItemsInRange(this IPoint3D center, Map map, int range)
		{
			return GetEntitiesInRange<Item>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all IEntity that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<IEntity> GetEntitiesInRange(this IPoint2D center, Map map, int range)
		{
			return GetEntitiesInRange<IEntity>(center, map, range);
		}

		/// <summary>
		///     Gets a collection of all IEntity that are within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static List<IEntity> GetEntitiesInRange(this IPoint3D center, Map map, int range)
		{
			return GetEntitiesInRange<IEntity>(center, map, range);
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		/// </summary>
		public static Point3D[] GetAllPointsInRange(this IPoint2D center, Map map, int range, bool avgZ = true)
		{
			return GetAllPointsInRange(center.ToPoint3D(), map, range, avgZ);
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'range' of 'center' on the given 'map'.
		/// </summary>
		public static Point3D[] GetAllPointsInRange(this IPoint3D center, Map map, int range, bool avgZ = true)
		{
			if (map == null || map == Map.Internal)
			{
				return new Point3D[0];
			}

			range = Math.Max(0, range);

			var points = new List<Point3D>((int)Math.Ceiling(Math.PI * Math.Sqrt(range)));

			ScanRange(
				center,
				map,
				range,
				result =>
				{
					if (!result.Excluded)
					{
						points.Add(result.Current);
					}

					return false;
				},
				avgZ);

			var arr = points.ToArray();

			points.Free(true);

			return arr;
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		/// </summary>
		public static Point3D[] GetAllPointsInRange(this IPoint2D center, Map map, int min, int max, bool avgZ = true)
		{
			return GetAllPointsInRange(center.ToPoint3D(), map, min, max, avgZ);
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations within 'min' and 'max' range of 'center' on the given 'map'.
		/// </summary>
		public static Point3D[] GetAllPointsInRange(this IPoint3D center, Map map, int min, int max, bool avgZ = true)
		{
			if (map == null || map == Map.Internal)
			{
				return new Point3D[0];
			}

			int ml = Math.Abs(Math.Min(min, max));
			int mr = Math.Abs(Math.Max(min, max));

			min = ml;
			max = mr;

			var oc = Math.PI * Math.Sqrt(min);
			var nc = Math.PI * Math.Sqrt(max);

			var points = new List<Point3D>((int)Math.Ceiling(nc - oc));

			ScanRange(
				center,
				map,
				min,
				max,
				result =>
				{
					if (!result.Excluded)
					{
						points.Add(result.Current);
					}

					return false;
				},
				avgZ);

			var arr = points.ToArray();

			points.Free(true);

			return arr;
		}

		public static Point2D GetRandomPoint2D(this IPoint2D start, int range)
		{
			var angle = Utility.RandomDouble() * Math.PI * 2;
			var radius = Math.Sqrt(Utility.RandomDouble()) * range;

			var x = (int)(radius * Math.Cos(angle));
			var y = (int)(radius * Math.Sin(angle));

			return start.Clone2D(x, y);
		}

		public static Point3D GetRandomPoint3D(this IPoint3D start, int range)
		{
			var angle = Utility.RandomDouble() * Math.PI * 2;
			var radius = Math.Sqrt(Utility.RandomDouble()) * range;

			var x = (int)(radius * Math.Cos(angle));
			var y = (int)(radius * Math.Sin(angle));

			return start.Clone3D(x, y);
		}
	}
}