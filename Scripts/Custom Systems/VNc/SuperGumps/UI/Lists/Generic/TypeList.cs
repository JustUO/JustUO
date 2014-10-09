#region Header
//   Vorspire    _,-'/-'/  TypeList.cs
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

using Server;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public abstract class TypeListGump : GenericListGump<Type>
	{
		public Type InputType { get; set; }

		public TypeListGump(
			PlayerMobile user,
			Gump parent = null,
			int? x = null,
			int? y = null,
			IEnumerable<Type> list = null,
			string emptyText = null,
			string title = null,
			IEnumerable<ListGumpEntry> opts = null,
			bool canAdd = true,
			bool canRemove = true,
			bool canClear = true,
			Action<Type> addCallback = null,
			Action<Type> removeCallback = null,
			Action<List<Type>> applyCallback = null,
			Action clearCallback = null)
			: base(
				user,
				parent,
				x,
				y,
				list,
				emptyText,
				title,
				opts,
				canAdd,
				canRemove,
				canClear,
				addCallback,
				removeCallback,
				applyCallback,
				clearCallback)
		{ }

		public override string GetSearchKeyFor(Type key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override bool OnBeforeListAdd()
		{
			if (InputType != null)
			{
				return true;
			}

			Send(
				new InputDialogGump(
					User,
					Refresh(),
					title: "Add Type by Name",
					html: "Write the name of a Type to add it to this list.\nExample: System.String",
					callback: (b1, text) =>
					{
						InputType =
							VitaNexCore.TryCatchGet(
								() =>
								Type.GetType(text, false, true) ??
								ScriptCompiler.FindTypeByFullName(text, true) ?? ScriptCompiler.FindTypeByName(text, true));

						HandleAdd();
						InputType = null;
					}));

			return false;
		}

		public override Type GetListAddObject()
		{
			return InputType;
		}
	}
}