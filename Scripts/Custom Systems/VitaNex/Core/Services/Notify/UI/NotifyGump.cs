#region Header
//   Vorspire    _,-'/-'/  NotifyGump.cs
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
using System.Linq;
using System.Text.RegularExpressions;

using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.Text;
#endregion

namespace VitaNex.Notify
{
	public class NotifyGump : SuperGump
	{
		public static event Action<NotifyGump> OnNotify;

		public static TimeSpan DefaultAnimDuration = TimeSpan.FromMilliseconds(500.0);
		public static TimeSpan DefaultPauseDuration = TimeSpan.FromSeconds(3.0);

		public static Size SizeMin = new Size(50, 50);
		public static Size SizeMax = new Size(800, 150);

		public enum AnimState
		{
			Show,
			Hide,
			Pause
		}

		public TimeSpan AnimDuration { get; set; }
		public TimeSpan PauseDuration { get; set; }

		public string Html { get; set; }
		public Color HtmlColor { get; set; }
		public int HtmlIndent { get; set; }

		public int BorderID { get; set; }
		public int BorderSize { get; set; }
		public bool BorderAlpha { get; set; }

		public int BackgroundID { get; set; }
		public bool BackgroundAlpha { get; set; }

		public int WidthMax { get; set; }
		public int HeightMax { get; set; }

		public bool AutoClose { get; set; }

		public int Frame { get; private set; }
		public AnimState State { get; private set; }

		public int FrameCount { get { return (int)Math.Ceiling(Math.Max(100.0, AnimDuration.TotalMilliseconds) / 100.0); } }
		public int FrameHeight { get; private set; }
		public int FrameWidth { get; private set; }

		public override bool InitPolling { get { return true; } }

		public NotifyGump(PlayerMobile user, string html)
			: base(user, null, 0, 120)
		{
			AnimDuration = DefaultAnimDuration;
			PauseDuration = DefaultPauseDuration;

			Html = html ?? String.Empty;
			HtmlColor = Color.White;
			HtmlIndent = 10;

			BorderSize = 4;
			BorderID = 9204;
			BorderAlpha = false;

			BackgroundID = 2624;
			BackgroundAlpha = true;

			AutoClose = true;

			Frame = 0;
			State = AnimState.Pause;

			CanMove = false;
			CanResize = false;

			ForceRecompile = true;
			AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
			AutoRefresh = true;

			var sMin = GetSizeMin();
			var sMax = GetSizeMax();

			WidthMax = Math.Max(sMin.Width, Math.Min(sMax.Width, 250));
			HeightMax = Math.Max(sMin.Height, Math.Min(sMax.Height, 30));
		}

		protected virtual Size GetSizeMin()
		{
			return SizeMin;
		}

		protected virtual Size GetSizeMax()
		{
			return SizeMax;
		}

