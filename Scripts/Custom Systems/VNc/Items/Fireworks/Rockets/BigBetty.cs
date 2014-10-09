#region Header
//   Vorspire    _,-'/-'/  BigBetty.cs
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
	public class BigBettyRocket : BaseFireworkRocket
	{
		public override FireworkStars DefStarsEffect { get { return FireworkStars.Willow; } }

		[Constructable]
		public BigBettyRocket()
			: this(Utility.RandomMetalHue())
		{ }

		[Constructable]
		public BigBettyRocket(int hue)
			: base(2887, hue)
		{
			Name = "Big Betty";
			Weight = 6.0;
		}

		public BigBettyRocket(Serial serial)
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