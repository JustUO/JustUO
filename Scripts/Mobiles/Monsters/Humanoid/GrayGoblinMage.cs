using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GrayGoblinMage : BaseCreature
    {
        [Constructable]
        public GrayGoblinMage() : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a gray goblin mage";
            Body = 334;
            BaseSoundID = 0x600;

            SetStr(122, 152);
            SetDex(73, 90);
            SetInt(452, 481);

            SetHits(122, 152);
            SetStam(73, 90);
            SetMana(452, 481);

            SetDamage(5, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 24, 30);
            SetResistance(ResistanceType.Fire, 35, 44);
            SetResistance(ResistanceType.Cold, 32, 40);
            SetResistance(ResistanceType.Poison, 36, 45);
            SetResistance(ResistanceType.Energy, 10, 19);

            SetSkill(SkillName.MagicResist, 142.0, 147.0);
            SetSkill(SkillName.Tactics, 80.4, 89.0);
            SetSkill(SkillName.Anatomy, 81.3, 86.8);
            SetSkill(SkillName.Wrestling, 97.0, 108.0);
            SetSkill(SkillName.Magery, 104.5, 117.0);
            SetSkill(SkillName.EvalInt, 94.0, 105.0);
            SetSkill(SkillName.Meditation, 92.3, 96.0);

            Fame = 15000;
            Karma = -15000;

            VirtualArmor = 30;

            QLPoints = 10;

            PackReg(6);
        }

        public GrayGoblinMage(Serial serial) : base(serial)
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