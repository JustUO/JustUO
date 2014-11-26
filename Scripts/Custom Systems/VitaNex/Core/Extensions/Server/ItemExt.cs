#region Header
//   Vorspire    _,-'/-'/  ItemExt.cs
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
using System.Text;

using Server.Items;
using Server.Mobiles;
using Server.Multis;

using VitaNex;
using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace Server
{
	public enum ItemBindResult
	{
		None,
		NoAccess,
		Bound,
		Unbound
	}

	[Flags]
	public enum GiveFlags : byte
	{
		None = 0x0,
		Pack = 0x1,
		Bank = 0x2,
		Feet = 0x4,
		Delete = 0x8,
		PackBank = Pack | Bank,
		PackBankDelete = Pack | Bank | Delete,
		PackFeet = Pack | Feet,
		PackFeetDelete = Pack | Feet | Delete,
		BankFeet = Bank | Feet,
		BankFeetDelete = Bank | Feet | Delete,
		PackBankFeet = Pack | Bank | Feet,
		All = Pack | Bank | Feet | Delete
	}

	public static class ItemExtUtility
	{
		public static int GetPaperdollArt(this Item item, bool female)
		{
			return item != null && item.Layer.IsValid() ? ArtworkSupport.LookupGump(item.ItemID, female) : 0;
		}

		public static PaperdollBounds GetPaperdollBounds(this Item item)
		{
			if (item == null)
			{
				return PaperdollBounds.Empty;
			}

			if (item.Layer == Layer.TwoHanded)
			{
				if (item is BaseRanged)
				{
					return PaperdollBounds.MainHand;
				}

				if (item is BaseEquipableLight || item is BaseShield)
				{
					return PaperdollBounds.OffHand;
				}
			}

			return PaperdollBounds.Find(item.Layer);
		}

		public static Mobile FindOwner(this Item item)
		{
			return FindOwner<Mobile>(item);
		}

		public static TMobile FindOwner<TMobile>(this Item item) where TMobile : Mobile
		{
			if (item == null || item.Deleted)
			{
				return null;
			}

			TMobile owner = item.RootParent as TMobile;

			if (owner == null)
			{
				var h = BaseHouse.FindHouseAt(item);

				if (h != null)
				{
					owner = h.Owner as TMobile;
				}
			}

			return owner;
		}

		public static GiveFlags GiveTo(
			this Item item, Mobile m, GiveFlags flags = GiveFlags.PackBankFeet, bool message = true)
		{
			if (item == null || item.Deleted || m == null || m.Deleted || flags == GiveFlags.None)
			{
				return GiveFlags.None;
			}

			bool pack = flags.HasFlag(GiveFlags.Pack);
			bool bank = flags.HasFlag(GiveFlags.Bank);
			bool feet = flags.HasFlag(GiveFlags.Feet);
			bool delete = flags.HasFlag(GiveFlags.Delete);

			GiveFlags result = VitaNexCore.TryCatchGet(
				() =>
				{
					if (pack && m.PlaceInBackpack(item))
					{
						return GiveFlags.Pack;
					}

					if (bank && m.BankBox.TryDropItem(m, item, false))
					{
						return GiveFlags.Bank;
					}

					if (feet)
					{
						MapPoint mp = m.ToMapPoint();

						if (!mp.Internal)
						{
							item.MoveToWorld(mp.Location, mp.Map);
							return GiveFlags.Feet;
						}
					}

					if (delete)
					{
						item.Delete();
						return GiveFlags.Delete;
					}

					return GiveFlags.None;
				});

			if (message)
			{
				string amount = String.Empty;
				string name = ResolveName(item, m);

				bool p = false;

				if (item.Stackable && item.Amount > 1)
				{
					amount = item.Amount.ToString("#,0") + " ";
					p = true;

					if (!Insensitive.EndsWith(name, "s") && !Insensitive.EndsWith(name, "z"))
					{
						name += "s";
					}
				}

				switch (result)
				{
					case GiveFlags.Pack:
						m.SendMessage("{0}{1} {2} been placed in your pack.", amount, name, p ? "have" : "has");
						break;
					case GiveFlags.Bank:
						m.SendMessage("{0}{1} {2} been placed in your bank.", amount, name, p ? "have" : "has");
						break;
					case GiveFlags.Feet:
						m.SendMessage("{0}{1} {2} been placed at your feet.", amount, name, p ? "have" : "has");
						break;
				}
			}

			return result;
		}

		public static bool IsBound(this Item item)
		{
			return item != null && item.BlessedFor != null;
		}

		public static bool IsBoundTo(this Item item, Mobile m)
		{
			return IsBound(item) && item.BlessedFor == m;
		}

		public static void CheckBinding(
			this Item item, Mobile m, bool message = true, bool confirm = true, bool forceChange = false)
		{
			CheckBinding(item, m, r => { }, message, confirm, forceChange);
		}

		public static void CheckBinding(
			this Item item,
			Mobile m,
			Action<ItemBindResult> callback,
			bool message = true,
			bool confirm = true,
			bool forceChange = false)
		{
			CheckBinding(
				item,
				m,
				(m1, r) =>
				{
					if (callback != null)
					{
						callback(r);
					}
				},
				message,
				confirm,
				forceChange);
		}

		public static void CheckBinding(
			this Item item,
			Mobile m,
			Action<Mobile, ItemBindResult> callback,
			bool message = true,
			bool confirm = true,
			bool forceChange = false)
		{
			if (item == null)
			{
				if (callback != null)
				{
					callback(m, ItemBindResult.None);
				}

				return;
			}

			if (IsBoundTo(item, m))
			{
				if (callback != null)
				{
					callback(m, ItemBindResult.Bound);
				}

				return;
			}

			if (IsBound(item) && !forceChange)
			{
				if (callback != null)
				{
					callback(m, ItemBindResult.NoAccess);
				}

				return;
			}

			if (m is PlayerMobile && confirm)
			{
				string name = item.ResolveName(m.GetLanguage());
				var html = new StringBuilder();

				html.AppendLine("Do you wish to bind this item to your character?");
				html.AppendLine("Binding " + name + " will bless it for " + m.RawName + ".");

				SuperGump.Send(
					new ConfirmDialogGump(
						(PlayerMobile)m,
						title: "Confirm Item Bind (" + name + ")",
						html: html.ToString(),
						onAccept: b =>
						{
							if (callback != null)
							{
								callback(m, InternalBind(item, m, message, forceChange));
							}
						}));
			}
			else
			{
				if (callback != null)
				{
					callback(m, InternalBind(item, m, message, forceChange));
				}
			}
		}

		private static ItemBindResult InternalBind(Item item, Mobile m, bool message, bool forceChange)
		{
			if (item == null)
			{
				return ItemBindResult.None;
			}

			if (m != null && !m.CanSee(item))
			{
				return ItemBindResult.NoAccess;
			}

			if (IsBoundTo(item, m))
			{
				return ItemBindResult.Bound;
			}

			if (IsBound(item) && !forceChange)
			{
				return ItemBindResult.NoAccess;
			}

			item.BlessedFor = m;

			if (m != null && message)
			{
				m.SendMessage(0x55, "{0} has been bound to you.", ResolveName(item, m.GetLanguage()));
			}

			return ((m == null) ? ItemBindResult.Unbound : ItemBindResult.Bound);
		}

		public static string ResolveName(this Item item, Mobile viewer, bool setIfNull = false)
		{
			return ResolveName(item, viewer == null ? ClilocLNG.ENU : viewer.GetLanguage(), setIfNull);
		}

		public static string ResolveName(this Item item, ClilocLNG lng = ClilocLNG.ENU, bool setIfNull = false)
		{
			if (item == null)
			{
				return String.Empty;
			}

			if (item.Name != null)
			{
				return item.Name;
			}

			string label = item.DefaultName;

			if (String.IsNullOrWhiteSpace(label) && item.LabelNumber > 0)
			{
				label = lng.GetString(item.LabelNumber);
			}

			if (String.IsNullOrWhiteSpace(label) && TileData.ItemTable.InBounds(item.ItemID))
			{
				label = TileData.ItemTable[item.ItemID].Name;
			}

			if (String.IsNullOrWhiteSpace(label))
			{
				label = item.GetType().Name.SpaceWords();
			}

			return setIfNull ? (item.Name = label) : label;
		}

		public static bool CheckDoubleClick(
			this Item item,
			Mobile from,
			bool handle = true,
			bool allowDead = false,
			int range = 20,
			bool packOnly = false,
			bool inTrade = false,
			bool inDisplay = true,
			AccessLevel access = AccessLevel.Player)
		{
			if (item == null || item.Deleted || from == null || from.Deleted)
			{
				return false;
			}

			if (from.AccessLevel < access)
			{
				if (handle)
				{
					from.SendMessage("You do not have sufficient access to use this item.");
				}

				return false;
			}

			if (!from.CanSee(item))
			{
				if (handle)
				{
					from.SendMessage("This item can't be seen.");
					item.OnDoubleClickCantSee(from);
				}

				return false;
			}

			if (!item.IsAccessibleTo(from))
			{
				if (handle)
				{
					item.OnDoubleClickNotAccessible(from);
				}

				return false;
			}

			if (item.InSecureTrade && !inTrade)
			{
				if (handle)
				{
					item.OnDoubleClickSecureTrade(from);
				}

				return false;
			}

			if (((item.Parent == null && !item.Movable && !item.IsLockedDown && !item.IsSecure && !item.InSecureTrade) ||
				 IsShopItem(item)) && !inDisplay)
			{
				if (handle)
				{
					from.SendMessage("This item can not be accessed because it is part of a display.");
				}

				return false;
			}

			if (!from.Alive && !allowDead)
			{
				if (handle)
				{
					item.OnDoubleClickDead(from);
				}

				return false;
			}

			if (range >= 0 && !from.InRange(item.GetWorldLocation(), range) && !packOnly)
			{
				if (handle)
				{
					if (range > 0)
					{
						from.SendMessage("You must be within {0:#,0} paces to use this item.", range);
					}
					else
					{
						from.SendMessage("You must be standing on this item to use it.");
					}

					item.OnDoubleClickOutOfRange(from);
				}

				return false;
			}

			if (packOnly && item.RootParent != from)
			{
				if (handle)
				{
					// This item must be in your backpack.
					from.SendLocalizedMessage(1054107);
				}

				return false;
			}

			return true;
		}

		public static bool IsShopItem(this Item item)
		{
			return HasParent(item, "Server.Mobiles.GenericBuyInfo+DisplayCache");
		}

		public static bool HasParent<TEntity>(this Item item) where TEntity : IEntity
		{
			return HasParent(item, typeof(TEntity));
		}

		public static bool HasParent(this Item item, string typeName)
		{
			if (item == null || String.IsNullOrWhiteSpace(typeName))
			{
				return false;
			}

			Type t = Type.GetType(typeName, false, false) ??
					 ScriptCompiler.FindTypeByFullName(typeName, false) ?? ScriptCompiler.FindTypeByName(typeName, false);

			return HasParent(item, t);
		}

		public static bool HasParent(this Item item, Type t)
		{
			if (item == null || t == null)
			{
				return false;
			}

			object p = item.Parent;

			while (p is Item)
			{
				if (p.GetType().IsEqualOrChildOf(t))
				{
					return true;
				}

				Item i = (Item)p;

				if (i.Parent == null)
				{
					break;
				}

				p = i.Parent;
			}

			return p is Mobile && p.GetType().IsEqualOrChildOf(t);
		}

		public static bool IsEquipped(this Item item)
		{
			return item != null && item.Parent is Mobile && ((Mobile)item.Parent).FindItemOnLayer(item.Layer) == item;
		}
	}
}