#region Header
//   Vorspire    _,-'/-'/  AnalogClock.cs
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
using System.Drawing;
using System.Globalization;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class AnalogClock : SuperGump
	{
		public static void Initialize()
		{
			CommandUtility.Register("Clock", AccessLevel.Counselor, e => DisplayTo(e.Mobile as PlayerMobile));
		}

		public static void DisplayTo(PlayerMobile user)
		{
			bool roman = EnumerateInstances<AnalogClock>(user).Any(g => g != null && !g.IsDisposed && g.RomanNumerals);

			DisplayTo(user, !roman);
		}

		public static void DisplayTo(PlayerMobile user, bool roman)
		{
			if (user != null)
			{
				new AnalogClock(user)
				{
					RomanNumerals = roman
				}.Send();
			}
		}

		public static int DefaultRadius = 40;

		public static Color DefaultNumeralsColor = Color.Gold;
		public static Color DefaultHourHandColor = Color.Gainsboro;
		public static Color DefaultMinuteHandColor = Color.Gainsboro;
		public static Color DefaultSecondHandColor = Color.Gainsboro;

		public TimeSpan Time { get; set; }

		public int Radius { get; set; }

		public bool RomanNumerals { get; set; }

		public bool DisplayNumerals { get; set; }
		public bool DisplayHourHand { get; set; }
		public bool DisplayMinuteHand { get; set; }
		public bool DisplaySecondHand { get; set; }

		public Color ColorNumerals { get; set; }
		public Color ColorHourHand { get; set; }
		public Color ColorMinuteHand { get; set; }
		public Color ColorSecondHand { get; set; }

		public bool RealTime { get; set; }

		public AnalogClock(
			PlayerMobile user, Gump parent = null, int? x = null, int? y = null, int radius = -1, TimeSpan? time = null)
			: base(user, parent, x, y)
		{
			Radius = radius <= 0 ? DefaultRadius : radius;

			RomanNumerals = false;

			DisplayNumerals = true;
			DisplayHourHand = true;
			DisplayMinuteHand = true;
			DisplaySecondHand = true;

			ColorNumerals = DefaultNumeralsColor;
			ColorHourHand = DefaultHourHandColor;
			ColorMinuteHand = DefaultMinuteHandColor;
			ColorSecondHand = DefaultSecondHandColor;

			if (time != null)
			{
				Time = time.Value;
				RealTime = false;
			}
			else
			{
				Time = DateTime.Now.TimeOfDay;
				RealTime = true;
			}

			ForceRecompile = true;

			AutoRefresh = RealTime;
		}

		protected virtual void ComputeRefreshRate()
		{
			if (DisplaySecondHand)
			{
				AutoRefreshRate = TimeSpan.FromSeconds(1.0);
			}
			else if (DisplayMinuteHand)
			{
				AutoRefreshRate = TimeSpan.FromMinutes(1.0);
			}
			else if (DisplayHourHand)
			{
				AutoRefreshRate = TimeSpan.FromHours(1.0);
			}
			else
			{
				AutoRefresh = false;
			}
		}

		protected override void OnBeforeSend()
		{
			ComputeRefreshRate();

			base.OnBeforeSend();
		}

		protected override void OnAutoRefresh()
		{
			if (RealTime)
			{
				Time = DateTime.Now.TimeOfDay;
			}

			ComputeRefreshRate();

			base.OnAutoRefresh();
		}

		protected virtual void GetBounds(out int x, out int y, out int w, out int h)
		{
			x = y = 15;
			w = h = Radius * 2;
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			int x, y, w, h;
			GetBounds(out x, out y, out w, out h);

			var c = new Point2D(x + (w / 2), y + (h / 2));

			layout.Add("clock/bg", () => AddBackground(x - 15, y - 15, w + 30, h + 30, 2620));

			if (DisplayNumerals)
			{
				CompileNumerals(layout, c);
			}

			if (DisplayHourHand)
			{
				CompileHourHand(layout, c);
			}

			if (DisplayMinuteHand)
			{
				CompileMinuteHand(layout, c);
			}

			if (DisplaySecondHand)
			{
				CompileSecondHand(layout, c);
			}
		}

		protected virtual void CompileHourHand(SuperGumpLayout layout, Point2D center)
		{
			layout.Add(
				"clock/hand/hour",
				() =>
				{
					var ha = 2.0f * Math.PI * (Time.Hours + Time.Minutes / 60.0f) / 12.0f;
					var hhp = center.Clone2D((int)(Radius * Math.Sin(ha) / 1.5f), (int)(-Radius * Math.Cos(ha) / 1.5f));

					foreach (var p in center.GetLine2D(hhp))
					{
						AddHtml(p.X - 1, p.Y - 1, 3, 3, " ".WrapUOHtmlBG(ColorHourHand), false, false);
					}
				});
		}

		protected virtual void CompileMinuteHand(SuperGumpLayout layout, Point2D center)
		{
			layout.Add(
				"clock/hand/minute",
				() =>
				{
					var ma = 2.0f * Math.PI * (Time.Minutes + Time.Seconds / 60.0f) / 60.0f;
					var mhp = center.Clone2D((int)(Radius * Math.Sin(ma)), (int)(-Radius * Math.Cos(ma)));

					foreach (var p in center.GetLine2D(mhp))
					{
						AddHtml(p.X - 1, p.Y - 1, 3, 3, " ".WrapUOHtmlBG(ColorMinuteHand), false, false);
					}
				});
		}

		protected virtual void CompileSecondHand(SuperGumpLayout layout, Point2D center)
		{
			layout.Add(
				"clock/hand/second",
				() =>
				{
					var sa = 2.0f * Math.PI * Time.Seconds / 60.0f;
					var shp = center.Clone2D((int)(Radius * Math.Sin(sa)), (int)(-Radius * Math.Cos(sa)));

					foreach (var p in center.GetLine2D(shp))
					{
						AddHtml(p.X, p.Y, 1, 1, " ".WrapUOHtmlBG(ColorSecondHand), false, false);
					}
				});
		}

		protected virtual void CompileNumerals(SuperGumpLayout layout, Point2D center)
		{
			for (int i = 1; i <= 12; i++)
			{
				CompileNumeral(layout, center, i);
			}
		}

		protected virtual void CompileNumeral(SuperGumpLayout layout, Point2D center, int num)
		{
			layout.Add(
				"clock/numeral/" + num,
				() =>
				AddHtml(
					(center.X - (RomanNumerals ? 20 : 10)) + (int)(-1 * (Radius * Math.Cos((Math.PI / 180.0f) * (num * 30 + 90)))),
					(center.Y - 10) + (int)(-1 * (Radius * Math.Sin((Math.PI / 180.0f) * (num * 30 + 90)))),
					RomanNumerals ? 40 : 20,
					40,
					GetNumeralString(num).WrapUOHtmlTag("CENTER").WrapUOHtmlColor(ColorNumerals),
					false,
					false));
		}

		protected virtual string GetNumeralString(int num)
		{
			return (RomanNumerals ? GetRomanNumeral(num) : num.ToString(CultureInfo.InvariantCulture)).WrapUOHtmlTag("B");
		}

		protected virtual string GetRomanNumeral(int num)
		{
			switch (num)
			{
				case 1:
				case 2:
				case 3:
				case 4: // Historical clocks with roman numerals use IIII for 4
					return new String('I', num);
					/*case 4:
					return "IV";*/
				case 5:
				case 6:
				case 7:
				case 8:
					return "V" + new String('I', num - 5);
				case 9:
					return "IX";
				case 10:
				case 11:
				case 12:
					return "X" + new String('I', num - 10);
			}

			return num.ToString(CultureInfo.InvariantCulture);
		}
	}
}