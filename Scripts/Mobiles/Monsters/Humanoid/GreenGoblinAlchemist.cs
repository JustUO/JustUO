using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GreenGoblinAlchemist : BaseCreature
    {
        [Constructable]
        public GreenGoblinAlchemist() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Green Goblin Alchemist";
            Body = 723;
            BaseSoundID = 0x600;

            SetStr(275, 420);
            SetDex(75, 80);
            SetInt(117, 118);

            SetHits(192, 365);
            SetStam(75, 80);
            SetMana(117, 118);

            SetDamage(5, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 47);
            SetResistance(ResistanceType.Fire, 45, 47);
            SetResistance(ResistanceType.Cold, 36, 38);
            SetResistance(ResistanceType.Poison, 37, 43);
            SetResistance(ResistanceType.Energy, 16, 20);

            SetSkill(SkillName.MagicResist, 124.3, 129.0);
            SetSkill(SkillName.Tactics, 80.1, 85.5);
            SetSkill(SkillName.Anatomy, 0.0, 0.0);
            SetSkill(SkillName.Wrestling, 104.0, 107.4);

            Fame = 15000;
            Karma = -15000;

            VirtualArmor = 28;

            QLPoints = 10;
        }

        public GreenGoblinAlchemist(Serial serial) : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int TreasureMapLevel
        {
            get { return 5; }
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
            AddLoot(LootPack.UltraRich);
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