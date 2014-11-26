#region Header
//   Vorspire    _,-'/-'/  EditAliasesGump.cs
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
using System.Globalization;
using System.Linq;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace Server.Misc
{
	public sealed class AntiAdvertsEditAliasesGump : GenericListGump<char>
	{
		public string Input { get; set; }

		public AntiAdvertsEditAliasesGump(PlayerMobile user, Gump parent = null)
			: base(
				user,
				parent,
				list: AntiAdverts.CMOptions.WhitespaceAliases,
				title: "Anti-Adverts: Whitespace Aliases",
				emptyText: "No whitespace aliases to display.",
				canAdd: true,
				canClear: true,
				canRemove: true)
		{ }

		public override string GetSearchKeyFor(char key)
		{
			return key.ToString(CultureInfo.InvariantCulture);
		}

		protected override bool OnBeforeListAdd()
		{
			if (Input != null)
			{
				return true;
			}

			Send(
				new InputDialogGump(
					User,
					Refresh(),
					title: "Add Whitespace Alias",
					html: "Write a single character to add it to this list.",
					limit: 1,
					callback: (b1, text) =>
					{
						Input = !String.IsNullOrWhiteSpace(text) ? text : String.Empty;
						HandleAdd();
						Input = null;
					}));

			return false;
		}

		public override List<char> GetExternalList()
		{
			return AntiAdverts.CMOptions.WhitespaceAliases;
		}

		public override char GetListAddObject()
		{
			return Input.FirstOrDefault();
		}
	}
}