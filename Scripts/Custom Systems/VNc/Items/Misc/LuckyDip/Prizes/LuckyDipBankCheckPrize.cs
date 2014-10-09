#region Header
//   Vorspire    _,-'/-'/  LuckyDipBankCheckPrize.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Items;
#endregion

namespace VitaNex.Items
{
	public class LuckyDipBankCheckPrize : LuckyDipPrize
	{
		public LuckyDipBankCheckPrize()
			: this(0.0, 0)
		{ }

		public LuckyDipBankCheckPrize(double chance, int worth)
			: base(chance, typeof(BankCheck), worth)
		{ }

		public int Worth { get { return Args[0] as int? ?? 0; } }
	}
}