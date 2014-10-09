#region Header
//   Vorspire    _,-'/-'/  ListEntry.cs
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

using Server.Gumps;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public struct ListGumpEntry : IEquatable<ListGumpEntry>, IEquatable<string>
	{
		public static ListGumpEntry Empty = new ListGumpEntry(String.Empty, () => { });

		private static Action<GumpButton> Resolve(Action handler)
		{
			return b =>
			{
				if (handler != null)
				{
					handler();
				}
			};
		}

		public static bool IsNullOrEmpty(ListGumpEntry entry)
		{
			return entry == null || entry == Empty;
		}

		public string Label { get; set; }
		public int Hue { get; set; }
		public Action<GumpButton> Handler { get; set; }

		public ListGumpEntry(string label, Action handler)
			: this(label, Resolve(handler), SuperGump.DefaultTextHue)
		{ }

		public ListGumpEntry(string label, Action handler, int hue)
			: this(label, Resolve(handler), hue)
		{ }

		public ListGumpEntry(string label, Action<GumpButton> handler)
			: this(label, handler, SuperGump.DefaultTextHue)
		{ }

		public ListGumpEntry(string label, Action<GumpButton> handler, int hue)
			: this()
		{
			Label = label;
			Handler = handler;
			Hue = hue;
		}

		public override string ToString()
		{
			return Label;
		}

		public override int GetHashCode()
		{
			return Label != null ? Label.GetHashCode() : 0;
		}

		public bool Equals(ListGumpEntry other)
		{
			return String.Equals(Label, other.Label);
		}

		public bool Equals(string other)
		{
			return String.Equals(Label, other);
		}

		public override bool Equals(object obj)
		{
			return !ReferenceEquals(null, obj) &&
				   (obj is string && Equals((string)obj) || (obj is ListGumpEntry && Equals((ListGumpEntry)obj)));
		}

		public static bool operator ==(ListGumpEntry a, ListGumpEntry b)
		{
			return String.Equals(a.Label, b.Label);
		}

		public static bool operator !=(ListGumpEntry a, ListGumpEntry b)
		{
			return !(a == b);
		}

		public static bool operator ==(ListGumpEntry a, string b)
		{
			return String.Equals(a.Label, b);
		}

		public static bool operator !=(ListGumpEntry a, string b)
		{
			return !(a == b);
		}

		public static bool operator ==(string a, ListGumpEntry b)
		{
			return String.Equals(a, b.Label);
		}

		public static bool operator !=(string a, ListGumpEntry b)
		{
			return !(a == b);
		}
	}
}