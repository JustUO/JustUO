#region Header
//   Vorspire    _,-'/-'/  JSON.cs
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
#endregion

namespace VitaNex.Text
{

	#region BETA Serialization
	/*
	[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = false, Inherited = true)]
	public sealed class JSONEntryAttribute : Attribute
	{
		public string Name { get; private set; }

		public JSONEntryAttribute(string name)
		{
			Name = name;
		}

		public static JSONEntryAttribute Find(FieldInfo f)
		{
			return f.GetCustomAttributes(typeof(JSONEntryAttribute), true).OfType<JSONEntryAttribute>().LastOrDefault();
		}

		public static JSONEntryAttribute Find(PropertyInfo p)
		{
			return p.GetCustomAttributes(typeof(JSONEntryAttribute), true).OfType<JSONEntryAttribute>().LastOrDefault();
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public sealed class JSONArrayAttribute : Attribute
	{
		public string Name { get; private set; }

		public JSONArrayAttribute(string name)
		{
			Name = name;
		}

		public static JSONArrayAttribute Find(Type t)
		{
			return t.GetCustomAttributes(typeof(JSONArrayAttribute), true).OfType<JSONArrayAttribute>().LastOrDefault();
		}
	}

	[AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct, AllowMultiple = false, Inherited = true)]
	public sealed class JSONObjectAttribute : Attribute
	{
		public string Name { get; private set; }

		public JSONObjectAttribute(string name)
		{
			Name = name;
		}

		public static JSONObjectAttribute Find(Type t)
		{
			return t.GetCustomAttributes(typeof(JSONObjectAttribute), true).OfType<JSONObjectAttribute>().LastOrDefault();
		}
	}

	[JSONObject("example")]
	public sealed class JSONExample
	{
		[JSONEntry("foo")]
		public int Foo { get; set; }

		[JSONEntry("bar")]
		public int Bar { get; set; }
	}

	public sealed class JSONStructure
	{
		public Type Type { get; private set; }

		public JSONObjectAttribute Object { get; private set; }
		public JSONArrayAttribute Array { get; private set; }

		public Dictionary<JSONEntryAttribute, FieldInfo> Fields { get; private set; }
		public Dictionary<JSONEntryAttribute, PropertyInfo> Properties { get; private set; }
		
		public bool IsObject { get { return Object != null; } }
		public bool IsArray { get { return Array != null; } }

		public JSONStructure(Type t)
		{
			Type = t;

			Object = JSONObjectAttribute.Find(Type);
			Array = JSONArrayAttribute.Find(Type);

			Fields = new Dictionary<JSONEntryAttribute, FieldInfo>();
			Properties = new Dictionary<JSONEntryAttribute, PropertyInfo>();

			foreach (var field in Type.GetFields(BindingFlags.Instance))
			{
				var eAttr = JSONEntryAttribute.Find(field);

				if (eAttr != null)
				{
					Fields.Add(eAttr, field);
				}
			}

			foreach (var prop in Type.GetProperties(BindingFlags.Instance))
			{
				var eAttr = JSONEntryAttribute.Find(prop);

				if (eAttr != null)
				{
					Properties.Add(eAttr, prop);
				}
			}
		}
	}

	public sealed class JSONSerializer
	{
		public bool Serialize(object instance, out string json)
		{
			var type = instance.GetType();
			var structure = new JSONStructure(type);

			if (structure.IsObject)
			{
				return SerializeObject(structure, instance, out json);
			}

			if (structure.IsArray)
			{
				return SerializeArray(structure, instance, out json);
			}

			json = String.Empty;
			return false;
		}

		private bool SerializeObject(JSONStructure structure, object instance, out string json)
		{
			Dictionary<string, SimpleType> values = new Dictionary<string, SimpleType>();

			structure.Fields.ForEach((k, v) =>
			{
				string key = k.Name;
				object val = v.GetValue(instance);

				if (SimpleType.IsSimpleType(val))
				{
					values.Add(key, SimpleType.FromObject(val));
				}
				else
				{
					Type type = val.GetType();

					if (JSONObjectAttribute.Find(type) != null)
					{
						
					}
				}
			});

			json = JSON.EncodeObject(structure.Object.Name, values);
			return true;
		}

		private bool SerializeArray(JSONStructure structure, object instance, out string json)
		{
			Dictionary<string, SimpleType> values = new Dictionary<string, SimpleType>();

			json = JSON.EncodeArray(structure.Array.Name, values);
			return true;
		}
	}
	*/
	#endregion

