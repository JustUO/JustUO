using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a water elemental corpse")]
    public class WaterElemental : BaseCreature
    {
        private Boolean m_HasDecanter = true;

        [Constructable]
        public WaterElemental()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a water elemental";
            Body = 16;
            BaseSoundID = 278;

            SetStr(126, 155);
            SetDex(66, 85);
            SetInt(101, 125);

            SetHits(76, 93);

            SetDamage(7, 9);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 10, 25);
            SetResistance(ResistanceType.Cold, 10, 25);
            SetResistance(ResistanceType.Poison, 60, 70);
            SetResistance(ResistanceType.Energy, 5, 10);

            SetSkill(SkillName.EvalInt, 60.1, 75.0);
            SetSkill(SkillName.Magery, 60.1, 75.0);
            SetSkill(SkillName.MagicResist, 100.1, 115.0);
            SetSkill(SkillName.Tactics, 50.1, 70.0);
            SetSkill(SkillName.Wrestling, 50.1, 70.0);

            Fame = 4500;
            Karma = -4500;

            VirtualArmor = 40;
            ControlSlots = 3;
            CanSwim = true;

            PackItem(new BlackPearl(3));
        }

        public WaterElemental(Serial serial)
            : base(serial)
        {
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public Boolean HasDecanter
        {
            get { return m_HasDecanter; }
            set { m_HasDecanter = value; }
        }

        public override double DispelDifficulty
        {
            get { return 117.5; }
        }

        public override double DispelFocus
        {
            get { return 45.0; }
        }

        public override bool BleedImmune
        {
            get { return true; }
        }

        public override int TreasureMapLevel
        {
            get { return 2; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
            AddLoot(LootPack.Potions);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.05)
            {
                c.DropItem(new AncientPotteryFragments());
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(1);

            writer.Write(m_HasDecanter);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();

            switch (version)
            {
                case 0:
                    break;
                case 1:
                    m_HasDecanter = reader.ReadBool();
                    break;
            }
        }
    }
}