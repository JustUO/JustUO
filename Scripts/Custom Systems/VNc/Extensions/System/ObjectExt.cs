#region Header
//   Vorspire    _,-'/-'/  ObjectExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
#endregion

namespace System
{
	public static class ObjectExtUtility
	{
		public static int CompareNull<TObj>(this TObj obj, TObj other) where TObj : class
		{
			int result = 0;

			CompareNull(obj, other, ref result);

			return result;
		}

		public static bool CompareNull<TObj>(this TObj obj, TObj other, ref int result) where TObj : class
		{
			if (obj == null && other == null)
			{
				return true;
			}

			if (obj == null)
			{
				++result;
				return true;
			}

			if (other == null)
			{
				--result;
				return true;
			}

			return false;
		}
	}
}