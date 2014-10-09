#region Header
//   Vorspire    _,-'/-'/  PyrotechnicsKit.cs
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
using Server.Engines.Craft;
using Server.Items;

using VitaNex.SuperCrafts;
#endregion

namespace VitaNex.Items
{
	[Flipable(39213, 39214)]
	[ArtworkSupport("7.0.26.0", new[] {39213, 39214}, 8792)]
	public class PyrotechnicsKit : BaseTool
	{
		public override CraftSystem CraftSystem { get { return SuperCraftSystem.Resolve<Pyrotechnics>(); } }

		[Constructable]
		public PyrotechnicsKit()
			: this(50)
		{ }

		[Constructable]
		public PyrotechnicsKit(int uses)
			: base(uses, Utility.RandomList(39213, 39214))
		{
			Name = "Pyrotechnics Kit";
			Weight = 2.0;
		}

		public PyrotechnicsKit(Serial serial)
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