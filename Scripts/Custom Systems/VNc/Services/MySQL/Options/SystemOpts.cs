#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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
	public class MySQLOptions : CoreServiceOptions
	{
		private int _MaxConnections = 100;

		[CommandProperty(MySQL.Access)]
		public virtual int MaxConnections { get { return _MaxConnections; } set { _MaxConnections = Math.Max(1, value); } }

		public MySQLOptions()
			: base(typeof(MySQL))
		{ }

		public MySQLOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			base.Clear();

			MaxConnections = 1;
		}

		public override void Reset()
		{
			base.Reset();

			MaxConnections = 100;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.Write(MaxConnections);
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
					MaxConnections = reader.ReadInt();
					break;
			}
		}
	}
}