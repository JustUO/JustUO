#region Header
//   Vorspire    _,-'/-'/  MobileSelect.cs
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
	///     Provides methods for selecting specific Mobiles of the given Type
	/// </summary>
	/// <typeparam name="TMobile">Type of the Item to be selected</typeparam>
	public class MobileSelectTarget<TMobile> : GenericSelectTarget<TMobile>
		where TMobile : Mobile
	{
		/// <summary>
		///     Create an instance of ItemSelectTarget with handlers
		/// </summary>
		public MobileSelectTarget(Action<Mobile, TMobile> success, Action<Mobile> fail)
			: base(success, fail)
		{ }

		/// <summary>
		///     Create an instance of ItemSelectTarget with handlers and additional options
		/// </summary>
		public MobileSelectTarget(
			Action<Mobile, TMobile> success, Action<Mobile> fail, int range, bool allowGround, TargetFlags flags)
			: base(success, fail, range, allowGround, flags)
		{ }
	}
}