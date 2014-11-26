#region Header
//   Vorspire    _,-'/-'/  List.cs
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
using System.Text.RegularExpressions;

using Server;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class ListGump<T> : SuperGumpList<T>
	{
		public static string DefaultTitle = "List View", DefaultEmptyText = "No entries to display.";

		protected bool WasModal { get; set; }

		public virtual string Title { get; set; }
		public virtual string EmptyText { get; set; }
		public virtual bool Minimized { get; set; }
		public virtual T Selected { get; set; }
		public virtual bool CanSearch { get; set; }
		public virtual string SearchText { get; set; }
		public virtual List<T> SearchResults { get; set; }
		public virtual MenuGumpOptions Options { get; set; }

		public MenuGump Menu { get; private set; }

		public ListGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			IEnumerable<T> list = null,
			string emptyText = null,
			string title = null,
			IEnumerable<ListGumpEntry> opts = null)
			: base(user, parent, x, y, list)
		{
			SearchResults = new List<T>();
			EmptyText = emptyText ?? DefaultEmptyText;
			Title = title ?? DefaultTitle;
			Minimized = false;
			CanMove = false;
			CanSearch = true;

			if (opts != null)
			{
				Options = new MenuGumpOptions(opts);
			}
		}

		public virtual bool IsSearching()
		{
			return (CanSearch && !String.IsNullOrWhiteSpace(SearchText));
		}

		protected override void Compile()
		{
			base.Compile();

			if (Options == null)
			{
				Options = new MenuGumpOptions();
			}
			else
			{
				Options.Clear();
			}

			CompileMenuOptions(Options);
		}

		protected override void CompileList(List<T> list)
		{
			SearchResults.Clear();

			if (IsSearching())
			{
				list.Where(
					key =>
					key != null && Regex.IsMatch(GetSearchKeyFor(key), Regex.Escape(SearchText), RegexOptions.IgnoreCase) &&
					!SearchResults.Contains(key)).ForEach(SearchResults.Add);

				if (Sorted)
				{
					SearchResults.Sort(SortCompare);
				}
			}

			base.CompileList(list);
		}

		public override void InvalidatePageCount()
		{
			if (!IsSearching())
			{
				base.InvalidatePageCount();
				return;
			}

			if (SearchResults.Count > EntriesPerPage)
			{
				if (EntriesPerPage > 0)
				{
					PageCount = (int)Math.Ceiling(SearchResults.Count / (double)EntriesPerPage);
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

		public virtual string GetSearchKeyFor(T key)
		{
			return (key != null) ? key.ToString() : "NULL";
		}

		protected virtual void OnClearSearch(GumpButton button)
		{
			SearchText = null;
			Refresh(true);
		}

		protected virtual void OnNewSearch(GumpButton button)
		{
			Send(
				new InputDialogGump(
					User,
					this,
					title: "Search",
					html: "Search " + Title + ".\nRegular Expressions are supported.",
					limit: 100,
					callback: (subBtn, input) =>
					{
						SearchText = input;
						Page = 0;
						Refresh(true);
					}));
		}

		protected virtual void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Minimized)
			{
				list.Replace("Minimize", new ListGumpEntry("Maximize", Maximize));
			}
			else
			{
				list.Replace("Maximize", new ListGumpEntry("Minimize", Minimize));
			}

			if (CanSearch)
			{
				if (!IsSearching())
				{
					list.Replace("Clear Search", new ListGumpEntry("New Search", OnNewSearch));
				}
				else
				{
					list.Replace("New Search", new ListGumpEntry("Clear Search", OnClearSearch));
				}
			}

			list.AppendEntry(new ListGumpEntry("Refresh", Refresh));

			if (CanClose)
			{
				list.AppendEntry(new ListGumpEntry("Exit", Close));
			}
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"background/header/base",
				() =>
				{
					AddBackground(0, 0, 420, 50, 9270);
					AddImageTiled(10, 10, 400, 30, 2624);
					//AddAlphaRegion(10, 10, 400, 30);
				});

			layout.Add(
				"button/header/options",
				() =>
				{
					AddButton(15, 15, 2008, 2007, ShowOptionMenu);
					AddTooltip(1015326);
				});

			layout.Add(
				"button/header/minimize",
				() =>
				{
					if (Minimized)
					{
						AddButton(390, 20, 10740, 10742, Maximize);
						AddTooltip(3002086);
					}
					else
					{
						AddButton(390, 20, 10741, 10742, Minimize);
						AddTooltip(3002085);
					}
				});

			layout.Add(
				"label/header/title",
				() => AddLabelCropped(90, 15, 285, 20, GetTitleHue(), String.IsNullOrWhiteSpace(Title) ? DefaultTitle : Title));

			if (Minimized)
			{
				return;
			}

			layout.Add("imagetiled/body/spacer", () => AddImageTiled(0, 50, 420, 10, 9274));

			var range = GetListRange();

			if (range.Count == 0)
			{
				layout.Add(
					"background/body/base",
					() =>
					{
						AddBackground(0, 55, 420, 50, 9270);
						AddImageTiled(10, 65, 400, 30, 2624);
						//AddAlphaRegion(10, 65, 400, 30); 
					});

				layout.Add(
					"label/list/empty",
					() => AddLabelCropped(15, 72, 325, 20, ErrorHue, String.IsNullOrEmpty(EmptyText) ? DefaultEmptyText : EmptyText));
			}
			else
			{
				layout.Add(
					"background/body/base",
					() =>
					{
						AddBackground(0, 55, 420, 20 + (range.Count * 30), 9270);
						AddImageTiled(10, 65, 400, (range.Count * 30), 2624);
						//AddAlphaRegion(10, 65, 400, (range.Count * 30));
					});

				layout.Add("imagetiled/body/vsep/0", () => AddImageTiled(50, 65, 5, (range.Count * 30), 9275));

				CompileEntryLayout(layout, range);
			}

			layout.Add(
				"widget/body/scrollbar",
				() =>
				AddScrollbarH(
					6,
					46,
					PageCount,
					Page,
					PreviousPage,
					NextPage,
					new Rectangle2D(30, 0, 348, 13),
					new Rectangle2D(0, 0, 28, 13),
					new Rectangle2D(380, 0, 28, 13)));
		}

		public override Dictionary<int, T> GetListRange(int index, int length)
		{
			if (!IsSearching())
			{
				return base.GetListRange(index, length);
			}

			index = Math.Max(0, Math.Min(List.Count, index));

			var list = new Dictionary<int, T>(length);
			SearchResults.ForRange(index, length, list.Add);
			return list;
		}

		protected virtual void CompileEntryLayout(SuperGumpLayout layout, Dictionary<int, T> range)
		{
			range.For((i, kv) => CompileEntryLayout(layout, range.Count, kv.Key, i, 70 + (i * 30), kv.Value));
		}

		protected virtual void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, T entry)
		{
			layout.Add("button/list/select/" + index, () => AddButton(15, yOffset, 4006, 4007, btn => SelectEntry(btn, entry)));

			layout.Add(
				"label/list/entry/" + index,
				() =>
				AddLabelCropped(65, 2 + yOffset, 325, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry)));

			if (pIndex < (length - 1))
			{
				layout.Add("imagetiled/body/hsep/" + index, () => AddImageTiled(12, 25 + yOffset, 400, 5, 9277));
			}
		}

		protected virtual void Minimize(GumpButton entry = null)
		{
			Minimized = true;

			if (Modal)
			{
				WasModal = true;
			}

			Modal = false;

			Refresh(true);
		}

		protected virtual void Maximize(GumpButton entry = null)
		{
			Minimized = false;

			if (WasModal)
			{
				Modal = true;
			}

			WasModal = false;

			Refresh(true);
		}

		protected virtual int GetTitleHue()
		{
			return HighlightHue;
		}

		protected virtual string GetLabelText(int index, int pageIndex, T entry)
		{
			return entry != null ? entry.ToString() : "NULL";
		}

		protected virtual int GetLabelHue(int index, int pageIndex, T entry)
		{
			return entry != null ? TextHue : ErrorHue;
		}

		protected virtual void SelectEntry(GumpButton button, T entry)
		{
			Selected = entry;
		}

		protected virtual void ShowOptionMenu(GumpButton button)
		{
			if (User != null && !User.Deleted && Options != null && Options.Count > 0)
			{
				Send(new MenuGump(User, Refresh(), Options, button));
			}
		}
	}
}