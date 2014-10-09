#region Header
//   Vorspire    _,-'/-'/  ArtworkSupport_Init.cs
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
using System.Linq;

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex.Items
{
	[CoreService("Artwork Support")]
	public static partial class ArtworkSupport
	{
		static ArtworkSupport()
		{
			Info = new Dictionary<Type, List<ArtworkInfo>>();
		}

		private static void CSConfig()
		{
			_Parent0x1A = OutgoingPacketOverrides.GetHandler(0x1A);
			_Parent0xF3 = OutgoingPacketOverrides.GetHandler(0xF3);

			OutgoingPacketOverrides.Register(0x1A, true, HandleWorldItem);
			OutgoingPacketOverrides.Register(0xF3, true, HandleWorldItemSAHS);
		}

		private static void CSInvoke()
		{
			Type type = typeof(Item);

			foreach (var entry in
				type.GetChildren().AsParallel().Select(
					child => new
					{
						Type = child,
						Attrs = child.GetCustomAttributes<ArtworkSupportAttribute>(false)
					}))
			{
				foreach (var attr in entry.Attrs)
				{
					foreach (var pair in attr.ItemIDs)
					{
						Register(entry.Type, attr.Version, pair.Left, pair.Right);
					}
				}
			}
		}
	}
}