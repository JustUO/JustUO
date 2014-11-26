#region Header
//   Vorspire    _,-'/-'/  Rectangle3DExt.cs
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
#endregion

namespace Server
{
	public static class Rectangle3DExtUtility
	{
		public static IEnumerable<Rectangle3D> ZFix(this IEnumerable<Rectangle3D> rects)
		{
			if (rects == null)
			{
				yield break;
			}

			foreach (var r in rects.Select(r => r.ZFix()))
			{
				yield return r;
			}
		}

		public static Rectangle3D ZFix(this Rectangle3D rect)
		{
			Point3D start = rect.Start, end = rect.End;

			start.Z = Region.MinZ;
			end.Z = Region.MaxZ;
			rect.Start = start;
			rect.End = end;

			return new Rectangle3D(start, end);
		}

		public static Rectangle2D ToRectangle2D(this Rectangle3D r)
		{
			return new Rectangle2D(r.Start, r.End);
		}

		public static int GetBoundsHashCode(this Rectangle3D r)
		{
			unchecked
			{
				int hash = r.Width * r.Height * r.Depth;

				hash = (hash * 397) ^ r.Start.GetHashCode();
				hash = (hash * 397) ^ r.End.GetHashCode();

				return hash;
			}
		}

		public static int GetBoundsHashCode(this IEnumerable<Rectangle3D> list)
		{
			unchecked
			{
				return list.Aggregate(0, (hash, r) => (hash * 397) ^ GetBoundsHashCode(r));
			}
		}

		public static bool Contains(this RegionRect[] rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this RegionRect[] rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this Rectangle3D[] rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this Rectangle3D[] rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this Rectangle3D[] rects, IPoint2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this Rectangle3D[] rects, Point2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this List<RegionRect> rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<RegionRect> rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect.Rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<Rectangle3D> rects, IPoint3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<Rectangle3D> rects, Point3D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y, p.Z));
		}

		public static bool Contains(this List<Rectangle3D> rects, IPoint2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this List<Rectangle3D> rects, Point2D p)
		{
			return rects.Any(rect => Contains(rect, p.X, p.Y));
		}

		public static bool Contains(this Rectangle3D rect, IPoint2D p)
		{
			return Contains(rect, p.X, p.Y);
		}

		public static bool Contains(this Rectangle3D rect, IPoint3D p)
		{
			return Contains(rect, p.X, p.Y, p.Z);
		}

		public static bool Contains(this Rectangle3D rect, int x, int y)
		{
			return x >= rect.Start.X && y >= rect.Start.Y && x < rect.End.X && y < rect.End.Y;
		}

		public static bool Contains(this Rectangle3D rect, int x, int y, int z)
		{
			return Contains(rect, x, y) && z >= rect.Start.Z && z < rect.End.Z;
		}

		public static Rectangle3D Resize(
			this Rectangle3D r,
			int xOffset = 0,
			int yOffset = 0,
			int zOffset = 0,
			int wOffset = 0,
			int hOffset = 0,
			int dOffset = 0)
		{
			return new Rectangle3D(r.Start.Clone3D(xOffset, yOffset, zOffset), r.End.Clone3D(wOffset, hOffset, dOffset));
		}

		public static IEnumerable<TEntity> FindEntities<TEntity>(this Rectangle3D r, Map m) where TEntity : IEntity
		{
			if (m == null || m == Map.Internal)
			{
				yield break;
			}

			IPooledEnumerable i = m.GetObjectsInBounds(r.ToRectangle2D());

			foreach (TEntity e in i.OfType<TEntity>().Where(o => o != null && o.Map == m && r.Contains(o)))
			{
				yield return e;
			}

			i.Free();
		}

		public static List<TEntity> GetEntities<TEntity>(this Rectangle3D r, Map m) where TEntity : IEntity
		{
			return FindEntities<TEntity>(r, m).ToList();
		}

		public static List<IEntity> GetEntities(this Rectangle3D r, Map m)
		{
			return FindEntities<IEntity>(r, m).ToList();
		}

		public static List<Item> GetItems(this Rectangle3D r, Map m)
		{
			return FindEntities<Item>(r, m).ToList();
		}

		public static List<Mobile> GetMobiles(this Rectangle3D r, Map m)
		{
			return FindEntities<Mobile>(r, m).ToList();
		}

		public static IEnumerable<Point3D> EnumeratePoints(this Rectangle3D r)
		{
			if (r.Depth > 10)
			{
				Utility.PushColor(ConsoleColor.Yellow);
				"> Warning!".ToConsole();
				"> Rectangle3DExtUtility.EnumeratePoints() called on Rectangle3D with depth exceeding 10;".ToConsole();
				"> This may cause serious performance issues.".ToConsole();
				Utility.PopColor();
			}

			for (int z = r.Start.Z; z <= r.End.Z; z++)
			{
				for (int x = r.Start.X; x <= r.End.X; x++)
				{
					for (int y = r.Start.Y; y <= r.End.Y; y++)
					{
						yield return new Point3D(x, y, z);
					}
				}
			}
		}

		public static void ForEach(this Rectangle3D r, Action<Point3D> action)
		{
			if (action == null)
			{
				return;
			}

			foreach (var p in EnumeratePoints(r))
			{
				action(p);
			}
		}

		public static IEnumerable<Point3D> GetBorder(this Rectangle3D r)
		{
			return
				EnumeratePoints(r)
					.Where(
						p =>
						p.X == r.Start.X || p.X == r.End.X || p.Y == r.Start.Y || p.Y == r.End.Y || p.Z == r.Start.Z || p.Z == r.End.Z);
		}
	}
}