#region Header
//   Vorspire    _,-'/-'/  Rectangle2DExt.cs
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
	public static class Rectangle2DExtUtility
	{
		public static Rectangle3D ToRectangle3D(this Rectangle2D r, int floor = 0, int roof = 0)
		{
			return new Rectangle3D(r.X, r.Y, floor, r.Width, r.Height, roof);
		}

		public static Rectangle2D Resize(
			this Rectangle2D r, int xOffset = 0, int yOffset = 0, int wOffset = 0, int hOffset = 0)
		{
			return new Rectangle2D(r.Start.Clone2D(xOffset, yOffset), r.End.Clone2D(wOffset, hOffset));
		}

		public static IEnumerable<TEntity> FindEntities<TEntity>(this Rectangle2D r, Map m) where TEntity : IEntity
		{
			if (m == null || m == Map.Internal)
			{
				yield break;
			}

			IPooledEnumerable i = m.GetObjectsInBounds(r);

			foreach (TEntity e in i.OfType<TEntity>().Where(o => o != null && o.Map == m && r.Contains(o)))
			{
				yield return e;
			}

			i.Free();
		}

		public static int GetBoundsHashCode(this Rectangle2D r)
		{
			unchecked
			{
				int hash = r.Width * r.Height;

				hash = (hash * 397) ^ r.Start.GetHashCode();
				hash = (hash * 397) ^ r.End.GetHashCode();

				return hash;
			}
		}

		public static int GetBoundsHashCode(this IEnumerable<Rectangle2D> list)
		{
			unchecked
			{
				return list.Aggregate(0, (hash, r) => (hash * 397) ^ GetBoundsHashCode(r));
			}
		}

		public static List<TEntity> GetEntities<TEntity>(this Rectangle2D r, Map m) where TEntity : IEntity
		{
			return FindEntities<TEntity>(r, m).ToList();
		}

		public static List<IEntity> GetEntities(this Rectangle2D r, Map m)
		{
			return FindEntities<IEntity>(r, m).ToList();
		}

		public static List<Item> GetItems(this Rectangle2D r, Map m)
		{
			return FindEntities<Item>(r, m).ToList();
		}

		public static List<Mobile> GetMobiles(this Rectangle2D r, Map m)
		{
			return FindEntities<Mobile>(r, m).ToList();
		}

		public static IEnumerable<Point2D> EnumeratePoints(this Rectangle2D r)
		{
			for (int x = r.Start.X; x <= r.End.X; x++)
			{
				for (int y = r.Start.Y; y <= r.End.Y; y++)
				{
					yield return new Point2D(x, y);
				}
			}
		}

		public static void ForEach(this Rectangle2D r, Action<Point2D> action)
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

		public static IEnumerable<Point2D> GetBorder(this Rectangle2D r)
		{
			return EnumeratePoints(r).Where(p => p.X == r.Start.X || p.X == r.End.X || p.Y == r.Start.Y || p.Y == r.End.Y);
		}
	}
}