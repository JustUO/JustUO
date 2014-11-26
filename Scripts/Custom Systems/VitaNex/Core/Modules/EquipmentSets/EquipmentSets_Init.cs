#region Header
//   Vorspire    _,-'/-'/  EquipmentSets_Init.cs
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

using Server;
using Server.Network;

using VitaNex.IO;
using VitaNex.Network;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	[CoreModule("Equipment Sets", "1.0.0.0")]
	public static partial class EquipmentSets
	{
		static EquipmentSets()
		{
			CMOptions = new EquipmentSetsOptions();

			Sets = new BinaryDataStore<Type, EquipmentSet>(VitaNexCore.SavesDirectory + "/EquipSets", "Sets")
			{
				OnSerialize = SaveSets,
				OnDeserialize = LoadSets
			};
		}

		private static void CMConfig()
		{
			EquipItemRequestParent = PacketHandlers.GetHandler(0x13);
			DropItemRequestParent = PacketHandlers.GetHandler(0x08);
			DropItemRequestParent6017 = PacketHandlers.Get6017Handler(0x08);

			EquipItemParent = OutgoingPacketOverrides.GetHandler(0x2E);

			PacketHandlers.Register(0x13, 10, true, EquipItemRequest);
			//PacketHandlers.Register6017(0x13, 10, true, EquipItemRequest6017);
			PacketHandlers.Register(0x08, 14, true, DropItemRequest);
			PacketHandlers.Register6017(0x08, 15, true, DropItemRequest6017);

			OutgoingPacketOverrides.Register(0x2E, true, EquipItem);
		}

		private static void CMInvoke()
		{
			EventSink.Login += OnLogin;
			ExtendedOPL.OnItemOPLRequest += GetProperties;
		}

		private static void CMSave()
		{
			DataStoreResult result = Sets.Export();
			CMOptions.ToConsole("{0:#,0} sets saved, {1}", Sets.Count, result);
		}

		private static void CMLoad()
		{
			DataStoreResult result = Sets.Import();
			CMOptions.ToConsole("{0:#,0} sets loaded, {1}.", Sets.Count, result);

			Sync();
		}

		private static void CMDispose()
		{
			if (EquipItemRequestParent != null && EquipItemRequestParent.OnReceive != null)
			{
				PacketHandlers.Register(0x13, 10, true, EquipItemRequestParent.OnReceive);
			}

			/*
			if (EquipItemRequestParent6017 != null && EquipItemRequestParent6017.OnReceive != null)
			{
				PacketHandlers.Register6017(0x13, 10, true, EquipItemRequestParent6017.OnReceive);
			}
			*/

			if (DropItemRequestParent != null && DropItemRequestParent.OnReceive != null)
			{
				PacketHandlers.Register(0x08, 14, true, DropItemRequestParent.OnReceive);
			}

			if (DropItemRequestParent6017 != null && DropItemRequestParent6017.OnReceive != null)
			{
				PacketHandlers.Register6017(0x08, 15, true, DropItemRequestParent6017.OnReceive);
			}
		}

		private static bool SaveSets(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.WriteBlockDictionary(Sets, (w, k, v) => w.WriteType(v, t => v.Serialize(w)));
					break;
			}

			return true;
		}

		private static bool LoadSets(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							r =>
							{
								EquipmentSet v = r.ReadTypeCreate<EquipmentSet>(r);
								return new KeyValuePair<Type, EquipmentSet>(v.GetType(), v);
							},
							Sets);
					}
					break;
			}

			return true;
		}
	}
}