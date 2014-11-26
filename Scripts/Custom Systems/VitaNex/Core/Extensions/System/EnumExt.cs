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
using System.Collections.Generic;
using System.Linq;
#endregion

namespace System
{
	public static class EnumExtUtility
	{
		public static TCast[] GetValues<TCast>(this Enum e)
		{
			return EnumerateValues<TCast>(e, false).ToArray();
		}

		public static TCast[] GetValues<TCast>(this Enum e, bool local)
		{
			return EnumerateValues<TCast>(e, local).ToArray();
		}

		public static IEnumerable<TCast> EnumerateValues<TCast>(this Enum e, bool local)
		{
			Type vType = typeof(TCast);
			var vals = Enum.GetValues(e.GetType()).Cast<Enum>();

			if (local)
			{
				vals = vals.Where(e.HasFlag);
			}

			if (vType.IsEqual(typeof(char)))
			{
				return vals.Select(Convert.ToChar).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(sbyte)))
			{
				return vals.Select(Convert.ToSByte).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(byte)))
			{
				return vals.Select(Convert.ToByte).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(short)))
			{
				return vals.Select(Convert.ToInt16).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(ushort)))
			{
				return vals.Select(Convert.ToUInt16).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(int)))
			{
				return vals.Select(Convert.ToInt32).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(uint)))
			{
				return vals.Select(Convert.ToUInt32).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(long)))
			{
				return vals.Select(Convert.ToInt64).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(ulong)))
			{
				return vals.Select(Convert.ToUInt64).Cast<TCast>();
			}

			if (vType.IsEqual(typeof(string)))
			{
				return vals.Select(Convert.ToString).Cast<TCast>();
			}

			return vals.Cast<TCast>();
		}
	}
}