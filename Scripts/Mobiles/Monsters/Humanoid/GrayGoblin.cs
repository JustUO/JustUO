using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GrayGoblin : BaseCreature
    {
        [Constructable]
        public GrayGoblin() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a gray goblin";
            Body = 334;
            BaseSoundID = 0x600;

            SetStr(257, 300);
            SetDex(63, 67);
            SetInt(117, 150);

            SetHits(173, 199);
            SetStam(63, 67);
            SetMana(117, 150);

            SetDamage(7, 14);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 45);
            SetResistance(ResistanceType.Fire, 35, 40);
            SetResistance(ResistanceType.Cold, 25, 30);
            SetResistance(ResistanceType.Poison, 10, 15);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.MagicResist, 121.0, 130.0);
            SetSkill(SkillName.Tactics, 80.0, 90.0);
            SetSkill(SkillName.Anatomy, 80.0, 90.0);
            SetSkill(SkillName.Wrestling, 95.0, 100.0);

            Fame = 10000;
            Karma = -10000;

            VirtualArmor = 28;

            QLPoints = 8;
        }

        public GrayGoblin(Serial serial) : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int TreasureMapLevel
        {
            get { return 4; }
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
            AddLoot(LootPack.Rich);
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