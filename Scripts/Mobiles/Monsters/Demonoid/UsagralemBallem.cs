using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an usagralem ballem corpse")]
    public class UsagralemBallem : BaseCreature
    {
        [Constructable]
        public UsagralemBallem()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an Usagrallem Ballem";
            Hue = 2071;
            Body = 318;
            BaseSoundID = 0x165;

            SetStr(500);
            SetDex(100);
            SetInt(1000);

            SetHits(30000);
            SetMana(5000);

            SetDamage(17, 21);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Fire, 20);
            SetDamageType(ResistanceType.Cold, 20);
            SetDamageType(ResistanceType.Poison, 20);
            SetDamageType(ResistanceType.Energy, 20);

            SetResistance(ResistanceType.Physical, 30);
            SetResistance(ResistanceType.Fire, 30);
            SetResistance(ResistanceType.Cold, 30);
            SetResistance(ResistanceType.Poison, 30);
            SetResistance(ResistanceType.Energy, 30);

            SetSkill(SkillName.DetectHidden, 80.0);
            SetSkill(SkillName.EvalInt, 100.0);
            SetSkill(SkillName.Magery, 100.0);
            SetSkill(SkillName.Meditation, 120.0);
            SetSkill(SkillName.MagicResist, 150.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 120.0);

            Fame = 28000;
            Karma = -28000;

            VirtualArmor = 64;
        }

        public UsagralemBallem(Serial serial)
            : base(serial)
        {
        }

        public override bool IgnoreYoungProtection
        {
            get { return Core.ML; }
        }

        public override bool AlwaysMurderer
        {
            get { return true; }
        }

        public override bool BardImmune
        {
            get { return !Core.SE; }
        }

        public override bool Unprovokable
        {
            get { return Core.SE; }
        }

        public override bool AreaPeaceImmune
        {
            get { return Core.SE; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Lethal; }
        }

        public override WeaponAbility GetWeaponAbility()
        {
            switch (Utility.Random(3))
            {
                default:
                case 0:
                    return WeaponAbility.DoubleStrike;
                case 1:
                    return WeaponAbility.WhirlwindAttack;
                case 2:
                    return WeaponAbility.CrushingBlow;
            }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.SuperBoss, 2);
            AddLoot(LootPack.HighScrolls, Utility.RandomMinMax(6, 10));
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.3)
                c.DropItem(new VoidOrb());

            if (Utility.RandomDouble() < 0.30)
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        AddToBackpack(new VoidEssence());
                        break;
                    case 1:
                        AddToBackpack(new AncientPotteryFragments());
                        break;
                }
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