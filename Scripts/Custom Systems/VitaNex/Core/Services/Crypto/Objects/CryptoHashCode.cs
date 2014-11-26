#region Header
//   Vorspire    _,-'/-'/  CryptoHashCode.cs
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
using System.Globalization;
using System.Linq;

using Server;
#endregion

namespace VitaNex.Crypto
{
	public class CryptoHashCode : IEquatable<CryptoHashCode>, IComparable<CryptoHashCode>, IEnumerable<char>
	{
		private int _ProviderID;
		private string _Seed;
		private string _Value;

		public virtual string Seed
		{
			get { return _Seed; }
			protected set
			{
				if (_Seed == value)
				{
					return;
				}

				_Seed = value;
				_Value = CryptoGenerator.GenString(_ProviderID, _Seed);
			}
		}

		public virtual int ProviderID
		{
			get { return _ProviderID; }
			protected set
			{
				if (_ProviderID == value || value < 0)
				{
					return;
				}

				_ProviderID = value;
				_Value = CryptoGenerator.GenString(_ProviderID, _Seed);
			}
		}

		public virtual string Value
		{
			//
			get { return _Value = String.IsNullOrWhiteSpace(_Value) ? CryptoGenerator.GenString(_ProviderID, _Seed) : _Value; }
		}

		public int ValueHash { get { return GetValueHash(); } }

		public int GetValueHash()
		{
			unchecked
			{
				// I know, this is super lazy...

				// Get all bytes in the hashcode as UInt64
				var alpha = _Value.Split('-').Select(b => Byte.Parse(b, NumberStyles.HexNumber)).ToArray();

				// XOR them all together using the length of alpha as the seed, producing 8 bytes
				// Each iteration multiplies the current value by ReSharper's golden number 397.
				var delta = BitConverter.GetBytes(alpha.Aggregate(alpha.LongLength, (l, r) => (l * 397) ^ r));

				// Fold the first 4 bytes with the second 4 bytes to produce the hash
				int hash = BitConverter.ToInt32(delta, 0) ^ BitConverter.ToInt32(delta, 3);

				// It may be negative, so ensure it is positive, normally this wouldn't be the case but negatives integers for 
				// almost unique id's should be positive for things like database keys.
				return Math.Abs(hash);
			}
		}

		public bool IsExtended { get { return CryptoService.IsExtended(_ProviderID); } }

		public virtual int Length { get { return Value.Length; } }
		public virtual char this[int index] { get { return Value[index]; } }

		public CryptoHashCode(CryptoHashType type, string seed)
		{
			_Seed = seed;
			_ProviderID = (int)type;
			_Value = CryptoGenerator.GenString(type, _Seed);
		}

		public CryptoHashCode(int providerID, string seed)
		{
			_Seed = seed;
			_ProviderID = providerID;
			_Value = CryptoGenerator.GenString(_ProviderID, _Seed);
		}

		public CryptoHashCode(GenericReader reader)
		{
			Deserialize(reader);
		}

		public virtual int CompareTo(CryptoHashCode code)
		{
			return code == null ? -1 : String.Compare(Value, code.Value, StringComparison.Ordinal);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public virtual IEnumerator<char> GetEnumerator()
		{
			return Value.GetEnumerator();
		}

		public virtual void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(_ProviderID);
						writer.Write(_Seed);
					}
					break;
			}
		}

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						_ProviderID = reader.ReadInt();
						_Seed = reader.ReadString();
						_Value = CryptoGenerator.GenString(_ProviderID, _Seed);
					}
					break;
			}
		}

		public override bool Equals(object obj)
		{
			return obj is CryptoHashCode && Equals((CryptoHashCode)obj);
		}

		public bool Equals(CryptoHashCode other)
		{
			return !ReferenceEquals(null, other) && (ReferenceEquals(this, other) || String.Equals(Value, other.Value));
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hash = _ProviderID;
				hash = (hash * 397) ^ (_Value != null ? _Value.GetHashCode() : 0);
				return hash;
			}
		}

		public override string ToString()
		{
			return Value;
		}

		public static bool operator ==(CryptoHashCode left, CryptoHashCode right)
		{
			return ReferenceEquals(null, left) ? ReferenceEquals(null, right) : left.Equals(right);
		}

		public static bool operator !=(CryptoHashCode left, CryptoHashCode right)
		{
			return ReferenceEquals(null, left) ? !ReferenceEquals(null, right) : !left.Equals(right);
		}
	}
}