#region Header
//   Vorspire    _,-'/-'/  EnumExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Linq;
#endregion

namespace System
{
	public static class EnumExtUtility
	{
		public static TCast[] GetValues<TCast>(this Enum e) where TCast : struct
		{
			Type vType = typeof(TCast);
			var vals = Enum.GetValues(e.GetType()).Cast<object>();

			if (vType.IsEqual(typeof(sbyte)))
			{
				return vals.Select(Convert.ToSByte).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(byte)))
			{
				return vals.Select(Convert.ToByte).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(short)))
			{
				return vals.Select(Convert.ToInt16).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(ushort)))
			{
				return vals.Select(Convert.ToUInt16).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(int)))
			{
				return vals.Select(Convert.ToInt32).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(uint)))
			{
				return vals.Select(Convert.ToUInt32).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(long)))
			{
				return vals.Select(Convert.ToInt64).Cast<TCast>().ToArray();
			}

			if (vType.IsEqual(typeof(ulong)))
			{
				return vals.Select(Convert.ToUInt64).Cast<TCast>().ToArray();
			}

			return vals.Cast<TCast>().ToArray();
		}
	}
}