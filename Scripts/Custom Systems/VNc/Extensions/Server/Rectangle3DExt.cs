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
			return (rects ?? new Rectangle3D[0]).Select(rect => rect.ZFix());
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

		public static Block3D[] GetBorder(this Rectangle3D rect)
		{
			var list = new List<Block3D>();

			int z = Math.Min(rect.Start.Z, rect.End.Z);
			int h = Math.Max(rect.Start.Z, rect.End.Z) - z;

			rect.ToRectangle2D().ForEach(
				p =>
				{
					if (p.X == rect.Start.X || p.X == rect.End.X || p.Y == rect.Start.Y || p.Y == rect.End.Y)
					{
						list.Add(new Block3D(p.X, p.Y, z, h));
					}
				});

			return list.ToArray();
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
			IPooledEnumerable i = m.GetObjectsInBounds(r.ToRectangle2D());

			foreach (TEntity e in i.OfType<TEntity>().Where(o => r.Contains(o)))
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
			for (int x = r.Start.X; x <= r.End.X; x++)
			{
				for (int y = r.Start.Y; y <= r.End.Y; y++)
				{
					for (int z = r.Start.Z; z <= r.End.Z; z++)
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

			foreach (var p in r.EnumeratePoints())
			{
				action(p);
			}
		}
	}
}