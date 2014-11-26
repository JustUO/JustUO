#region Header
//   Vorspire    _,-'/-'/  Grid.cs
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
using System.Drawing;
using System.Linq;

using Server;
#endregion

namespace VitaNex
{
	public class Grid<T> : IEnumerable<T>
	{
		private readonly List<List<T>> _InternalGrid = new List<List<T>>();

		private Size _Size = Size.Empty;

		public Action<GenericWriter, T, int, int> OnSerializeContent;
		public Func<GenericReader, Type, int, int, T> OnDeserializeContent;

		public virtual T DefaultValue { get; set; }

		public virtual int Width { get { return _Size.Width; } set { Resize(value, _Size.Height); } }
		public virtual int Height { get { return _Size.Height; } set { Resize(_Size.Width, value); } }

		public virtual T this[int x, int y]
		{
			get
			{
				var val = DefaultValue;

				if (_InternalGrid.InBounds(x) && _InternalGrid[x].InBounds(y))
				{
					val = _InternalGrid[x][y];
				}

				return val;
			}
			set
			{
				if (_InternalGrid.InBounds(x) && _InternalGrid[x].InBounds(y))
				{
					_InternalGrid[x][y] = value;
				}
			}
		}

		public int Count { get { return _InternalGrid.SelectMany(e => e).Count(e => e != null); } }
		public int Capacity { get { return Width * Height; } }

		public Grid()
			: this(1, 1)
		{
			DefaultValue = default(T);
		}

		public Grid(Grid<T> grid)
			: this(grid.Width, grid.Height)
		{
			SetAllContent(grid.GetContent);
		}

		public Grid(int width, int height)
		{
			Resize(width, height);
		}

		public Grid(GenericReader reader)
		{
			Deserialize(reader);
		}

		public void Clear()
		{
			_InternalGrid.AsEnumerable().Free(true);
			_InternalGrid.Free(true);
		}

		public void TrimExcess()
		{
			_InternalGrid.AsEnumerable().Free(false);
			_InternalGrid.Free(false);
		}

		public void Free(bool clear)
		{
			if (clear)
			{
				Clear();
				return;
			}

			TrimExcess();
		}

		public void TrimOverflow()
		{
			for (int x = Width - 1; x >= 0; x--)
			{
				if (FindCells(x, 0, 1, Height).All(e => e == null))
				{
					RemoveColumn(x);
				}
			}

			for (int y = Height - 1; y >= 0; y--)
			{
				if (FindCells(0, y, Width, 1).All(e => e == null))
				{
					RemoveRow(y);
				}
			}

			TrimExcess();
		}

