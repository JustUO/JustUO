#region Header
//   Vorspire    _,-'/-'/  AdvancedSBInfo.cs
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
using System.Reflection;

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Mobiles
{
	public class AdvancedSBInfo : SBInfo
	{
		public IAdvancedVendor Vendor { get; private set; }

		private readonly List<GenericBuyInfo> _BuyInfo;
		public override sealed List<GenericBuyInfo> BuyInfo { get { return _BuyInfo; } }

		private readonly AdvancedSellInfo _SellInfo;
		public override sealed IShopSellInfo SellInfo { get { return _SellInfo; } }

		public AdvancedSBInfo(IAdvancedVendor vendor)
		{
			Vendor = vendor;

			_BuyInfo = new List<GenericBuyInfo>();
			_SellInfo = new AdvancedSellInfo(this);

			InitBuyInfo();
		}

		public virtual void InitBuyInfo()
		{ }

		public void AddStock<TObj>(int price, string name = null, int amount = 100, object[] args = null)
		{
			AddStock(typeof(TObj), price, name, amount, args);
		}

		public virtual void AddStock(Type type, int price, string name = null, int amount = 100, object[] args = null)
		{
			AddStock(new AdvancedBuyInfo(this, type, price, name, amount, args));
		}

		public virtual void AddStock(AdvancedBuyInfo buy)
		{
			_BuyInfo.Add(buy);
		}

		public void RemoveStock<TObj>()
		{
			RemoveStock(typeof(TObj));
		}

		public virtual void RemoveStock(Type type)
		{
			_BuyInfo.OfType<AdvancedBuyInfo>().Where(b => b.Type.TypeEquals(type)).ForEach(RemoveStock);
		}

		public virtual void RemoveStock(AdvancedBuyInfo buy)
		{
			_BuyInfo.Remove(buy);
		}

		public void AddOrder<TObj>(int price)
		{
			AddOrder(typeof(TObj), price);
		}

		public virtual void AddOrder(Type type, int price)
		{
			_SellInfo.Add(type, price);
		}

		public void RemoveOrder<TObj>()
		{
			RemoveOrder(typeof(TObj));
		}

		public virtual void RemoveOrder(Type type)
		{
			_SellInfo.Remove(type);
		}

		public IEnumerable<KeyValuePair<Type, int>> EnumerateOrders(Func<Type, int, bool> predicate = null)
		{
			return predicate != null ? _SellInfo.Table.Where(kv => predicate(kv.Key, kv.Value)) : _SellInfo.Table;
		}

		public IEnumerable<AdvancedBuyInfo> EnumerateStock(Func<AdvancedBuyInfo, bool> predicate = null)
		{
			return predicate != null ? _BuyInfo.OfType<AdvancedBuyInfo>().Where(predicate) : _BuyInfo.OfType<AdvancedBuyInfo>();
		}
	}

	public class AdvancedBuyInfo : GenericBuyInfo
	{
		public AdvancedSBInfo Parent { get; private set; }

		public virtual int Slots { get; set; }
		public override sealed int ControlSlots { get { return Slots; } }

		public AdvancedBuyInfo(
			AdvancedSBInfo parent, Type type, int price, string name = null, int amount = 100, object[] args = null)
			: base(name, type, price, amount, 0x14F0, 0, args)
		{
			Parent = parent;

			IEntity e = GetDisplayEntity();

			if (e is Mobile)
			{
				Mobile m = (Mobile)e;

				if (String.IsNullOrWhiteSpace(name))
				{
					Name = m.RawName ?? type.Name.SpaceWords();
				}
				else
				{
					m.RawName = name;
				}

				ItemID = ShrinkTable.Lookup(m);
				Hue = m.Hue;

				if (m is BaseCreature)
				{
					Slots = ((BaseCreature)m).ControlSlots;
				}
			}
			else if (e is Item)
			{
				Item i = (Item)e;

				if (String.IsNullOrWhiteSpace(name))
				{
					Name = i.ResolveName();
				}
				else
				{
					i.Name = name;
				}

				ItemID = i.ItemID;
				Hue = i.Hue;
			}
			else if (String.IsNullOrWhiteSpace(name))
			{
				Name = type.Name.SpaceWords();
			}
		}
	}

	public class AdvancedSellInfo : GenericSellInfo
	{
		private static readonly FieldInfo _TableField =
			typeof(GenericSellInfo).GetField("m_Table", BindingFlags.Instance | BindingFlags.NonPublic) ??
			typeof(GenericSellInfo).GetField("_Table", BindingFlags.Instance | BindingFlags.NonPublic);

		public AdvancedSBInfo Parent { get; private set; }
		public Dictionary<Type, int> Table { get; private set; }

		public AdvancedSellInfo(AdvancedSBInfo parent)
		{
			Parent = parent;

			Table = _TableField.GetValue(this) as Dictionary<Type, int> ?? new Dictionary<Type, int>();
		}

		public void Add<TObj>(int price)
		{
			Add(typeof(TObj), price);
		}

		public new virtual void Add(Type type, int price)
		{
			base.Add(type, price);
		}

		public bool Remove<TObj>()
		{
			return Remove(typeof(TObj));
		}

		public virtual bool Remove(Type type)
		{
			return Table.Remove(type);
		}
	}
}