#region Header
//   Vorspire    _,-'/-'/  Profile.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Accounting;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public sealed class DonationProfile : IEnumerable<DonationTransaction>, IEquatable<DonationProfile>
	{
		public DonationProfile()
		{
			Transactions = new Dictionary<string, DonationTransaction>();
		}

		public DonationProfile(IAccount account)
			: this(account, 0)
		{ }

		public DonationProfile(IAccount account, DonationCredits credits)
		{
			Account = account;
			Credits = credits;
			Transactions = new Dictionary<string, DonationTransaction>();
			Gifts = new Dictionary<string, string>();
		}

		public DonationProfile(GenericReader reader)
		{
			Deserialize(reader);
		}

		public IAccount Account { get; private set; }
		public DonationCredits Credits { get; set; }

		public Dictionary<string, DonationTransaction> Transactions { get; private set; }
		public Dictionary<string, string> Gifts { get; private set; }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Transactions.Values.GetEnumerator();
		}

		public IEnumerator<DonationTransaction> GetEnumerator()
		{
			return Transactions.Values.GetEnumerator();
		}

		public bool Equals(DonationProfile other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return Equals(Account, other.Account);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			return obj is DonationProfile && Equals((DonationProfile)obj);
		}

		public override int GetHashCode()
		{
			return (Account != null ? Account.GetHashCode() : 0);
		}

		public static bool operator ==(DonationProfile left, DonationProfile right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DonationProfile left, DonationProfile right)
		{
			return !Equals(left, right);
		}

		public void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Account);
						writer.Write(Credits);

						writer.WriteDictionary(
							Transactions,
							(k, v) =>
							{
								if (v == null || v.Account == null || String.IsNullOrWhiteSpace(v.Account.Username))
								{
									writer.Write(false);
								}
								else
								{
									writer.Write(true);
									v.Serialize(writer);
								}
							});

						writer.WriteDictionary(
							Gifts,
							(k, v) =>
							{
								writer.Write(k);
								writer.Write(v);
							});
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
						Account = reader.ReadAccount();
						Credits = reader.ReadLong();

						Transactions = reader.ReadDictionary(
							() =>
							{
								if (reader.ReadBool())
								{
									DonationTransaction t = new DonationTransaction(reader);
									return new KeyValuePair<string, DonationTransaction>(t.ID, t);
								}

								return new KeyValuePair<string, DonationTransaction>(null, null);
							});

						Gifts = reader.ReadDictionary(
							() =>
							{
								string k = reader.ReadString();
								string v = reader.ReadString();
								return new KeyValuePair<string, string>(k, v);
							});
					}
					break;
			}
		}

		public DonationTransaction[] Find(DonationTransactionState state)
		{
			return Transactions.Values.Where(trans => trans.State == state).ToArray();
		}

		public DonationTransaction[] Find(string[] ids)
		{
			return ids.Select(Find).Where(trans => trans != null).ToArray();
		}

		public DonationTransaction Find(string id)
		{
			return Transactions.ContainsKey(id) ? Transactions[id] : null;
		}

		public bool Contains(DonationTransaction trans)
		{
			return Transactions.ContainsKey(trans.ID);
		}

		public void Add(DonationTransaction trans)
		{
			if (Contains(trans))
			{
				Transactions[trans.ID] = trans;
			}
			else
			{
				Transactions.Add(trans.ID, trans);
			}
		}

		public bool Remove(DonationTransaction trans)
		{
			return Contains(trans) && Transactions.Remove(trans.ID);
		}

		public void AddGift(DonationTransaction trans)
		{
			AddGift(trans.ID);
		}

		public void AddGift(DonationTransaction trans, string message, params object[] args)
		{
			AddGift(trans.ID, message, args);
		}

		public void AddGift(string transID)
		{
			AddGift(transID, String.Empty);
		}

		public void AddGift(string transID, string message, params object[] args)
		{
			if (!Gifts.ContainsKey(transID))
			{
				Gifts.Add(transID, String.Format(message, args));
			}
		}

		public bool RemoveGift(DonationTransaction trans)
		{
			return RemoveGift(trans.ID);
		}

		public bool RemoveGift(string transID)
		{
			if (Gifts.ContainsKey(transID))
			{
				return Gifts.Remove(transID);
			}

			return false;
		}

		public bool Process(DonationTransaction trans)
		{
			if (Contains(trans))
			{
				return Transactions[trans.ID].Process();
			}

			if (trans.Process())
			{
				Add(trans);
				return true;
			}

			return false;
		}

		public bool Void(DonationTransaction trans)
		{
			if (Contains(trans))
			{
				return Transactions[trans.ID].Void();
			}

			if (trans.Void())
			{
				Add(trans);
				return true;
			}

			return false;
		}

		public bool Claim(DonationTransaction trans, Mobile from, Mobile to)
		{
			return Claim(trans, from, to, String.Empty);
		}

		public bool Claim(DonationTransaction trans, Mobile from, Mobile to, string message, params object[] args)
		{
			if (Contains(trans))
			{
				return Transactions[trans.ID].Claim(from, to, message, args);
			}

			if (trans.Claim(from, to, message, args))
			{
				Add(trans);
				return true;
			}

			return false;
		}
	}
}