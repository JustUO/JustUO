#region Header
//   Vorspire    _,-'/-'/  ItemsList.cs
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
	public class PvPRestrictItemsListGump : PvPRestrictionListGump<Type>
	{
		public PvPRestrictItemsListGump(
			PlayerMobile user, PvPBattleItemRestrictions res, Gump parent = null, bool useConfirm = true)
			: base(user, res, parent, false, useConfirm)
		{ }

		public PvPBattleItemRestrictions ItemRestrictions { get { return Restrictions as PvPBattleItemRestrictions; } }

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
				   "\nUse Type names derived from Item.\nAdding 'Item' itself will count for every derived type.";
		}

		protected override void OnAddEntryConfirm(GumpButton b, string text)
		{
			ItemRestrictions.SetRestricted(text, true);
			Refresh(true);
		}
	}
}