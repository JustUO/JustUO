#region Header
//   Vorspire    _,-'/-'/  SuperGumpDictionary.cs
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

using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract class SuperGumpDictionary<T1, T2> : SuperGump, ISuperGumpPages
	{
		public static int DefaultEntriesPerPage = 10;

		private int _EntriesPerPage;
		private int _Page;

		public int PageCount { get; protected set; }

		public virtual int EntriesPerPage { get { return _EntriesPerPage; } set { _EntriesPerPage = Math.Max(1, value); } }
		public virtual int Page { get { return _Page; } set { _Page = Math.Max(0, value); } }

		public Dictionary<T1, T2> List { get; set; }

		public SuperGumpDictionary(
			PlayerMobile user, Gump parent = null, int? x = null, int? y = null, IDictionary<T1, T2> list = null)
			: base(user, parent, x, y)
		{
			if (list != null)
			{
				List = new Dictionary<T1, T2>(list);
			}

			EntriesPerPage = DefaultEntriesPerPage;
		}

		protected override void Compile()
		{
			if (List == null)
			{
				List = new Dictionary<T1, T2>();
			}

			CompileList(List);
			InvalidatePageCount();

			base.Compile();
		}

		private void InvalidatePageCount()
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

		public Dictionary<int, KeyValuePair<T1, T2>> GetListRange()
		{
			return GetListRange(Page * EntriesPerPage, EntriesPerPage);
		}

		public Dictionary<int, KeyValuePair<T1, T2>> GetListRange(int index, int length)
		{
			index = Math.Max(0, Math.Min(List.Count, index));

			var list = new Dictionary<int, KeyValuePair<T1, T2>>();
			List.ForRange(index, length, list.Add);
			return list;
		}

		protected virtual void CompileList(Dictionary<T1, T2> list)
		{ }
	}
}