		public virtual void ForEach(Action<int, int> action)
		{
			if (action == null)
			{
				return;
			}

			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					action(x, y);
				}
			}
		}

		public virtual void ForEach(Action<int, int, T> action)
		{
			if (action == null)
			{
				return;
			}

			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					action(x, y, this[x, y]);
				}
			}
		}

		public virtual void Resize(int width, int height)
		{
			width = Math.Max(1, width);
			height = Math.Max(1, height);

			while (Height != height)
			{
				if (Height < height)
				{
					InsertRow(0);
				}
				else if (Height > height)
				{
					RemoveRow(0);
				}
			}

			while (Width != width)
			{
				if (Width < width)
				{
					InsertColumn(0);
				}
				else if (Width > width)
				{
					RemoveColumn(0);
				}
			}

			_Size = new Size(width, height);
		}

		public virtual Point GetLocaton(T content)
		{
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					if (Equals(this[x, y], content))
					{
						return new Point(x, y);
					}
				}
			}

			return new Point(-1, -1);
		}

		public virtual T[] GetCells()
		{
			return GetCells(0, 0, Width, Height);
		}

		public virtual T[] GetCells(int x, int y, int w, int h)
		{
			return FindCells(x, y, w, h).ToArray();
		}

		public virtual IEnumerable<T> FindCells(int x, int y, int w, int h)
		{
			for (int col = x; col < x + w; col++)
			{
				if (!_InternalGrid.InBounds(col))
				{
					continue;
				}

				for (int row = y; row < y + h; row++)
				{
					if (!_InternalGrid[col].InBounds(row))
					{
						continue;
					}

					yield return this[col, row];
				}
			}
		}

		public virtual T[][] SelectCells(Rectangle2D bounds)
		{
			return SelectCells(bounds.X, bounds.Y, bounds.Width, bounds.Height);
		}

		public virtual T[][] SelectCells(int x, int y, int w, int h)
		{
			var list = new T[w][];

			for (int xx = 0, col = x; col < x + w; col++, xx++)
			{
				list[xx] = new T[h];

				for (int yy = 0, row = y; row < y + h; row++, yy++)
				{
					if (col >= Width || row >= Height)
					{
						list[xx][yy] = DefaultValue;
					}
					else
					{
						list[xx][yy] = this[col, row];
					}
				}
			}

			return list;
		}

		public virtual void PrependColumn()
		{
			InsertColumn(0);
		}

		public virtual void AppendColumn()
		{
			InsertColumn(Width);
		}

		public virtual void InsertColumn(int x)
		{
			if (x < 0)
			{
				x = 0;
			}

			var col = new List<T>(Height);

			col.SetAll(DefaultValue);

			if (x >= _InternalGrid.Count)
			{
				_InternalGrid.Add(col);
			}
			else
			{
				_InternalGrid.Insert(x, col);
			}

			_Size = new Size(_Size.Width + 1, _Size.Height);
		}

		public virtual void RemoveColumn(int x)
		{
			if (Width <= 1 || x < 0 || x >= Width)
			{
				return;
			}

			_InternalGrid.RemoveAt(x);
			_InternalGrid.Free(false);

			_Size = new Size(_Size.Width - 1, _Size.Height);
		}

		public virtual void PrependRow()
		{
			InsertRow(0);
		}

		public virtual void AppendRow()
		{
			InsertRow(Height);
		}

		public virtual void InsertRow(int y)
		{
			if (y < 0)
			{
				y = 0;
			}

			for (int x = 0; x < Width; x++)
			{
				if (y >= _InternalGrid[x].Count)
				{
					_InternalGrid[x].Add(DefaultValue);
				}
				else
				{
					_InternalGrid[x].Insert(y, DefaultValue);
				}
			}

			_Size = new Size(_Size.Width, _Size.Height + 1);
		}

		public virtual void RemoveRow(int y)
		{
			if (Height <= 1 || y < 0 || y >= Height)
			{
				return;
			}

			for (int x = 0; x < Width; x++)
			{
				_InternalGrid[x].RemoveAt(y);
				_InternalGrid[x].Free(false);
			}

			_Size = new Size(_Size.Width, _Size.Height - 1);
		}

		public virtual T GetContent(int x, int y)
		{
			return this[x, y];
		}

		public virtual void SetContent(int x, int y, T content)
		{
			if (y >= Height)
			{
				Resize(Width, y);
			}

			if (x >= Width)
			{
				Resize(x, Height);
			}

			this[x, y] = content;
		}

		public virtual void SetAllContent(T content, Predicate<T> replace)
		{
			ForEach(
				(x, y, c) =>
				{
					if (replace == null || replace(c))
					{
						this[x, y] = content;
					}
				});
		}

		public virtual void SetAllContent(Func<int, int, T> resolve)
		{
			if (resolve != null)
			{
				ForEach((x, y, c) => this[x, y] = resolve(x, y));
			}
		}

		public virtual void SetAllContent(Func<int, int, T, T> resolve)
		{
			if (resolve != null)
			{
				ForEach((x, y, c) => this[x, y] = resolve(x, y, c));
			}
		}

		public virtual IEnumerator<T> GetEnumerator()
		{
			return _InternalGrid.SelectMany(x => x).GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Width);
						writer.Write(Height);

						ForEach(
							(x, y, c) =>
							{
								if (c == null)
								{
									writer.Write(false);
								}
								else
								{
									writer.Write(true);
									writer.WriteType(c.GetType());
									SerializeContent(writer, c, x, y);
								}
							});
					}
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						int width = reader.ReadInt();
						int height = reader.ReadInt();

						Resize(width, height);

						ForEach(
							(x, y, c) =>
							{
								if (!reader.ReadBool())
								{
									return;
								}

								Type type = reader.ReadType();

								this[x, y] = DeserializeContent(reader, type, x, y);
							});
					}
					break;
			}
		}

		public virtual void SerializeContent(GenericWriter writer, T content, int x, int y)
		{
			if (OnSerializeContent != null)
			{
				OnSerializeContent(writer, content, x, y);
			}
		}

		public virtual T DeserializeContent(GenericReader reader, Type type, int x, int y)
		{
			return OnDeserializeContent != null ? OnDeserializeContent(reader, type, x, y) : DefaultValue;
		}

		public static implicit operator Rectangle2D(Grid<T> grid)
		{
			return new Rectangle2D(0, 0, grid.Width, grid.Height);
		}

		public static implicit operator Grid<T>(Rectangle2D rect)
		{
			return new Grid<T>(rect.Width, rect.Height);
		}
	}
}