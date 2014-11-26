#region Header
//   Vorspire    _,-'/-'/  Reward.cs
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
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public enum PvPRewardDeliveryMethod
	{
		None,
		Custom,
		Backpack,
		Bank
	}

	public enum PvPRewardClass
	{
		None,
		Custom,
		Item
	}

	public class PvPReward : ItemTypeSelectProperty
	{
		private int _Amount;
		private PvPRewardClass _Class;
		private PvPRewardDeliveryMethod _DeliveryMethod = PvPRewardDeliveryMethod.Backpack;

		public PvPReward(string type = "")
			: base(type)
		{ }

		public PvPReward(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Enabled { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Amount { get { return _Amount; } set { _Amount = Math.Max(1, Math.Min(60000, value)); } }

		[CommandProperty(AutoPvP.Access)]
		public override string TypeName
		{
			get { return base.TypeName; }
			set
			{
				base.TypeName = value;

				if (InternalType != null)
				{
					if (InternalType.IsConstructableFrom(typeof(Item), new Type[0]))
					{
						_Class = PvPRewardClass.Item;
						DeliveryMethod = PvPRewardDeliveryMethod.Backpack;
					}
					else
					{
						_Class = PvPRewardClass.Custom;
						DeliveryMethod = PvPRewardDeliveryMethod.Custom;
					}
				}
				else
				{
					_Class = PvPRewardClass.None;
					DeliveryMethod = PvPRewardDeliveryMethod.None;
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPRewardDeliveryMethod DeliveryMethod
		{
			get { return _DeliveryMethod; }
			set
			{
				if (Class == PvPRewardClass.Custom)
				{
					value = PvPRewardDeliveryMethod.Custom;
				}

				_DeliveryMethod = value;
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public PvPRewardClass Class { get { return _Class; } }

		public override void Clear()
		{
			base.Clear();

			Enabled = false;
			Amount = 1;
			_Class = PvPRewardClass.None;
			DeliveryMethod = PvPRewardDeliveryMethod.None;
		}

		public override void Reset()
		{
			base.Reset();

			Enabled = false;
			Amount = 1;
			_Class = PvPRewardClass.None;
			DeliveryMethod = PvPRewardDeliveryMethod.Backpack;
		}

		public virtual Item[] GiveReward(PlayerMobile pm)
		{
			if (pm == null || pm.Deleted || !Enabled)
			{
				return new Item[0];
			}

			Item e = CreateInstance();

			if (e == null)
			{
				return new Item[0];
			}

			var items = new List<Item>
			{
				e
			};

			if (e.Stackable)
			{
				e.Amount = Amount;
			}
			else
			{
				int count = Amount - 1;

				while (--count >= 0)
				{
					items.Add(CreateInstance());
				}
			}

			items.ToArray().ForEach(
				item =>
				{
					switch (_DeliveryMethod)
					{
						case PvPRewardDeliveryMethod.Bank:
							{
								if (pm.BankBox == null || pm.BankBox.Deleted || !pm.BankBox.TryDropItem(pm, item, true))
								{
									if (!pm.AddToBackpack(item))
									{
										item.Delete();
										items.Remove(item);
									}
								}
							}
							break;
						default:
							{
								if (!pm.AddToBackpack(item))
								{
									if (pm.BankBox == null || pm.BankBox.Deleted || !pm.BankBox.TryDropItem(pm, item, true))
									{
										item.Delete();
										items.Remove(item);
									}
								}
							}
							break;
					}
				});

			return items.ToArray();
		}

		public override string ToString()
		{
			return "Battle Reward";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					writer.Write(Amount);
					goto case 0;
				case 0:
					{
						writer.Write(Enabled);
						writer.WriteFlag(_Class);
						writer.WriteFlag(DeliveryMethod);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					Amount = reader.ReadInt();
					goto case 0;
				case 0:
					{
						Enabled = reader.ReadBool();
						_Class = reader.ReadFlag<PvPRewardClass>();
						DeliveryMethod = reader.ReadFlag<PvPRewardDeliveryMethod>();

						if (version < 1)
						{
							Amount = 1;
						}
					}
					break;
			}
		}
	}
}