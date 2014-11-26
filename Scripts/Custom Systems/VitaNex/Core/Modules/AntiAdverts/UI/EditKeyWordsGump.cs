#region Header
//   Vorspire    _,-'/-'/  EditKeyWordsGump.cs
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

using VitaNex.SuperGumps.UI;
#endregion

namespace Server.Misc
{
	public sealed class AntiAdvertsEditKeyWordsGump : GenericListGump<string>
	{
		public string Input { get; set; }

		public AntiAdvertsEditKeyWordsGump(PlayerMobile user, Gump parent = null)
			: base(
				user,
				parent,
				list: AntiAdverts.CMOptions.KeyWords,
				title: "Anti-Adverts: Key Words",
				emptyText: "No key words to display.",
				canAdd: true,
				canClear: true,
				canRemove: true)
		{ }

		public override string GetSearchKeyFor(string key)
		{
			return key ?? String.Empty;
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
					title: "Add Key Word",
					html: "Write a phrase to add it to this list.",
					callback: (b1, text) =>
					{
						Input = !String.IsNullOrWhiteSpace(text) ? text : null;
						HandleAdd();
						Input = null;
					}));

			return false;
		}

		public override List<string> GetExternalList()
		{
			return AntiAdverts.CMOptions.KeyWords;
		}

		public override string GetListAddObject()
		{
			return Input;
		}
	}
}