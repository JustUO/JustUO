#region Header
//   Vorspire    _,-'/-'/  HueSelector.cs
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

using Server;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class HueSelector : SuperGump, IEnumerable<int>
	{
		public static int DefaultIcon = 4650;

		private readonly Grid<int> _HueGrid = new Grid<int>
		{
			DefaultValue = -1
		};

		private int[] _Hues = new int[0];

		public virtual int ScrollX { get; set; }
		public virtual int ScrollY { get; set; }

		public virtual int ScrollWidth { get; set; }
		public virtual int ScrollHeight { get; set; }

		public virtual Action<int> AcceptCallback { get; set; }
		public virtual Action<int> CancelCallback { get; set; }

		public virtual int this[int idx] { get { return idx >= 0 && idx < _Hues.Length ? _Hues[idx] : -1; } }
		public virtual int this[int x, int y] { get { return x >= 0 && x < _HueGrid.Width && y >= 0 && y < _HueGrid.Height ? _HueGrid[x, y] : -1; } }

		public virtual int[] Hues { get { return _Hues; } set { SetHues(value); } }
		public virtual int Selected { get; set; }

		public virtual string Title { get; set; }

		public virtual int PreviewIcon { get; set; }

		public HueSelector(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			string title = "Color Chart",
			int[] hues = null,
			int selected = -1,
			Action<int> accept = null,
			Action<int> cancel = null)
			: base(user, parent, x, y)
		{
			CanDispose = false;

			ScrollX = ScrollY = 0;
			ScrollWidth = ScrollHeight = 5;

			Selected = selected;
			AcceptCallback = accept;
			CancelCallback = cancel;

			Title = title ?? "Color Chart";

			PreviewIcon = DefaultIcon;

			SetHues(hues);
		}

		public void SetHues(params int[] hues)
		{
			hues = hues ?? new int[0];

			if (hues.Length == 0)
			{
				_Hues = hues;
			}
			else
			{
				var tmp = new List<int>(hues.Length);

				hues.ForEach(
					hue =>
					{
						if (hue >= 0 && hue <= 2999 && !tmp.Contains(hue))
						{
							tmp.Add(hue);
						}
					});

				tmp.TrimExcess();
				tmp.Sort();

				_Hues = tmp.ToArray();
				tmp.Clear();
			}

			int size = (int)Math.Ceiling(Math.Sqrt(_Hues.Length));

			_HueGrid.DefaultValue = -1;
			_HueGrid.Resize(size, size);

			int i = 0;

			for (int y = 0; y < size; y++)
			{
				for (int x = 0; x < size; x++)
				{
					_HueGrid.SetContent(x, y, i < _Hues.Length ? _Hues[i] : -1);
					++i;
				}
			}

			Refresh(true);
		}

		protected override void Compile()
		{
			if (_Hues.Length <= 0)
			{
				Selected = -1;
			}
			else if (!_Hues.Contains(Selected))
			{
				Selected = _Hues[0];
			}

			base.Compile();
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			int w = 44 + (44 * ScrollWidth);
			int h = 44 + (44 * ScrollHeight);

			w = Math.Max(176, w);
			h = Math.Max(176, h);

			/* Layout:
			 *  ___________
			 * [___________|<]
			 * |  |O O O O |^|
			 * |  |O O O O | |
			 * |  |O O O O | |
			 * |  |________|v|
			 * |__|<______>|>]
			 */

			layout.Add("panel/top", () => AddBackground(0, 0, w + 100, 30, 2620));
			layout.Add("panel/left", () => AddBackground(0, 30, 100, h + 30, 2620));
			layout.Add("panel/center", () => AddBackground(100, 30, w, h, 2620));
			layout.Add("panel/right", () => AddBackground(w + 101, 30, 26, h, 2620));
			layout.Add("panel/bottom", () => AddBackground(100, h + 31, w, 30, 2620));

			layout.Add("title", () => AddLabelCropped(10, 5, w - 20, 20, TextHue, Title));

			layout.Add(
				"preview",
				() =>
				{
					AddHtml(
						10,
						45,
						80,
						40,
						String.Format("<center><basefont color=#{0:X6}>{1}", Color.Gold.ToArgb(), GetHueLabel(Selected)),
						false,
						false);
					AddItem(20, 90, PreviewIcon, Selected);
				});

			CompileEntries(layout);

			layout.Add(
				"scrollX",
				() =>
				AddScrollbarH(
					100,
					38 + h,
					Math.Max(0, (_HueGrid.Width + 1) - ScrollWidth),
					ScrollX,
					b => ScrollLeft(),
					b => ScrollRight(),
					new Rectangle2D(30, 0, w - 60, 16),
					new Rectangle2D(7, 0, 16, 16),
					new Rectangle2D(w - 25, 0, 16, 16),
					Tuple.Create(9354, 9304),
					Tuple.Create(5607, 5603, 5607),
					Tuple.Create(5605, 5601, 5605)));

			layout.Add(
				"scrollY",
				() =>
				AddScrollbarV(
					106 + w,
					30,
					Math.Max(0, (_HueGrid.Height + 1) - ScrollHeight),
					ScrollY,
					b => ScrollUp(),
					b => ScrollDown(),
					new Rectangle2D(0, 30, 16, h - 60),
					new Rectangle2D(0, 10, 16, 16),
					new Rectangle2D(0, h - 25, 16, 16),
					Tuple.Create(9354, 9304),
					Tuple.Create(5604, 5600, 5604),
					Tuple.Create(5606, 5602, 5606)));

			layout.Add(
				"cancel",
				() =>
				{
					AddButton(w + 104, 4, 5538, 5539, OnCancel);
					AddTooltip(1006045);
				});

			layout.Add(
				"accept",
				() =>
				{
					AddButton(w + 104, h + 34, 5541, 5542, OnAccept);
					AddTooltip(1006044);
				});
		}

		protected virtual void CompileEntries(SuperGumpLayout layout)
		{
			var cells = _HueGrid.SelectCells(ScrollX, ScrollY, ScrollWidth, ScrollHeight);

			int i = 0;

			for (int y = 0; y < ScrollHeight; y++)
			{
				for (int x = 0; x < ScrollWidth; x++)
				{
					CompileEntry(layout, x, y, i++, x < cells.Length && y < cells[x].Length ? cells[x][y] : -1);
				}
			}
		}

		protected virtual void CompileEntry(SuperGumpLayout layout, int x, int y, int idx, int hue)
		{
			if (hue <= -1)
			{
				return;
			}

			int xOffset = 120 + (x * 44);
			int yOffset = 50 + (y * 44);

			layout.Add(
				"entry/" + idx,
				() =>
				{
					const int itemID = 4011;
					bool s = Selected == hue;

					AddButton(xOffset, yOffset, 24024, 24024, b => SelectEntry(x, y, idx, hue));
					AddImageTiled(xOffset, yOffset, 44, 44, 2702);

					if (s)
					{
						AddItem(xOffset, yOffset + 5, itemID, 2050);
						AddItem(xOffset, yOffset + 2, itemID, 1);
					}

					AddItem(xOffset, yOffset, itemID, hue);
				});
		}

		protected virtual void SelectEntry(int x, int y, int idx, int hue)
		{
			Selected = hue;

			OnSelected(x, y, idx, hue);
		}

		protected virtual void OnSelected(int x, int y, int idx, int hue)
		{
			Refresh(true);
		}

		public virtual void OnCancel(GumpButton b)
		{
			if (CancelCallback != null)
			{
				CancelCallback(Selected);
			}
		}

		public virtual void OnAccept(GumpButton b)
		{
			if (AcceptCallback != null)
			{
				AcceptCallback(Selected);
			}
		}

		public virtual void ScrollLeft()
		{
			ScrollX--;

			Refresh(true);
		}

		public virtual void ScrollRight()
		{
			ScrollX++;

			Refresh(true);
		}

		public virtual void ScrollUp()
		{
			ScrollY--;

			Refresh(true);
		}

		public virtual void ScrollDown()
		{
			ScrollY++;

			Refresh(true);
		}

		public virtual string GetHueLabel(int hue)
		{
			return hue <= 0 ? (Utility.RandomBool() ? "Cillit Bang" : "Industrial Bleach") : "N°. " + hue;
		}

		public virtual IEnumerator<int> GetEnumerator()
		{
			return _Hues.GetEnumerator<int>();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}
	}
}