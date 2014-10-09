#region Header
//   Vorspire    _,-'/-'/  BottleRocket.cs
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
#endregion

namespace VitaNex.Items
{
	public class BottleRocket : BaseFireworkRocket
	{
		public override FireworkStars DefStarsEffect { get { return FireworkStars.Peony; } }

		[Constructable]
		public BottleRocket()
			: this(Utility.RandomMetalHue())
		{ }

		[Constructable]
		public BottleRocket(int hue)
			: base(6189, hue)
		{
			Name = "Bottle Rocket";
			Weight = 2.0;
		}

		public BottleRocket(Serial serial)
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