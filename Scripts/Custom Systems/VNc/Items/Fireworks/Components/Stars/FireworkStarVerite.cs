#region Header
//   Vorspire    _,-'/-'/  FireworkStarVerite.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;
using Server.Items;
#endregion

namespace VitaNex.Items
{
	public class FireworkStarVerite : BaseFireworkStar
	{
		[Constructable]
		public FireworkStarVerite()
			: this(1)
		{ }

		[Constructable]
		public FireworkStarVerite(int amount)
			: base(CraftResource.Verite, amount)
		{ }

		public FireworkStarVerite(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}