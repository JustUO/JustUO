#region Header
//   Vorspire    _,-'/-'/  SerializeExt.cs
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
using System.IO;
using System.Linq;

using Server.Accounting;

using VitaNex;
using VitaNex.Crypto;
using VitaNex.Reflection;
#endregion

namespace Server
{
	public static class SerializeExtUtility
	{
		#region Initializers
		public static GenericWriter GetBinaryWriter(this FileStream stream)
		{
			return new BinaryFileWriter(stream, true);
		}

		public static GenericReader GetBinaryReader(this FileStream stream)
		{
			return new BinaryFileReader(new BinaryReader(stream));
		}

		public static FileStream GetStream(
			this FileInfo file, FileAccess access = FileAccess.ReadWrite, FileShare share = FileShare.ReadWrite)
		{
			return file.Open(FileMode.OpenOrCreate, access, share);
		}

		public static void Serialize(this FileInfo file, Action<GenericWriter> handler)
		{
			if (handler == null)
			{
				return;
			}

			using (FileStream stream = file.GetStream())
			{
				GenericWriter writer = stream.GetBinaryWriter();
				handler(writer);
				writer.Close();
			}
		}

		public static void Deserialize(this FileInfo file, Action<GenericReader> handler)
		{
			if (!file.Exists || file.Length == 0 || handler == null)
			{
				return;
			}

			using (FileStream stream = file.GetStream(FileAccess.Read, FileShare.Read))
			{
				GenericReader reader = stream.GetBinaryReader();
				handler(reader);
			}
		}
		#endregion Initializers

