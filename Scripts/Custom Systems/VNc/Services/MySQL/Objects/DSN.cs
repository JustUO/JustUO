#region Header
//   Vorspire    _,-'/-'/  DSN.cs
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

using Server;
#endregion

namespace VitaNex.MySQL
{
	public sealed class MySQLDSN : PropertyObject, IEquatable<MySQLDSN>
	{
		public MySQLDSN(string name = null, string desc = null)
		{
			Name = name ?? String.Empty;
			Desc = desc ?? String.Empty;
		}

		public MySQLDSN(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(MySQL.Access)]
		public string Name { get; set; }

		[CommandProperty(MySQL.Access)]
		public string Desc { get; set; }

		public bool Equals(MySQLDSN dsn)
		{
			return (dsn != null && Name == dsn.Name && Desc == dsn.Desc);
		}

		public override void Clear()
		{
			Name = String.Empty;
			Desc = String.Empty;
		}

		public override void Reset()
		{
			Name = String.Empty;
			Desc = String.Empty;
		}

		public override string ToString()
		{
			return String.IsNullOrWhiteSpace(Name) ? String.Empty : Name;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Name);
						writer.Write(Desc);
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
				case 0:
					{
						Name = reader.ReadString();
						Desc = reader.ReadString();
					}
					break;
			}
		}
	}
}