#region Header
//   Vorspire    _,-'/-'/  SpellExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Spells;

using VitaNex;
#endregion

namespace Server
{
	public static class SpellExtUtility
	{
		public static SpellInfo GetSpellInfo(this ISpell s)
		{
			return SpellUtility.GetSpellInfo(s);
		}
	}
}