#region Header
//   Vorspire    _,-'/-'/  HueOverride.cs
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

namespace VitaNex.Modules.EquipmentSets
{
	public class HueOverrideSetMod : EquipmentSetMod
	{
		public HueOverrideSetMod(int partsReq = 1, bool display = true, int hue = -1)
			: base("Taste The Rainbow", String.Empty, partsReq, display)
		{
			Hue = hue >= 0 ? hue : Utility.RandomPinkHue();
		}

		public HueOverrideSetMod(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(EquipmentSets.Access)]
		public virtual int Hue { get; set; }

		protected override bool OnActivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			m.SolidHueOverride = Hue;
			return true;
		}

		protected override bool OnDeactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			m.SolidHueOverride = -1;
			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.Write(Hue);
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
					Hue = reader.ReadInt();
					break;
			}
		}
	}
}