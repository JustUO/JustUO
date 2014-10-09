#region Header
//   Vorspire    _,-'/-'/  Menus.cs
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

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.Text;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class MenuGumpOptions : IEnumerable<ListGumpEntry>
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
			return index >= 0 && index < _InternalRegistry.Count ? _InternalRegistry.GetValue(index) : ListGumpEntry.Empty;
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
	}

	public class MenuGump : SuperGumpList<ListGumpEntry>
	{
		public int GuessWidth { get; protected set; }
		public GumpButton Clicked { get; set; }
		public ListGumpEntry Selected { get; set; }

		public MenuGump(
			PlayerMobile user, Gump parent = null, IEnumerable<ListGumpEntry> list = null, GumpButton clicked = null)
			: base(user, parent, DefaultX, DefaultY, list ?? new ListGumpEntry[0])
		{
			Clicked = clicked;

			if (Clicked != null)
			{
				if (Clicked.Parent != null)
				{
					X = Clicked.Parent.X + Clicked.X;
					Y = Clicked.Parent.Y + Clicked.Y;

					if (Parent == null)
					{
						Parent = Clicked.Parent;
					}
				}
				else if (Parent != null)
				{
					X = Parent.X;
					Y = Parent.Y;
				}
			}
			else if (Parent != null)
			{
				X = Parent.X;
				Y = Parent.Y;
			}

			ForceRecompile = true;
			CanMove = false;
			CanResize = false;
			Modal = true;
		}

		protected virtual void InvalidateWidth()
		{
			List.For(
				(i, e) =>
				{
					string str = GetLabelText(i, (int)Math.Ceiling((i + 1) / (double)EntriesPerPage), e);
					GuessWidth = Math.Max(GuessWidth, 50 + str.ComputeWidth(UOFont.Font0));
				});
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			var range = GetListRange();

			layout.Add(
				"background/body/base",
				() =>
				{
					AddBackground(0, 0, GuessWidth, 30 + (range.Count * 30), 9270);
					AddImageTiled(10, 10, GuessWidth - 20, 10 + (range.Count * 30), 2624);
					//AddAlphaRegion(10, 10, GuessWidth - 20, 10 + (range.Count * 30));
				});

			layout.Add("imagetiled/body/vsep/0", () => AddImageTiled(50, 20, 5, range.Count * 30, 9275));

			CompileEntryLayout(layout, range);

			layout.Add(
				"widget/body/scrollbar",
				() =>
				AddScrollbarH(
					6,
					6,
					PageCount,
					Page,
					PreviousPage,
					NextPage,
					new Rectangle2D(30, 0, GuessWidth - 72, 13),
					new Rectangle2D(0, 0, 28, 13),
					new Rectangle2D(GuessWidth - 40, 0, 28, 13)));
		}

		public virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, ListGumpEntry> range)
		{
			range.For((i, kv) => CompileEntryLayout(layout, range.Count, kv.Key, i, 25 + (i * 30), kv.Value));
		}

		public virtual void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, ListGumpEntry entry)
		{
			layout.Add("button/list/select/" + index, () => AddButton(15, yOffset, 4006, 4007, b => SelectEntry(b, entry)));

			layout.Add(
				"label/list/entry/" + index,
				() =>
				AddLabelCropped(
					65, 2 + yOffset, GuessWidth - 75, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry)));

			if (pIndex < (length - 1))
			{
				layout.Add("imagetiled/body/hsep/" + index, () => AddImageTiled(10, 25 + yOffset, GuessWidth - 20, 5, 9277));
			}
		}

		protected override sealed void CompileList(List<ListGumpEntry> list)
		{
			var opts = new MenuGumpOptions(list);
			CompileOptions(opts);
			list.Clear();
			list.AddRange(opts);
			InvalidateWidth();
		}

		protected virtual void CompileOptions(MenuGumpOptions list)
		{
			list.Insert(list.Count, new ListGumpEntry("Cancel", Cancel, ErrorHue));
		}

		protected virtual string GetLabelText(int index, int pageIndex, ListGumpEntry entry)
		{
			return entry != null ? entry.Label : "NULL";
		}

		protected virtual int GetLabelHue(int index, int pageIndex, ListGumpEntry entry)
		{
			return entry != null ? entry.Hue : ErrorHue;
		}

		protected virtual void SelectEntry(GumpButton button, ListGumpEntry entry)
		{
			Selected = entry;

			if (entry == null || entry.Handler == null)
			{
				Close();
				return;
			}

			entry.Handler(button);
		}

		protected virtual void Cancel(GumpButton button)
		{
			Close();
		}
	}
}