#region Header
//   Vorspire    _,-'/-'/  CryptoHashCodeTable.cs
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
#endregion

namespace VitaNex.Crypto
{
	public class CryptoHashCodeTable : IEnumerable<CryptoHashCode>, IDisposable
	{
		private Dictionary<int, CryptoHashCode> _ExtendedHashCodes;
		private CryptoHashCode[] _HashCodes;
		private CryptoHashCode _UID;

		public CryptoHashCodeTable(string seed)
		{
			_HashCodes = new CryptoHashCode[CryptoService.Providers.Length];

			foreach (CryptoHashCodeProvider hcp in CryptoService.Providers)
			{
				_HashCodes[hcp.ProviderID] = CryptoGenerator.GenHashCode(hcp.ProviderID, seed);
			}

			_ExtendedHashCodes = new Dictionary<int, CryptoHashCode>(CryptoService.ExtendedProviders.Count);

			foreach (CryptoHashCodeProvider hcp in CryptoService.ExtendedProviders.Values)
			{
				if (_ExtendedHashCodes.ContainsKey(hcp.ProviderID))
				{
					_ExtendedHashCodes[hcp.ProviderID] = CryptoGenerator.GenHashCode(hcp.ProviderID, seed);
				}
				else
				{
					_ExtendedHashCodes.Add(hcp.ProviderID, CryptoGenerator.GenHashCode(hcp.ProviderID, seed));
				}
			}

			_UID = GenerateUID(this);
		}

		public CryptoHashCode UID { get { return _UID = _UID ?? GenerateUID(this); } }

		public virtual void Dispose()
		{
			_HashCodes = null;

			if (_ExtendedHashCodes == null)
			{
				return;
			}

			_ExtendedHashCodes.Clear();
			_ExtendedHashCodes = null;
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<CryptoHashCode> GetEnumerator()
		{
			return GetHashCodes().GetEnumerator();
		}

		public static CryptoHashCode GenerateUID(CryptoHashCodeTable table)
		{
			return CryptoGenerator.GenHashCode(CryptoHashType.MD5, String.Join("+", table));
		}

		public virtual IEnumerable<CryptoHashCode> GetHashCodes()
		{
			foreach (CryptoHashCode v in _HashCodes)
			{
				yield return v;
			}

			foreach (var v in _ExtendedHashCodes)
			{
				yield return v.Value;
			}
		}

		public override bool Equals(object obj)
		{
			return base.Equals(obj);
		}

		public override int GetHashCode()
		{
			return _UID.GetHashCode();
		}

		public override string ToString()
		{
			return _UID.Value;
		}
	}
}