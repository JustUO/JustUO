using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GrayGoblinKeeper : BaseCreature
    {
        [Constructable]
        public GrayGoblinKeeper() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a gray goblin keeper";
            Body = 334;
            BaseSoundID = 0x600;

            SetStr(289, 330);
            SetDex(80, 80);
            SetInt(102, 109);

            SetHits(168, 195);
            SetStam(80, 80);
            SetMana(102, 109);

            SetDamage(5, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 44, 48);
            SetResistance(ResistanceType.Fire, 35, 40);
            SetResistance(ResistanceType.Cold, 30, 35);
            SetResistance(ResistanceType.Poison, 10, 15);
            SetResistance(ResistanceType.Energy, 15, 20);

            SetSkill(SkillName.MagicResist, 129.0, 130.0);
            SetSkill(SkillName.Tactics, 83.0, 85.0);
            SetSkill(SkillName.Anatomy, 82.0, 84.8);
            SetSkill(SkillName.Wrestling, 104.0, 106.0);

            //PackItem(new SaTrinketBag());


            Fame = 15000;
            Karma = -15000;

            VirtualArmor = 28;

            QLPoints = 10;
        }

        public GrayGoblinKeeper(Serial serial) : base(serial)
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