#region Header
//   Vorspire    _,-'/-'/  SuperGumpLists.cs
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

using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract class SuperGumpList<T> : SuperGump, ISuperGumpPages
	{
		public static int DefaultEntriesPerPage = 10;

		private int _Page;

		public int PageCount { get; protected set; }

		public virtual int EntriesPerPage { get; set; }
		public virtual int Page { get { return _Page; } set { _Page = Math.Max(0, Math.Min(PageCount, value)); } }

		public virtual List<T> List { get; set; }

		public virtual bool Sorted { get; set; }

		public SuperGumpList(PlayerMobile user, Gump parent = null, int? x = null, int? y = null, IEnumerable<T> list = null)
			: base(user, parent, x, y)
		{
			List = list.Ensure().ToList();

			EntriesPerPage = DefaultEntriesPerPage;
		}

		public virtual int SortCompare(T a, T b)
		{
			return a.CompareNull(b);
		}

		public virtual void Sort()
		{
			List.Sort(SortCompare);
		}

		public virtual void Sort(Comparison<T> comparison)
		{
			if (comparison != null)
			{
				List.Sort(comparison);
				return;
			}

			Sort();
		}

		public virtual void Sort(IComparer<T> comparer)
		{
			if (comparer != null)
			{
				List.Sort(comparer);
				return;
			}

			Sort();
		}

		protected override void Compile()
		{
			if (List == null)
			{
				List = new List<T>();
			}

			CompileList(List);

			List.Free(false);

			if (Sorted)
			{
				Sort();
			}

			InvalidatePageCount();

			base.Compile();
		}

		public virtual void InvalidatePageCount()
		{
			if (List.Count > EntriesPerPage)
			{
				if (EntriesPerPage > 0)
				{
					PageCount = (int)Math.Ceiling(List.Count / (double)EntriesPerPage);
					PageCount = Math.Max(1, PageCount);
				}
				else
				{
					PageCount = 1;
				}
			}
			else
			{
				PageCount = 1;
			}

			Page = Math.Max(0, Math.Min(PageCount - 1, Page));
		}

		protected virtual void PreviousPage(GumpButton entry)
		{
			PreviousPage(true);
		}

		public virtual void PreviousPage()
		{
			PreviousPage(true);
		}

		public virtual void PreviousPage(bool recompile)
		{
			Page--;
			Refresh(recompile);
		}

		protected virtual void NextPage(GumpButton entry)
		{
			NextPage(true);
		}

		public virtual void NextPage()
		{
			NextPage(true);
		}

		public virtual void NextPage(bool recompile)
		{
			Page++;
			Refresh(recompile);
		}

		public virtual Dictionary<int, T> GetListRange()
		{
			return GetListRange(Page * EntriesPerPage, EntriesPerPage);
		}

		public virtual Dictionary<int, T> GetListRange(int index, int length)
		{
			index = Math.Max(0, Math.Min(List.Count, index));

			return List.AsParallel().Skip(index).Take(length).ToDictionary(o => index++, o => o);
		}

		protected virtual void CompileList(List<T> list)
		{ }

		protected override void OnDispose()
		{
			base.OnDispose();

			VitaNexCore.TryCatch(() => List.Free(true));

			EntriesPerPage = 0;
			PageCount = 0;
			Page = 0;
		}

		protected override void OnDisposed()
		{
			base.OnDisposed();

			List = null;
		}
	}
}