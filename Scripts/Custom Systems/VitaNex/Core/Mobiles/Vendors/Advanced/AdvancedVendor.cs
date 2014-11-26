#region Header
//   Vorspire    _,-'/-'/  AdvancedVendor.cs
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
using System.Drawing;
using System.Linq;

using Server;
using Server.Engines.BulkOrders;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
#endregion

namespace VitaNex.Mobiles
{
	public abstract class AdvancedVendor : BaseVendor, IAdvancedVendor
	{
		public static event Action<AdvancedVendor> OnCreated;
		public static event Action<AdvancedSBInfo> OnInit;

		public static List<AdvancedVendor> Instances { get; private set; }

		static AdvancedVendor()
		{
			Instances = new List<AdvancedVendor>();
		}

		private readonly List<SBInfo> _SBInfos = new List<SBInfo>();
		protected override sealed List<SBInfo> SBInfos { get { return _SBInfos; } }

		public AdvancedSBInfo AdvancedStock { get; private set; }

		private DateTime _NextYell = DateTime.UtcNow.AddSeconds(Utility.RandomMinMax(30, 120));

		[CommandProperty(AccessLevel.Administrator)]
		public int Discount { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public bool DiscountEnabled { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public bool DiscountYell { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public ObjectProperty CashProperty { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public TypeSelectProperty<object> CashType { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public TextDefinition CashName { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool ShowCashName { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool Trading { get; set; }

		public override VendorShoeType ShoeType { get { return Utility.RandomBool() ? VendorShoeType.ThighBoots : VendorShoeType.Boots; } }

		public AdvancedVendor(string title, Type cashType, TextDefinition cashName, bool showCashName = true)
			: this(title)
		{
			CashProperty = new ObjectProperty();
			CashType = cashType ?? typeof(Gold);
			CashName = cashName ?? "Gold";
			ShowCashName = showCashName;
		}

		public AdvancedVendor(string title, string cashProp, TextDefinition cashName, bool showCashName = true)
			: this(title)
		{
			CashProperty = new ObjectProperty(cashProp);
			CashType = typeof(ObjectProperty);
			CashName = cashName ?? "Credits";
			ShowCashName = showCashName;
		}

		private AdvancedVendor(string title)
			: base(title)
		{
			Trading = true;

			Female = Utility.RandomBool();
			Name = NameList.RandomName(Female ? "female" : "male");

			Race = Race.AllRaces.GetRandom();

			Instances.Add(this);

			Timer.DelayCall(
				() =>
				{
					if (OnCreated != null)
					{
						OnCreated(this);
					}
				});
		}

		public AdvancedVendor(Serial serial)
			: base(serial)
		{
			Instances.Add(this);
		}

		public override int GetPriceScalar()
		{
			int scalar = CashType.TypeEquals<Gold>() ? base.GetPriceScalar() : 100;

			if (DiscountEnabled)
			{
				scalar -= Math.Max(0, Math.Min(100, Discount));
			}

			return Math.Max(0, scalar);
		}

		protected override void OnRaceChange(Race oldRace)
		{
			base.OnRaceChange(oldRace);

			Items.Where(
				item =>
				item != null && !item.Deleted && !(item is Container) && !(item is IMount) && !(item is IMountItem) &&
				item.IsEquipped()).ForEach(item => item.Delete());

			Hue = /*FaceHue =*/ Race.RandomSkinHue();
			/*
			FaceItemID = Race.RandomFace(this);
			FaceHue = Hue;
			*/
			HairItemID = Race.RandomHair(this);
			HairHue = Race.RandomHairHue();

			FacialHairItemID = Race.RandomFacialHair(this);
			FacialHairHue = HairHue;

			InitOutfit();
		}

		public virtual void ResolveCurrency(out Type type, out TextDefinition name)
		{
			type = CashType;
			name = CashName;
		}

		public virtual object GetCashObject(Mobile m)
		{
			return m;
		}

		public override void InitOutfit()
		{
			if (Race == Race.Gargoyle)
			{
				//InitGargOutfit();
				return;
			}

			base.InitOutfit();
		}

		public override sealed void InitSBInfo()
		{
			_SBInfos.ForEach(sb => sb.BuyInfo.ForEach(b => b.DeleteDisplayEntity()));
			_SBInfos.Clear();

			_SBInfos.Add(AdvancedStock = new AdvancedSBInfo(this));

			InitBuyInfo();

			if (OnInit != null)
			{
				OnInit(AdvancedStock);
			}
		}

		protected abstract void InitBuyInfo();

		public void AddStock<TObj>(int price, string name = null, int amount = 100, object[] args = null)
		{
			AddStock(typeof(TObj), price, name, amount, args);
		}

		public virtual void AddStock(Type type, int price, string name = null, int amount = 100, object[] args = null)
		{
			AdvancedStock.AddStock(type, price, name, amount, args);
		}

		public void RemoveStock<TObj>()
		{
			RemoveStock(typeof(TObj));
		}

		public virtual void RemoveStock(Type type)
		{
			AdvancedStock.RemoveStock(type);
		}

		public void AddOrder<TObj>(int price)
		{
			AddOrder(typeof(TObj), price);
		}

		public virtual void AddOrder(Type type, int price)
		{
			AdvancedStock.AddOrder(type, price);
		}

		public void RemoveOrder<TObj>()
		{
			RemoveOrder(typeof(TObj));
		}

		public virtual void RemoveOrder(Type type)
		{
			AdvancedStock.RemoveOrder(type);
		}

		public override bool CheckVendorAccess(Mobile m)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			if (m.AccessLevel >= AccessLevel.GameMaster)
			{
				return true;
			}

			if (!Trading || !DesignContext.Check(m))
			{
				return false;
			}

			return true;
		}

		public override void OnThink()
		{
			base.OnThink();

			if (!DiscountEnabled || !DiscountYell || Discount <= 0 || DateTime.UtcNow <= _NextYell)
			{
				return;
			}

			Yell("Sale! {0}% Off!", Discount.ToString("#,0"));
			_NextYell = DateTime.UtcNow + TimeSpan.FromSeconds(Utility.RandomMinMax(20, 120));
		}

		public override void OnDelete()
		{
			base.OnDelete();

			Instances.Remove(this);
		}

		public override void OnAfterDelete()
		{
			base.OnAfterDelete();

			Instances.Remove(this);
		}

		private GenericBuyInfo LookupDisplayObject(object obj)
		{
			return GetBuyInfo().OfType<GenericBuyInfo>().FirstOrDefault(gbi => gbi.GetDisplayEntity() == obj);
		}

		private void ProcessSinglePurchase(
			BuyItemResponse buy,
			IBuyItemInfo bii,
			ICollection<BuyItemResponse> validBuy,
			ref int controlSlots,
			ref bool fullPurchase,
			ref int totalCost)
		{
			if (!Trading || CashType == null || !CashType.IsNotNull)
			{
				return;
			}

			int amount = buy.Amount;

			if (amount > bii.Amount)
			{
				amount = bii.Amount;
			}

			if (amount <= 0)
			{
				return;
			}

			int slots = bii.ControlSlots * amount;

			if (controlSlots >= slots)
			{
				controlSlots -= slots;
			}
			else
			{
				fullPurchase = false;
				return;
			}

			totalCost += bii.Price * amount;
			validBuy.Add(buy);
		}

		private void ProcessValidPurchase(int amount, IBuyItemInfo bii, Mobile buyer, Container cont)
		{
			if (!Trading || CashType == null || !CashType.IsNotNull)
			{
				return;
			}

			if (amount > bii.Amount)
			{
				amount = bii.Amount;
			}

			if (amount < 1)
			{
				return;
			}

			bii.Amount -= amount;

			IEntity o = bii.GetEntity();

			if (o is Item)
			{
				var item = (Item)o;

				if (item.Stackable)
				{
					item.Amount = amount;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
					{
						item.MoveToWorld(buyer.Location, buyer.Map);
					}
				}
				else
				{
					item.Amount = 1;

					if (cont == null || !cont.TryDropItem(buyer, item, false))
					{
						item.MoveToWorld(buyer.Location, buyer.Map);
					}

					for (int i = 1; i < amount; i++)
					{
						item = bii.GetEntity() as Item;

						if (item == null)
						{
							continue;
						}

						item.Amount = 1;

						if (cont == null || !cont.TryDropItem(buyer, item, false))
						{
							item.MoveToWorld(buyer.Location, buyer.Map);
						}
					}
				}
			}
			else if (o is Mobile)
			{
				var m = (Mobile)o;

				m.Direction = (Direction)Utility.Random(8);
				m.MoveToWorld(buyer.Location, buyer.Map);
				m.PlaySound(m.GetIdleSound());

				if (m is BaseCreature)
				{
					((BaseCreature)m).SetControlMaster(buyer);
				}

				for (int i = 1; i < amount; ++i)
				{
					m = bii.GetEntity() as Mobile;

					if (m == null)
					{
						continue;
					}

					m.Direction = (Direction)Utility.Random(8);
					m.MoveToWorld(buyer.Location, buyer.Map);

					if (m is BaseCreature)
					{
						((BaseCreature)m).SetControlMaster(buyer);
					}
				}
			}
		}

		public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
		{
			if (!Trading || !IsActiveSeller || CashType == null || !CashType.IsNotNull)
			{
				return false;
			}

			if (!buyer.CheckAlive())
			{
				return false;
			}

			if (!CheckVendorAccess(buyer))
			{
				Say("My shop is closed!");
				//Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			UpdateBuyInfo();

			var info = GetSellInfo();
			int totalCost = 0;
			var validBuy = new List<BuyItemResponse>(list.Count);
			bool fromBank = false;
			bool fullPurchase = true;
			int controlSlots = buyer.FollowersMax - buyer.Followers;

			foreach (BuyItemResponse buy in list)
			{
				Serial ser = buy.Serial;
				int amount = buy.Amount;

				if (ser.IsItem)
				{
					Item item = World.FindItem(ser);

					if (item == null)
					{
						continue;
					}

					GenericBuyInfo gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost);
					}
					else if (item != BuyPack && item.IsChildOf(BuyPack))
					{
						if (amount > item.Amount)
						{
							amount = item.Amount;
						}

						if (amount <= 0)
						{
							continue;
						}

						foreach (IShopSellInfo ssi in
							info.Where(ssi => ssi.IsSellable(item)).Where(ssi => ssi.IsResellable(item)))
						{
							totalCost += ssi.GetBuyPriceFor(item) * amount;
							validBuy.Add(buy);
							break;
						}
					}
				}
				else if (ser.IsMobile)
				{
					Mobile mob = World.FindMobile(ser);

					if (mob == null)
					{
						continue;
					}

					GenericBuyInfo gbi = LookupDisplayObject(mob);

					if (gbi != null)
					{
						ProcessSinglePurchase(buy, gbi, validBuy, ref controlSlots, ref fullPurchase, ref totalCost);
					}
				}
			}

			if (fullPurchase && validBuy.Count == 0)
			{
				// Thou hast bought nothing!
				SayTo(buyer, 500190);
			}
			else if (validBuy.Count == 0)
			{
				// Your order cannot be fulfilled, please try again.
				SayTo(buyer, 500187);
			}

			if (validBuy.Count == 0)
			{
				return false;
			}

			bool bought = buyer.AccessLevel >= AccessLevel.GameMaster;

			if (!bought && CashType.TypeEquals<ObjectProperty>())
			{
				object cashSource = GetCashObject(buyer) ?? buyer;

				bought = CashProperty.Consume(cashSource, totalCost);

				if (!bought)
				{
					// Begging thy pardon, but thou cant afford that.
					SayTo(buyer, 500192);
					return false;
				}

				SayTo(buyer, "{0:#,0} {1} has been deducted from your total.", totalCost, CashName.GetString(buyer));
			}

			Container cont = buyer.Backpack;

			if (!bought && cont != null && CashType.TypeEquals<Gold>())
			{
				VitaNexCore.TryCatch(
					() =>
					{
						var lt = Type.GetType("GoldLedger") ?? ScriptCompiler.FindTypeByName("GoldLedger");

						if (lt == null)
						{
							return;
						}

						var ledger = cont.FindItemByType(lt);

						if (ledger == null || ledger.Deleted)
						{
							return;
						}

						var lp = lt.GetProperty("Gold");

						if (lp == null || !lp.PropertyType.TypeEquals<Int32>())
						{
							return;
						}

						int lg = (int)lp.GetValue(ledger, null);

						if (lg < totalCost)
						{
							return;
						}

						lp.SetValue(ledger, lg - totalCost, null);
						bought = true;

						buyer.SendMessage(2125, "{0:#,0} gold has been withdrawn from your ledger.", totalCost);
					});
			}

			if (!bought && cont != null)
			{
				if (cont.ConsumeTotal(CashType, totalCost))
				{
					bought = true;
				}
				else
				{
					// Begging thy pardon, but thou cant afford that.
					SayTo(buyer, 500192);
				}
			}

			if (!bought)
			{
				cont = buyer.FindBankNoCreate();

				if (cont != null && cont.ConsumeTotal(CashType, totalCost))
				{
					bought = true;
					fromBank = true;
				}
				else
				{
					// Begging thy pardon, but thy bank account lacks these funds.
					SayTo(buyer, 500191);
				}
			}

			if (!bought)
			{
				return false;
			}

			buyer.PlaySound(0x32);

			cont = buyer.Backpack ?? buyer.BankBox;

			foreach (BuyItemResponse buy in validBuy)
			{
				Serial ser = buy.Serial;
				int amount = buy.Amount;

				if (amount < 1)
				{
					continue;
				}

				if (ser.IsItem)
				{
					Item item = World.FindItem(ser);

					if (item == null)
					{
						continue;
					}

					GenericBuyInfo gbi = LookupDisplayObject(item);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
					}
					else
					{
						if (amount > item.Amount)
						{
							amount = item.Amount;
						}

						if (info.Where(ssi => ssi.IsSellable(item)).Any(ssi => ssi.IsResellable(item)))
						{
							Item buyItem;

							if (amount >= item.Amount)
							{
								buyItem = item;
							}
							else
							{
								buyItem = LiftItemDupe(item, item.Amount - amount) ?? item;
							}

							if (cont == null || !cont.TryDropItem(buyer, buyItem, false))
							{
								buyItem.MoveToWorld(buyer.Location, buyer.Map);
							}
						}
					}
				}
				else if (ser.IsMobile)
				{
					Mobile mob = World.FindMobile(ser);

					if (mob == null)
					{
						continue;
					}

					GenericBuyInfo gbi = LookupDisplayObject(mob);

					if (gbi != null)
					{
						ProcessValidPurchase(amount, gbi, buyer, cont);
					}
				}
			}

			if (fullPurchase)
			{
				if (buyer.AccessLevel >= AccessLevel.GameMaster)
				{
					SayTo(buyer, true, "I would not presume to charge thee anything.  Here are the goods you requested.");
				}
				else if (fromBank)
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}, which has been withdrawn from your bank account.  My thanks for the patronage.",
						totalCost,
						CashName.GetString(buyer));
				}
				else
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}.  My thanks for the patronage.",
						totalCost,
						CashName.GetString(buyer));
				}
			}
			else
			{
				if (buyer.AccessLevel >= AccessLevel.GameMaster)
				{
					SayTo(
						buyer,
						true,
						"I would not presume to charge thee anything.  Unfortunately, I could not sell you all the goods you requested.");
				}
				else if (fromBank)
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}, which has been withdrawn from your bank account.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
						totalCost,
						CashName.GetString(buyer));
				}
				else
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0:#,0} {1}.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
						totalCost,
						CashName.GetString(buyer));
				}
			}

			return true;
		}

		public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
		{
			if (!Trading || CashType == null || !CashType.IsNotNull)
			{
				return false;
			}

			if (!IsActiveBuyer)
			{
				return false;
			}

			if (!seller.CheckAlive())
			{
				return false;
			}

			if (!CheckVendorAccess(seller))
			{
				Say("My shop is closed!");
				//Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			seller.PlaySound(0x32);

			var info = GetSellInfo();
			var buyInfo = GetBuyInfo();
			int giveCurrency = 0;
			Container cont;

			var finalList = list.Where(resp => CanSell(seller, info, resp)).ToArray();

			if (finalList.Length > 500)
			{
				SayTo(seller, true, "You may only sell 500 items at a time!");
				return false;
			}

			if (finalList.Length == 0)
			{
				return true;
			}

			foreach (SellItemResponse resp in finalList)
			{
				foreach (IShopSellInfo ssi in info)
				{
					if (!ssi.IsSellable(resp.Item))
					{
						continue;
					}

					int amount = resp.Amount;

					if (amount > resp.Item.Amount)
					{
						amount = resp.Item.Amount;
					}

					int worth = GetSellPrice(seller, ssi, resp) * amount;

					if (ssi.IsResellable(resp.Item))
					{
						bool found = false;

						if (buyInfo.Any(bii => bii.Restock(resp.Item, amount)))
						{
							resp.Item.Consume(amount);
							found = true;
						}

						if (!found)
						{
							cont = BuyPack;

							if (amount < resp.Item.Amount)
							{
								Item item = LiftItemDupe(resp.Item, resp.Item.Amount - amount);

								if (item != null)
								{
									item.SetLastMoved();

									if (!cont.TryDropItem(this, resp.Item, false))
									{
										resp.Item.Delete();
									}
								}
								else
								{
									resp.Item.SetLastMoved();

									if (!cont.TryDropItem(this, resp.Item, false))
									{
										resp.Item.Delete();
									}
								}
							}
							else
							{
								resp.Item.SetLastMoved();

								if (!cont.TryDropItem(this, resp.Item, false))
								{
									resp.Item.Delete();
								}
							}
						}
					}
					else
					{
						if (amount < resp.Item.Amount)
						{
							resp.Item.Amount -= amount;
						}
						else
						{
							resp.Item.Delete();
						}
					}

					giveCurrency += worth;
					break;
				}
			}

			if (giveCurrency <= 0)
			{
				return false;
			}

			while (giveCurrency > 0)
			{
				Item c = null;

				if (CashType.TypeEquals<ObjectProperty>())
				{
					object cashSource = GetCashObject(seller) ?? seller;

					if (!CashProperty.Add(cashSource, giveCurrency))
					{
						c = new Static(0x14F0, 1)
						{
							Name = String.Format("Staff I.O.U [{0} {1}]", giveCurrency.ToString("#,0"), CashName.GetString(seller)),
							Hue = 85,
							Movable = true,
							BlessedFor = seller,
							LootType = LootType.Blessed
						};
					}

					giveCurrency = 0;
				}

				if (c == null && giveCurrency > 0)
				{
					c = CashType.CreateInstance<Item>();

					if (c == null)
					{
						c = new Static(0x14F0, 1)
						{
							Name = String.Format("Staff I.O.U [{0} {1}]", giveCurrency.ToString("#,0"), CashName.GetString(seller)),
							Hue = 85,
							Movable = true,
							BlessedFor = seller,
							LootType = LootType.Blessed
						};
						giveCurrency = 0;
					}
					else
					{
						if (giveCurrency >= 60000)
						{
							c.Amount = 60000;
							giveCurrency -= 60000;
						}
						else
						{
							c.Amount = giveCurrency;
							giveCurrency = 0;
						}
					}
				}

				if (c != null && !seller.AddToBackpack(c) && !seller.BankBox.TryDropItem(seller, c, true))
				{
					c.MoveToWorld(seller.Location, seller.Map);
				}
			}

			seller.PlaySound(0x0037); //Gold dropping sound

			if (SupportsBulkOrders(seller))
			{
				Item bulkOrder = CreateBulkOrder(seller, false);

				if (bulkOrder is LargeBOD)
				{
					seller.SendGump(new LargeBODAcceptGump(seller, (LargeBOD)bulkOrder));
				}
				else if (bulkOrder is SmallBOD)
				{
					seller.SendGump(new SmallBODAcceptGump(seller, (SmallBOD)bulkOrder));
				}
			}

			return true;
		}

		public virtual bool CanSell(Mobile seller, IShopSellInfo[] info, SellItemResponse resp)
		{
			return resp.Item != null && resp.Item.RootParent == seller && resp.Amount > 0 && resp.Item.Movable &&
				   (!(resp.Item is Container) || resp.Item.Items.Count == 0) && info.Any(ssi => ssi.IsSellable(resp.Item));
		}

		public virtual int GetSellPrice(Mobile seller, IShopSellInfo info, SellItemResponse resp)
		{
			return info.GetSellPriceFor(resp.Item);
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (!Trading)
			{
				list.Add("Not Trading".WrapUOHtmlColor(Color.OrangeRed));
				return;
			}

			if (!ShowCashName)
			{
				return;
			}

			var name = CashName.GetString();

			if (!String.IsNullOrWhiteSpace(name))
			{
				list.Add("Trades For {0}".WrapUOHtmlColor(Color.SkyBlue), name);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					CashProperty.Serialize(writer);
					goto case 0;
				case 0:
					{
						CashType.Serialize(writer);
						writer.WriteTextDef(CashName);
						writer.Write(ShowCashName);

						writer.Write(Trading);

						writer.Write(Discount);
						writer.Write(DiscountEnabled);
						writer.Write(DiscountYell);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 1:
					CashProperty = new ObjectProperty(reader);
					goto case 0;
				case 0:
					{
						if (version < 1)
						{
							var t = new ItemTypeSelectProperty(reader);
							CashType = t.InternalType;
						}
						else
						{
							CashType = new TypeSelectProperty<object>(reader);
						}

						CashName = reader.ReadTextDef();
						ShowCashName = reader.ReadBool();

						Trading = reader.ReadBool();

						Discount = reader.ReadInt();
						DiscountEnabled = reader.ReadBool();
						DiscountYell = reader.ReadBool();
					}
					break;
			}

			if (CashProperty == null)
			{
				CashProperty = new ObjectProperty();
			}
		}
	}
}