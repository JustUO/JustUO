#region Header
//   Vorspire    _,-'/-'/  VersionInfo.cs
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
using System.Globalization;
using System.Linq;

using Server;
#endregion

namespace VitaNex
{
	[AttributeUsage(AttributeTargets.All, AllowMultiple = false, Inherited = false)]
	public class VersionInfoAttribute : Attribute
	{
		public VersionInfo Version { get; set; }

		public string Name { get { return Version.Name; } set { Version.Name = value; } }
		public string Description { get { return Version.Description; } set { Version.Description = value; } }

		public VersionInfoAttribute(string version = "1.0.0.0", string name = "", string description = "")
		{
			Version = version;
			Name = name;
			Description = description;
		}
	}

	public class VersionInfo
		: PropertyObject, IEquatable<VersionInfo>, IComparable<VersionInfo>, IEquatable<Version>, IComparable<Version>
	{
		public static Version DefaultVersion { get { return new Version(1, 0, 0, 0); } }

		protected Version InternalVersion { get; set; }

		public Version Version { get { return InternalVersion ?? (InternalVersion = DefaultVersion); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual string Value { get { return ToString(4); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual int Major { get { return Version.Major; } set { InternalVersion = new Version(value, Minor, Build, Revision); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual int Minor { get { return Version.Minor; } set { InternalVersion = new Version(Major, value, Build, Revision); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual int Build { get { return Version.Build; } set { InternalVersion = new Version(Major, Minor, value, Revision); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual int Revision { get { return Version.Revision; } set { InternalVersion = new Version(Major, Minor, Build, value); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual string Name { get; set; }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public virtual string Description { get; set; }

		public VersionInfo()
		{
			InternalVersion = DefaultVersion;
		}

		public VersionInfo(int major = 1, int minor = 0, int build = 0, int revision = 0)
		{
			InternalVersion = new Version(major, minor, build, revision);
		}

		public VersionInfo(string version)
		{
			Version v;

			if (!Version.TryParse(version, out v))
			{
				v = DefaultVersion;
			}

			InternalVersion = new Version(
				Math.Max(0, v.Major), Math.Max(0, v.Minor), Math.Max(0, v.Build), Math.Max(0, v.Revision));
		}

		public VersionInfo(Version v)
		{
			InternalVersion = new Version(
				Math.Max(0, v.Major), Math.Max(0, v.Minor), Math.Max(0, v.Build), Math.Max(0, v.Revision));
		}

		public VersionInfo(GenericReader reader)
			: base(reader)
		{ }

		public virtual int CompareTo(Version version)
		{
			if (version == null)
			{
				return -1;
			}

			return Version.CompareTo(version);
		}

		public virtual int CompareTo(VersionInfo version)
		{
			if (version == null)
			{
				return -1;
			}

			return CompareTo(version.Version);
		}

		public virtual bool Equals(Version version)
		{
			if (version == null)
			{
				return false;
			}

			if (ReferenceEquals(Version, version))
			{
				return true;
			}

			return Version.Equals(version);
		}

		public virtual bool Equals(VersionInfo version)
		{
			if (version == null)
			{
				return false;
			}

			if (ReferenceEquals(this, version))
			{
				return true;
			}

			return Equals(version.Version);
		}

		public override void Clear()
		{
			InternalVersion = DefaultVersion;
		}

		public override void Reset()
		{
			InternalVersion = DefaultVersion;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 1:
					{
						writer.Write(Name);
						writer.Write(Description);
					}
					goto case 0;
				case 0:
					{
						writer.Write(Version.Major);
						writer.Write(Version.Minor);
						writer.Write(Version.Build);
						writer.Write(Version.Revision);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 1:
					{
						Name = reader.ReadString();
						Description = reader.ReadString();
					}
					goto case 0;
				case 0:
					{
						int major = reader.ReadInt(), minor = reader.ReadInt(), build = reader.ReadInt(), revision = reader.ReadInt();

						InternalVersion = new Version(Math.Max(0, major), Math.Max(0, minor), Math.Max(0, build), Math.Max(0, revision));
					}
					break;
			}
		}

		public override string ToString()
		{
			return Version.ToString();
		}

		public virtual string ToString(int fieldCount)
		{
			return Version.ToString(fieldCount);
		}

		public override int GetHashCode()
		{
			return Version.GetHashCode();
		}

		public override bool Equals(object obj)
		{
			if (obj == null)
			{
				return false;
			}

			if (obj is Version)
			{
				return Equals((Version)obj);
			}

			if (obj is VersionInfo)
			{
				return Equals((VersionInfo)obj);
			}

			return base.Equals(obj);
		}

		public static bool TryParse(string s, out VersionInfo version)
		{
			version = DefaultVersion;

			if (String.IsNullOrWhiteSpace(s))
			{
				return false;
			}

			string value = String.Empty;

			foreach (string c in s.Select(c => c.ToString(CultureInfo.InvariantCulture)))
			{
				if (c == ".")
				{
					if (value.Length > 0)
					{
						value += c;
					}

					continue;
				}

				byte b;

				if (Byte.TryParse(c, out b))
				{
					value += b;
				}
			}

			Version v;

			if (Version.TryParse(value, out v))
			{
				version = new Version(Math.Max(0, v.Major), Math.Max(0, v.Minor), Math.Max(0, v.Build), Math.Max(0, v.Revision));
				return true;
			}

			return false;
		}

		public static implicit operator VersionInfo(string version)
		{
			return new VersionInfo(version);
		}

		public static implicit operator string(VersionInfo version)
		{
			return version.Value;
		}

		public static implicit operator VersionInfo(Version version)
		{
			return new VersionInfo(version);
		}

		public static implicit operator Version(VersionInfo a)
		{
			return a.Version;
		}

		public static bool operator ==(VersionInfo v1, VersionInfo v2)
		{
			return (!ReferenceEquals(v1, null) && !ReferenceEquals(v2, null) && v1.Version == v2.Version) ||
				   (ReferenceEquals(v1, null) && ReferenceEquals(v2, null));
		}

		public static bool operator !=(VersionInfo v1, VersionInfo v2)
		{
			return (!ReferenceEquals(v1, null) && !ReferenceEquals(v2, null) && v1.Version != v2.Version) ||
				   (ReferenceEquals(v1, null) && !ReferenceEquals(v2, null)) ||
				   (!ReferenceEquals(v1, null) && ReferenceEquals(v2, null));
		}

		public static bool operator <=(VersionInfo v1, VersionInfo v2)
		{
			return (!ReferenceEquals(v1, null) && !ReferenceEquals(v2, null) && v1.Version <= v2.Version);
		}

		public static bool operator >=(VersionInfo v1, VersionInfo v2)
		{
			return (!ReferenceEquals(v1, null) && !ReferenceEquals(v2, null) && v1.Version >= v2.Version);
		}

		public static bool operator <(VersionInfo v1, VersionInfo v2)
		{
			return (!ReferenceEquals(v1, null) && !ReferenceEquals(v2, null) && v1.Version < v2.Version);
		}

		public static bool operator >(VersionInfo v1, VersionInfo v2)
		{
			return (!ReferenceEquals(v1, null) && !ReferenceEquals(v2, null) && v1.Version > v2.Version);
		}
	}
}