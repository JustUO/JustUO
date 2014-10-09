#region Header
//   Vorspire    _,-'/-'/  FireworkStarAgapite.cs
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
	public class FireworkStarAgapite : BaseFireworkStar
	{
		[Constructable]
		public FireworkStarAgapite()
			: this(1)
		{ }

		[Constructable]
		public FireworkStarAgapite(int amount)
			: base(CraftResource.Agapite, amount)
		{ }

		public FireworkStarAgapite(Serial serial)
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