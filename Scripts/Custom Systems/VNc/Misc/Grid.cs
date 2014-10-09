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
		private T[][] _InternalGrid;
		private Size _Size = Size.Empty;

		public Action<GenericWriter, T, int, int> OnSerializeContent;
		public Func<GenericReader, Type, int, int, T> OnDeserializeContent;

		public virtual T DefaultValue { get; set; }

		public virtual int Width { get { return _Size.Width; } set { Resize(Math.Max(0, value), _Size.Height); } }
		public virtual int Height { get { return _Size.Height; } set { Resize(_Size.Width, Math.Max(0, value)); } }

		public virtual T this[int x, int y]
		{
			get { return x >= 0 && x < _Size.Width && y >= 0 && y < _Size.Height ? _InternalGrid[x][y] : DefaultValue; }
			set
			{
				if (x >= 0 && x < _Size.Width && y >= 0 && y < _Size.Height)
				{
					_InternalGrid[x][y] = value;
				}
			}
		}

		public int Count { get { return _InternalGrid.SelectMany(e => e).Count(e => e != null); } }
		public int Capacity { get { return _Size.Width * _Size.Height; } }

		public Grid()
		{
			DefaultValue = default(T);
		}

		public Grid(Grid<T> grid)
			: this()
		{
			_InternalGrid = grid._InternalGrid.Dupe();
		}

		public Grid(int width, int height)
			: this()
		{
			Resize(width < 0 ? 0 : width, height < 0 ? 0 : height);
		}

		public Grid(GenericReader reader)
		{
			Deserialize(reader);
		}

		public virtual void ForEach(Action<T> action)
		{
			if (action != null)
			{
				ForEach((o, x, y) => action(o));
			}
		}

		public virtual void ForEach(Action<T, int, int> action)
		{
			if (action == null)
			{
				return;
			}

			for (int x = 0; x < _Size.Width; x++)
			{
				for (int y = 0; y < _Size.Height; y++)
				{
					action(_InternalGrid[x][y], x, y);
				}
			}
		}

		public virtual void Resize(int width, int height)
		{
			_Size.Width = width;
			_Size.Height = height;

			if (_InternalGrid == null)
			{
				_InternalGrid = new T[_Size.Width][];
			}

			var cols = new List<T[]>(_InternalGrid);

			if (_Size.Width > cols.Count)
			{
				cols.AddRange(new T[_Size.Width - cols.Count][]);
			}
			else if (_Size.Width < cols.Count)
			{
				cols.RemoveRange(_Size.Width, cols.Count - _Size.Width);
			}

			for (int index = 0; index < cols.Count; index++)
			{
				if (cols[index] == null)
				{
					cols[index] = new T[_Size.Height];
				}

				var entries = new List<T>(cols[index]);

				if (_Size.Height > entries.Count)
				{
					entries.AddRange(new T[_Size.Height - entries.Count]);
				}
				else if (_Size.Height < entries.Count)
				{
					entries.RemoveRange(_Size.Height, entries.Count - _Size.Height);
				}

				cols[index] = entries.ToArray();
				entries.Clear();
			}

			_InternalGrid = cols.ToArray();
			cols.Clear();
		}

		public virtual Point GetLocaton(T content)
		{
			for (int x = 0; x < _Size.Width; x++)
			{
				for (int y = 0; y < _Size.Height; y++)
				{
					if (this[x, y] != null && this[x, y].GetHashCode() == content.GetHashCode())
					{
						return new Point(x, y);
					}
				}
			}

			return new Point(-1, -1);
		}

		public virtual T[] GetCells()
		{
			return GetCells(0, 0, _Size.Width, _Size.Height);
		}

		public virtual T[] GetCells(int x, int y, int w, int h)
		{
			return FindCells(x, y, w, h).ToArray();
		}

		public virtual IEnumerable<T> FindCells(int x, int y, int w, int h)
		{
			for (int col = x; col < x + w && col < _Size.Width; col++)
			{
				for (int row = y; row < y + h && row < _Size.Height; row++)
				{
					yield return _InternalGrid[col][row];
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
					if (col >= _Size.Width || row >= _Size.Height)
					{
						list[xx][yy] = DefaultValue;
					}
					else
					{
						list[xx][yy] = _InternalGrid[col][row];
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
			InsertColumn(_Size.Width);
		}

		public virtual void InsertColumn(int x)
		{
			if (x < 0)
			{
				x = 0;
			}

			if (x >= _Size.Width)
			{
				Resize(x, _Size.Height);
			}
			else
			{
				var tmp = new List<T[]>(_InternalGrid);
				tmp.Insert(x, new T[_Size.Height]);
				_InternalGrid = tmp.ToArray();
				tmp.Clear();

				_Size.Width++;
			}
		}

		public virtual void RemoveColumn(int x)
		{
			if (_InternalGrid.Length == 0 || x < 0 || x >= _InternalGrid.Length)
			{
				return;
			}

			var tmp = new List<T[]>(_InternalGrid);
			tmp.RemoveAt(x);
			_InternalGrid = tmp.ToArray();
			tmp.Clear();

			_Size.Width--;
		}

		public virtual void PrependRow()
		{
			InsertRow(0);
		}

		public virtual void AppendRow()
		{
			InsertRow(_Size.Height);
		}

		public virtual void InsertRow(int y)
		{
			if (y < 0)
			{
				y = 0;
			}

			if (y >= _Size.Height)
			{
				Resize(_Size.Width, y);
			}
			else
			{
				for (int x = 0; x < _InternalGrid.Length; x++)
				{
					var tmp = new List<T>(_InternalGrid[x]);
					tmp.Insert(y, DefaultValue);
					_InternalGrid[x] = tmp.ToArray();
					tmp.Clear();
				}

				_Size.Height++;
			}
		}

		public virtual void RemoveRow(int y)
		{
			if (y < 0 || _InternalGrid.Length == 0)
			{
				return;
			}

			for (int x = 0; x < _InternalGrid.Length; x++)
			{
				if (_InternalGrid[x].Length == 0 || y >= _InternalGrid[x].Length)
				{
					continue;
				}

				var tmp = new List<T>(_InternalGrid[x]);
				tmp.RemoveAt(y);
				_InternalGrid[x] = tmp.ToArray();
				tmp.Clear();
			}

			_Size.Height--;
		}

		public virtual T GetContent(int x, int y)
		{
			return this[x, y];
		}

		public virtual void SetContent(int x, int y, T content)
		{
			if (x >= _Size.Width)
			{
				Resize(x + 1, _Size.Height);
			}

			if (y >= _Size.Height)
			{
				Resize(_Size.Width, y + 1);
			}

			this[x, y] = content;
		}

		public virtual void SetAllContent(T content, Predicate<T> replace)
		{
			for (int x = 0; x < _Size.Width; x++)
			{
				for (int y = 0; y < _Size.Height; y++)
				{
					if (replace != null && replace(_InternalGrid[x][y]))
					{
						_InternalGrid[x][y] = content;
					}
				}
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
						writer.Write(_Size.Width);
						writer.Write(_Size.Height);

						for (int x = 0; x < _Size.Width; x++)
						{
							for (int y = 0; y < _Size.Height; y++)
							{
								T content = GetContent(x, y);

								if (content == null)
								{
									writer.Write(false);
								}
								else
								{
									writer.Write(true);
									writer.WriteType(content.GetType());
									SerializeContent(writer, content, x, y);
								}
							}
						}
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
						Resize(reader.ReadInt(), reader.ReadInt());

						for (int x = 0; x < _Size.Width; x++)
						{
							for (int y = 0; y < _Size.Height; y++)
							{
								if (reader.ReadBool())
								{
									Type type = reader.ReadType();

									if (type == null)
									{
										continue;
									}

									SetContent(x, y, DeserializeContent(reader, type, x, y));
								}
							}
						}
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