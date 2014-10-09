#region Header
//   Vorspire    _,-'/-'/  GeoExt.cs
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

using Server.Misc;
using Server.Mobiles;
#endregion

namespace Server
{
	public static class GeoExtUtility
	{
		public static Point3D ToPoint3D(this IPoint3D p)
		{
			return new Point3D(p.X, p.Y, p.Z);
		}

		public static Point3D ToPoint3D(this IPoint2D p, int z = 0)
		{
			return new Point3D(p.X, p.Y, z);
		}

		public static Point2D ToPoint2D(this IPoint3D p)
		{
			return new Point2D(p.X, p.Y);
		}

		public static Point2D ToPoint2D(this StaticTile t)
		{
			return new Point2D(t.X, t.Y);
		}

		public static Point3D ToPoint3D(this StaticTile t)
		{
			return new Point3D(t.X, t.Y, t.Z);
		}

		public static string ToCoordsString(this IPoint2D p, Map map)
		{
			return p.ToCoords(map).ToString();
		}

		public static Coords ToCoords(this IPoint2D p, Map m)
		{
			return new Coords(m, p);
		}

		public static Coords ToCoords(this IEntity e)
		{
			return e == null ? Coords.Zero : new Coords(e.Map, e.Location);
		}

		public static Coords ToCoords(this PlayerMobile m)
		{
			if (m == null)
			{
				return Coords.Zero;
			}

			bool online = m.IsOnline();
			return new Coords(online ? m.Map : m.LogoutMap, online ? m.Location : m.LogoutLocation);
		}

		public static Coords ToCoords(this StaticTile t, Map m)
		{
			return new Coords(m, t.ToPoint3D());
		}

		public static MapPoint ToMapPoint(this IPoint2D p, Map m, int z = 0)
		{
			return new MapPoint(m, p, z);
		}

		public static MapPoint ToMapPoint(this IPoint3D p, Map m)
		{
			return new MapPoint(m, p);
		}

		public static MapPoint ToMapPoint(this IEntity e)
		{
			return e == null ? MapPoint.Empty : new MapPoint(e.Map, e.Location);
		}

		public static MapPoint ToMapPoint(this PlayerMobile m)
		{
			if (m == null)
			{
				return MapPoint.Empty;
			}

			bool online = m.IsOnline();
			return new MapPoint(online ? m.Map : m.LogoutMap, online ? m.Location : m.LogoutLocation);
		}

		public static MapPoint ToMapPoint(this StaticTile t, Map m)
		{
			return new MapPoint(m, t.ToPoint3D());
		}

		public static Direction GetDirection(this IPoint2D from, IPoint2D to)
		{
			int dx = to.X - from.X, dy = to.Y - from.Y, adx = Math.Abs(dx), ady = Math.Abs(dy);

			if (adx >= ady * 3)
			{
				if (dx > 0)
				{
					return Direction.East;
				}

				return Direction.West;
			}

			if (ady >= adx * 3)
			{
				if (dy > 0)
				{
					return Direction.South;
				}

				return Direction.North;
			}

			if (dx > 0)
			{
				if (dy > 0)
				{
					return Direction.Down;
				}

				return Direction.Right;
			}

			if (dy > 0)
			{
				return Direction.Left;
			}

			return Direction.Up;
		}

		/// <summary>
		///     Gets a Point2D collection representing all locations between 'start' and 'end', including 'start' and 'end', on the given 'map'.
		/// </summary>
		public static Point2D[] GetLine2D(this IPoint2D start, IPoint2D end, Map map)
		{
			var path = new List<Point2D>();

			Geometry.Line2D(start.ToPoint3D(), end.ToPoint3D(), map, (p, m) => path.Add(p.ToPoint2D()));

			return path.OrderBy(p => start.GetDistance(p)).ToArray();
		}

		/// <summary>
		///     Gets a Point3D collection representing all locations between 'start' and 'end', including 'start' and 'end', on the given 'map'.
		/// </summary>
		public static Point3D[] GetLine3D(this IPoint3D start, IPoint3D end, Map map, bool avgZ = true)
		{
			var path2d = GetLine2D(start, end, map);
			var path3d = new Point3D[path2d.Length];

			if (avgZ)
			{
				path3d.SetAll(i => path2d[i].ToPoint3D(map.GetAverageZ(path2d[i].X, path2d[i].Y)));
			}
			else
			{
				path3d.SetAll(i => path2d[i].ToPoint3D(start.Z + (int)Math.Floor((end.Z - start.Z) * (i / (double)path2d.Length))));
			}

			return path3d;
		}

