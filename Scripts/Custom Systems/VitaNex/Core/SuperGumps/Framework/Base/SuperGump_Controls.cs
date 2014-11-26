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
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

using Server;
using Server.Gumps;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public virtual void AddPaperdoll(int x, int y, bool bg, bool props, Mobile m)
		{
			AddPaperdoll(
				x,
				y,
				bg,
				props,
				m.Items,
				m.Body,
				m.Hue,
				m.SolidHueOverride,
				m.HairItemID,
				m.HairHue,
				m.FacialHairItemID,
				m.FacialHairHue);
		}

		public virtual void AddPaperdoll(
			int x,
			int y,
			bool bg,
			bool props,
			IEnumerable<Item> list,
			Body body,
			int hue = 0,
			int solidHue = -1,
			int hairID = 0,
			int hairHue = 0,
			int facialHairID = 0,
			int facialHairHue = 0)
		{
			if (bg)
			{
				AddBackground(x, y, 200, 250, 2620);
			}

			x += 5;
			y += 5;

			if (solidHue >= 0)
			{
				hue = solidHue;
			}

			int doll = ArtworkSupport.LookupGump(body);

			if (doll <= 0)
			{
				doll = ShrinkTable.Lookup(body, 0);

				if (doll > 0)
				{
					if (hue > 0 && hue <= 3000)
					{
						AddItem(x, y, doll, hue - 1);
					}
					else
					{
						AddItem(x, y, doll);
					}

					return;
				}
			}

			if (doll <= 0)
			{
				return;
			}

			if (hue > 0 && hue <= 3000)
			{
				AddImage(x, y, doll, hue - 1);
			}
			else
			{
				AddImage(x, y, doll);
			}

			var items = list.ToList();

			items.SortLayers();

			bool alive = !body.IsGhost;
			bool hideHair = body.IsGhost;
			bool hidePants = false;

			var propsList = props ? new List<Item>(items.Count) : null;

			foreach (Item item in items.TakeWhile(item => item.Layer.IsOrdered()).Where(item => alive || item.ItemID == 8270))
			{
				if (item.ItemID == 0x1411 || item.ItemID == 0x141A) // plate legs
				{
					hidePants = true;
				}
				else if (hidePants && item.Layer == Layer.Pants)
				{
					continue;
				}

				if (!hideHair && item.Layer == Layer.Helm)
				{
					hideHair = true;
				}

				var gump = ArtworkSupport.LookupGump(item.ItemID, body.IsFemale);

				if (gump <= 0)
				{
					continue;
				}

				int iHue = solidHue >= 0 ? solidHue : item.Hue;

				if (iHue > 0 && iHue <= 3000)
				{
					AddImage(x, y, gump, iHue - 1);
				}
				else
				{
					AddImage(x, y, gump);
				}

				if (props)
				{
					propsList.Add(item);
				}
			}

			if (alive && facialHairID > 0)
			{
				var gump = ArtworkSupport.LookupGump(facialHairID, body.IsFemale);

				if (gump > 0)
				{
					int hHue = solidHue >= 0 ? solidHue : facialHairHue;

					if (hHue > 0 && hHue <= 3000)
					{
						AddImage(x, y, gump, hHue - 1);
					}
					else
					{
						AddImage(x, y, gump);
					}
				}
			}

			if (!hideHair && hairID > 0)
			{
				var gump = ArtworkSupport.LookupGump(hairID, body.IsFemale);

				if (gump > 0)
				{
					int hHue = solidHue >= 0 ? solidHue : hairHue;

					if (hHue > 0 && hHue <= 3000)
					{
						AddImage(x, y, gump, hHue - 1);
					}
					else
					{
						AddImage(x, y, gump);
					}
				}
			}

			items.Free(true);

			if (!props)
			{
				return;
			}

			foreach (var item in propsList)
			{
				foreach (var b in item.GetPaperdollBounds())
				{
					AddLabelCropped(x + b.X, y + b.Y, b.Width, b.Height, 0, " ");
					AddProperties(item);
				}
			}

			propsList.Free(true);
		}

		public virtual void AddEnumSelect<TEnum>(
			int x,
			int y,
			int normalID,
			int pressedID,
			int labelXOffset,
			int labelYOffset,
			int labelWidth,
			int labelHeight,
			int labelHue,
			TEnum selected,
			Action<TEnum> onSelect) where TEnum : struct
		{
			if (!typeof(TEnum).IsEnum)
			{
				return;
			}

			var vals = (default(TEnum) as Enum).GetValues<TEnum>();
			var opts = new MenuGumpOptions();

			ListGumpEntry? def = null;

			foreach (var val in vals)
			{
				ListGumpEntry e = new ListGumpEntry(val.ToString(), b => onSelect(val));

				opts.AppendEntry(e);

				if (Equals(val, selected))
				{
					def = e;
				}
			}

			if (def != null)
			{
				AddMenuButton(
					x, y, normalID, pressedID, labelXOffset, labelYOffset, labelWidth, labelHeight, labelHue, opts, def.Value);
			}
		}

		public virtual void AddMenuButton(
			int x,
			int y,
			int normalID,
			int pressedID,
			int labelXOffset,
			int labelYOffset,
			int labelWidth,
			int labelHeight,
			int labelHue,
			MenuGumpOptions opts,
			ListGumpEntry defSelection)
		{
			AddButton(x, y, normalID, pressedID, b => Send(new MenuGump(User, Refresh(), opts, b)));
			AddLabel(x + labelXOffset, y + labelYOffset, labelHue, defSelection.Label ?? String.Empty);
		}

		/// <summary>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="range"></param>
		/// <param name="value"></param>
		/// <param name="prev"></param>
		/// <param name="next"></param>
		/// <param name="trackBounds">
		///     Relative to <see cref="x" /> and <see cref="y" />
		/// </param>
		/// <param name="prevBounds">
		///     Relative to <see cref="x" /> and <see cref="y" />
		/// </param>
		/// <param name="nextBounds">
		///     Relative to <see cref="x" /> and <see cref="y" />
		/// </param>
		/// <param name="trackIDs">Background, Foreground</param>
		/// <param name="prevIDs">NormalID, PressedID, DisabledID</param>
		/// <param name="nextIDs">NormalID, PressedID, DisabledID</param>
		/// <param name="toolTips"></param>
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
			value = Math.Max(0, Math.Min(range - 1, value));

			double bh = Math.Min(trackBounds.Height, Math.Max(1, trackBounds.Height / (double)range));
			double by = Math.Min(trackBounds.Height, Math.Max(bh, trackBounds.Height * ((value + 1) / (double)range))) - bh;

			Rectangle2D barBounds = new Rectangle2D(trackBounds.X, trackBounds.Y + (int)by, trackBounds.Width, (int)bh);

			/*
			int bH = Math.Max(0, Math.Min(trackBounds.Height, (int)Math.Ceiling(trackBounds.Height / (double)range)));
			int bY = Math.Max(
				trackBounds.Y, Math.Min(trackBounds.Y + trackBounds.Height, trackBounds.Y + ((bH * (value + 1)) - bH)));

			Rectangle2D barBounds = new Rectangle2D(trackBounds.X, bY, trackBounds.Width, bH);
			*/

			if (value > 0)
			{
				AddButton(x + prevBounds.X, y + prevBounds.Y, prevIDs.Item1, prevIDs.Item2, prev);

				if (toolTips)
				{
					AddTooltip(1011067);
				}
			}
			else if (prevIDs.Item3 > 0)
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
			else if (nextIDs.Item3 > 0)
			{
				AddImage(x + nextBounds.X, y + nextBounds.Y, nextIDs.Item3);
			}
		}

		/// <summary>
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <param name="range"></param>
		/// <param name="value"></param>
		/// <param name="prev"></param>
		/// <param name="next"></param>
		/// <param name="trackBounds">
		///     Relative to <see cref="x" /> and <see cref="y" />
		/// </param>
		/// <param name="prevBounds">
		///     Relative to <see cref="x" /> and <see cref="y" />
		/// </param>
		/// <param name="nextBounds">
		///     Relative to <see cref="x" /> and <see cref="y" />
		/// </param>
		/// <param name="trackIDs">Background, Foreground</param>
		/// <param name="prevIDs">NormalID, PressedID, DisabledID</param>
		/// <param name="nextIDs">NormalID, PressedID, DisabledID</param>
		/// <param name="toolTips"></param>
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

			double bw = Math.Min(trackBounds.Width, Math.Max(1, trackBounds.Width / (double)range));
			double bx = Math.Min(trackBounds.Width, Math.Max(bw, trackBounds.Width * ((value + 1) / (double)range))) - bw;

			Rectangle2D barBounds = new Rectangle2D(trackBounds.X + (int)bx, trackBounds.Y, (int)bw, trackBounds.Height);

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

		public virtual void AddClockBasic(
			int x,
			int y,
			DateTime time,
			bool bg = true,
			int bgHue = 0,
			bool center = true,
			int centerHue = 0,
			bool num = true,
			int numStyle = 0,
			Color? numColor = null,
			bool hour = true,
			Color? hourColor = null,
			bool min = true,
			Color? minColor = null,
			bool sec = true,
			Color? secColor = null)
		{
			var b = new Rectangle2D(x, y, 80, 80);
			var c = new Point2D(b.X + (b.Width / 2), b.Y + (b.Height / 2));

			if (bg)
			{
				AddImage(b.X, b.Y, 1417, bgHue);
			}

			if (center)
			{
				AddImage(c.X - 7, c.Y - 7, 1210, centerHue);
			}

			if (num)
			{
				for (int i = 1; i <= 12; i++)
				{
					var n = i.ToString(CultureInfo.InvariantCulture);

					switch (numStyle)
					{
						case 1:
							n = n.WrapUOHtmlTag("SMALL");
							break;
						case 2:
							n = n.WrapUOHtmlTag("BIG");
							break;
					}

					AddHtml(
						(c.X - 10) + (int)(-1 * (40 * Math.Cos((Math.PI / 180.0f) * (i * 30 + 90)))),
						(c.Y - 10) + (int)(-1 * (40 * Math.Sin((Math.PI / 180.0f) * (i * 30 + 90)))),
						20,
						40,
						n.WrapUOHtmlTag("B").WrapUOHtmlTag("CENTER").WrapUOHtmlColor(numColor ?? DefaultHtmlColor),
						false,
						false);
				}
			}

			var h = time.Hour;
			var m = time.Minute;
			var s = time.Second;

			if (hour)
			{
				var ha = 2.0f * Math.PI * (h + m / 60.0f) / 12.0f;
				var hhp = c.Clone2D((int)(40 * Math.Sin(ha) / 1.5f), (int)(-40 * Math.Cos(ha) / 1.5f));

				foreach (var p in c.GetLine2D(hhp))
				{
					AddHtml(p.X - 1, p.Y - 1, 3, 3, " ".WrapUOHtmlBG(hourColor ?? DefaultHtmlColor), false, false);
				}
			}

			if (min)
			{
				var ma = 2.0f * Math.PI * (m + s / 60.0f) / 60.0f;
				var mhp = c.Clone2D((int)(40 * Math.Sin(ma)), (int)(-40 * Math.Cos(ma)));

				foreach (var p in c.GetLine2D(mhp))
				{
					AddHtml(p.X - 1, p.Y - 1, 3, 3, " ".WrapUOHtmlBG(minColor ?? DefaultHtmlColor), false, false);
				}
			}

			if (!sec)
			{
				return;
			}

			var sa = 2.0f * Math.PI * s / 60.0f;
			var shp = c.Clone2D((int)(40 * Math.Sin(sa)), (int)(-40 * Math.Cos(sa)));

			foreach (var p in c.GetLine2D(shp))
			{
				AddHtml(p.X, p.Y, 1, 1, " ".WrapUOHtmlBG(secColor ?? DefaultHtmlColor), false, false);
			}
		}
	}
}