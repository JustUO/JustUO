#region Header
//   Vorspire    _,-'/-'/  Data.cs
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
#endregion

namespace VitaNex.MySQL
{
	public struct MySQLData : IEquatable<MySQLData>
	{
		public static readonly MySQLData Empty = new MySQLData();
		public static MySQLData[] EmptyBuffer = new MySQLData[0];

		public static string Sanitize(object value)
		{
			var s = SimpleType.FromObject(value);

			return s.ToString();
		}

		public string Key { get; private set; }
		public object Value { get; private set; }

		public string ValueString { get { return Value != null ? Value.ToString() : String.Empty; } }

		public MySQLData(string key, object value)
			: this()
		{
			Key = key ?? String.Empty;
			Value = value;
		}

		public T GetValue<T>()
		{
			if (Value == null || Value is DBNull)
			{
				return default(T);
			}

			try
			{
				return (T)Value;
			}
			catch
			{
				return default(T);
			}
		}

		public override string ToString()
		{
			return String.Format("`{0}` = '{1}'", Key, ValueString);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Key.GetHashCode();

				hash = (hash * 397) ^ (Value != null ? Value.GetHashCode() : 0);

				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is MySQLData && Equals((MySQLData)obj);
		}

		public bool Equals(MySQLData other)
		{
			return Key == other.Key && Value == other.Value;
		}

		public static bool operator ==(MySQLData l, MySQLData r)
		{
			return l.Equals(r);
		}

		public static bool operator !=(MySQLData l, MySQLData r)
		{
			return !l.Equals(r);
		}

		// Legacy support
		public static implicit operator MySQLData(MySQLCondition data)
		{
			return new MySQLData(data.Key, data.Value);
		}
	}
}