		public static Point2D Rotate2D(this IPoint2D from, IPoint2D to, int count)
		{
			int rx = from.X - to.X, ry = from.Y - to.Y;

			for (int i = 0; i < count; ++i)
			{
				int temp = rx;
				rx = -ry;
				ry = temp;
			}

			return new Point2D(to.X + rx, to.Y + ry);
		}

		public static Point3D Rotate3D(this IPoint3D from, IPoint3D to, int count)
		{
			return new Point3D(Rotate2D(from, to, count), from.Z);
		}

		public static Point2D Clone2D(this IPoint2D p, int xOffset = 0, int yOffset = 0)
		{
			return new Point2D(p.X + xOffset, p.Y + yOffset);
		}

		public static Point3D Clone3D(this IPoint3D p, int xOffset = 0, int yOffset = 0, int zOffset = 0)
		{
			return new Point3D(p.X + xOffset, p.Y + yOffset, p.Z + zOffset);
		}

		public static Point2D Lerp2D(this IPoint2D start, IPoint2D end, double percent)
		{
			return start.Clone2D((int)((end.X - start.X) * percent), (int)((end.Y - start.Y) * percent));
		}

		public static Point3D Lerp3D(this IPoint3D start, IPoint3D end, double percent)
		{
			return start.Clone3D(
				(int)((end.X - start.X) * percent), (int)((end.Y - start.Y) * percent), (int)((end.Z - start.Z) * percent));
		}

		public static double GetDistance(this IPoint2D from, IPoint2D to)
		{
			int x = from.X - to.X;
			int y = from.Y - to.Y;

			return Math.Sqrt((x * x) + (y * y));
		}

		public static double GetDistance(this IPoint3D from, IPoint3D to)
		{
			int x = from.X - to.X;
			int y = from.Y - to.Y;
			int z = from.Z - to.Z;

			return Math.Sqrt((x * x) + (y * y) + ((z * z) / 44.0));
		}

		public static TimeSpan GetTravelTime(this IPoint2D from, IPoint2D to, double speed)
		{
			var d = GetDistance(from, to);
			var s = (speed * 1.25);
			var tt = TimeSpan.FromSeconds(d / s);

			//Console.WriteLine("2D Travel Time: {0} / {1} = {2}", d, s, tt.TotalSeconds);

			return tt;
		}

		public static TimeSpan GetTravelTime(this IPoint3D from, IPoint3D to, double speed)
		{
			var d = GetDistance(from, to);
			var s = (speed * 1.25);
			var tt = TimeSpan.FromSeconds(d / s);

			//Console.WriteLine("3D Travel Time: {0} / {1} = {2}", d, s, tt.TotalSeconds);

			return tt;
		}

		public static double GetAngle(this Direction d, bool radians = false)
		{
			if (d.HasFlag(Direction.Running))
			{
				d &= ~Direction.Running;
			}

			if (d.HasFlag(Direction.ValueMask))
			{
				d &= ~Direction.ValueMask;
			}

			double angle = 0.0;

			switch (d)
			{
				case Direction.Up:
					angle = 0.0;
					break;
				case Direction.North:
					angle = 45.0;
					break;
				case Direction.Right:
					angle = 90.0;
					break;
				case Direction.East:
					angle = 135.0;
					break;
				case Direction.Down:
					angle = 180.0;
					break;
				case Direction.South:
					angle = 225.0;
					break;
				case Direction.Left:
					angle = 270.0;
					break;
				case Direction.West:
					angle = 315.0;
					break;
			}

			return radians ? Geometry.DegreesToRadians(angle) : angle;
		}

		public static double GetAngle(this IPoint2D source, IPoint2D target, bool radians = false)
		{
			int x = source.X - target.X;
			int y = source.Y - target.Y;

			double angle = Math.Atan2(y, x) - Math.Atan2(y, x);

			if (!radians)
			{
				angle = Geometry.RadiansToDegrees(angle);
			}

			return angle;
		}

		public static Point3D GetSurfaceTop(this IPoint2D p, Map map, bool items = true)
		{
			if (map == null || map == Map.Internal)
			{
				return p.ToPoint3D();
			}

			return GetSurfaceTop(p.ToPoint3D(Region.MaxZ), map, items);
		}

