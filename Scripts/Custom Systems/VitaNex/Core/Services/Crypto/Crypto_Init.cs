#region Header
//   Vorspire    _,-'/-'/  Crypto_Init.cs
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
	[CoreService("Crypto", "1.0.0.0", TaskPriority.Highest)]
	public static partial class CryptoService
	{
		static CryptoService()
		{
			Providers = new CryptoHashCodeProvider[6];
			ExtendedProviders = new Dictionary<int, CryptoHashCodeProvider>();
		}

		private static void CSConfig()
		{
			RegisterProvider(CryptoHashType.MD5, MD5.Create());
			RegisterProvider(CryptoHashType.SHA1, SHA1.Create());
			RegisterProvider(CryptoHashType.SHA256, SHA256.Create());
			RegisterProvider(CryptoHashType.SHA384, SHA384.Create());
			RegisterProvider(CryptoHashType.SHA512, SHA512.Create());
			RegisterProvider(CryptoHashType.RIPEMD160, RIPEMD160.Create());
		}
	}
}