#region Header
//   Vorspire    _,-'/-'/  EquipmentSets.cs
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
using Server.Network;

using VitaNex.IO;
using VitaNex.Network;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public static partial class EquipmentSets
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static readonly Type TypeOfEquipmentSet = typeof(EquipmentSet);

		public static EquipmentSetsOptions CMOptions { get; private set; }

		public static BinaryDataStore<Type, EquipmentSet> Sets { get; private set; }

		public static OutgoingPacketOverrideHandler EquipItemParent { get; private set; }

		public static PacketHandler DropItemRequestParent { get; private set; }
		public static PacketHandler DropItemRequestParent6017 { get; private set; }
		public static PacketHandler EquipItemRequestParent { get; private set; }

		public static void Sync()
		{
			VitaNexCore.TryCatch(
				() =>
				{
					int added = 0;

					foreach (var t in TypeOfEquipmentSet.GetConstructableChildren())
					{
						Sets.AddOrReplace(
							t,
							set =>
							{
								if (set == null)
								{
									set = t.CreateInstanceSafe<EquipmentSet>();

									if (set != null)
									{
										++added;
									}
								}

								return set;
							});
					}

					if (added > 0)
					{
						CMOptions.ToConsole("{0:#,0} new sets added.", added);
					}
				},
				CMOptions.ToConsole);
		}

		private static void OnLogin(LoginEventArgs e)
		{
			if (CMOptions.ModuleEnabled && e.Mobile != null)
			{
				Invalidate(e.Mobile);
			}
		}

		private static void GetProperties(Item item, Mobile viewer, ExtendedOPL list)
		{
			if (!CMOptions.ModuleEnabled || item == null || item.Deleted || !item.Layer.IsEquip() || list == null)
			{
				return;
			}

			if (viewer == null && item.Parent is Mobile)
			{
				viewer = (Mobile)item.Parent;
			}

			if (viewer == null)
			{
				return;
			}

			Type itemType = item.GetType();
			bool equipped = item.IsEquipped();

			foreach (EquipmentSet set in
				FindSetsFor(itemType)
					.Where(s => s.Display && !s.Parts.Any(p => p.Valid && p.Display && p.IsTypeOf(itemType) && !p.DisplaySet)))
			{
				set.GetProperties(viewer, list, equipped);

				if (set.DisplayParts)
				{
					foreach (EquipmentSetPart part in set.Parts.Where(p => p.Valid && p.Display))
					{
						part.GetProperties(viewer, list, equipped);
					}
				}

				if (!set.DisplayMods)
				{
					continue;
				}

				foreach (EquipmentSetMod mod in set.Mods.Where(mod => mod.Valid && mod.Display))
				{
					mod.GetProperties(viewer, list, equipped);
				}
			}
		}

		private static void EquipItem(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			int pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(1, SeekOrigin.Begin);

			Item item = World.FindItem(reader.ReadInt32());

			reader.Seek(pos, SeekOrigin.Begin);

			if (EquipItemParent != null)
			{
				EquipItemParent(state, reader, ref buffer, ref length);
			}

			if (!CMOptions.ModuleEnabled || item == null || item.Deleted || !item.Layer.IsEquip())
			{
				return;
			}

			if (CMOptions.ModuleDebug)
			{
				CMOptions.ToConsole("EquipItem: {0} equiped {1}", state.Mobile, item);
			}

			Timer.DelayCall(() => Invalidate(state.Mobile, item));
		}

		private static void EquipItemRequest(NetState state, PacketReader pvSrc)
		{
			Item item = state.Mobile.Holding;

			if (EquipItemRequestParent != null && EquipItemRequestParent.OnReceive != null)
			{
				EquipItemRequestParent.OnReceive(state, pvSrc);
			}

			if (!CMOptions.ModuleEnabled || item == null || item.Deleted || !item.Layer.IsEquip())
			{
				return;
			}

			if (CMOptions.ModuleDebug)
			{
				CMOptions.ToConsole("EquipItemRequest: {0} equiping {1}", state.Mobile, item);
			}

			Timer.DelayCall(() => Invalidate(state.Mobile, item));
		}

		private static void DropItemRequest(NetState state, PacketReader pvSrc)
		{
			Item item = state.Mobile.Holding;

			if (DropItemRequestParent != null && DropItemRequestParent.OnReceive != null)
			{
				DropItemRequestParent.OnReceive(state, pvSrc);
			}

			if (!CMOptions.ModuleEnabled || item == null || item.Deleted || !item.Layer.IsEquip())
			{
				return;
			}

			if (CMOptions.ModuleDebug)
			{
				CMOptions.ToConsole("DropItemRequest: {0} dropping {1}", state.Mobile, item);
			}

			Timer.DelayCall(() => Invalidate(state.Mobile, item));
		}

		private static void DropItemRequest6017(NetState state, PacketReader pvSrc)
		{
			Item item = state.Mobile.Holding;

			if (DropItemRequestParent6017 != null && DropItemRequestParent6017.OnReceive != null)
			{
				DropItemRequestParent6017.OnReceive(state, pvSrc);
			}

			if (!CMOptions.ModuleEnabled || item == null || item.Deleted || !item.Layer.IsEquip())
			{
				return;
			}

			if (CMOptions.ModuleDebug)
			{
				CMOptions.ToConsole("DropItemRequest6017: {0} dropping {1}", state.Mobile, item);
			}

			Timer.DelayCall(() => Invalidate(state.Mobile, item));
		}

		public static List<EquipmentSet> GetSetsFor(Item item)
		{
			return FindSetsFor(item.GetType()).ToList();
		}

		public static List<EquipmentSet> GetSetsFor(Type type)
		{
			return FindSetsFor(type).ToList();
		}

		public static IEnumerable<EquipmentSet> FindSetsFor(Item item)
		{
			return FindSetsFor(item.GetType());
		}

		public static IEnumerable<EquipmentSet> FindSetsFor(Type type)
		{
			return Sets.Values.Where(set => set.Valid && set.HasPartTypeOf(type));
		}

		public static void Invalidate(Mobile owner)
		{
			if (!CMOptions.ModuleEnabled || owner == null)
			{
				return;
			}

			foreach (Item item in owner.FindEquippedItems<Item>())
			{
				Invalidate(owner, item);
			}
		}

		public static void Invalidate(Mobile owner, Item item)
		{
			if (!CMOptions.ModuleEnabled || owner == null || item == null || item.Deleted || !item.Layer.IsEquip())
			{
				return;
			}

			if (!CMOptions.ModuleDebug)
			{
				foreach (EquipmentSet set in FindSetsFor(item))
				{
					set.Invalidate(owner, item);
				}
			}
			else
			{
				var sets = GetSetsFor(item);

				CMOptions.ToConsole("Found {0} sets for '{1}'", sets.Count, item);

				if (sets.Count > 0)
				{
					CMOptions.ToConsole("'{0}'", String.Join("', '", sets.Select(s => s.Name)));

					sets.ForEach(set => set.Invalidate(owner, item));
				}

				sets.Free(true);
			}
		}
	}
}