		public static Point3D GetSurfaceTop(this IPoint3D p, Map map, bool items = true)
		{
			if (map == null || map == Map.Internal)
			{
				return p.ToPoint3D();
			}

			Point3D point = Point3D.Zero;

			if (p is Item)
			{
				point = p.Clone3D(0, 0, ((Item)p).ItemData.Height);
			}
			else if (p is Mobile)
			{
				point = p.Clone3D(0, 0, 20);
			}
			/*else
			{
				point = p.Clone3D(0, 0, 5);
			}*/

			object o = map.GetTopSurface(point);

			if (o != null && o != p)
			{
				if (o is LandTile)
				{
					point.Z = ((LandTile)o).Z + ((LandTile)o).Height + 1;
				}
				else if (o is StaticTile)
				{
					point.Z = ((StaticTile)o).Z + ((StaticTile)o).Height + 1;
				}
				else if (o is Item && items)
				{
					point = ((Item)o).GetSurfaceTop();
				}
			}

			return point;
		}

		public static Point3D GetWorldTop(this IPoint2D p, Map map)
		{
			return GetWorldTop(p.ToPoint3D(Region.MaxZ), map);
		}

		public static Point3D GetWorldTop(this IPoint3D p, Map map)
		{
			if (map == null || map == Map.Internal)
			{
				return p.ToPoint3D();
			}

			Point3D point;

			if (p is Item)
			{
				point = p.Clone3D(0, 0, ((Item)p).ItemData.Height);
			}
			else if (p is Mobile)
			{
				point = p.Clone3D(0, 0, 20);
			}
			else if (p is Entity)
			{
				point = p.Clone3D(0, 0, 5);
			}
			else
			{
				point = p.ToPoint3D();
			}

			int z, a, t;

			GetAverageZ(point, map, out z, out a, out t);

			point.Z = t;

			return point;
		}

		/// <summary>
		///     ((   ,A,_,A,
		///     )) ,{=^;^=}
		///     (( {,,}#{,,}
		///     `{,,}{,,}
		/// </summary>
		public static int GetAverageZ(this IPoint2D p, Map map)
		{
			int c, a, t;

			GetAverageZ(p, map, out c, out a, out t);

			return a;
		}

		public static void GetAverageZ(this IPoint2D p, Map map, out int cur, out int avg, out int top)
		{
			var land = new {
				T = map.Tiles.GetLandTile(p.X, p.Y),
				L = map.Tiles.GetLandTile(p.X, p.Y + 1),
				R = map.Tiles.GetLandTile(p.X + 1, p.Y),
				B = map.Tiles.GetLandTile(p.X + 1, p.Y + 1)
			};

			var surf = new {
				T = GetSurfaceTop(p, map, false),
				L = GetSurfaceTop(p.Clone2D(0, 1), map, false),
				R = GetSurfaceTop(p.Clone2D(1), map, false),
				B = GetSurfaceTop(p.Clone2D(1, 1), map, false)
			};

			int zT = (land.T.Ignored || TileData.LandTable[land.T.ID].Name == "NoName") ? surf.T.Z : land.T.Z;
			int zL = (land.L.Ignored || TileData.LandTable[land.L.ID].Name == "NoName") ? surf.L.Z : land.L.Z;
			int zR = (land.R.Ignored || TileData.LandTable[land.R.ID].Name == "NoName") ? surf.R.Z : land.R.Z;
			int zB = (land.B.Ignored || TileData.LandTable[land.B.ID].Name == "NoName") ? surf.B.Z : land.B.Z;

			cur = zT;

			if (zL < cur)
			{
				cur = zL;
			}

			if (zR < cur)
			{
				cur = zR;
			}

			if (zB < cur)
			{
				cur = zB;
			}

			top = zT;

			if (zL > top)
			{
				top = zL;
			}

			if (zR > top)
			{
				top = zR;
			}

			if (zB > top)
			{
				top = zB;
			}

			int vL = zL + zR;

			if (vL < 0)
			{
				--vL;
			}

			int vR = zT + zB;

			if (vR < 0)
			{
				--vR;
			}

			avg = Math.Abs(zT - zB) > Math.Abs(zL - zR) ? vL / 2 : vR / 2;
		}
	}
}