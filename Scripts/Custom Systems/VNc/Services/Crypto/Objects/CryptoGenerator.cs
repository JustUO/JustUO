#region Header
//   Vorspire    _,-'/-'/  CryptoGenerator.cs
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
#endregion

namespace VitaNex.Crypto
{
	public static class CryptoGenerator
	{
		public static string GenString(CryptoHashType type, string seed)
		{
			return CryptoService.Providers[(int)type].Generate(seed);
		}

		public static string GenString(int type, string seed)
		{
			return !CryptoService.IsExtended(type)
					   ? CryptoService.Providers[type].Generate(seed)
					   : (CryptoService.ExtendedProviders.ContainsKey(type) && CryptoService.ExtendedProviders[type] != null
							  ? CryptoService.ExtendedProviders[type].Generate(seed)
							  : String.Empty);
		}

		public static CryptoHashCode GenHashCode(CryptoHashType type, string seed)
		{
			return new CryptoHashCode((int)type, seed);
		}

		public static CryptoHashCode GenHashCode(int type, string seed)
		{
			return new CryptoHashCode(type, seed);
		}

		public static CryptoHashCodeTable GenTable(string seed)
		{
			return new CryptoHashCodeTable(seed);
		}
	}
}