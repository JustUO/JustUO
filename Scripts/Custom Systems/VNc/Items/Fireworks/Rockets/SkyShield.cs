#region Header
//   Vorspire    _,-'/-'/  SkyShield.cs
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
	public class SkyShieldRocket : BaseFireworkRocket
	{
		public override FireworkStars DefStarsEffect { get { return FireworkStars.Crossette; } }

		[Constructable]
		public SkyShieldRocket()
			: this(Utility.RandomMetalHue())
		{ }

		[Constructable]
		public SkyShieldRocket(int hue)
			: base(576, hue)
		{
			Name = "Sky Shield";
			Weight = 3.0;
		}

		public SkyShieldRocket(Serial serial)
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