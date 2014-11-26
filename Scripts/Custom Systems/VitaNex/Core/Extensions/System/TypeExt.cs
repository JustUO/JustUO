#region Header
//   Vorspire    _,-'/-'/  TypeExt.cs
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
using System.Reflection;

using Server;
#endregion

namespace System
{
	public static class TypeExtUtility
	{
		private static readonly Dictionary<Type, Type[]> _ChildrenCache = new Dictionary<Type, Type[]>();
		private static readonly Dictionary<Type, Type[]> _ConstructableChildrenCache = new Dictionary<Type, Type[]>();

		public static TAttribute[] GetCustomAttributes<TAttribute>(this Type t, bool inherit) where TAttribute : Attribute
		{
			return t != null
					   ? t.GetCustomAttributes(typeof(TAttribute), inherit).OfType<TAttribute>().ToArray()
					   : new TAttribute[0];
		}

		public static int CompareTo(this Type t, Type other)
		{
			int result = 0;

			if (t.CompareNull(other, ref result))
			{
				return result;
			}

			var lp = t.BaseType;

			while (lp != null)
			{
				if (lp == other)
				{
					return -1;
				}

				lp = lp.BaseType;
			}

			return 1;
		}

		public static bool IsEqual(this Type a, Type b)
		{
			return a != null && b != null && a == b;
		}

		public static bool IsEqual<TObj>(this Type t)
		{
			return IsEqual(t, typeof(TObj));
		}

		public static bool IsEqualOrChildOf(this Type a, Type b)
		{
			return a != null && b != null && (a == b || a.IsChildOf(b));
		}

		public static bool IsEqualOrChildOf<TObj>(this Type t)
		{
			return IsEqualOrChildOf(t, typeof(TObj));
		}

		public static bool IsChildOf(this Type a, Type b)
		{
			return a != null && b != null && a != b && !a.IsInterface && !a.IsEnum &&
				   (b.IsInterface ? HasInterface(a, b) : b.IsAssignableFrom(a));
		}

		public static bool IsChildOf<TObj>(this Type t)
		{
			return IsChildOf(t, typeof(TObj));
		}

		public static bool HasInterface<TObj>(this Type t)
		{
			return HasInterface(t, typeof(TObj));
		}

		public static bool HasInterface(this Type t, Type i)
		{
			return t != null && i != null && i.IsInterface && t.GetInterface(i.FullName) != null;
		}

		public static bool IsConstructable(this Type a)
		{
			return IsConstructable(a, new Type[0]);
		}

		public static bool IsConstructable(this Type a, Type[] argTypes)
		{
			if (a == null || a.IsAbstract || a.IsInterface || a.IsEnum)
			{
				return false;
			}

			return a.GetConstructor(argTypes) != null;
		}

		public static bool IsConstructableFrom(this Type a, Type b)
		{
			if (a == null || b == null || a.IsAbstract || !a.IsChildOf(b))
			{
				return false;
			}

			return a.GetConstructors().Length > 0;
		}

		public static bool IsConstructableFrom<TObj>(this Type t)
		{
			return IsConstructableFrom(t, typeof(TObj));
		}

		public static bool IsConstructableFrom(this Type a, Type b, Type[] argTypes)
		{
			if (a == null || b == null || a.IsAbstract || !a.IsChildOf(b))
			{
				return false;
			}

			return a.GetConstructor(argTypes) != null;
		}

		public static bool IsConstructableFrom<TObj>(this Type t, Type[] argTypes)
		{
			return IsConstructableFrom(t, typeof(TObj), argTypes);
		}

		public static Type[] GetConstructableChildren(this Type type, Predicate<Type> predicate = null)
		{
			if (type == null)
			{
				return new Type[0];
			}

			if (_ConstructableChildrenCache.ContainsKey(type))
			{
				return _ConstructableChildrenCache[type];
			}

			if (_ConstructableChildrenCache.Count >= 100)
			{
				_ConstructableChildrenCache.Pop();
			}

			var types = new List<Type>(100);
			var assemblies = new List<Assembly>(ScriptCompiler.Assemblies);

			if (!assemblies.Contains(Assembly.GetCallingAssembly()))
			{
				assemblies.Add(Assembly.GetCallingAssembly());
			}

			assemblies.ForEach(
				asm => asm.GetTypes().ForEach(
					t =>
					{
						if (t.IsEqual(type) || !t.IsChildOf(type) || !t.IsConstructableFrom(type))
						{
							return;
						}

						if (predicate != null)
						{
							if (predicate(t))
							{
								types.Add(t);
							}
						}
						else
						{
							types.Add(t);
						}
					}));

			var ret = types.ToArray();

			types.Free(true);

			if (ret.Length <= 100)
			{
				_ConstructableChildrenCache.Add(type, ret);
			}

			return ret;
		}

		public static Type[] GetChildren(this Type type, Predicate<Type> predicate = null)
		{
			if (type == null)
			{
				return new Type[0];
			}

			if (_ChildrenCache.ContainsKey(type))
			{
				return _ChildrenCache[type];
			}

			if (_ChildrenCache.Count >= 100)
			{
				_ChildrenCache.Pop();
			}

			var asm = new List<Assembly>(ScriptCompiler.Assemblies);

			asm.AddOrReplace(Assembly.GetCallingAssembly());

			var ret =
				asm.SelectMany(a => a.GetTypes().Where(t => t.IsChildOf(type) && (predicate == null || predicate(t)))).ToArray();

			asm.Free(true);

			if (ret.Length <= 100)
			{
				_ChildrenCache.Add(type, ret);
			}

			return ret;
		}

		public static TObj CreateInstance<TObj>(this Type t, params object[] args)
		{
			return t == null || t.IsAbstract || t.IsInterface
					   ? default(TObj)
					   : ((args == null || args.Length == 0)
							  ? (TObj)Activator.CreateInstance(t)
							  : (TObj)Activator.CreateInstance(t, args));
		}

		public static TObj CreateInstanceSafe<TObj>(this Type t, params object[] args)
		{
			try
			{
				return CreateInstance<TObj>(t, args);
			}
			catch
			{
				return default(TObj);
			}
		}

		public static object CreateInstance(this Type t, params object[] args)
		{
			return CreateInstanceSafe<object>(t, args);
		}
	}
}