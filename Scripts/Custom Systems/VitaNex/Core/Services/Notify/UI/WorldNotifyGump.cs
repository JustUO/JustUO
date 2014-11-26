#region Header
//   Vorspire    _,-'/-'/  WorldNotifyGump.cs
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

using Server.Mobiles;
#endregion

namespace VitaNex.Notify
{
	public class WorldNotifyGump : NotifyGump
	{
		public WorldNotifyGump(PlayerMobile user, string html)
			: this(user, html, false)
		{ }

		public WorldNotifyGump(PlayerMobile user, string html, bool autoClose)
			: base(user, html)
		{
			AnimDuration = TimeSpan.FromSeconds(1.0);
			PauseDuration = TimeSpan.FromSeconds(10.0);
			HtmlColor = Color.Yellow;
			AutoClose = autoClose;
		}

		public class Sub0 : WorldNotifyGump
		{
			public Sub0(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub0(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub1 : WorldNotifyGump
		{
			public Sub1(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub1(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub2 : WorldNotifyGump
		{
			public Sub2(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub2(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub3 : WorldNotifyGump
		{
			public Sub3(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub3(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub4 : WorldNotifyGump
		{
			public Sub4(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub4(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub5 : WorldNotifyGump
		{
			public Sub5(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub5(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub6 : WorldNotifyGump
		{
			public Sub6(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub6(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub7 : WorldNotifyGump
		{
			public Sub7(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub7(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub8 : WorldNotifyGump
		{
			public Sub8(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub8(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}

		public class Sub9 : WorldNotifyGump
		{
			public Sub9(PlayerMobile user, string html)
				: base(user, html)
			{ }

			public Sub9(PlayerMobile user, string html, bool autoClose)
				: base(user, html, autoClose)
			{ }
		}
	}
}