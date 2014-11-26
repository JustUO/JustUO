#region Header
//   Vorspire    _,-'/-'/  PlateSet.cs
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
using Server.Items;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public sealed class PlateArmorSet : EquipmentSet
	{
		public PlateArmorSet()
			: base("Plate Avenger")
		{
			/*Add Parts to this Set*/
			Add(new EquipmentSetPart("Avengers' Chestguard", typeof(PlateChest)));
			Add(new EquipmentSetPart("Avengers' Pauldrons", typeof(PlateArms)));
			Add(new EquipmentSetPart("Avengers' Gauntlets", typeof(PlateGloves)));
			Add(new EquipmentSetPart("Avengers' Neckguard", typeof(PlateGorget)));
			Add(new EquipmentSetPart("Avengers' Legguards", typeof(PlateLegs)));
			Add(new EquipmentSetPart("Avengers' Helmet", typeof(PlateHelm)));
			Add(new EquipmentSetPart("Avengers' Shield", typeof(BaseShield), true, true, false));
			Add(new EquipmentSetPart("Avengers' Weapon", typeof(BaseWeapon), true, true, false));

			/*Add Mods to this Set*/
			Add(new StatOffsetSetMod("PlateAvenger1", "Avenger 1", 2, true, StatType.All, 5, TimeSpan.Zero));
			Add(new StatOffsetSetMod("PlateAvenger2", "Avenger 2", 4, true, StatType.All, 5, TimeSpan.Zero));
			Add(new StatOffsetSetMod("PlateAvenger3", "Avenger 3", 6, true, StatType.All, 5, TimeSpan.Zero));
			Add(new StatOffsetSetMod("PlateAvenger4", "Avenger 4", 8, true, StatType.All, 5, TimeSpan.Zero));
		}

		public PlateArmorSet(GenericReader reader)
			: base(reader)
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