#region Header
//   Vorspire    _,-'/-'/  SuperGump_ItemProps.cs
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
		private bool _ItemPropertyWarning;

		public void AddItemProperty(Item item)
		{
			if (item != null && !item.Deleted)
			{
				AddItemProperty(item.Serial);
			}
		}

		public void AddItemProperty(Serial serial)
		{
			if (_ItemPropertyWarning || !serial.IsValid)
			{
				return;
			}

			var mi = typeof(Gump).GetMethod("AddItemProperty", new[] {typeof(int)});

			if (mi != null)
			{
				mi.Invoke(this, new object[] {serial.Value});
				return;
			}

			Utility.PushColor(ConsoleColor.Red);
			Console.WriteLine(GetType().FullName);
			Console.WriteLine("Server.Gump does not support method AddItemProperty( int serial )");
			Utility.PopColor();

			_ItemPropertyWarning = true;
		}
	}
}