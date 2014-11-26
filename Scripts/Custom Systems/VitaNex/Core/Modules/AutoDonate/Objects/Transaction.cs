#region Header
//   Vorspire    _,-'/-'/  Transaction.cs
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
using Server.Accounting;
using Server.Items;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public enum DonationTransactionState
	{
		Void = 0,
		Pending,
		Processed,
		Claimed
	}

	public sealed class DonationTransaction
	{
		private DonationCredits _Credit;
		private string _Extra;
		private int _InternalVersion;
		private string _Notes;

		private DonationTransactionState _State;
		private TimeStamp _Time;

		public string ID { get; private set; }
		public int Version { get; private set; }

		public DonationTransactionState State
		{
			get { return _State; }
			set
			{
				if (_State == value)
				{
					return;
				}

				DonationTransactionState oldState = _State, newState = value;

				_State = newState;
				Version = _InternalVersion + 1;

				OnStateChanged(oldState);
			}
		}

		public IAccount Account { get; private set; }
		public string Email { get; private set; }
		public decimal Total { get; private set; }

		public DonationCredits Credit
		{
			get { return _Credit; }
			set
			{
				_Credit = value;
				Version = _InternalVersion + 1;
			}
		}

		public TimeStamp Time
		{
			get { return _Time; }
			set
			{
				_Time = value;
				Version = _InternalVersion + 1;
			}
		}

		public string Notes
		{
			get { return _Notes; }
			set
			{
				_Notes = value;
				Version = _InternalVersion + 1;
			}
		}

		public string Extra
		{
			get { return _Extra; }
			set
			{
				_Extra = value;
				Version = _InternalVersion + 1;
			}
		}

		public Mobile DeliverFrom { get; private set; }
		public Mobile DeliverTo { get; private set; }
		public TimeStamp DeliveryTime { get; private set; }

		public bool IsGift { get { return DeliverFrom != null && DeliverFrom != DeliverTo; } }

		public bool Hidden { get; set; }

		public DonationTransaction(
			string id,
			DonationTransactionState state,
			IAccount account,
			string email,
			decimal total,
			DonationCredits credit,
			TimeStamp time,
			int version = 0,
			string notes = null,
			string extra = null)
		{
			ID = id;
			_State = state;
			Account = account;
			Email = email;
			Total = total;
			_Credit = credit;
			_Time = time;
			Version = version;
			_InternalVersion = version;
			_Notes = notes ?? String.Empty;
			_Extra = extra ?? String.Empty;
		}

		public DonationTransaction(GenericReader reader)
		{
			Deserialize(reader);
		}

		private void OnStateChanged(DonationTransactionState oldState)
		{
			DonationEvents.InvokeStateChanged(this, oldState);
		}

		public bool Process()
		{
			if ((State = DonationTransactionState.Processed) == DonationTransactionState.Processed)
			{
				DonationEvents.InvokeTransProcessed(this);
				return true;
			}

			return false;
		}

		public bool Void()
		{
			if ((State = DonationTransactionState.Void) == DonationTransactionState.Void)
			{
				if (!AutoDonate.CMOptions.ShowHistory)
				{
					Hidden = true;
				}

				DonationEvents.InvokeTransVoided(this);
				return true;
			}

			return false;
		}

		public bool Claim(Mobile from, Mobile to)
		{
			return Claim(from, to, String.Empty);
		}

		public bool Claim(Mobile from, Mobile to, string message, params object[] args)
		{
			if (from != null && to != null &&
				((from.Player && from.Account == Account) || from.AccessLevel >= AccessLevel.Administrator))
			{
				DeliverFrom = from;
				DeliverTo = to;
				DeliveryTime = TimeStamp.UtcNow;

				if (Deliver(message, args))
				{
					DonationEvents.InvokeTransDelivered(this);

					if (!AutoDonate.CMOptions.ShowHistory)
					{
						Hidden = true;
					}

					State = DonationTransactionState.Claimed;
					DonationEvents.InvokeTransClaimed(this);
					return true;
				}

				DeliverFrom = null;
				DeliverTo = null;
				DeliveryTime = TimeStamp.Zero;
			}

			return false;
		}

		private bool Deliver()
		{
			return Deliver(String.Empty);
		}

		private bool Deliver(string message, params object[] args)
		{
			IAccount a = DeliverTo.Account;

			if (a == null)
			{
				return false;
			}

			if (AutoDonate.Find(a) == null)
			{
				AutoDonate.Register(a, new DonationProfile(a));
			}

			AutoDonate.Profiles[a].Credits += _Credit;

			if (IsGift)
			{
				AutoDonate.Profiles[a].AddGift(this, message, args);
			}

			return DeliveryConversion();
		}

		private bool DeliveryConversion()
		{
			const int amountCap = 60000;

			double exchangeRate = AutoDonate.CMOptions.ExchangeRate;
			DonationProfile dp = AutoDonate.Find(DeliverTo.Account);
			Container bank = DeliverTo.BankBox;

			if (bank != null)
			{
				Bag bag = new Bag();

				if (IsGift)
				{
					bag.Name = "A Donation Gift Bag";
					bag.Hue = 1152;

					string text = dp.Gifts.ContainsKey(ID) ? dp.Gifts[ID] : null;

					if (String.IsNullOrWhiteSpace(text))
					{
						text = "Hi, " + DeliverTo.RawName + ", ";
						text += "Here is a gift, a token of my appreciation. ";
						text += "Don't spend it all in one place! ";
						text += "Regards, " + DeliverFrom.RawName;
					}

					bag.DropItem(new DonationGiftBook(DeliverFrom, text));
				}
				else
				{
					bag.Name = "A Donation Reward Bag";
					bag.Hue = 1152;
				}

				long exchanged = (long)(Credit * exchangeRate);

				while (exchanged >= amountCap)
				{
					Item cur = AutoDonate.CMOptions.CurrencyType.CreateInstance();
					cur.Amount = amountCap;

					bag.DropItem(cur);

					if (cur.IsChildOf(bag))
					{
						exchanged -= cur.Amount;
					}
				}

				if (exchanged > 0)
				{
					Item cur = AutoDonate.CMOptions.CurrencyType.CreateInstance();
					cur.Amount = (int)exchanged;

					bag.DropItem(cur);

					if (cur.IsChildOf(bag))
					{
						exchanged -= cur.Amount;
					}
				}

				if (exchanged > 0)
				{
					bag.Delete();
					return false;
				}

				bank.DropItem(bag);
				dp.Credits -= Credit;
			}

			return true;
		}

		public void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(ID);
						writer.WriteFlag(_State);
						writer.Write(Account);
						writer.Write(Email);
						writer.Write(Total);
						writer.Write(_Credit);
						writer.Write(_Time.Stamp);
						writer.Write(Version);
						writer.Write(_InternalVersion);
						writer.Write(_Notes);
						writer.Write(_Extra);

						writer.Write(DeliverFrom);
						writer.Write(DeliverTo);

						writer.Write(DeliveryTime.Stamp);
					}
					break;
			}
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						ID = reader.ReadString();
						_State = reader.ReadFlag<DonationTransactionState>();
						Account = reader.ReadAccount();
						Email = reader.ReadString();
						Total = reader.ReadDecimal();
						_Credit = reader.ReadLong();
						_Time = reader.ReadDouble();
						Version = reader.ReadInt();
						_InternalVersion = reader.ReadInt();
						_Notes = reader.ReadString();
						_Extra = reader.ReadString();

						DeliverFrom = reader.ReadMobile();
						DeliverTo = reader.ReadMobile();

						DeliveryTime = reader.ReadDouble();
					}
					break;
			}
		}
	}
}