		#region Operations
		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static long Seek(this GenericWriter writer, long offset, SeekOrigin origin)
		{
			if (writer != null)
			{
				if (writer is BinaryFileWriter)
				{
					var bin = (BinaryFileWriter)writer;

					if (bin.UnderlyingStream != null)
					{
						return bin.UnderlyingStream.Seek(offset, origin);
					}
				}
				else if (writer is AsyncWriter)
				{
					var bin = (AsyncWriter)writer;

					if (bin.MemStream != null)
					{
						return bin.MemStream.Seek(offset, origin);
					}
				}
				else
				{
					var pl = new PropertyList<GenericWriter>(writer);
					var st = pl.Select<Stream>();

					if (st.Count > 0)
					{
						return st.GetValue(0).Seek(offset, origin);
					}
				}
			}

			throw new InvalidOperationException("Can't perform seek operation");
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static long Seek(this GenericReader reader, long offset, SeekOrigin origin)
		{
			if (reader != null)
			{
				if (reader is BinaryFileReader)
				{
					var bin = (BinaryFileReader)reader;
					return bin.Seek(offset, origin);
				}

				long ret = -1;
				var pl = new PropertyList<GenericReader>(reader);
				var st = pl.Select<Stream>();

				if (st.Count > 0)
				{
					var v = st.Values.FirstOrDefault(s => (s != null && s.CanRead && s.CanSeek));

					if (v != null)
					{
						ret = v.Seek(offset, origin);
					}
				}

				if (ret >= 0)
				{
					return ret;
				}
			}

			throw new InvalidOperationException("Can't perform seek operation");
		}
		#endregion Operations

		#region Raw Data
		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBytes(this GenericWriter writer, byte[] buffer, int offset, int count, out int length)
		{
			var start = (int)writer.Seek(0, SeekOrigin.Current);
			writer.Write(count);

			for (int i = offset; i >= 0 && i < (offset + count) && i < buffer.Length; i++)
			{
				writer.Write(buffer[i]);
			}

			var end = (int)writer.Seek(0, SeekOrigin.Current);
			length = (end - start);
			writer.Seek(start, SeekOrigin.Begin);
			writer.Write(length);
			writer.Seek(end, SeekOrigin.Begin);
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static byte[] ReadBytes(this GenericReader reader)
		{
			int count = reader.ReadInt();
			var buffer = new byte[count];

			for (int i = 0; i < count; i++)
			{
				buffer[i] = reader.ReadByte();
			}

			return buffer;
		}
		#endregion Raw Data

		#region Block Data
		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlock(this GenericWriter writer, Action onSerialize)
		{
			WriteBlock(writer, w => onSerialize());
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlock(this GenericWriter writer, Action<GenericWriter> onSerialize)
		{
			long start = writer.Seek(0, SeekOrigin.Current);
			writer.Write(start);

			VitaNexCore.TryCatch(() => onSerialize(writer), VitaNexCore.ToConsole);

			long end = writer.Seek(0, SeekOrigin.Current);
			writer.Seek(start, SeekOrigin.Begin);
			writer.Write(end - start);
			writer.Seek(end, SeekOrigin.Begin);
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void ReadBlock(this GenericReader reader, Action onDeserialize)
		{
			ReadBlock(reader, r => onDeserialize());
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void ReadBlock(this GenericReader reader, Action<GenericReader> onDeserialize)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long pos = reader.Seek(0, SeekOrigin.Current);
					long length = reader.ReadLong();

					VitaNexCore.TryCatch(() => onDeserialize(reader), VitaNexCore.ToConsole);

					reader.Seek(pos + length, SeekOrigin.Begin);
				},
				VitaNexCore.ToConsole);
		}
		#endregion Block Data

		#region T[]
		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlockArray<TObj>(this GenericWriter writer, TObj[] list, Action<TObj> onSerialize)
		{
			WriteBlockArray(writer, list, (w, o) => onSerialize(o));
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlockArray<TObj>(
			this GenericWriter writer, TObj[] list, Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new TObj[0];

			writer.Write(list.Length);

			foreach (TObj obj in list)
			{
				WriteBlock(
					writer,
					() =>
					{
						if (obj == null)
						{
							writer.Write(false);
						}
						else
						{
							writer.Write(true);
							onSerialize(writer, obj);
						}
					});
			}
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static TObj[] ReadBlockArray<TObj>(this GenericReader reader, Func<TObj> onDeserialize, TObj[] list = null)
		{
			return ReadBlockArray(reader, r => onDeserialize(), list);
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static TObj[] ReadBlockArray<TObj>(
			this GenericReader reader, Func<GenericReader, TObj> onDeserialize, TObj[] list = null)
		{
			int count = reader.ReadInt();

			list = list ?? new TObj[count];

			for (int index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					() =>
					{
						if (!reader.ReadBool())
						{
							if (index < list.Length)
							{
								list[index] = default(TObj);
							}

							return;
						}

						if (index < list.Length)
						{
							list[index] = onDeserialize(reader);
						}
						else
						{
							onDeserialize(reader);
						}
					});
			}

			return list;
		}

		public static void WriteArray<TObj>(this GenericWriter writer, TObj[] list, Action<TObj> onSerialize)
		{
			WriteArray(writer, list, (w, o) => onSerialize(o));
		}

		public static void WriteArray<TObj>(this GenericWriter writer, TObj[] list, Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new TObj[0];

			writer.Write(list.Length);

			foreach (TObj obj in list)
			{
				if (obj == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, obj);
				}
			}
		}

		public static TObj[] ReadArray<TObj>(this GenericReader reader, Func<TObj> onDeserialize, TObj[] list = null)
		{
			return ReadArray(reader, r => onDeserialize(), list);
		}

		public static TObj[] ReadArray<TObj>(
			this GenericReader reader, Func<GenericReader, TObj> onDeserialize, TObj[] list = null)
		{
			int count = reader.ReadInt();

			list = list ?? new TObj[count];

			for (int index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					if (index < list.Length)
					{
						list[index] = default(TObj);
					}

					continue;
				}

				if (index < list.Length)
				{
					list[index] = onDeserialize(reader);
				}
				else
				{
					onDeserialize(reader);
				}
			}

			return list;
		}
		#endregion T[]

		#region List<T>
		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlockList<TObj>(this GenericWriter writer, List<TObj> list, Action<TObj> onSerialize)
		{
			WriteBlockList(writer, list, (w, o) => onSerialize(o));
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlockList<TObj>(
			this GenericWriter writer, List<TObj> list, Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new List<TObj>();

			writer.Write(list.Count);

			foreach (TObj obj in list)
			{
				WriteBlock(
					writer,
					() =>
					{
						if (obj == null)
						{
							writer.Write(false);
						}
						else
						{
							writer.Write(true);
							onSerialize(writer, obj);
						}
					});
			}
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static List<TObj> ReadBlockList<TObj>(
			this GenericReader reader, Func<TObj> onDeserialize, List<TObj> list = null)
		{
			return ReadBlockList(reader, r => onDeserialize(), list);
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static List<TObj> ReadBlockList<TObj>(
			this GenericReader reader, Func<GenericReader, TObj> onDeserialize, List<TObj> list = null)
		{
			int count = reader.ReadInt();

			list = list ?? new List<TObj>(count);

			for (int index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					() =>
					{
						if (!reader.ReadBool())
						{
							return;
						}

						TObj obj = onDeserialize(reader);

						if (obj != null && !list.Contains(obj))
						{
							list.Add(obj);
						}
					});
			}

			return list;
		}

		public static void WriteList<TObj>(this GenericWriter writer, List<TObj> list, Action<TObj> onSerialize)
		{
			WriteList(writer, list, (w, o) => onSerialize(o));
		}

		public static void WriteList<TObj>(
			this GenericWriter writer, List<TObj> list, Action<GenericWriter, TObj> onSerialize)
		{
			list = list ?? new List<TObj>();

			writer.Write(list.Count);

			foreach (TObj obj in list)
			{
				if (obj == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, obj);
				}
			}
		}

		public static List<TObj> ReadList<TObj>(this GenericReader reader, Func<TObj> onDeserialize, List<TObj> list = null)
		{
			return ReadList(reader, r => onDeserialize(), list);
		}

		public static List<TObj> ReadList<TObj>(
			this GenericReader reader, Func<GenericReader, TObj> onDeserialize, List<TObj> list = null)
		{
			int count = reader.ReadInt();

			list = list ?? new List<TObj>(count);

			for (int index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					continue;
				}

				TObj obj = onDeserialize(reader);

				if (obj != null && !list.Contains(obj))
				{
					list.Add(obj);
				}
			}

			return list;
		}
		#endregion List<T>

		#region Dictionary<TKey, TVal>
		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlockDictionary<TKey, TVal>(
			this GenericWriter writer, Dictionary<TKey, TVal> list, Action<TKey, TVal> onSerialize)
		{
			WriteBlockDictionary(writer, list, (w, key, val) => onSerialize(key, val));
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static void WriteBlockDictionary<TKey, TVal>(
			this GenericWriter writer, Dictionary<TKey, TVal> list, Action<GenericWriter, TKey, TVal> onSerialize)
		{
			list = list ?? new Dictionary<TKey, TVal>();

			writer.Write(list.Count);

			foreach (var kvp in list)
			{
				WriteBlock(
					writer,
					() =>
					{
						if (kvp.Key == null)
						{
							writer.Write(false);
						}
						else
						{
							writer.Write(true);
							onSerialize(writer, kvp.Key, kvp.Value);
						}
					});
			}
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static Dictionary<TKey, TVal> ReadBlockDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			return ReadBlockDictionary(reader, r => onDeserialize(), list, replace);
		}

		/// <summary>
		///     DO NOT USE IN ITEM, MOBILE OR GUILD SERIALIZATION
		/// </summary>
		public static Dictionary<TKey, TVal> ReadBlockDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<GenericReader, KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			int count = reader.ReadInt();

			list = list ?? new Dictionary<TKey, TVal>(count);

			for (int index = 0; index < count; index++)
			{
				ReadBlock(
					reader,
					() =>
					{
						if (!reader.ReadBool())
						{
							return;
						}

						var kvp = onDeserialize(reader);

						if (kvp.Key == null)
						{
							return;
						}

						if (!list.ContainsKey(kvp.Key))
						{
							list.Add(kvp.Key, kvp.Value);
						}
						else if (replace)
						{
							list[kvp.Key] = kvp.Value;
						}
					});
			}

			return list;
		}

		public static void WriteDictionary<TKey, TVal>(
			this GenericWriter writer, Dictionary<TKey, TVal> list, Action<TKey, TVal> onSerialize)
		{
			WriteDictionary(writer, list, (w, key, val) => onSerialize(key, val));
		}

		public static void WriteDictionary<TKey, TVal>(
			this GenericWriter writer, Dictionary<TKey, TVal> list, Action<GenericWriter, TKey, TVal> onSerialize)
		{
			list = list ?? new Dictionary<TKey, TVal>();

			writer.Write(list.Count);

			foreach (var kvp in list)
			{
				if (kvp.Key == null)
				{
					writer.Write(false);
				}
				else
				{
					writer.Write(true);
					onSerialize(writer, kvp.Key, kvp.Value);
				}
			}
		}

		public static Dictionary<TKey, TVal> ReadDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			return ReadDictionary(reader, r => onDeserialize(), list, replace);
		}

		public static Dictionary<TKey, TVal> ReadDictionary<TKey, TVal>(
			this GenericReader reader,
			Func<GenericReader, KeyValuePair<TKey, TVal>> onDeserialize,
			Dictionary<TKey, TVal> list = null,
			bool replace = true)
		{
			int count = reader.ReadInt();

			list = list ?? new Dictionary<TKey, TVal>(count);

			for (int index = 0; index < count; index++)
			{
				if (!reader.ReadBool())
				{
					continue;
				}

				var kvp = onDeserialize(reader);

				if (kvp.Key == null)
				{
					continue;
				}

				if (!list.ContainsKey(kvp.Key))
				{
					list.Add(kvp.Key, kvp.Value);
				}
				else if (replace)
				{
					list[kvp.Key] = kvp.Value;
				}
			}

			return list;
		}
		#endregion Dictionary<TKey, TVal>

		#region Custom Types
		public static void Write(this GenericWriter writer, TimeStamp ts)
		{
			ts.Serialize(writer);
		}

		public static TimeStamp ReadTimeStamp(this GenericReader reader)
		{
			return new TimeStamp(reader);
		}

		public static void WriteBlock3D(this GenericWriter writer, Block3D b)
		{
			writer.Write(b.X);
			writer.Write(b.Y);
			writer.Write(b.Z);
			writer.Write(b.H);
		}

		public static Block3D ReadBlock3D(this GenericReader reader)
		{
			int x = reader.ReadInt();
			int y = reader.ReadInt();
			int z = reader.ReadInt();
			int h = reader.ReadInt();

			return new Block3D(x, y, z, h);
		}

		public static void Write(this GenericWriter writer, Coords c)
		{
			writer.Write(c.Map);
			writer.Write(c.X);
			writer.Write(c.Y);
		}

		public static Coords ReadCoords(this GenericReader reader)
		{
			Map map = reader.ReadMap();
			int x = reader.ReadInt();
			int y = reader.ReadInt();

			return new Coords(map, x, y);
		}

		public static void Write(this GenericWriter writer, MapPoint mp)
		{
			writer.Write(mp.Map);
			writer.Write(mp.Location);
		}

		public static MapPoint ReadMapPoint(this GenericReader reader)
		{
			Map map = reader.ReadMap();
			Point3D p = reader.ReadPoint3D();

			return new MapPoint(map, p);
		}
		#endregion

		#region Type
		public static void WriteType(this GenericWriter writer, object obj, Action<Type> onSerialize, bool full = true)
		{
			WriteType(
				writer,
				obj,
				(w, t) =>
				{
					if (onSerialize != null)
					{
						onSerialize(t);
					}
				},
				full);
		}

		public static void WriteType(
			this GenericWriter writer, object obj, Action<GenericWriter, Type> onSerialize = null, bool full = true)
		{
			Type type = null;

			if (obj != null)
			{
				if (obj is Type)
				{
					type = (Type)obj;
				}
				else if (obj is ITypeSelectProperty)
				{
					type = ((ITypeSelectProperty)obj).InternalType;
				}
				else
				{
					type = obj.GetType();
				}
			}

			if (type == null)
			{
				writer.Write(false);
			}
			else
			{
				writer.Write(true);
				writer.Write(full);
				writer.Write(full ? type.FullName : type.Name);
			}

			if (onSerialize != null)
			{
				onSerialize(writer, type);
			}
		}

		public static Type ReadType(this GenericReader reader)
		{
			if (!reader.ReadBool())
			{
				return null;
			}

			bool full = reader.ReadBool();
			string name = reader.ReadString();

			if (String.IsNullOrWhiteSpace(name))
			{
				return null;
			}

			Type type = Type.GetType(name, false) ??
						(full ? ScriptCompiler.FindTypeByFullName(name) : ScriptCompiler.FindTypeByName(name));

			return type;
		}

		public static object ReadTypeCreate(this GenericReader reader, params object[] args)
		{
			return ReadTypeCreate<object>(reader, args);
		}

		public static TObj ReadTypeCreate<TObj>(this GenericReader reader, params object[] args) where TObj : class
		{
			TObj obj = null;

			VitaNexCore.TryCatch(
				() =>
				{
					Type t = ReadType(reader);

					if (t == null)
					{
						return;
					}

					obj = t.CreateInstanceSafe<TObj>(args);
				},
				VitaNexCore.ToConsole);

			return obj;
		}
		#endregion Type

		#region Enums
		public static void WriteFlag(this GenericWriter writer, Enum flag)
		{
			Type ut = Enum.GetUnderlyingType(flag.GetType());

			if (ut == typeof(byte))
			{
				writer.Write((byte)0x01);
				writer.Write(Convert.ToByte(flag));
			}
			else if (ut == typeof(short))
			{
				writer.Write((byte)0x02);
				writer.Write(Convert.ToInt16(flag));
			}
			else if (ut == typeof(ushort))
			{
				writer.Write((byte)0x03);
				writer.Write(Convert.ToUInt16(flag));
			}
			else if (ut == typeof(int))
			{
				writer.Write((byte)0x04);
				writer.Write(Convert.ToInt32(flag));
			}
			else if (ut == typeof(uint))
			{
				writer.Write((byte)0x05);
				writer.Write(Convert.ToUInt32(flag));
			}
			else if (ut == typeof(long))
			{
				writer.Write((byte)0x06);
				writer.Write(Convert.ToInt64(flag));
			}
			else if (ut == typeof(ulong))
			{
				writer.Write((byte)0x07);
				writer.Write(Convert.ToUInt64(flag));
			}
			else
			{
				writer.Write((byte)0x00);
			}
		}

		public static TEnum ReadFlag<TEnum>(this GenericReader reader) where TEnum : struct
		{
			TEnum flag = default(TEnum);

			if (!typeof(TEnum).IsEnum)
			{
				return flag;
			}

			switch (reader.ReadByte())
			{
				case 0x01:
					flag = ToEnum<TEnum>(reader.ReadByte());
					break;
				case 0x02:
					flag = ToEnum<TEnum>(reader.ReadShort());
					break;
				case 0x03:
					flag = ToEnum<TEnum>(reader.ReadUShort());
					break;
				case 0x04:
					flag = ToEnum<TEnum>(reader.ReadInt());
					break;
				case 0x05:
					flag = ToEnum<TEnum>(reader.ReadUInt());
					break;
				case 0x06:
					flag = ToEnum<TEnum>(reader.ReadLong());
					break;
				case 0x07:
					flag = ToEnum<TEnum>(reader.ReadULong());
					break;
			}

			return flag;
		}

		private static TEnum ToEnum<TEnum>(object val) where TEnum : struct
		{
			TEnum flag = default(TEnum);

			if (!typeof(TEnum).IsEnum)
			{
				return flag;
			}

			Enum.TryParse(val.ToString(), out flag);
			return flag;
		}
		#endregion Enums

		#region Simple Types
		public static void WriteSimpleType(this GenericWriter writer, object obj)
		{
			SimpleType.FromObject(obj).Serialize(writer);
		}

		public static SimpleType ReadSimpleType(this GenericReader reader)
		{
			return new SimpleType(reader);
		}

		public static TObj ReadSimpleType<TObj>(this GenericReader reader)
		{
			object value = new SimpleType(reader).Value;
			return value is TObj ? (TObj)value : default(TObj);
		}
		#endregion Simple types

		#region Accounts
		public static void Write(this GenericWriter writer, IAccount a)
		{
			writer.Write(a == null ? String.Empty : a.Username);
		}

		public static IAccount ReadAccount(this GenericReader reader, bool defaultToOwner = false)
		{
			string username = reader.ReadString();
			IAccount a = Accounts.GetAccount(username ?? String.Empty);

			if (a == null && defaultToOwner)
			{
				a = Accounts.GetAccounts().FirstOrDefault(ac => ac.AccessLevel == AccessLevel.Owner);
			}

			return a;
		}
		#endregion Accounts

		#region Versioning
		public static int SetVersion(this GenericWriter writer, int version)
		{
			writer.Write(version);
			return version;
		}

		public static int GetVersion(this GenericReader reader)
		{
			return reader.ReadInt();
		}
		#endregion Versioning

		#region Crypto
		public static void Write(this GenericWriter writer, CryptoHashCode hash)
		{
			WriteType(writer, hash, t =>
			{
				if(t != null)
				{
					hash.Serialize(writer);
				}
			});
		}

		public static THashCode ReadHashCode<THashCode>(this GenericReader reader) where THashCode : CryptoHashCode
		{
			return ReadTypeCreate<THashCode>(reader, reader);
		}

		public static CryptoHashCode ReadHashCode(this GenericReader reader)
		{
			return ReadTypeCreate<CryptoHashCode>(reader, reader);
		}
		#endregion Crypto
	}
}