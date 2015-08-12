using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GreenGoblin : BaseCreature
    {
        [Constructable]
        public GreenGoblin() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Green Goblin";
            Body = 723;
            BaseSoundID = 0x600;

            SetStr(329, 329);
            SetDex(67, 67);
            SetInt(137, 137);

            SetHits(191, 191);
            SetStam(67, 67);
            SetMana(137, 137);

            SetDamage(5, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 49, 49);
            SetResistance(ResistanceType.Fire, 37, 37);
            SetResistance(ResistanceType.Cold, 35, 35);
            SetResistance(ResistanceType.Poison, 17, 17);
            SetResistance(ResistanceType.Energy, 15, 15);

            SetSkill(SkillName.MagicResist, 123.9, 123.9);
            SetSkill(SkillName.Tactics, 87.3, 87.3);
            SetSkill(SkillName.Anatomy, 81.3, 81.3);
            SetSkill(SkillName.Wrestling, 101.0, 101.0);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 28;

            QLPoints = 8;

            switch (Utility.Random(20))
            {
                case 0:
                    PackItem(new Scimitar());
                    break;
                case 1:
                    PackItem(new Katana());
                    break;
                case 2:
                    PackItem(new WarMace());
                    break;
                case 3:
                    PackItem(new WarHammer());
                    break;
                case 4:
                    PackItem(new Kryss());
                    break;
                case 5:
                    PackItem(new Pitchfork());
                    break;
            }

            PackItem(new ThighBoots());

            switch (Utility.Random(3))
            {
                case 0:
                    PackItem(new Ribs());
                    break;
                case 1:
                    PackItem(new Shaft());
                    break;
                case 2:
                    PackItem(new Candle());
                    break;
            }

            if (0.2 > Utility.RandomDouble())
                PackItem(new BolaBall());
        }

        public GreenGoblin(Serial serial) : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int TreasureMapLevel
        {
            get { return 1; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.SavagesAndOrcs; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.05)
            {
                c.DropItem(new GoblinBlood());
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}