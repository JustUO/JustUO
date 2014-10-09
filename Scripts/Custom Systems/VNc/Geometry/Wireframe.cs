#region Header
//   Vorspire    _,-'/-'/  Wireframe.cs
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
using System.Linq;
#endregion

namespace Server
{
	public interface IWireframe
	{
		bool Intersects(IBlock3D b);
		bool Intersects(int x, int y, int z);
		bool Intersects(int x, int y, int z, int h);
		bool Intersects(IWireframe frame);
		Rectangle3D GetBounds();
		IEnumerable<Block3D> Offset(int x = 0, int y = 0, int z = 0, int h = 0);
	}

	public struct Wireframe : IEnumerable<Block3D>, IEquatable<Wireframe>, IWireframe
	{
		public static readonly Wireframe Empty = new Wireframe();

		public Block3D[] Blocks { get; private set; }

		public int Volume { get { return Blocks != null ? Blocks.Length : 0; } }

		public Wireframe(int length)
			: this(new Block3D[length])
		{ }

		public Wireframe(IEnumerable<IBlock3D> blocks)
			: this(blocks.ToArray())
		{ }

		public Wireframe(params IBlock3D[] blocks)
			: this(blocks.Select(b => new Block3D(b)).ToArray())
		{ }

		public Wireframe(IEnumerable<Block3D> blocks)
			: this(blocks.ToArray())
		{ }

		public Wireframe(params Block3D[] blocks)
			: this()
		{
			Blocks = blocks;
		}

		public bool Intersects(IPoint3D p)
		{
			return Intersects(p.X, p.Y, p.Z);
		}

		public bool Intersects(IPoint3D p, int h)
		{
			return Intersects(p.X, p.Y, p.Z, h);
		}

		public bool Intersects(IBlock3D b)
		{
			return Intersects(b.X, b.Y, b.Z, b.H);
		}

		public bool Intersects(int x, int y, int z)
		{
			return Intersects(x, y, z, 0);
		}

		public bool Intersects(int x, int y, int z, int h)
		{
			return Blocks.Any(b => b.Intersects(x, y, z, h));
		}

		public bool Intersects(IWireframe frame)
		{
			return frame != null && Blocks.Any(b => frame.Intersects(b));
		}

		public Rectangle3D GetBounds()
		{
			return new Rectangle3D(
				new Point3D(Blocks.Min(o => o.X), Blocks.Min(o => o.Y), Blocks.Min(o => o.Z)),
				new Point3D(Blocks.Max(o => o.X), Blocks.Max(o => o.Y), Blocks.Max(o => o.Z + o.H)));
		}

		public IEnumerable<Block3D> Offset(int x = 0, int y = 0, int z = 0, int h = 0)
		{
			return this.Select(b => new Block3D(b.X + x, b.Y + y, b.Z + z, b.H + h));
		}

		public IEnumerator<Block3D> GetEnumerator()
		{
			return Blocks.AsEnumerable().GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public override bool Equals(object obj)
		{
			return obj is Wireframe ? Equals((Wireframe)obj) : base.Equals(obj);
		}

		public bool Equals(Wireframe other)
		{
			return GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Blocks.Length;
				Blocks.OrderByDescending(b => b.GetHashCode()).ForEach(b => hash = (hash * 397) ^ b.GetHashCode());
				return hash;
			}
		}

		public override string ToString()
		{
			return String.Format("Wireframe ({0:#,0} blocks)", Blocks.Length);
		}

		public static bool operator ==(Wireframe l, Wireframe r)
		{
			return l.Equals(r);
		}

		public static bool operator !=(Wireframe l, Wireframe r)
		{
			return !l.Equals(r);
		}
	}

	public class DynamicWireframe : IEquatable<DynamicWireframe>, IWireframe
	{
		public static readonly DynamicWireframe Empty = new DynamicWireframe(0);

		public bool Rendering { get; protected set; }

		public virtual List<Block3D> Blocks { get; set; }

		public virtual int Volume { get { return Blocks != null ? Blocks.Count : 0; } }

		public DynamicWireframe(params IBlock3D[] blocks)
			: this(blocks.Select(b => new Block3D(b)))
		{ }

		public DynamicWireframe(IEnumerable<IBlock3D> blocks)
			: this(blocks.Select(b => new Block3D(b)))
		{ }

		public DynamicWireframe(Wireframe frame)
			: this(frame.Blocks)
		{ }

