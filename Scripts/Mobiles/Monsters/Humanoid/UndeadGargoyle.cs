/* Based on Gargoyle, still no infos on Undead Gargoyle... Have to get also the correct body ID */

using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an undead gargoyle corpse")]
    public class UndeadGargoyle : BaseCreature
    {
        [Constructable]
        public UndeadGargoyle()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an Undead Gargoyle";
            Body = 722;
            BaseSoundID = 372;

            SetStr(250, 350);
            SetDex(120, 140);
            SetInt(250, 350);

            SetHits(200, 300);

            SetDamage(15, 27);

            SetDamageType(ResistanceType.Physical, 10);
            SetDamageType(ResistanceType.Cold, 50);
            SetDamageType(ResistanceType.Energy, 40);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 40, 55);
            SetResistance(ResistanceType.Poison, 55, 65);
            SetResistance(ResistanceType.Energy, 40, 50);

            SetSkill(SkillName.EvalInt, 90.1, 110.0);
            SetSkill(SkillName.Magery, 120);
            SetSkill(SkillName.MagicResist, 100.1, 120.0);
            SetSkill(SkillName.Tactics, 60.1, 70.0);
            SetSkill(SkillName.Wrestling, 60.1, 70.0);

            Fame = 3500;
            Karma = -3500;

            VirtualArmor = 32;

            if (0.025 > Utility.RandomDouble())
                PackItem(new GargoylesPickaxe());
        }

        public UndeadGargoyle(Serial serial)
            : base(serial)
        {
        }

        public override int TreasureMapLevel
        {
            get { return 1; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.25)

                switch (Utility.Random(2))
                {
                    case 0:
                        c.DropItem(new UndeadGargMedallion());
                        break;
                    case 1:
                        c.DropItem(new UndeadGargHorn());
                        break;
                }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.MedScrolls);
            AddLoot(LootPack.Gems, Utility.RandomMinMax(1, 4));
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