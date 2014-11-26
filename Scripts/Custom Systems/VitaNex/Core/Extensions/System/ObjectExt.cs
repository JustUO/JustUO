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
using System.Reflection;

using VitaNex;
using VitaNex.Reflection;
#endregion

namespace System
{
	public static class ObjectExtUtility
	{
		public static bool TypeEquals<T>(this object obj, bool child = true)
		{
			return TypeEquals(obj, typeof(T), child);
		}

		public static bool TypeEquals(this object obj, object other, bool child = true)
		{
			if (obj == null || other == null)
			{
				return false;
			}

			if (ReferenceEquals(obj, other))
			{
				return true;
			}

			Type l, r;

			if (obj is ITypeSelectProperty)
			{
				l = ((ITypeSelectProperty)obj).InternalType;
			}
			else
			{
				l = obj as Type ?? obj.GetType();
			}

			if (other is ITypeSelectProperty)
			{
				r = ((ITypeSelectProperty)other).InternalType;
			}
			else
			{
				r = other as Type ?? other.GetType();
			}

			if (l == null || r == null)
			{
				return false;
			}

			return child ? l.IsEqualOrChildOf(r) : l.IsEqual(r);
		}

		public static bool CheckNull<T>(this T obj, T other)
		{
			int result = 0;

			return CompareNull(obj, other, ref result);
		}

		public static int CompareNull<T>(this T obj, T other)
		{
			int result = 0;

			CompareNull(obj, other, ref result);

			return result;
		}

		public static bool CompareNull<T>(this T obj, T other, ref int result)
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

		public static FieldList<T> GetFields<T>(
			this T obj, BindingFlags flags = BindingFlags.Default, Func<FieldInfo, bool> filter = null)
		{
			return new FieldList<T>(obj, flags, filter);
		}

		public static PropertyList<T> GetProperties<T>(
			this T obj, BindingFlags flags = BindingFlags.Default, Func<PropertyInfo, bool> filter = null)
		{
			return new PropertyList<T>(obj, flags, filter);
		}
	}
}