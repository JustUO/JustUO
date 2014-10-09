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

using Server;
#endregion

namespace VitaNex.Crypto
{
	public class CryptoHashCode : IEquatable<CryptoHashCode>, IComparable<CryptoHashCode>, IEnumerable<char>
	{
		private int _ProviderID;
		private string _Seed;
		private string _Value;

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

		public virtual string Value { get { return _Value = String.IsNullOrWhiteSpace(_Value) ? CryptoGenerator.GenString(_ProviderID, _Seed) : _Value; } }

		public bool IsExtended { get { return CryptoService.IsExtended(_ProviderID); } }

		public virtual int Length { get { return Value.Length; } }
		public virtual char this[int index] { get { return Value[index]; } }

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

		public bool Equals(CryptoHashCode other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return String.Equals(_Value, other._Value);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			var other = obj as CryptoHashCode;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return (_Value != null ? _Value.GetHashCode() : 0);
		}

		public static bool operator ==(CryptoHashCode left, CryptoHashCode right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(CryptoHashCode left, CryptoHashCode right)
		{
			return !Equals(left, right);
		}

		public override string ToString()
		{
			return Value;
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
	}
}