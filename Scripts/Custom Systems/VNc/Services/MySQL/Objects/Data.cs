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
using System.Collections;
using System.Collections.Generic;
#endregion

namespace VitaNex.MySQL
{
	public class MySQLRow : IEnumerable<MySQLData>
	{
		public MySQLRow(int id, List<MySQLData> results)
			: this(id, results.ToArray())
		{ }

		public MySQLRow(int id, MySQLData[] results)
		{
			ID = id;
			Results = new Dictionary<string, MySQLData>(results.Length);

			foreach (MySQLData result in results)
			{
				Results.Add(result.Key, result);
			}
		}

		public MySQLRow(MySQLRow row)
		{
			ID = row.ID;
			Results = row.Results;
		}

		public int ID { get; private set; }
		public Dictionary<string, MySQLData> Results { get; private set; }

		public MySQLData this[string key]
		{
			get
			{
				if (Results.ContainsKey(key))
				{
					return Results[key];
				}

				return null;
			}
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return Results.Values.GetEnumerator();
		}

		public IEnumerator<MySQLData> GetEnumerator()
		{
			return Results.Values.GetEnumerator();
		}
	}

	public class MySQLData
	{
		public MySQLData(string key, object value)
		{
			Key = key;
			Value = value;
		}

		public string Key { get; private set; }
		public object Value { get; private set; }

		public override string ToString()
		{
			return String.Format("[{0}] => '{1}'", Key, Value);
		}

		public T GetValue<T>()
		{
			if (Value == null || Value is DBNull)
			{
				return default(T);
			}

			/*	if (typeof(T) == typeof(int))
				{
					return Int32.Parse(Value.ToString());
				}*/

			try
			{
				return (T)Value;
			}
			catch
			{
				return default(T);
			}
		}
	}
}