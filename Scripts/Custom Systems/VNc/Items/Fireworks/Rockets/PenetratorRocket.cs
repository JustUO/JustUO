#region Header
//   Vorspire    _,-'/-'/  PenetratorRocket.cs
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
	public class PenetratorRocket : BaseFireworkRocket
	{
		public override FireworkStars DefStarsEffect { get { return FireworkStars.Chrysanthemum; } }

		[Constructable]
		public PenetratorRocket()
			: this(Utility.RandomMetalHue())
		{ }

		[Constructable]
		public PenetratorRocket(int hue)
			: base(6202, hue)
		{
			Name = "The Penetrator";
			Weight = 4.0;
		}

		public PenetratorRocket(Serial serial)
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