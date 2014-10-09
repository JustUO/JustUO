#region Header
//   Vorspire    _,-'/-'/  OffsetSelectorGump.cs
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

using Server;
using Server.Accounting;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class OffsetSelectorGump : SuperGump
	{
		public static Point DefaultOffset = new Point(DefaultX, DefaultY);

		private Point _Value;

		public Point Value
		{
			get { return _Value; }
			set
			{
				if (value.X < 0)
				{
					value.X = Math.Abs(value.X);
				}

				if (value.Y < 0)
				{
					value.Y = Math.Abs(value.Y);
				}

				if (_Value == value)
				{
					return;
				}

				Point oldValue = _Value;
				_Value = value;
				OnValueChanged(oldValue);
			}
		}

		public virtual Action<OffsetSelectorGump, Point> ValueChanged { get; set; }

		public OffsetSelectorGump(
			PlayerMobile user, Gump parent = null, Point? value = null, Action<OffsetSelectorGump, Point> valueChanged = null)
			: base(user, parent, 0, 0)
		{
			ForceRecompile = true;
			CanMove = false;
			CanClose = true;
			CanDispose = true;

			_Value = value ?? DefaultOffset;
			ValueChanged = valueChanged;
		}

		protected override void OnBeforeSend()
		{
			User.SendMessage(0x55, "Generating Offset Selection Interface, please wait...");

			base.OnBeforeSend();
		}

		protected virtual void OnValueChanged(Point oldValue)
		{
			if (ValueChanged != null)
			{
				ValueChanged(this, oldValue);
			}

			Refresh();
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.AddReplace(
				"buttongrid/base",
				() =>
				{
					HardwareInfo hi = ((Account)User.Account).HardwareInfo;

					int w = (hi != null ? hi.ScreenWidth : 1920) / 20;
					int h = (hi != null ? hi.ScreenHeight : 1080) / 20;

					for (int x = 0; x < w; x++)
					{
						for (int y = 0; y < h; y++)
						{
							AddButton(x * 20, y * 20, 9028, 9021, OnSelectPoint);
						}
					}
				});

			layout.AddReplace("image/marker", () => AddImage(Value.X + 5, Value.Y, 9009));
		}

		protected virtual void OnSelectPoint(GumpButton b)
		{
			Value = new Point(b.X, b.Y);
		}
	}
}