		protected override void Compile()
		{
			base.Compile();

			var sMin = GetSizeMin();
			var sMax = GetSizeMax();

			WidthMax = Math.Max(sMin.Width, Math.Min(sMax.Width, WidthMax));
			HeightMax = Math.Max(sMin.Height, Math.Min(sMax.Height, HeightMax));

			var lines =
				Html.ParseBBCode(HtmlColor)
					.Replace("<br>", "\n")
					.Replace("<BR>", "\n")
					.Split('\n')
					.Select(line => Regex.Replace(line, @"<[^>]*>", String.Empty))
					.Not(String.IsNullOrWhiteSpace)
					.Select(line => line.ComputeSize(UOFont.Font0))
					.ToArray();

			int wm = WidthMax - (BorderSize * 2);
			int h =
				lines.Sum(
					s => (s.Height + UOFont.Font0.LineSpacing) * (s.Width <= wm ? 1 : (int)Math.Ceiling(s.Width / (double)wm)));

			HeightMax = Math.Max(sMin.Height, Math.Min(!Initialized ? HeightMax : sMax.Height, Math.Min(sMax.Height, h)));

			HtmlIndent = Math.Max(0, Math.Min(10, HtmlIndent));
			BorderSize = Math.Max(0, Math.Min(10, BorderSize));

			double f = Frame / (double)FrameCount;

			FrameWidth = (int)Math.Ceiling(WidthMax * f);
			FrameHeight = (int)Math.Ceiling(HeightMax * f);
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Add(
				"frame",
				() =>
				{
					if (BorderSize > 0 && BorderID >= 0)
					{
						AddImageTiled(0, 0, FrameWidth, FrameHeight, BorderID);

						if (BorderAlpha)
						{
							AddAlphaRegion(0, 0, FrameWidth, FrameHeight);
						}
					}

					if (FrameWidth > BorderSize * 2 && FrameHeight > BorderSize * 2 && BackgroundID >= 0)
					{
						AddImageTiled(BorderSize, BorderSize, FrameWidth - (BorderSize * 2), FrameHeight - (BorderSize * 2), BackgroundID);

						if (BackgroundAlpha)
						{
							AddAlphaRegion(BorderSize, BorderSize, FrameWidth - (BorderSize * 2), FrameHeight - (BorderSize * 2));
						}
					}

					if (Frame < FrameCount)
					{
						return;
					}

					string html = Html.ParseBBCode(HtmlColor).Replace("<br>", "\n").Replace("<BR>", "\n");

					AddHtml(
						BorderSize + HtmlIndent,
						BorderSize,
						(FrameWidth - (BorderSize * 2)) - HtmlIndent,
						FrameHeight - (BorderSize * 2),
						html.WrapUOHtmlColor(HtmlColor, false),
						false,
						FrameHeight >= GetSizeMax().Height);
				});
		}

		protected override bool CanAutoRefresh()
		{
			return State == AnimState.Pause && Frame > 0 ? AutoClose && base.CanAutoRefresh() : base.CanAutoRefresh();
		}

		protected override void OnAutoRefresh()
		{
			base.OnAutoRefresh();

			AnimateList();

			switch (State)
			{
				case AnimState.Show:
					{
						if (Frame++ >= FrameCount)
						{
							AutoRefreshRate = PauseDuration;
							State = AnimState.Pause;
							Frame = FrameCount;
						}
					}
					break;
				case AnimState.Hide:
					{
						if (Frame-- <= 0)
						{
							AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
							State = AnimState.Pause;
							Frame = 0;
							Close(true);
						}
					}
					break;
				case AnimState.Pause:
					{
						AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
						State = Frame <= 0 ? AnimState.Show : AutoClose ? AnimState.Hide : AnimState.Pause;
					}
					break;
			}
		}

		protected override void OnBeforeSend()
		{
			if (!Initialized)
			{
				if (Notify.IsIgnored(GetType(), User))
				{
					Close(true);
					return;
				}

				if (!Notify.IsAnimated(GetType(), User))
				{
					AnimDuration = TimeSpan.Zero;
				}

				if (OnNotify != null)
				{
					OnNotify(this);
				}
			}

			base.OnBeforeSend();
		}

		public override void Close(bool all = false)
		{
			if (all)
			{
				base.Close(true);
			}
			else
			{
				AutoRefreshRate = TimeSpan.FromMilliseconds(100.0);
				AutoClose = true;
			}
		}

		private void AnimateList()
		{
			VitaNexCore.TryCatch(
				() =>
				{
					var p = this;

					foreach (var g in
						GetInstances<NotifyGump>(User, true)
							.Where(g => g != this && g.IsOpen && !g.IsDisposed && g.Y >= p.Y)
							.OrderBy(g => g.Y))
					{
						g.Y = p.Y + p.FrameHeight;
						p = g;

						if (g.State != AnimState.Pause)
						{
							return;
						}

						var lr = g.LastAutoRefresh;
						g.Refresh(true);
						g.LastAutoRefresh = lr;
					}
				},
				e => e.ToConsole());
		}
	}
}