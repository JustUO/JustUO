#region Header
//   Vorspire    _,-'/-'/  ArtworkSupport.cs
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
using System.IO;
using System.Linq;

using Server;
using Server.Items;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex.Items
{
	public static partial class ArtworkSupport
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static CoreServiceOptions _CSOptions = new CoreServiceOptions(typeof(ArtworkSupport));
		public static CoreServiceOptions CSOptions { get { return _CSOptions ?? (_CSOptions = new CoreServiceOptions(typeof(ArtworkSupport))); } }

		private static OutgoingPacketOverrideHandler _Parent0x1A;
		private static OutgoingPacketOverrideHandler _Parent0xF3;

		public static ClientVersion DefaultVersion = new ClientVersion(7, 0, 24, 3);

		public static Dictionary<Type, List<ArtworkInfo>> Info { get; private set; }

		private static void HandleWorldItem(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (_Parent0x1A != null)
			{
				_Parent0x1A(state, reader, ref buffer, ref length);
			}

			if(state == null || reader == null || buffer == null)
			{
				return;
			}

			int pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(3, SeekOrigin.Begin);
			Serial serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			if(!serial.IsValid || !serial.IsItem)
			{
				return;
			}

			var item = World.FindItem(serial);
			var info = Lookup(item);

			if(info == null)
			{
				return;
			}

			if (CSOptions.ServiceDebug)
			{
				CSOptions.ToConsole(
					"Rewriting packet buffer ItemID for '{0}' -> \n(0x{1:X4} -> 0x{2:X4})",
					item.GetType().Name,
					info.ItemID.Left,
					info.ItemID.Right);
			}

			info.SwitchWorldItem(state, item is BaseMulti, ref buffer);
		}

		private static void HandleWorldItemSAHS(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (_Parent0xF3 != null)
			{
				_Parent0xF3(state, reader, ref buffer, ref length);
			}

			if(state == null || reader == null || buffer == null)
			{
				return;
			}

			int pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(4, SeekOrigin.Begin);
			Serial serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			if(!serial.IsValid || !serial.IsItem)
			{
				return;
			}

			var item = World.FindItem(serial);
			var info = Lookup(item);

			if(info == null)
			{
				return;
			}

			if(CSOptions.ServiceDebug)
			{
				CSOptions.ToConsole(
					"Rewriting packet buffer ItemID for '{0}' -> \n(0x{1:X4} -> 0x{2:X4})",
					item.GetType().Name,
					info.ItemID.Left,
					info.ItemID.Right);
			}

			info.SwitchWorldItemSAHS(state, item is BaseMulti, ref buffer);
		}

		public static void Register<TItem>(ClientVersion version, int oldItemID, int newItemID) where TItem : Item
		{
			Register(typeof(TItem), version, oldItemID, newItemID);
		}

		public static void Register(Type type, ClientVersion version, int oldItemID, int newItemID)
		{
			List<ArtworkInfo> infoList;

			if(!Info.TryGetValue(type, out infoList))
			{
				Info.Add(type, (infoList = new List<ArtworkInfo>()));
			}
			else if(infoList == null)
			{
				Info[type] = infoList = new List<ArtworkInfo>();
			}

			var info = infoList.FirstOrDefault(i => i.ItemID.Left == oldItemID);

			if(info != null)
			{
				if (CSOptions.ServiceDebug)
				{
					CSOptions.ToConsole(
						"Replacing ArtworkInfo for '{0}' -> \n({1}, 0x{2:X4} -> 0x{3:X4}) with ({4}, 0x{5:X4} -> 0x{6:X4})",
						type.Name,
						info.Version,
						info.ItemID.Left,
						info.ItemID.Right,
						version,
						oldItemID,
						newItemID);
				}

				info.Version = version;
				info.ItemID = Pair.Create(oldItemID, newItemID);
			}
			else
			{
				if (CSOptions.ServiceDebug)
				{
					CSOptions.ToConsole(
						"Adding ArtworkInfo for '{0}' -> \n({1}, 0x{2:X4} -> 0x{3:X4})", type.Name, version, oldItemID, newItemID);
				}

				infoList.Add(new ArtworkInfo(version, oldItemID, newItemID));
			}
		}

		public static ArtworkInfo Lookup(Item item)
		{
			return item != null ? Lookup(item.GetType(), item.ItemID) : null;
		}

		public static ArtworkInfo Lookup(Type type, int itemID)
		{
			List<ArtworkInfo> infoList;

			return Info.TryGetValue(type, out infoList) && infoList != null && infoList.Count > 0
					   ? infoList.FirstOrDefault(i => i.ItemID.Left == itemID)
					   : null;
		}
	}
}