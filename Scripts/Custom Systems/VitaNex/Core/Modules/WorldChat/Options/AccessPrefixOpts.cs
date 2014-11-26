#region Header
//   Vorspire    _,-'/-'/  AccessPrefixOpts.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2013  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public sealed class WorldChatAccessPrefixOptions : PropertyObject
	{
		[CommandProperty(WorldChat.Access)]
		public string Player { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Counselor { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string GameMaster { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Seer { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Administrator { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Developer { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Owner { get; set; }

		public string this[AccessLevel access]
		{
			get
			{
				switch (access)
				{
					case AccessLevel.Player:
						return Player;
					case AccessLevel.Counselor:
						return Counselor;
					case AccessLevel.GameMaster:
						return GameMaster;
					case AccessLevel.Seer:
						return Seer;
					case AccessLevel.Administrator:
						return Administrator;
					case AccessLevel.Developer:
						return Developer;
					case AccessLevel.Owner:
						return Owner;
				}

				return string.Empty;
			}
			set
			{
				value = value ?? string.Empty;

				switch (access)
				{
					case AccessLevel.Player:
						Player = value;
						break;
					case AccessLevel.Counselor:
						Player = value;
						break;
					case AccessLevel.GameMaster:
						Player = value;
						break;
					case AccessLevel.Seer:
						Player = value;
						break;
					case AccessLevel.Administrator:
						Player = value;
						break;
					case AccessLevel.Developer:
						Player = value;
						break;
					case AccessLevel.Owner:
						Player = value;
						break;
				}
			}
		}

		public WorldChatAccessPrefixOptions()
		{
			Player = "";
			Counselor = "+";
			GameMaster = "%";
			Seer = "%";
			Administrator = "@";
			Developer = "@";
			Owner = "#";
		}

		public WorldChatAccessPrefixOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			Player = "";
			Counselor = "";
			GameMaster = "";
			Seer = "";
			Administrator = "";
			Developer = "";
			Owner = "";
		}

		public override void Reset()
		{
			Player = "";
			Counselor = "+";
			GameMaster = "%";
			Seer = "%";
			Administrator = "@";
			Developer = "@";
			Owner = "#";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Player);
			writer.Write(Counselor);
			writer.Write(GameMaster);
			writer.Write(Seer);
			writer.Write(Administrator);
			writer.Write(Developer);
			writer.Write(Owner);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Player = reader.ReadString();
			Counselor = reader.ReadString();
			GameMaster = reader.ReadString();
			Seer = reader.ReadString();
			Administrator = reader.ReadString();
			Developer = reader.ReadString();
			Owner = reader.ReadString();
		}
	}
}