		public DynamicWireframe()
			: this(0)
		{ }

		public DynamicWireframe(int capacity)
		{
			Blocks = new List<Block3D>(capacity);
		}

		public DynamicWireframe(params Block3D[] blocks)
		{
			Blocks = blocks.ToList();
		}

		public DynamicWireframe(IEnumerable<Block3D> blocks)
		{
			Blocks = blocks.ToList();
		}

		public DynamicWireframe(GenericReader reader)
		{
			Rendering = true;
			Deserialize(reader);
			Rendering = false;
		}

		public bool Intersects(IPoint3D p)
		{
			return Intersects(p.X, p.Y, p.Z);
		}

		public bool Intersects(IBlock3D b)
		{
			return Intersects(b.X, b.Y, b.Z, b.H);
		}

		public bool Intersects(int x, int y, int z)
		{
			return Intersects(x, y, z, 0);
		}

		public bool Intersects(int x, int y, int z, int h)
		{
			return Blocks.Any(b => b.Intersects(x, y, z, h));
		}

		public bool Intersects(IWireframe frame)
		{
			return frame != null && Blocks.Any(b => frame.Intersects(b));
		}

		public Rectangle3D GetBounds()
		{
			return new Rectangle3D(
				new Point3D(Blocks.Min(o => o.X), Blocks.Min(o => o.Y), Blocks.Min(o => o.Z)),
				new Point3D(Blocks.Max(o => o.X), Blocks.Max(o => o.Y), Blocks.Max(o => o.Z + o.H)));
		}

		public IEnumerable<Block3D> Offset(int x = 0, int y = 0, int z = 0, int h = 0)
		{
			return Blocks.Select(b => new Block3D(b.X + x, b.Y + y, b.Z + z, b.H + h));
		}

		public override bool Equals(object obj)
		{
			return obj is DynamicWireframe ? Equals((DynamicWireframe)obj) : base.Equals(obj);
		}

		public bool Equals(DynamicWireframe other)
		{
			if (ReferenceEquals(other, null))
			{
				return false;
			}

			if (ReferenceEquals(other, this))
			{
				return true;
			}

			return GetHashCode() == other.GetHashCode();
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Blocks.Count;
				Blocks.OrderByDescending(b => b.GetHashCode()).ForEach(b => hash = (hash * 397) ^ b.GetHashCode());
				return hash;
			}
		}

		public override string ToString()
		{
			return String.Format("Wireframe ({0:#,0} blocks)", Blocks.Count);
		}

		public virtual void Clear()
		{
			Blocks.Clear();
		}

		public virtual void Add(Block3D item)
		{
			Blocks.Add(item);
		}

		public virtual void AddRange(IEnumerable<Block3D> collection)
		{
			Blocks.AddRange(collection);
		}

		public virtual bool Remove(Block3D item)
		{
			return Blocks.Remove(item);
		}

		public virtual int RemoveAll(Predicate<Block3D> match)
		{
			return Blocks.RemoveAll(match);
		}

		public virtual void RemoveAt(int index)
		{
			Blocks.RemoveAt(index);
		}

		public virtual void RemoveRange(int index, int count)
		{
			Blocks.RemoveRange(index, count);
		}

		public virtual void ForEach(Action<Block3D> action)
		{
			Blocks.ForEach(action);
		}

		public virtual IEnumerator<Block3D> GetEnumerator()
		{
			return Blocks.GetEnumerator();
		}

		public virtual Block3D this[int index]
		{
			get
			{
				if (index < 0 || index >= Blocks.Count)
				{
					return Block3D.Empty;
				}

				return Blocks[index];
			}
			set
			{
				if (index >= 0 && index < Blocks.Count)
				{
					Blocks[index] = value;
				}
			}
		}

		public virtual void Serialize(GenericWriter writer)
		{
			writer.SetVersion(0);

			writer.WriteList(Blocks, writer.WriteBlock3D);
		}

		public virtual void Deserialize(GenericReader reader)
		{
			reader.GetVersion();

			Blocks = reader.ReadList(r => reader.ReadBlock3D());
		}

		public static bool operator ==(DynamicWireframe l, DynamicWireframe r)
		{
			return !ReferenceEquals(l, null) && l.Equals(r);
		}

		public static bool operator !=(DynamicWireframe l, DynamicWireframe r)
		{
			return !ReferenceEquals(l, null) && !l.Equals(r);
		}
	}
}