#region Header
//   Vorspire    _,-'/-'/  FieldCache.cs
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
	///     Used to mark object fields that should be included when accessing the FieldCache service
	/// </summary>
	[AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
	public class CachedFieldAttribute : Attribute
	{
		public CachedFieldAttribute(bool persist = true)
		{
			Persist = persist;
		}

		/// <summary>
		///     Gets or sets a value indecating whether the marked field should be included when deserialized
		/// </summary>
		public bool Persist { get; set; }
	}

	/// <summary>
	///     Provides a caching service for object fields which have the CachedFieldAttribute
	/// </summary>
	public static class FieldCache
	{
		public static readonly Type TypeOfCachedFieldAttribute = typeof(CachedFieldAttribute);

		public static int PopCap = 0x100;

		private static readonly Dictionary<Type, FieldInfo[]> _InternalCache = new Dictionary<Type, FieldInfo[]>(PopCap);

		/// <summary>
		///     Gets a collection representing the cached fields for the specified type
		/// </summary>
		public static FieldInfo[] GetFields<TObj>(bool ignoreCA = false)
		{
			return GetFields<TObj>(BindingFlags.Default, ignoreCA);
		}

		/// <summary>
		///     Gets a collection representing the cached fields for the specified type
		/// </summary>
		public static FieldInfo[] GetFields(Type t, bool ignoreCA = false)
		{
			return GetFields(t, BindingFlags.Default, ignoreCA);
		}

		/// <summary>
		///     Gets a collection representing the cached fields for the specified type using the specified BindingFlags
		/// </summary>
		public static FieldInfo[] GetFields<TObj>(BindingFlags flags, bool ignoreCA = false)
		{
			return GetFields(typeof(TObj), flags, ignoreCA);
		}

		/// <summary>
		///     Gets a collection representing the cached fields for the specified type using the specified BindingFlags
		/// </summary>
		public static FieldInfo[] GetFields(Type t, BindingFlags flags, bool ignoreCA = false)
		{
			if ((flags & BindingFlags.Default) == BindingFlags.Default)
			{
				flags = BindingFlags.NonPublic | BindingFlags.Instance;
			}

			var fields = ignoreCA ? new FieldInfo[0] : GetCachedFields(t);

			if (fields.Length == 0)
			{
				fields = new List<FieldInfo>(FindFields(t, flags, ignoreCA)).ToArray();

				if (fields.Length > 0)
				{
					if (_InternalCache.Count + 1 > PopCap)
					{
						Pop();
					}

					_InternalCache.Add(t, fields);
				}
			}

			return fields;
		}

		private static KeyValuePair<Type, FieldInfo[]> Pop()
		{
			return _InternalCache.Pop();
		}

		/// <summary>
		///     Gets a collection of types that have at least one CachedFieldAttribute
		/// </summary>
		public static Type[] GetTypes()
		{
			return GetTypes(TypeOfCachedFieldAttribute.Assembly);
		}

		/// <summary>
		///     Gets a collection of types in the specified assembly that have at least one CachedFieldAttribute
		/// </summary>
		public static Type[] GetTypes(Assembly a)
		{
			return new List<Type>(FindTypes(a)).ToArray();
		}

		private static IEnumerable<Type> FindTypes()
		{
			return FindTypes(TypeOfCachedFieldAttribute.Assembly);
		}

		private static IEnumerable<Type> FindTypes(Assembly a)
		{
			return a.GetTypes().Where(type => type != null && !type.IsAbstract);
		}

		private static FieldInfo[] GetCachedFields(Type t)
		{
			return _InternalCache.ContainsKey(t) ? _InternalCache[t] : new FieldInfo[0];
		}

		private static IEnumerable<FieldInfo> FindFields(
			Type t, BindingFlags flags = BindingFlags.Default, bool ignoreCA = false)
		{
			if ((flags & BindingFlags.Default) == BindingFlags.Default)
			{
				flags = BindingFlags.NonPublic | BindingFlags.Instance;
			}

			object[] attrs;
			foreach (FieldInfo field in t.GetFields(flags))
			{
				if (ignoreCA)
				{
					yield return field;
				}

				attrs = field.GetCustomAttributes(TypeOfCachedFieldAttribute, true);

				if (attrs.Length > 0)
				{
					yield return field;
				}
			}
		}
	}

	/// <summary>
	///     Represents a collection of object field names and values
	/// </summary>
	/// <typeparam name="TObj">The type instance associated with the field names and values</typeparam>
	public class FieldList<TObj> : Dictionary<string, object>, IDisposable
	{
		public FieldList(TObj instance, bool ignoreCA = false)
		{
			IgnoreCA = ignoreCA;
			Instance = instance;
			Deserialize(Instance);
		}

		public bool IgnoreCA { get; private set; }
		public TObj Instance { get; private set; }

		/// <summary>
		///     Gets or sets the value associated with the specified field name
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
		///     Writes the current field values to the specified object
		/// </summary>
		public void Serialize(TObj o)
		{
			foreach (FieldInfo field in FieldCache.GetFields(typeof(TObj)))
			{
				field.SetValue(o, this[field.Name]);
			}
		}

		/// <summary>
		///     Reads the current field values from the specified object
		/// </summary>
		public void Deserialize(TObj o)
		{
			foreach (FieldInfo field in FieldCache.GetFields(typeof(TObj)))
			{
				this[field.Name] = field.GetValue(o);
			}
		}

		/// <summary>
		///     Selects all fields of which value types match the specified generic type
		/// </summary>
		public Dictionary<string, TField> Select<TField>()
		{
			return this.Where(kvp => kvp.Value is TField).ToDictionary(kvp => kvp.Key, kvp => (TField)kvp.Value);
		}

		/// <summary>
		///     Gets the value for the specified field name and converts it to the specified generic type
		/// </summary>
		public TField Get<TField>(string name)
		{
			if (String.IsNullOrEmpty(name))
			{
				return default(TField);
			}

			return this[name] is TField ? (TField)this[name] : default(TField);
		}

		/// <summary>
		///     Sets the cached value for the specified field name
		/// </summary>
		public void Set(string name, object value)
		{
			this[name] = value;
		}
	}
}