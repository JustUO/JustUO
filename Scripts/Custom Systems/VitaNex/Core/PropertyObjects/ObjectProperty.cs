#region Header
//   Vorspire    _,-'/-'/  ObjectProperty.cs
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
using System.Reflection;

using Server;
#endregion

namespace VitaNex
{
	public sealed class ObjectProperty : PropertyObject
	{
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string Name { get; set; }

		public ObjectProperty()
			: this(String.Empty)
		{ }

		public ObjectProperty(string name)
		{
			Name = name ?? String.Empty;
		}

		public ObjectProperty(GenericReader reader)
			: base(reader)
		{ }

		public override void Reset()
		{
			Name = String.Empty;
		}

		public override void Clear()
		{
			Name = String.Empty;
		}

		public object GetValue(object o, object def = null)
		{
			if (String.IsNullOrWhiteSpace(Name) || o == null)
			{
				return def;
			}

			try
			{
				return o.GetType().GetProperty(Name, BindingFlags.Instance | BindingFlags.Public).GetValue(o, null);
			}
			catch
			{
				return def;
			}
		}

		public bool SetValue(object o, object val)
		{
			if (String.IsNullOrWhiteSpace(Name) || o == null)
			{
				return false;
			}

			try
			{
				var p = o.GetType().GetProperty(Name, BindingFlags.Instance | BindingFlags.Public);

				p.SetValue(o, val, null);
				return true;
			}
			catch
			{
				return false;
			}
		}

		public bool Add(object o, object val)
		{
			var cur = GetValue(o);

			if (cur is sbyte)
			{
				return val is sbyte && SetValue(o, unchecked((sbyte)cur + (sbyte)val));
			}

			if (cur is byte)
			{
				return val is byte && SetValue(o, unchecked((byte)cur + (byte)val));
			}

			if (cur is short)
			{
				return val is short && SetValue(o, unchecked((short)cur + (short)val));
			}

			if (cur is ushort)
			{
				return val is ushort && SetValue(o, unchecked((ushort)cur + (ushort)val));
			}

			if (cur is int)
			{
				return val is int && SetValue(o, unchecked((int)cur + (int)val));
			}

			if (cur is uint)
			{
				return val is uint && SetValue(o, unchecked((uint)cur + (uint)val));
			}

			if (cur is long)
			{
				return val is long && SetValue(o, unchecked((long)cur + (long)val));
			}

			if (cur is ulong)
			{
				return val is ulong && SetValue(o, unchecked((ulong)cur + (ulong)val));
			}

			if (cur is TimeSpan)
			{
				return val is TimeSpan && SetValue(o, unchecked((TimeSpan)cur + (TimeSpan)val));
			}

			if (cur is DateTime)
			{
				return val is TimeSpan && SetValue(o, unchecked((DateTime)cur + (TimeSpan)val));
			}

			return false;
		}

		public bool Subtract(object o, object val, bool limit = false)
		{
			var cur = GetValue(o);

			if (cur is sbyte)
			{
				return val is sbyte && (!limit || (sbyte)cur >= (sbyte)val) && SetValue(o, unchecked((sbyte)cur - (sbyte)val));
			}

			if (cur is byte)
			{
				return val is byte && (!limit || (byte)cur >= (byte)val) && SetValue(o, unchecked((byte)cur - (byte)val));
			}

			if (cur is short)
			{
				return val is short && (!limit || (short)cur >= (short)val) && SetValue(o, unchecked((short)cur - (short)val));
			}

			if (cur is ushort)
			{
				return val is ushort && (!limit || (ushort)cur >= (ushort)val) && SetValue(o, unchecked((ushort)cur - (ushort)val));
			}

			if (cur is int)
			{
				return val is int && (!limit || (int)cur >= (int)val) && SetValue(o, unchecked((int)cur - (int)val));
			}

			if (cur is uint)
			{
				return val is uint && (!limit || (uint)cur >= (uint)val) && SetValue(o, unchecked((uint)cur - (uint)val));
			}

			if (cur is long)
			{
				return val is long && (!limit || (long)cur >= (long)val) && SetValue(o, unchecked((long)cur - (long)val));
			}

			if (cur is ulong)
			{
				return val is ulong && (!limit || (ulong)cur >= (ulong)val) && SetValue(o, unchecked((ulong)cur - (ulong)val));
			}

			if (cur is TimeSpan)
			{
				return val is TimeSpan && SetValue(o, unchecked((TimeSpan)cur - (TimeSpan)val));
			}

			if (cur is DateTime)
			{
				return val is TimeSpan && SetValue(o, unchecked((DateTime)cur - (TimeSpan)val));
			}

			return false;
		}

		public bool Consume(object o, object val)
		{
			return Subtract(o, val, true);
		}

		public bool SetDefault(object o)
		{
			var cur = GetValue(o);

			if (cur is sbyte)
			{
				return SetValue(o, (sbyte)0);
			}

			if (cur is byte)
			{
				return SetValue(o, (byte)0);
			}

			if (cur is short)
			{
				return SetValue(o, (short)0);
			}

			if (cur is ushort)
			{
				return SetValue(o, (ushort)0);
			}

			if (cur is int)
			{
				return SetValue(o, 0);
			}

			if (cur is uint)
			{
				return SetValue(o, (uint)0);
			}

			if (cur is long)
			{
				return SetValue(o, (long)0);
			}

			if (cur is ulong)
			{
				return SetValue(o, (ulong)0);
			}

			if (cur is TimeSpan)
			{
				return SetValue(o, TimeSpan.Zero);
			}

			if (cur is DateTime)
			{
				return SetValue(o, DateTime.MinValue);
			}

			if (cur is char)
			{
				return SetValue(o, ' ');
			}

			if (cur is string)
			{
				return SetValue(o, String.Empty);
			}

			return SetValue(o, null);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Name);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Name = reader.ReadString();
		}
	}
}