#region Header
//   Vorspire    _,-'/-'/  PropertyCache.cs
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
using System.Linq;
using System.Reflection;
#endregion

namespace VitaNex.Reflection
{
	/// <summary>
	///     Used to mark object properties that should be included when accessing the PropertyCache service
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class CachedPropertyAttribute : Attribute
	{
		public CachedPropertyAttribute(bool persist = true)
		{
			Persist = persist;
		}

		/// <summary>
		///     Gets or sets a value indecating whether the marked property should be included when deserialized
		/// </summary>
		public bool Persist { get; set; }
	}

	/// <summary>
	///     Provides a caching service for object properties which have the CachedPropertyAttribute
	/// </summary>
	public class PropertyCache
	{
		public static readonly Type TypeOfCachedPropertyAttribute = typeof(CachedPropertyAttribute);

		public static int PopCap = 0x100;

		private static readonly Dictionary<Type, PropertyInfo[]> _InternalCache = new Dictionary<Type, PropertyInfo[]>(PopCap);

		/// <summary>
		///     Gets a collection representing the cached properties for the specified type
		/// </summary>
		public static PropertyInfo[] GetProperties<TObj>(bool ignoreCA = false)
		{
			return GetProperties<TObj>(BindingFlags.Default, ignoreCA);
		}

		/// <summary>
		///     Gets a collection representing the cached properties for the specified type
		/// </summary>
		public static PropertyInfo[] GetProperties(Type t, bool ignoreCA = false)
		{
			return GetProperties(t, BindingFlags.Default, ignoreCA);
		}

		/// <summary>
		///     Gets a collection representing the cached properties for the specified type using the specified BindingFlags
		/// </summary>
		public static PropertyInfo[] GetProperties<TObj>(BindingFlags flags, bool ignoreCA = false)
		{
			return GetProperties(typeof(TObj), flags, ignoreCA);
		}

		/// <summary>
		///     Gets a collection representing the cached properties for the specified type using the specified BindingFlags
		/// </summary>
		public static PropertyInfo[] GetProperties(Type t, BindingFlags flags, bool ignoreCA = false)
		{
			if ((flags & BindingFlags.Default) == BindingFlags.Default)
			{
				flags = BindingFlags.Public | BindingFlags.Instance;
			}

			var props = ignoreCA ? new PropertyInfo[0] : GetCachedProperties(t);

			if (props.Length == 0)
			{
				props = new List<PropertyInfo>(FindProperties(t, flags, ignoreCA)).ToArray();

				if (props.Length > 0)
				{
					if (_InternalCache.Count + 1 > PopCap)
					{
						Pop();
					}

					_InternalCache.Add(t, props);
				}
			}

			return props;
		}

		private static KeyValuePair<Type, PropertyInfo[]> Pop()
		{
			return _InternalCache.Pop();
		}

		/// <summary>
		///     Gets a collection of types that have at least one CachedPropertyAttribute
		/// </summary>
		public static Type[] GetTypes()
		{
			return GetTypes(TypeOfCachedPropertyAttribute.Assembly);
		}

		/// <summary>
		///     Gets a collection of types in the specified assembly that have at least one CachedPropertyAttribute
		/// </summary>
		public static Type[] GetTypes(Assembly a)
		{
			return new List<Type>(FindTypes(a)).ToArray();
		}

		private static IEnumerable<Type> FindTypes()
		{
			return FindTypes(TypeOfCachedPropertyAttribute.Assembly);
		}

		private static IEnumerable<Type> FindTypes(Assembly a)
		{
			return a.GetTypes().Where(type => type != null && !type.IsAbstract);
		}

		private static PropertyInfo[] GetCachedProperties(Type t)
		{
			return _InternalCache.ContainsKey(t) ? _InternalCache[t] : new PropertyInfo[0];
		}

		private static IEnumerable<PropertyInfo> FindProperties(
			Type t, BindingFlags flags = BindingFlags.Default, bool ignoreCA = false)
		{
			if ((flags & BindingFlags.Default) == BindingFlags.Default)
			{
				flags = BindingFlags.Public | BindingFlags.Instance;
			}

			object[] attrs;
			foreach (PropertyInfo prop in t.GetProperties(flags))
			{
				if (ignoreCA)
				{
					yield return prop;
				}

				attrs = prop.GetCustomAttributes(TypeOfCachedPropertyAttribute, true);

				if (attrs.Length > 0)
				{
					yield return prop;
				}
			}
		}
	}

	/// <summary>
	///     Represents a collection of object property names and values
	/// </summary>
	/// <typeparam name="TObj">The type instance associated with the property names and values</typeparam>
	public class PropertyList<TObj> : Dictionary<string, object>, IDisposable
	{
		public PropertyList(TObj instance, bool ignoreCA = false)
		{
			IgnoreCA = ignoreCA;
			Instance = instance;
			Deserialize(Instance);
		}

		public bool IgnoreCA { get; private set; }
		public TObj Instance { get; private set; }

		/// <summary>
		///     Gets or sets the value associated with the specified property name
		/// </summary>
		public new object this[string name]
		{
			get { return ContainsKey(name) ? base[name] : null; }
			set
			{
				if (ContainsKey(name))
				{
					base[name] = value;
				}
				else
				{
					Add(name, value);
				}
			}
		}

		public void Dispose()
		{
			Instance = default(TObj);
			Clear();
		}

		/// <summary>
		///     Writes the current property values to the specified object
		/// </summary>
		public void Serialize(TObj o)
		{
			foreach (PropertyInfo prop in PropertyCache.GetProperties(typeof(TObj)).Where(prop => prop.CanWrite))
			{
				prop.SetValue(o, this[prop.Name], null);
			}
		}

		/// <summary>
		///     Reads the current property values from the specified object
		/// </summary>
		public void Deserialize(TObj o)
		{
			foreach (PropertyInfo prop in PropertyCache.GetProperties(typeof(TObj)).Where(prop => prop.CanRead))
			{
				this[prop.Name] = prop.GetValue(o, null);
			}
		}

		/// <summary>
		///     Selects all properties of which value types match the specified generic type
		/// </summary>
		public Dictionary<string, TProp> Select<TProp>()
		{
			return this.Where(kvp => kvp.Value is TProp).ToDictionary(kvp => kvp.Key, kvp => (TProp)kvp.Value);
		}

		/// <summary>
		///     Gets the value for the specified property name and converts it to the specified generic type
		/// </summary>
		public TProp Get<TProp>(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				return default(TProp);
			}

			return this[name] is TProp ? (TProp)this[name] : default(TProp);
		}

		/// <summary>
		///     Sets the cached value for the specified property name
		/// </summary>
		public void Set(string name, object value)
		{
			this[name] = value;
		}
	}
}