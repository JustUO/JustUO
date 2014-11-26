#region Header
//   Vorspire    _,-'/-'/  Crypto.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Security.Cryptography;
#endregion

namespace VitaNex.Crypto
{
	public static partial class CryptoService
	{
		public static CryptoHashCodeProvider[] Providers { get; private set; }
		public static Dictionary<int, CryptoHashCodeProvider> ExtendedProviders { get; private set; }

		public static void RegisterProvider(CryptoHashType type, HashAlgorithm hal)
		{
			Providers[(int)type] = new CryptoHashCodeProvider((int)type, hal);
		}

		public static void RegisterProvider(int type, HashAlgorithm hal)
		{
			if (type < 0)
			{
				return;
			}

			if (!IsExtended(type))
			{
				Providers[type] = new CryptoHashCodeProvider(type, hal);
			}
			else
			{
				if (ExtendedProviders.ContainsKey(type))
				{
					ExtendedProviders[type] = new CryptoHashCodeProvider(type, hal);
				}
				else
				{
					ExtendedProviders.Add(type, new CryptoHashCodeProvider(type, hal));
				}
			}
		}

		public static bool IsExtended(int type)
		{
			return (type >= Providers.Length);
		}

		public static CryptoHashCodeProvider GetProvider(CryptoHashType type)
		{
			return Providers[(int)type];
		}

		public static CryptoHashCodeProvider GetProvider(int type)
		{
			return type >= 0
					   ? (!IsExtended(type) ? Providers[type] : (ExtendedProviders.ContainsKey(type) ? ExtendedProviders[type] : null))
					   : null;
		}
	}
}