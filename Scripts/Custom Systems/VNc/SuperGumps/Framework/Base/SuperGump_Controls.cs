#region Header
//   Vorspire    _,-'/-'/  SuperGump_Controls.cs
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

using Server;
using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public virtual void AddScrollbarV(
			int x,
			int y,
			int range,
			int value,
			Action<GumpButton> prev,
			Action<GumpButton> next,
			Rectangle2D trackBounds,
			Rectangle2D prevBounds,
			Rectangle2D nextBounds,
			Tuple<int, int> trackIDs = null,
			Tuple<int, int, int> prevIDs = null,
			Tuple<int, int, int> nextIDs = null,
			bool toolTips = true)
		{
			trackIDs = trackIDs ?? new Tuple<int, int>(10740, 10742);
			prevIDs = prevIDs ?? new Tuple<int, int, int>(10701, 10702, 10700);
			nextIDs = nextIDs ?? new Tuple<int, int, int>(10721, 10722, 10720);

			range = Math.Max(1, range);
			value = Math.Max(0, Math.Min(range, value));

			int bH = Math.Max(0, Math.Min(trackBounds.Height, trackBounds.Height / range));
			int bY = Math.Max(
				trackBounds.Y, Math.Min(trackBounds.Y + trackBounds.Height, trackBounds.Y + ((bH * (value + 1)) - bH)));

			Rectangle2D barBounds = new Rectangle2D(trackBounds.X, bY, trackBounds.Width, bH);

			if (value > 0)
			{
				AddButton(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item1, prevIDs.Item2, prev);

				if (toolTips)
				{
					AddTooltip(1011067);
				}
			}
			else
			{
				AddImage(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item3);
			}

			AddImageTiled(x + trackBounds.X, y + trackBounds.Y, trackBounds.Width, trackBounds.Height, trackIDs.Item1);

			if (range > 1)
			{
				AddImageTiled(x + barBounds.X, y + barBounds.Y, barBounds.Width, barBounds.Height, trackIDs.Item2);
			}

			if (value + 1 < range)
			{
				AddButton(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item1, nextIDs.Item2, next);

				if (toolTips)
				{
					AddTooltip(1011066);
				}
			}
			else
			{
				AddImage(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item3);
			}
		}

		public virtual void AddScrollbarH(
			int x,
			int y,
			int range,
			int value,
			Action<GumpButton> prev,
			Action<GumpButton> next,
			Rectangle2D trackBounds,
			Rectangle2D prevBounds,
			Rectangle2D nextBounds,
			Tuple<int, int> trackIDs = null,
			Tuple<int, int, int> prevIDs = null,
			Tuple<int, int, int> nextIDs = null,
			bool toolTips = true)
		{
			trackIDs = trackIDs ?? new Tuple<int, int>(10740, 10742);
			prevIDs = prevIDs ?? new Tuple<int, int, int>(10731, 10732, 10730);
			nextIDs = nextIDs ?? new Tuple<int, int, int>(10711, 10712, 10710);

			range = Math.Max(1, range);
			value = Math.Max(0, Math.Min(range, value));

			int bW = Math.Max(0, Math.Min(trackBounds.Width, trackBounds.Width / range));
			int bX = Math.Max(
				trackBounds.X, Math.Min(trackBounds.X + trackBounds.Width, trackBounds.X + ((bW * (value + 1)) - bW)));

			Rectangle2D barBounds = new Rectangle2D(bX, trackBounds.Y, bW, trackBounds.Height);

			if (value > 0)
			{
				AddButton(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item1, prevIDs.Item2, prev);

				if (toolTips)
				{
					AddTooltip(1011067);
				}
			}
			else
			{
				AddImage(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item3);
			}

			AddImageTiled(x + trackBounds.X, y + trackBounds.Y, trackBounds.Width, trackBounds.Height, trackIDs.Item1);

			if (range > 1)
			{
				AddImageTiled(x + barBounds.X, y + barBounds.Y, barBounds.Width, barBounds.Height, trackIDs.Item2);
			}

			if (value + 1 < range)
			{
				AddButton(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item1, nextIDs.Item2, next);

				if (toolTips)
				{
					AddTooltip(1011066);
				}
			}
			else
			{
				AddImage(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item3);
			}
		}
	}
}