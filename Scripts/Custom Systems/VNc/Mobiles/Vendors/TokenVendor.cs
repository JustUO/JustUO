#region Header
//   Vorspire    _,-'/-'/  TokenVendor.cs
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

using VitaNex.Items;
#endregion

namespace VitaNex.Mobiles
{
	public abstract class BaseTokenVendor : BaseVendor
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual TypeSelectProperty<IVendorToken> TokenType { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual string TokenName { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual bool ShowTokenName { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual bool Trading { get; set; }

		public BaseTokenVendor(string title, string tokenName, Type tokenType)
			: this(title, tokenName, tokenType, true)
		{ }

		public BaseTokenVendor(string title, string tokenName, Type tokenType, bool trading)
			: this(title, tokenName, tokenType, trading, true)
		{ }

		public BaseTokenVendor(string title, string tokenName, Type tokenType, bool trading, bool showTokenName)
			: this(title, tokenName, tokenType != null ? tokenType.FullName : String.Empty, trading, showTokenName)
		{ }

		public BaseTokenVendor(string title, string tokenName, string tokenType)
			: this(title, tokenName, tokenType, true)
		{ }

		public BaseTokenVendor(string title, string tokenName, string tokenType, bool trading)
			: this(title, tokenName, tokenType, trading, true)
		{ }

		public BaseTokenVendor(string title, string tokenName, string tokenType, bool trading, bool showTokenName)
			: base(title)
		{
			Trading = trading;
			ShowTokenName = showTokenName;
			TokenName = tokenName;
			TokenType = tokenType;
		}

		public BaseTokenVendor(Serial serial)
			: base(serial)
		{ }

		private GenericBuyInfo LookupDisplayObject(object obj)
		{
			var buyInfo = GetBuyInfo();

			return buyInfo.Cast<GenericBuyInfo>().FirstOrDefault(gbi => gbi.GetDisplayEntity() == obj);
		}

		private void ProcessSinglePurchase(
			BuyItemResponse buy,
			IBuyItemInfo bii,
			ICollection<BuyItemResponse> validBuy,
			ref int controlSlots,
			ref bool fullPurchase,
			ref int totalCost)
		{
			if (!Trading || TokenType == null || !TokenType.IsNotNull)
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
			if (!Trading || TokenType == null || !TokenType.IsNotNull)
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

						if (item != null)
						{
							item.Amount = 1;

							if (cont == null || !cont.TryDropItem(buyer, item, false))
							{
								item.MoveToWorld(buyer.Location, buyer.Map);
							}
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

					if (m != null)
					{
						m.Direction = (Direction)Utility.Random(8);
						m.MoveToWorld(buyer.Location, buyer.Map);

						if (m is BaseCreature)
						{
							((BaseCreature)m).SetControlMaster(buyer);
						}
					}
				}
			}
		}

		public override bool OnBuyItems(Mobile buyer, List<BuyItemResponse> list)
		{
			if (!Trading || TokenType == null || !TokenType.IsNotNull)
			{
				return false;
			}

			if (!IsActiveSeller)
			{
				return false;
			}

			if (!buyer.CheckAlive())
			{
				return false;
			}

			if (!CheckVendorAccess(buyer))
			{
				Say(501522); // I shall not treat with scum like thee!
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

			bool bought = (buyer.AccessLevel >= AccessLevel.GameMaster);
			Container cont = buyer.Backpack;

			if (!bought && cont != null)
			{
				if (cont.ConsumeTotal(TokenType, totalCost))
				{
					bought = true;
				}
				else if (totalCost < 2000)
				{
					// Begging thy pardon, but thou casnt afford that.
					SayTo(buyer, 500192);
				}
			}

			if (!bought && totalCost >= 2000)
			{
				cont = buyer.FindBankNoCreate();

				if (cont != null && cont.ConsumeTotal(TokenType, totalCost))
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
						"The total of thy purchase is {0} {1}, which has been withdrawn from your bank account.  My thanks for the patronage.",
						totalCost,
						TokenName);
				}
				else
				{
					SayTo(buyer, true, "The total of thy purchase is {0} {1}.  My thanks for the patronage.", totalCost, TokenName);
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
						"The total of thy purchase is {0} {1}, which has been withdrawn from your bank account.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
						totalCost,
						TokenName);
				}
				else
				{
					SayTo(
						buyer,
						true,
						"The total of thy purchase is {0} {1}.  My thanks for the patronage.  Unfortunately, I could not sell you all the goods you requested.",
						totalCost,
						TokenName);
				}
			}

			return true;
		}

		public override bool OnSellItems(Mobile seller, List<SellItemResponse> list)
		{
			if (!Trading || TokenType == null || !TokenType.IsNotNull)
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
				Say(501522); // I shall not treat with scum like thee!
				return false;
			}

			seller.PlaySound(0x32);

			var info = GetSellInfo();
			var buyInfo = GetBuyInfo();
			int giveCurrency = 0;
			Container cont;

			int sold =
				list.Where(
					resp =>
					resp.Item.RootParent == seller && resp.Amount > 0 && resp.Item.Movable &&
					(!(resp.Item is Container) || (resp.Item).Items.Count == 0))
					.Count(resp => info.Any(ssi => ssi.IsSellable(resp.Item)));

			if (sold > 500)
			{
				SayTo(seller, true, "You may only sell 500 items at a time!");
				return false;
			}

			if (sold == 0)
			{
				return true;
			}

			foreach (SellItemResponse resp in
				list.Where(
					resp =>
					resp.Item.RootParent == seller && resp.Amount > 0 && resp.Item.IsStandardLoot() && resp.Item.Movable &&
					(!(resp.Item is Container) || (resp.Item).Items.Count == 0)))
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
									cont.DropItem(item);
								}
								else
								{
									resp.Item.SetLastMoved();
									cont.DropItem(resp.Item);
								}
							}
							else
							{
								resp.Item.SetLastMoved();
								cont.DropItem(resp.Item);
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

					giveCurrency += ssi.GetSellPriceFor(resp.Item) * amount;
					break;
				}
			}

			if (giveCurrency <= 0)
			{
				return false;
			}

			while (giveCurrency > 0)
			{
				Item c = TokenType.CreateInstanceObject() as Item;

				if (c == null)
				{
					c = new Static(0x14F0, 1)
					{
						Name = String.Format("Staff I.O.U [{0} {1}]", giveCurrency.ToString("#,#"), TokenName),
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

				if (!seller.AddToBackpack(c) && !seller.BankBox.TryDropItem(seller, c, true))
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

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (!Trading)
			{
				list.Add("<basefont color=#{0:X6}>Not Trading<basefont color=#ffffff>", Color.OrangeRed.ToArgb());
				return;
			}

			if (ShowTokenName && !String.IsNullOrWhiteSpace(TokenName))
			{
				list.Add("<basefont color=#{0:X6}>Trades For {1}<basefont color=#ffffff>", Color.SkyBlue.ToArgb(), TokenName);
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					TokenType.Serialize(writer);
					writer.Write(TokenName);
					writer.Write(ShowTokenName);
					goto case 0;
				case 0:
					writer.Write(Trading);
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
					{
						TokenType = new TypeSelectProperty<IVendorToken>(reader);
						TokenName = reader.ReadString();
						ShowTokenName = reader.ReadBool();
					}
					goto case 0;
				case 0:
					{
						Trading = reader.ReadBool();

						if (version == 0)
						{
							TokenType = String.Empty;
							TokenName = String.Empty;
							ShowTokenName = true;
						}
					}
					break;
			}
		}
	}
}