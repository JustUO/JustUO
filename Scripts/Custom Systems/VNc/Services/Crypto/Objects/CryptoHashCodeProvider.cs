#region Header
//   Vorspire    _,-'/-'/  CryptoHashCodeProvider.cs
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
using System.Security.Cryptography;
using System.Text;
#endregion

namespace VitaNex.Crypto
{
	public class CryptoHashCodeProvider : ICryptoProvider
	{
		public CryptoHashCodeProvider(int id, HashAlgorithm hal)
		{
			ProviderID = id;
			Provider = hal;
		}

		public int ProviderID { get; private set; }
		public HashAlgorithm Provider { get; private set; }
		public byte[] Buffer { get; private set; }

		public string Generate(string seed)
		{
			Buffer = Provider == null
						 ? Encoding.Default.GetBytes(Transform(seed))
						 : Provider.ComputeHash(Encoding.Default.GetBytes(Transform(seed)));

			return Mutate(BitConverter.ToString(Buffer));
		}

		public virtual string Transform(string seed)
		{
			return seed;
		}

		public virtual string Mutate(string hash)
		{
			return hash;
		}

		public void Dispose()
		{
			if (Provider != null)
			{
				Provider.Clear();
				Provider = null;
			}

			Buffer = null;
		}
	}
}