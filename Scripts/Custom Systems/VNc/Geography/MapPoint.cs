#region Header
//   Vorspire    _,-'/-'/  MapPoint.cs
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
#endregion

namespace Server
{
	public interface IMapPoint : IPoint3D
	{
		bool Internal { get; }
		Map Map { get; set; }
		Point3D Location { get; set; }
	}

	[PropertyObject]
	public sealed class MapPoint
		: IMapPoint, IEquatable<MapPoint>, IEquatable<IMapPoint>, IEquatable<Map>, IEquatable<IPoint3D>, IEquatable<IPoint2D>
	{
		public static MapPoint Empty { get { return new MapPoint(Map.Internal, Point3D.Zero); } }

		private Map _Map;
		private Point3D _Location;

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Map Map { get { return _Map ?? (_Map = Map.Internal); } set { _Map = value ?? Map.Internal; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Internal { get { return _Map == Map.Internal; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Zero { get { return _Location == Point3D.Zero; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int X { get { return _Location.X; } set { _Location.X = Math.Max(0, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Y { get { return _Location.Y; } set { _Location.Y = Math.Max(0, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public int Z { get { return _Location.Z; } set { _Location.Z = Math.Max(Region.MinZ, Math.Min(Region.MaxZ, value)); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D Location
		{
			get { return _Location; }
			set
			{
				X = value.X;
				Y = value.Y;
				Z = value.Z;
			}
		}

		public MapPoint(Map map, IPoint2D p, int z)
			: this(map, p.X, p.Y, z)
		{ }

		public MapPoint(Map map, IPoint3D p)
			: this(map, p.X, p.Y, p.Z)
		{ }

		public MapPoint(Map map, int x, int y, int z)
		{
			_Map = map;
			_Location = new Point3D(x, y, z);
		}

		public MapPoint(GenericReader reader)
		{
			Deserialize(reader);
		}

		public bool MoveToWorld(Mobile m)
		{
			if (Internal || Zero)
			{
				return false;
			}

			m.MoveToWorld(this, Map);
			return true;
		}

		public bool MoveToWorld(Item i)
		{
			if (Internal || Zero)
			{
				return false;
			}

			i.MoveToWorld(this, Map);
			return true;
		}

		public override string ToString()
		{
			return String.Format("{0} ({1}, {2}, {3})", Map.Name, X, Y, Z);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return (_Location.GetHashCode() * 397) ^ _Map.MapIndex;
			}
		}

		public bool Equals(MapPoint other)
		{
			return _Location == other._Location && _Map == other._Map;
		}

		public bool Equals(IMapPoint other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			return _Location == other.Location && _Map == other.Map;
		}

		public bool Equals(Map other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			return _Map == other;
		}

		public bool Equals(IPoint3D other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			return _Location == other;
		}

		public bool Equals(IPoint2D other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			return _Location.ToPoint2D() == other;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (obj is MapPoint)
			{
				return Equals((MapPoint)obj);
			}

			if (obj is Map)
			{
				return Equals((Map)obj);
			}

			if (obj is IMapPoint)
			{
				return Equals((IMapPoint)obj);
			}

			if (obj is IPoint3D)
			{
				return Equals((IPoint3D)obj);
			}

			if (obj is IPoint2D)
			{
				return Equals((IPoint2D)obj);
			}

			return false;
		}

		public void Serialize(GenericWriter writer)
		{
			writer.Write(_Location);
			writer.Write(_Map);
		}

		public void Deserialize(GenericReader reader)
		{
			_Location = reader.ReadPoint3D();
			_Map = reader.ReadMap();
		}

		#region Conversion Operators
		public static implicit operator MapPoint(Mobile m)
		{
			return m == null ? Empty : new MapPoint(m.Map, m.Location);
		}

		public static implicit operator MapPoint(Item i)
		{
			return i == null ? Empty : new MapPoint(i.Map, i.Location);
		}

		public static implicit operator MapPoint(Entity e)
		{
			return e == null ? Empty : new MapPoint(e.Map, e.Location);
		}

		public static implicit operator Point3D(MapPoint m)
		{
			return ReferenceEquals(m, null) ? Point3D.Zero : m._Location;
		}

		public static implicit operator Map(MapPoint m)
		{
			return ReferenceEquals(m, null) ? Map.Internal : m.Map;
		}
		#endregion Conversion Operators

		#region MapPoint Operators
		public static bool operator ==(MapPoint l, MapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return ReferenceEquals(r, null);
			}

			return l.Equals(r);
		}

		public static bool operator !=(MapPoint l, MapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return !ReferenceEquals(r, null);
			}

			return !l.Equals(r);
		}

		public static bool operator ==(MapPoint l, IMapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return ReferenceEquals(r, null);
			}

			return l.Equals(r);
		}

		public static bool operator !=(MapPoint l, IMapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return !ReferenceEquals(r, null);
			}

			return !l.Equals(r);
		}

		public static bool operator >(MapPoint l, MapPoint r)
		{
			if (ReferenceEquals(l, null) || ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X > r.X && l.Y > r.Y && l.Z > r.Z;
		}

		public static bool operator >(MapPoint l, IMapPoint r)
		{
			if (ReferenceEquals(l, null) || ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X > r.X && l.Y > r.Y && l.Z > r.Z;
		}

		public static bool operator <(MapPoint l, MapPoint r)
		{
			if (ReferenceEquals(l, null) || ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X < r.X && l.Y < r.Y && l.Z < r.Z;
		}

		public static bool operator <(MapPoint l, IMapPoint r)
		{
			if (ReferenceEquals(l, null) || ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X < r.X && l.Y < r.Y && l.Z < r.Z;
		}

		public static bool operator >=(MapPoint l, MapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return ReferenceEquals(r, null);
			}

			if (ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X >= r.X && l.Y >= r.Y && l.Z >= r.Z;
		}

		public static bool operator >=(MapPoint l, IMapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return ReferenceEquals(r, null);
			}

			if (ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X >= r.X && l.Y >= r.Y && l.Z >= r.Z;
		}

		public static bool operator <=(MapPoint l, MapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return ReferenceEquals(r, null);
			}

			if (ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X <= r.X && l.Y <= r.Y && l.Z <= r.Z;
		}

		public static bool operator <=(MapPoint l, IMapPoint r)
		{
			if (ReferenceEquals(l, null))
			{
				return ReferenceEquals(r, null);
			}

			if (ReferenceEquals(r, null))
			{
				return false;
			}

			return l.X <= r.X && l.Y <= r.Y && l.Z <= r.Z;
		}
		#endregion MapPoint Operators
	}
}