#region Header
//   Vorspire    _,-'/-'/  Condition.cs
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
	public struct MySQLCondition : IEquatable<MySQLCondition>
	{
		public static readonly MySQLCondition Empty = new MySQLCondition();
		public static MySQLCondition[] EmptyBuffer = new MySQLCondition[0];

		public static string Sanitize(object value)
		{
			var s = SimpleType.FromObject(value);

			return s.ToString();
		}

		public string Key { get; private set; }
		public MySQLOperator Operator { get; private set; }
		public object Value { get; private set; }
		public MySQLQueryJoin QueryJoin { get; private set; }

		public string ValueString
		{
			get
			{
				return Value != null
						   ? (Operator == MySQLOperator.Like ? String.Format("%{0}%", Value) : Value.ToString())
						   : String.Empty;
			}
		}

		public MySQLCondition(string key, object value)
			: this(key, MySQLOperator.Equal, value, MySQLQueryJoin.AND)
		{ }

		public MySQLCondition(string key, object value, MySQLQueryJoin join)
			: this(key, MySQLOperator.Equal, value, join)
		{ }

		public MySQLCondition(string key, MySQLOperator op, object value)
			: this(key, op, value, MySQLQueryJoin.AND)
		{ }

		public MySQLCondition(string key, MySQLOperator op, object value, MySQLQueryJoin join)
			: this()
		{
			Key = key ?? String.Empty;
			Operator = op;
			Value = value;
			QueryJoin = join;
		}

		private void InvalidateOperator()
		{
			switch (Operator)
			{
				case MySQLOperator.None:
					Operator = MySQLOperator.Equal;
					break;
				case MySQLOperator.Not:
					Operator = MySQLOperator.NotEqual;
					break;
				default:
					{
						if (Operator.HasFlag(MySQLOperator.Like))
						{
							Operator = MySQLOperator.Like;
							break;
						}

						if (Operator.HasFlag(MySQLOperator.Not))
						{
							Operator &= ~MySQLOperator.Not;
						}

						if (Operator.HasFlag(MySQLOperator.Lower))
						{
							if (Operator.HasFlag(MySQLOperator.Equal) && Operator != MySQLOperator.LowerOrEqual)
							{
								Operator = MySQLOperator.LowerOrEqual;
							}
							else
							{
								Operator = MySQLOperator.Lower;
							}

							break;
						}

						if (Operator.HasFlag(MySQLOperator.Greater))
						{
							if (Operator.HasFlag(MySQLOperator.Equal) && Operator != MySQLOperator.GreaterOrEqual)
							{
								Operator = MySQLOperator.GreaterOrEqual;
							}
							else
							{
								Operator = MySQLOperator.Greater;
							}

							break;
						}

						if (Operator == MySQLOperator.None)
						{
							Operator = MySQLOperator.Equal;
						}
					}
					break;
			}
		}

		public string GetOperation()
		{
			InvalidateOperator();

			switch (Operator)
			{
				case MySQLOperator.Not:
				case MySQLOperator.NotEqual:
					return "!=";
				case MySQLOperator.Lower:
					return "<";
				case MySQLOperator.LowerOrEqual:
					return "<=";
				case MySQLOperator.Greater:
					return ">";
				case MySQLOperator.GreaterOrEqual:
					return ">=";
				case MySQLOperator.Like:
					return "LIKE";
				default:
					return "=";
			}
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
			return String.Format("`{0}` {1} '{2}'", Key, GetOperation(), ValueString);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = Key.GetHashCode();

				hash = (hash * 397) ^ Operator.GetHashCode();
				hash = (hash * 397) ^ (Value != null ? Value.GetHashCode() : 0);
				hash = (hash * 397) ^ QueryJoin.GetHashCode();

				return hash;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is MySQLCondition && Equals((MySQLCondition)obj);
		}

		public bool Equals(MySQLCondition other)
		{
			return Key == other.Key && Operator == other.Operator && Value == other.Value && QueryJoin == other.QueryJoin;
		}

		public static bool operator ==(MySQLCondition l, MySQLCondition r)
		{
			return l.Equals(r);
		}

		public static bool operator !=(MySQLCondition l, MySQLCondition r)
		{
			return !l.Equals(r);
		}

		// Legacy support
		public static implicit operator MySQLCondition(MySQLData data)
		{
			return new MySQLCondition(data.Key, MySQLOperator.Equal, data.Value, MySQLQueryJoin.AND);
		}
	}
}