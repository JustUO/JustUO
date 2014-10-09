#region Header
//   Vorspire    _,-'/-'/  ItemSelect.cs
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

using Server;
using Server.Targeting;
#endregion

namespace VitaNex.Targets
{
	/// <summary>
	///     Provides methods for selecting specific Items of the given Type
	/// </summary>
	/// <typeparam name="TItem">Type of the Item to be selected</typeparam>
	public class ItemSelectTarget<TItem> : GenericSelectTarget<TItem>
		where TItem : Item
	{
		/// <summary>
		///     Create an instance of ItemSelectTarget with handlers
		/// </summary>
		public ItemSelectTarget(Action<Mobile, TItem> success, Action<Mobile> fail)
			: base(success, fail)
		{ }

		/// <summary>
		///     Create an instance of ItemSelectTarget with handlers and additional options
		/// </summary>
		public ItemSelectTarget(
			Action<Mobile, TItem> success, Action<Mobile> fail, int range, bool allowGround, TargetFlags flags)
			: base(success, fail, range, allowGround, flags)
		{ }
	}
}