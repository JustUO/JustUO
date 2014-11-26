#region Header
//   Vorspire    _,-'/-'/  MenuGumpOptions.cs
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

namespace VitaNex.SuperGumps.UI
{
	public class MenuGumpOptions : IEnumerable<ListGumpEntry>, IEquatable<MenuGumpOptions>
	{
		private readonly Dictionary<string, ListGumpEntry> _InternalRegistry;
		private readonly List<ListGumpEntry> _ValueCache;
		private ListGumpEntry[] _InsertCache;

		public int Count { get { return _InternalRegistry.Count; } }

		public ListGumpEntry this[int index] { get { return GetEntryAt(index); } set { this[value.Label] = value; } }

		public ListGumpEntry this[string label]
		{
			get { return _InternalRegistry.ContainsKey(label) ? _InternalRegistry[label] : ListGumpEntry.Empty; }
			set
			{
				if (value == null)
				{
					if (label != null && _InternalRegistry.ContainsKey(label))
					{
						_InternalRegistry.Remove(label);
					}
				}
				else if (label != value.Label)
				{
					Replace(label, value);
				}
				else if (_InternalRegistry.ContainsKey(label))
				{
					_InternalRegistry[label] = value;
				}
				else
				{
					_InternalRegistry.Add(label, value);
				}
			}
		}

		public MenuGumpOptions()
		{
			_InternalRegistry = new Dictionary<string, ListGumpEntry>();
			_ValueCache = new List<ListGumpEntry>();
		}

		public MenuGumpOptions(IEnumerable<ListGumpEntry> options)
			: this()
		{
			AppendRange(options);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<ListGumpEntry> GetEnumerator()
		{
			return _InternalRegistry.Values.GetEnumerator();
		}

		public void Clear()
		{
			_InternalRegistry.Clear();
		}

		public void AppendRange(IEnumerable<ListGumpEntry> options)
		{
			if (options != null)
			{
				options.ForEach(AppendEntry);
			}
		}

		public void AppendEntry(ListGumpEntry entry)
		{
			if (!ListGumpEntry.IsNullOrEmpty(entry) && !_InternalRegistry.ContainsKey(entry.Label))
			{
				_InternalRegistry.Add(entry.Label, entry);
			}
		}

		public void PrependRange(IEnumerable<ListGumpEntry> options)
		{
			if (options != null)
			{
				options.ForEach(PrependEntry);
			}
		}

		public void PrependEntry(ListGumpEntry entry)
		{
			if (!ListGumpEntry.IsNullOrEmpty(entry) && !_InternalRegistry.ContainsKey(entry.Label))
			{
				Insert(0, entry);
			}
		}

		public bool RemoveEntry(ListGumpEntry entry)
		{
			return !ListGumpEntry.IsNullOrEmpty(entry) && RemoveEntry(entry.Label);
		}

		public bool RemoveEntry(string label)
		{
			return !String.IsNullOrWhiteSpace(label) && _InternalRegistry.ContainsKey(label) && _InternalRegistry.Remove(label);
		}

		public ListGumpEntry GetEntryAt(int index)
		{
			return index >= 0 && index < _InternalRegistry.Count ? _InternalRegistry.GetValueAt(index) : ListGumpEntry.Empty;
		}

		public int IndexOfEntry(ListGumpEntry entry)
		{
			return !ListGumpEntry.IsNullOrEmpty(entry) ? IndexOfLabel(entry.Label) : -1;
		}

		public int IndexOfLabel(string label)
		{
			return _InternalRegistry.Keys.IndexOf(label);
		}

		public void Insert(int index, ListGumpEntry entry)
		{
			if (ListGumpEntry.IsNullOrEmpty(entry))
			{
				return;
			}

			index = Math.Max(0, Math.Min(_InternalRegistry.Count, index));

			_InsertCache = new ListGumpEntry[_InternalRegistry.Count + 1];
			_InsertCache[index] = entry;

			int cur = 0;

			_InternalRegistry.Values.ForEach(
				val =>
				{
					if (_InsertCache[cur] != null)
					{
						cur++;
					}

					_InsertCache[cur] = val;
					cur++;
				});

			_InternalRegistry.Clear();
			AppendRange(_InsertCache);
			_InsertCache = null;
		}

		public void Replace(string label, ListGumpEntry entry)
		{
			if (String.IsNullOrWhiteSpace(label) || ListGumpEntry.IsNullOrEmpty(entry))
			{
				return;
			}

			int index = IndexOfLabel(label);

			if (index != -1)
			{
				Insert(index, entry);
				_InternalRegistry.Remove(label);
			}
			else
			{
				AppendEntry(entry);
			}
		}

		public bool Contains(string label)
		{
			return IndexOfLabel(label) != -1;
		}

		public bool Contains(ListGumpEntry entry)
		{
			return IndexOfEntry(entry) != -1;
		}

		public override int GetHashCode()
		{
			unchecked
			{
				return this.Aggregate(Count, (hash, e) => (hash * 397) ^ e.GetHashCode());
			}
		}

		public override bool Equals(object obj)
		{
			return obj is MenuGumpOptions && Equals((MenuGumpOptions)obj);
		}

		public virtual bool Equals(MenuGumpOptions other)
		{
			return !ReferenceEquals(other, null) && (ReferenceEquals(this, other) || GetHashCode() == other.GetHashCode());
		}

		public static bool operator ==(MenuGumpOptions l, MenuGumpOptions r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(MenuGumpOptions l, MenuGumpOptions r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}
	}
}