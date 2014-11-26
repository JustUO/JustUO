#region Header
//   Vorspire    _,-'/-'/  DataTypes.cs
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

using Server;
#endregion

namespace System
{
	public enum DataType
	{
		Null = 0,
		Bool,
		Char,
		Byte,
		SByte,
		Short,
		UShort,
		Int,
		UInt,
		Long,
		ULong,
		Float,
		Decimal,
		Double,
		String,
		DateTime,
		TimeSpan
	}

	public struct SimpleType
	{
		public DataType Flag { get; private set; }
		public Type Type { get; private set; }
		public object Value { get; private set; }

		public bool HasValue { get { return Value != null; } }

		public SimpleType(object obj)
			: this()
		{
			Value = obj;
			Type = obj != null ? obj.GetType() : null;
			Flag = DataTypes.Lookup(Type);

			//Console.WriteLine("SimpleType: {0} ({1}) [{2}]", Value, Type, Flag);

			if (Flag == DataType.Null)
			{
				Value = null;
			}
		}

		public SimpleType(GenericReader reader)
			: this()
		{
			Deserialize(reader);
		}

		public bool TryCast<T>(out T value)
		{
			if (Value is T)
			{
				value = (T)Value;
				return true;
			}

			value = default(T);
			return false;
		}

		public T Cast<T>() where T : struct
		{
			return Value is T ? (T)Value : default(T);
		}

		public override string ToString()
		{
			return String.Format("{0} ({1})", Value != null ? Value.ToString() : "null", Flag);
		}

		public void Serialize(GenericWriter writer)
		{
			writer.WriteFlag(Flag);

			switch (Flag)
			{
				case DataType.Null:
					break;
				case DataType.Bool:
					writer.Write(Convert.ToBoolean(Value));
					break;
				case DataType.Char:
					writer.Write(Convert.ToChar(Value));
					break;
				case DataType.Byte:
					writer.Write(Convert.ToByte(Value));
					break;
				case DataType.SByte:
					writer.Write(Convert.ToSByte(Value));
					break;
				case DataType.Short:
					writer.Write(Convert.ToInt16(Value));
					break;
				case DataType.UShort:
					writer.Write(Convert.ToUInt16(Value));
					break;
				case DataType.Int:
					writer.Write(Convert.ToInt32(Value));
					break;
				case DataType.UInt:
					writer.Write(Convert.ToUInt32(Value));
					break;
				case DataType.Long:
					writer.Write(Convert.ToInt64(Value));
					break;
				case DataType.ULong:
					writer.Write(Convert.ToUInt64(Value));
					break;
				case DataType.Float:
					writer.Write(Convert.ToSingle(Value));
					break;
				case DataType.Decimal:
					writer.Write(Convert.ToDecimal(Value));
					break;
				case DataType.Double:
					writer.Write(Convert.ToDouble(Value));
					break;
				case DataType.String:
					writer.Write(Convert.ToString(Value));
					break;
				case DataType.DateTime:
					writer.Write(Convert.ToDateTime(Value));
					break;
				case DataType.TimeSpan:
					writer.Write((TimeSpan)Value);
					break;
			}
		}

		public void Deserialize(GenericReader reader)
		{
			Flag = reader.ReadFlag<DataType>();
			Type = Flag.ToType();

			switch (Flag)
			{
				case DataType.Null:
					Value = null;
					break;
				case DataType.Bool:
					Value = reader.ReadBool();
					break;
				case DataType.Char:
					Value = reader.ReadChar();
					break;
				case DataType.Byte:
					Value = reader.ReadByte();
					break;
				case DataType.SByte:
					Value = reader.ReadSByte();
					break;
				case DataType.Short:
					Value = reader.ReadShort();
					break;
				case DataType.UShort:
					Value = reader.ReadUShort();
					break;
				case DataType.Int:
					Value = reader.ReadInt();
					break;
				case DataType.UInt:
					Value = reader.ReadUInt();
					break;
				case DataType.Long:
					Value = reader.ReadLong();
					break;
				case DataType.ULong:
					Value = reader.ReadULong();
					break;
				case DataType.Float:
					Value = reader.ReadFloat();
					break;
				case DataType.Decimal:
					Value = reader.ReadDecimal();
					break;
				case DataType.Double:
					Value = reader.ReadDouble();
					break;
				case DataType.String:
					Value = reader.ReadString() ?? String.Empty;
					break;
				case DataType.DateTime:
					Value = reader.ReadDateTime();
					break;
				case DataType.TimeSpan:
					Value = reader.ReadTimeSpan();
					break;
			}
		}

