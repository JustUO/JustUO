#region Header
//   Vorspire    _,-'/-'/  VoteVendor.cs
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

using VitaNex.Items;
using VitaNex.Mobiles;
#endregion

namespace VitaNex.Modules.Voting
{
	public class VoteVendor : AdvancedVendor
	{
		[Constructable]
		public VoteVendor()
			: base("the vote registrar", typeof(VoteToken), "Vote Tokens")
		{ }

		public VoteVendor(Serial serial)
			: base(serial)
		{ }

		protected override void InitBuyInfo()
		{
			AddStock<RuneCodex>(250);
			AddStock<StrobeLantern>(100);

			AddStock<ThrowableBomb>(1);
			AddStock<ThrowableHealBomb>(1);
			AddStock<ThrowableCureBomb>(1);
			AddStock<ThrowableManaBomb>(1);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
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
					break;
			}
		}
	}
}