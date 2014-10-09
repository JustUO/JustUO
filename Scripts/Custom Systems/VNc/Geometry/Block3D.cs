#region Header
//   Vorspire    _,-'/-'/  Block3D.cs
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
using System.Collections;
using System.Collections.Generic;
#endregion

namespace Server
{
	public interface IBlock3D : IPoint3D, IEquatable<IBlock3D>, IEnumerable<IPoint3D>
	{
		int H { get; set; }

		bool Intersects(IPoint3D p);
		bool Intersects(IBlock3D b);
		bool Intersects(int z);
		bool Intersects(int x, int y, int z);
		bool Intersects(int x, int y, int z, int h);

		IEnumerable<IPoint3D> Scan();
	}

	public struct Block3D : IBlock3D
	{
		public static readonly Block3D Empty = new Block3D(0, 0, 0, 0);

		public int X { get; set; }
		public int Y { get; set; }
		public int Z { get; set; }
		public int H { get; set; }

		public Block3D(IBlock3D b)
			: this(b.X, b.Y, b.Z, b.H)
		{ }

		public Block3D(IPoint3D p, int h)
			: this(p.X, p.Y, p.Z, h)
		{ }

		public Block3D(int x, int y, int z, int h)
			: this()
		{
			X = x;
			Y = y;
			Z = z;
			H = h;
		}

		public bool Intersects(IPoint3D p)
		{
			return Intersects(p.X, p.Y, p.Z);
		}

		public bool Intersects(IBlock3D b)
		{
			return Intersects(b.X, b.Y, b.Z, b.H);
		}

		public bool Intersects(int z)
		{
			return Intersects(X, Y, z);
		}

		public bool Intersects(int z, int h)
		{
			return Intersects(X, Y, z, h);
		}

		public bool Intersects(int x, int y, int z)
		{
			return Intersects(x, y, z, 0);
		}

		public bool Intersects(int x, int y, int z, int h)
		{
			return X == x && Y == y && z <= Z + H && z + h >= Z;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<IPoint3D> GetEnumerator()
		{
			return Scan().GetEnumerator();
		}

		public IEnumerable<IPoint3D> Scan()
		{
			for (int z = Z; z <= Z + H; z++)
			{
				yield return this.ToPoint3D(z);
			}
		}

		public override string ToString()
		{
			return String.Format("{0}, {1}, {2}, {3}", X, Y, Z, H);
		}

		public bool Equals(IBlock3D b)
		{
			return b != null && X == b.X && Y == b.Y && Z == b.Z && H == b.H;
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(obj, null) && (base.Equals(obj) || (obj is IBlock3D && Equals((IBlock3D)obj)));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = 0;
				hash = (hash * 397) ^ X;
				hash = (hash * 397) ^ Y;
				hash = (hash * 397) ^ Z;
				hash = (hash * 397) ^ H;
				return hash;
			}
		}

		public static bool operator ==(Block3D a, IBlock3D b)
		{
			return a.Equals(b);
		}

		public static bool operator !=(Block3D a, IBlock3D b)
		{
			return !a.Equals(b);
		}

		public static bool operator ==(IBlock3D a, Block3D b)
		{
			return b.Equals(a);
		}

		public static bool operator !=(IBlock3D a, Block3D b)
		{
			return !b.Equals(a);
		}

		public static implicit operator Block3D(Point3D p)
		{
			return new Block3D(p, 0);
		}

		public static implicit operator Point3D(Block3D p)
		{
			return new Point3D(p.X, p.Y, p.Z);
		}
	}
}