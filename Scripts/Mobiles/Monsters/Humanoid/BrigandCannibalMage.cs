using System;
using Server.Items;

namespace Server.Mobiles 
{ 
    [CorpseName("an BrigandCannibalMage corpse")] 
    public class BrigandCannibalMage : BaseCreature 
    { 
        [Constructable] 
        public BrigandCannibalMage()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("Brigand Cannibal Mage");
            Body = Utility.RandomList(125, 126);

            PackItem(new Robe(Utility.RandomMetalHue())); 

            SetStr(76, 115);
            SetDex(92, 95);
            SetInt(110, 115);

            SetHits(2073, 2074);
            SetMana(552, 553);

            SetDamage(10, 23);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 65, 68);
            SetResistance(ResistanceType.Fire, 65, 66);
            SetResistance(ResistanceType.Cold, 62, 69);
            SetResistance(ResistanceType.Poison, 62, 67);
            SetResistance(ResistanceType.Energy, 64, 68);

            SetSkill(SkillName.MagicResist, 96.9, 96.9);
            SetSkill(SkillName.Tactics, 94.0, 94.0);
            SetSkill(SkillName.Wrestling, 54.3, 54.3);

            Fame = 14500;//No details
            Karma = -14500;//No details

            VirtualArmor = 16;
            //All Loot Unknown So i just gave him what Archmage had
			switch (Utility.Random(16))
            {
                case 0: PackItem(new BloodOathScroll()); break;
                case 1: PackItem(new CurseWeaponScroll()); break;
                case 2: PackItem(new StrangleScroll()); break;
                case 3: PackItem(new LichFormScroll()); break;
			}
            PackReg(23);
            PackItem(new Sandals());

            if (Utility.RandomDouble() < 0.75)
            {
                PackItem(new SeveredHumanEars());
            }
        }

        public BrigandCannibalMage(Serial serial)
            : base(serial)
        { 
        }

        public override bool CanRummageCorpses
        {
            get
            {
                return true;
            }
        }
        public override bool AlwaysMurderer
        {
            get
            {
                return true;
            }
        }
        public override int Meat
        {
            get
            {
                return 1;
            }
        }
        public override int TreasureMapLevel
        {
            get
            {
                return Core.AOS ? 2 : 0;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.MedScrolls, 2);
        }

        public override void Serialize(GenericWriter writer) 
        { 
            base.Serialize(writer); 
            writer.Write((int)0); 
        }

        public override void Deserialize(GenericReader reader) 
        { 
            base.Deserialize(reader); 
            int version = reader.ReadInt(); 
        }
    }
}