		public static bool IsSimpleType(object value)
		{
			return value != null && FromObject(value).Value == value;
		}

		public static object ToObject(SimpleType value)
		{
			return value.Value;
		}

		public static SimpleType FromObject(object value)
		{
			return new SimpleType(value);
		}

		public static bool TryParse(string data, DataType flag, out SimpleType value)
		{
			try
			{
				switch (flag)
				{
					case DataType.Null:
						value = new SimpleType(null);
						break;
					case DataType.Bool:
						value = new SimpleType(Boolean.Parse(data));
						break;
					case DataType.Char:
						value = new SimpleType(Char.Parse(data));
						break;
					case DataType.Byte:
						value = new SimpleType(Byte.Parse(data));
						break;
					case DataType.SByte:
						value = new SimpleType(SByte.Parse(data));
						break;
					case DataType.Short:
						value = new SimpleType(Int16.Parse(data));
						break;
					case DataType.UShort:
						value = new SimpleType(UInt16.Parse(data));
						break;
					case DataType.Int:
						value = new SimpleType(Int32.Parse(data));
						break;
					case DataType.UInt:
						value = new SimpleType(UInt32.Parse(data));
						break;
					case DataType.Long:
						value = new SimpleType(Int64.Parse(data));
						break;
					case DataType.ULong:
						value = new SimpleType(UInt64.Parse(data));
						break;
					case DataType.Float:
						value = new SimpleType(Single.Parse(data));
						break;
					case DataType.Decimal:
						value = new SimpleType(Decimal.Parse(data));
						break;
					case DataType.Double:
						value = new SimpleType(Double.Parse(data));
						break;
					case DataType.String:
						value = new SimpleType(data);
						break;
					case DataType.DateTime:
						value = new SimpleType(DateTime.Parse(data));
						break;
					case DataType.TimeSpan:
						value = new SimpleType(TimeSpan.Parse(data));
						break;
					default:
						value = new SimpleType(null);
						break;
				}

				return true;
			}
			catch
			{
				value = new SimpleType(null);
				return false;
			}
		}

		public static implicit operator SimpleType(bool value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(char value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(byte value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(sbyte value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(short value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(ushort value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(int value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(uint value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(long value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(ulong value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(float value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(decimal value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(double value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(string value)
		{
			return new SimpleType(value ?? String.Empty);
		}

		public static implicit operator SimpleType(DateTime value)
		{
			return new SimpleType(value);
		}

		public static implicit operator SimpleType(TimeSpan value)
		{
			return new SimpleType(value);
		}
	}

	public static class DataTypes
	{
		private static readonly Dictionary<DataType, Type> _DataTypeTable = new Dictionary<DataType, Type> {
			{DataType.Null, null},
			{DataType.Bool, typeof(bool)},
			{DataType.Char, typeof(char)},
			{DataType.Byte, typeof(byte)},
			{DataType.SByte, typeof(sbyte)},
			{DataType.Short, typeof(short)},
			{DataType.UShort, typeof(ushort)},
			{DataType.Int, typeof(int)},
			{DataType.UInt, typeof(uint)},
			{DataType.Long, typeof(long)},
			{DataType.ULong, typeof(ulong)},
			{DataType.Float, typeof(float)},
			{DataType.Decimal, typeof(decimal)},
			{DataType.Double, typeof(double)},
			{DataType.String, typeof(string)},
			{DataType.DateTime, typeof(DateTime)},
			{DataType.TimeSpan, typeof(TimeSpan)},
		};

		public static Type ToType(this DataType f)
		{
			return Lookup(f);
		}

		public static DataType FromType(Type t)
		{
			return Lookup(t);
		}

		public static DataType Lookup(Type t)
		{
			return _DataTypeTable.GetKey(t);
		}

		public static Type Lookup(DataType f)
		{
			return _DataTypeTable.GetValue(f);
		}
	}
}