	public static class JSON
	{
		public static string Encode(SimpleType value)
		{
			//Console.WriteLine("{0}: {1}", value, value.Flag);

			switch (value.Flag)
			{
				case DataType.Null:
					return "null";
				case DataType.Char:
				case DataType.String:
					return String.Format("\"{0}\"", Escape((string)value.Value));
				case DataType.Bool:
					return (bool)value.Value ? "true" : "false";
				case DataType.Byte:
				case DataType.UShort:
				case DataType.UInt:
				case DataType.ULong:
				case DataType.SByte:
				case DataType.Short:
				case DataType.Int:
				case DataType.Long:
					return String.Format("{0:0}", value.Value);
				case DataType.Decimal:
				case DataType.Double:
				case DataType.Float:
					return String.Format("{0:0.0#}", value.Value);
				case DataType.DateTime:
					{
						TimeStamp stamp = (DateTime)value.Value;

						return String.Format("{0:0.0}", stamp.Stamp);
					}
				case DataType.TimeSpan:
					{
						TimeSpan span = (TimeSpan)value.Value;

						return String.Format("{0:0.0}", span.TotalSeconds);
					}
				default:
					return String.Format("\"{0}\"", Escape(value.ToString()));
			}
		}

		public static string Encode(string key, SimpleType value)
		{
			return Format(Encode(key), Encode(value));
		}

		public static string EncodeArray(IEnumerable<SimpleType> values)
		{
			return FormatArray(values.Select(Encode));
		}

		public static string EncodeArray(IDictionary<string, SimpleType> values)
		{
			var list = new Dictionary<string, string>(values.Count);

			values.ForEach(kv => list.Add(Encode(kv.Key), Encode(kv.Value)));

			return FormatArray(list);
		}

		public static string EncodeArray(string key, IEnumerable<SimpleType> values)
		{
			return FormatArray(Encode(key), values.Select(Encode));
		}

		public static string EncodeArray(string key, IDictionary<string, SimpleType> values)
		{
			var list = new Dictionary<string, string>(values.Count);

			values.ForEach(kv => list.Add(Encode(kv.Key), Encode(kv.Value)));

			return FormatArray(Encode(key), list);
		}

		public static string EncodeObject(IDictionary<string, SimpleType> values)
		{
			var list = new Dictionary<string, string>(values.Count);

			values.ForEach(kv => list.Add(Encode(kv.Key), Encode(kv.Value)));

			return FormatObject(list);
		}

		public static string EncodeObject(string key, IDictionary<string, SimpleType> values)
		{
			var list = new Dictionary<string, string>(values.Count);

			values.ForEach(kv => list.Add(Encode(kv.Key), Encode(kv.Value)));

			return FormatObject(Encode(key), list);
		}

		public static string Format(string key, string value)
		{
			return String.Format("{0}: {1}", key, value);
		}

		public static string FormatArray(IEnumerable<string> values)
		{
			return String.Format("[ {0} ]", String.Join(", ", values));
		}

		public static string FormatArray(IDictionary<string, string> values)
		{
			return String.Format("[ {0} ]", String.Join(", ", FormatObject(values)));
		}

		public static string FormatArray(string key, IEnumerable<string> values)
		{
			key = Encode(key.Trim('"'));

			return Format(key, FormatArray(values));
		}

		public static string FormatArray(string key, IDictionary<string, string> values)
		{
			key = Encode(key.Trim('"'));
			return Format(key, FormatArray(values));
		}

		public static string FormatObject(IDictionary<string, string> values)
		{
			return String.Format("{{ {0} }}", String.Join(", ", values.Select(kv => Format(kv.Key, kv.Value))));
		}

		public static string FormatObject(string key, IDictionary<string, string> values)
		{
			key = Encode(key.Trim('"'));

			return Format(key, FormatObject(values));
		}

		public static string Escape(string value)
		{
			if (String.IsNullOrEmpty(value))
			{
				return String.Empty;
			}

			var chars = new List<char>(value.Length);

			var search = new[] {'"', '\'', '/', '\\'};
			var ignored = new[] {'b', 'f', 'n', 'r', 't', 'u', 'v'};

			ignored = search.Merge(ignored);

			char prev, next;

			value.For(
				(i, c) =>
				{
					if (search.Any(sc => sc == c))
					{
						if (c == '\\')
						{
							if (i == 0)
							{
								if (i + 1 < value.Length)
								{
									next = value[i + 1];

									if (!ignored.Any(ic => ic == next))
									{
										chars.Add('\\');
									}
								}
								else
								{
									chars.Add('\\');
								}
							}
							else
							{
								prev = value[i - 1];

								if (i + 1 < value.Length)
								{
									next = value[i + 1];

									if (prev != '\\' && !ignored.Any(ic => ic == next))
									{
										chars.Add('\\');
									}
								}
								else if (prev != '\\')
								{
									chars.Add('\\');
								}
							}
						}
						else
						{
							if (i == 0)
							{
								chars.Add('\\');
							}
							else
							{
								prev = value[i - 1];

								if (prev != '\\')
								{
									chars.Add('\\');
								}
							}
						}
					}

					chars.Add(c);
				});

			return new string(chars.ToArray());
		}
	}
}