#region Header
//   Vorspire    _,-'/-'/  SuperGump_Blueprint.cs
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
using System.Reflection;

using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		private Size _InternalSize = new Size(0, 0);

		public virtual bool CanMove { get { return Dragable; } set { Dragable = value; } }
		public virtual bool CanClose { get { return Closable; } set { Closable = value; } }
		public virtual bool CanDispose { get { return Disposable; } set { Disposable = value; } }
		public virtual bool CanResize { get { return Resizable; } set { Resizable = value; } }

		public virtual int ModalXOffset { get; set; }
		public virtual int ModalYOffset { get; set; }
		public virtual int XOffset { get; set; }
		public virtual int YOffset { get; set; }

		public Size OuterSize { get { return _InternalSize; } }

		public int OuterWidth { get { return _InternalSize.Width; } }
		public int OuterHeight { get { return _InternalSize.Height; } }

		public void InvalidateSize()
		{
			Entries.ForEach(
				entry =>
				{
					Type eType = entry.GetType();
					PropertyInfo xProp = eType.GetProperty("X", typeof(int)),
								 yProp = eType.GetProperty("Y", typeof(int)),
								 wProp = eType.GetProperty("Width", typeof(int)),
								 hProp = eType.GetProperty("Height", typeof(int));

					int x = 0, y = 0, w = 0, h = 0;

					if (xProp != null)
					{
						x = (int)xProp.GetValue(entry, null);
					}

					if (yProp != null)
					{
						y = (int)yProp.GetValue(entry, null);
					}

					if (wProp != null)
					{
						w = (int)wProp.GetValue(entry, null);
					}

					if (hProp != null)
					{
						h = (int)hProp.GetValue(entry, null);
					}

					_InternalSize.Width = Math.Max(_InternalSize.Width, x + w);
					_InternalSize.Height = Math.Max(_InternalSize.Height, y + h);
				});
		}

		private void InvalidateOffsets()
		{
			Entries.For(
				(i, entry) =>
				{
					if (Modal)
					{
						if (entry is GumpBackground)
						{
							var e = (GumpBackground)entry;

							if ((e.X == 0 || (e.X % 1024) == 0) && (e.Y == 0 || (e.Y % 786) == 0) && e.Width == 1024 && e.Height == 786)
							{
								return;
							}
						}

						if (entry is GumpImageTiled)
						{
							var e = (GumpImageTiled)entry;

							if ((e.X == 0 || (e.X % 1024) == 0) && (e.Y == 0 || (e.Y % 786) == 0) && e.Width == 1024 && e.Height == 786)
							{
								return;
							}
						}

						if (entry is GumpAlphaRegion)
						{
							var e = (GumpAlphaRegion)entry;

							if ((e.X == 0 || (e.X % 1024) == 0) && (e.Y == 0 || (e.Y % 786) == 0) && e.Width == 1024 && e.Height == 786)
							{
								return;
							}
						}
					}

					Type entryType = entry.GetType();

					PropertyInfo xProp = entryType.GetProperty("X", typeof(int)), yProp = entryType.GetProperty("Y", typeof(int));

					if (xProp != null)
					{
						var x = (int)xProp.GetValue(entry, null);
						xProp.SetValue(entry, x + (ModalXOffset + XOffset), null);
					}

					if (yProp != null)
					{
						var y = (int)yProp.GetValue(entry, null);
						yProp.SetValue(entry, y + (ModalYOffset + YOffset), null);
					}
				});
		}
	}
}