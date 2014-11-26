#region Header
//   Vorspire    _,-'/-'/  SpellsList.cs
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

using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPRestrictSpellsListGump : PvPRestrictionListGump<Type>
	{
		public PvPRestrictSpellsListGump(
			PlayerMobile user, PvPBattleSpellRestrictions res, Gump parent = null, bool useConfirm = true)
			: base(user, res, parent, false, useConfirm)
		{ }

		public PvPBattleSpellRestrictions SpellRestrictions { get { return Restrictions as PvPBattleSpellRestrictions; } }

		protected override void CompileList(List<Type> list)
		{
			base.CompileList(list);

			list.Sort((a, b) => (String.Compare(GetSearchKeyFor(a), GetSearchKeyFor(b), StringComparison.Ordinal)));
		}

		public override string GetSearchKeyFor(Type key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override string GetLabelText(int index, int pageIndex, Type entry)
		{
			return entry != null ? entry.Name : base.GetLabelText(index, pageIndex, null);
		}

		protected override string OnAddEntryGetHtml()
		{
			return base.OnAddEntryGetHtml() +
				   "\nUse Type names derived from Spell.\nAdding 'Spell' itself will count for every derived type.";
		}

		protected override void OnAddEntryConfirm(GumpButton b, string text)
		{
			SpellRestrictions.SetRestricted(text, true);
			Refresh(true);
		}
	}
}