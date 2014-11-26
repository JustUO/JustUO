#region Header
//   Vorspire    _,-'/-'/  Tree.cs
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
using System.Drawing;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public abstract class TreeGump : SuperGumpList<TreeGumpNode>
	{
		public static string DefaultTitle = "Tree View";

		public string Title { get; set; }
		public Color TitleColor { get; set; }

		public Color HtmlColor { get; set; }

		public TreeGumpNode SelectedNode { get; set; }

		public virtual Dictionary<TreeGumpNode, Action<Rectangle2D, int, TreeGumpNode>> Nodes { get; private set; }

		public TreeGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			IEnumerable<TreeGumpNode> nodes = null,
			TreeGumpNode selected = null,
			string title = null)
			: base(user, parent, x, y, nodes)
		{
			Nodes = new Dictionary<TreeGumpNode, Action<Rectangle2D, int, TreeGumpNode>>();

			Title = title ?? DefaultTitle;
			TitleColor = DefaultHtmlColor;

			HtmlColor = DefaultHtmlColor;

			HighlightHue = 1258;
			TextHue = 2049;

			EntriesPerPage = 14;

			SelectedNode = selected ?? String.Empty;

			ForceRecompile = true;
			CanMove = true;
			Sorted = true;
		}

		protected override void Compile()
		{
			if (Nodes == null)
			{
				Nodes = new Dictionary<TreeGumpNode, Action<Rectangle2D, int, TreeGumpNode>>();
			}

			CompileNodes(Nodes);

			base.Compile();
		}

		protected virtual void CompileNodes(Dictionary<TreeGumpNode, Action<Rectangle2D, int, TreeGumpNode>> list)
		{ }

		protected override void CompileList(List<TreeGumpNode> list)
		{
			foreach (var n in Nodes.Keys)
			{
				list.AddOrReplace(n);
			}

			var nodes = new List<TreeGumpNode>(list.Count);

			foreach (var n in list)
			{
				foreach (var p in n.GetParents())
				{
					nodes.AddOrReplace(p);
				}

				nodes.AddOrReplace(n);
			}

			if (SelectedNode.HasParent)
			{
				nodes.AddOrReplace(SelectedNode.Parent);
			}

			var selectedParents = SelectedNode.GetParents().ToArray();

			nodes.RemoveAll(
				c =>
				{
					var parents = c.GetParents().ToArray();

					if (parents.Length > 0)
					{
						if (parents.Length <= selectedParents.Length && c != SelectedNode && !parents.Contains(SelectedNode) &&
							!selectedParents.Any(p => p == c || c.Parent == p))
						{
							return true;
						}

						if (parents.Length > selectedParents.Length && c.Parent != SelectedNode)
						{
							return true;
						}
					}

					return false;
				});

			list.Clear();
			list.AddRange(nodes);

			nodes.Free(true);

			base.CompileList(list);
		}

		public override int SortCompare(TreeGumpNode l, TreeGumpNode r)
		{
			int res = 0;

			if (l.CompareNull(r, ref res))
			{
				return res;
			}

			return Insensitive.Compare(l.FullName, r.FullName);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"body/bg",
				() =>
				{
					AddBackground(0, 43, 600, 400, 9260);
					AddImage(15, 18, 1419);

					AddBackground(15, 58, 234, 50, 9270);
					AddImage(92, 0, 1417);
				});
			layout.Add("body/mainbutton", () => AddButton(101, 9, 5545, 5546, MainButtonHandler));

			layout.Add("panel/left", () => AddBackground(45, 115, 204, 312, 9270));
			layout.Add("panel/left/overlay", () => AddImageTiled(55, 125, 184, 292, 1280));

			layout.Add("panel/right", () => AddBackground(255, 58, 330, 370, 9270));
			layout.Add("panel/right/overlay", () => AddImageTiled(265, 68, 310, 350, 1280));

			CompileTreeLayout(layout);

			layout.Add(
				"title",
				() => AddHtml(25, 78, 214, 40, Title.WrapUOHtmlTag("CENTER").WrapUOHtmlColor(TitleColor, false), false, false));

			if (SelectedNode.IsEmpty)
			{
				CompileEmptyNodeLayout(layout, 265, 70, 310, 350, List.IndexOf(SelectedNode), SelectedNode);
			}
			else
			{
				CompileNodeLayout(layout, 265, 70, 310, 350, List.IndexOf(SelectedNode), SelectedNode);
			}

			layout.Add("dragon", () => AddImage(567, 0, 10441, 0));
		}

		protected virtual void CompileTreeLayout(SuperGumpLayout layout)
		{
			layout.Add(
				"tree/scrollbar",
				() =>
				{
					AddBackground(15, 115, 25, 312, 9270);

					AddScrollbarV(
						15,
						115,
						PageCount,
						Page,
						PreviousPage,
						NextPage,
						new Rectangle2D(6, 40, 13, 234),
						new Rectangle2D(6, 5, 13, 28),
						new Rectangle2D(6, 280, 13, 28),
						// track, handle
						Tuple.Create(10740, 10742),
						// normal, pressed, inactive
						Tuple.Create(10701, 10702, 10700),
						// normal, pressed, inactive
						Tuple.Create(10721, 10722, 10720));
				});

			var range = GetListRange();

			const int catSpacing = 21;

			int cIndex = 0;

			foreach (var c in range.Values)
			{
				int index = cIndex;
				var depth = c.Depth;

				layout.AddBefore(
					"panel/left",
					"tree/button/" + index,
					() => AddButton(55, 125 + (catSpacing * index), 1122, 1124, btn => SelectNode(index, c)));

				layout.Add(
					"tree/node/" + index,
					() =>
					{
						int offset = Math.Min(150, depth * 10);

						//AddBackground(60 + offset, 125 + (catSpacing * index), 174 - offset, 20, 9200);

						AddLabelCropped(
							65 + offset,
							125 + (catSpacing * index),
							165 - offset,
							20,
							SelectedNode == c || SelectedNode.IsChildOf(c) ? HighlightHue : TextHue,
							String.IsNullOrWhiteSpace(c.Name) ? "..." : c.Name);
					});

				++cIndex;
			}
		}

		protected virtual void CompileEmptyNodeLayout(
			SuperGumpLayout layout, int x, int y, int w, int h, int index, TreeGumpNode node)
		{ }

		protected virtual void CompileNodeLayout(
			SuperGumpLayout layout, int x, int y, int w, int h, int index, TreeGumpNode node)
		{
			if (Nodes == null || Nodes.Count <= 0)
			{
				return;
			}

			Action<Rectangle2D, Int32, TreeGumpNode> nodeLayout;

			if (Nodes.TryGetValue(node.FullName, out nodeLayout) && nodeLayout != null)
			{
				layout.Add("node/page/" + index, () => nodeLayout(new Rectangle2D(x, y, w, h), index, node));
			}
		}

		public override void InvalidatePageCount()
		{
			//int oldpc = PageCount;

			PageCount = 1 + Math.Max(0, List.Count - EntriesPerPage);

			/*if (oldpc < PageCount)
			{
				++Page;
			}
			else if (oldpc > PageCount)
			{
				--Page;
			}*/

			Page = Math.Max(0, Math.Min(PageCount - 1, Page));
		}

		public override Dictionary<int, TreeGumpNode> GetListRange()
		{
			return GetListRange(Page, EntriesPerPage);
		}

		public void SelectNode(int index, TreeGumpNode node)
		{
			if (SelectedNode != node)
			{
				SelectedNode = node;
			}
			else if (SelectedNode.HasParent)
			{
				SelectedNode = SelectedNode.Parent;
			}
			else
			{
				SelectedNode = String.Empty;
			}

			OnSelected(index, SelectedNode);
		}

		protected virtual void OnSelected(int index, TreeGumpNode node)
		{
			Refresh(true);
		}

		protected virtual void MainButtonHandler(GumpButton b)
		{
			Refresh();
		}

		protected override void OnDispose()
		{
			base.OnDispose();

			VitaNexCore.TryCatch(Nodes.Clear);
		}

		protected override void OnDisposed()
		{
			base.OnDisposed();

			SelectedNode = null;

			Nodes = null;
		}
	}
}