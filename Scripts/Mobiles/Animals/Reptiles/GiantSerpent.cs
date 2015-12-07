using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a giant serpent corpse")]
    [TypeAlias("Server.Mobiles.Serpant")]
    public class GiantSerpent : BaseCreature
    {
        [Constructable]
        public GiantSerpent()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a giant serpent";
            Body = 0x15;
            Hue = Utility.RandomSnakeHue();
            BaseSoundID = 219;

            SetStr(186, 215);
            SetDex(56, 80);
            SetInt(66, 85);

            SetHits(112, 129);
            SetMana(0);

            SetDamage(7, 17);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Poison, 60);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 70, 90);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.Poisoning, 70.1, 100.0);
            SetSkill(SkillName.MagicResist, 25.1, 40.0);
            SetSkill(SkillName.Tactics, 65.1, 70.0);
            SetSkill(SkillName.Wrestling, 60.1, 80.0);

            Fame = 2500;
            Karma = -2500;

            VirtualArmor = 32;

            PackItem(new Bone());
            // TODO: Body parts
        }

        public GiantSerpent(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Greater; }
        }

        public override Poison HitPoison
        {
            get { return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly); }
        }

        public override bool DeathAdderCharmable
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 4; }
        }

        public override int Hides
        {
            get { return 15; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
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

            if (BaseSoundID == -1)
                BaseSoundID = 219;
        }
    }

    [CorpseName("a giant serpent corpse")]
    [TypeAlias("Server.Mobiles.Serpant")]
    public class GiantSerpent1 : BaseCreature
    {
        [Constructable]
        public GiantSerpent1()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a giant serpent";
            Body = 0x15;
            Hue = Utility.RandomSnakeHue();
            BaseSoundID = 219;

            SetStr(186, 215);
            SetDex(56, 80);
            SetInt(66, 85);

            SetHits(112, 129);
            SetMana(0);

            SetDamage(7, 17);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Poison, 60);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 70, 90);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.Poisoning, 70.1, 100.0);
            SetSkill(SkillName.MagicResist, 25.1, 40.0);
            SetSkill(SkillName.Tactics, 65.1, 70.0);
            SetSkill(SkillName.Wrestling, 60.1, 80.0);

            Fame = 2500;
            Karma = -2500;

            VirtualArmor = 32;

            PackItem(new Bone());
            // TODO: Body parts
        }

        public GiantSerpent1(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Greater; }
        }

        public override Poison HitPoison
        {
            get { return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly); }
        }

        public override bool DeathAdderCharmable
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 4; }
        }

        public override int Hides
        {
            get { return 15; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.10)
            {
                c.DropItem(new RareSerpentEgg1());
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

            if (BaseSoundID == -1)
                BaseSoundID = 219;
        }
    }

    [CorpseName("a giant serpent corpse")]
    [TypeAlias("Server.Mobiles.Serpant")]
    public class GiantSerpent2 : BaseCreature
    {
        [Constructable]
        public GiantSerpent2()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a giant serpent";
            Body = 0x15;
            Hue = Utility.RandomSnakeHue();
            BaseSoundID = 219;

            SetStr(186, 215);
            SetDex(56, 80);
            SetInt(66, 85);

            SetHits(112, 129);
            SetMana(0);

            SetDamage(7, 17);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Poison, 60);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 70, 90);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.Poisoning, 70.1, 100.0);
            SetSkill(SkillName.MagicResist, 25.1, 40.0);
            SetSkill(SkillName.Tactics, 65.1, 70.0);
            SetSkill(SkillName.Wrestling, 60.1, 80.0);

            Fame = 2500;
            Karma = -2500;

            VirtualArmor = 32;

            PackItem(new Bone());
            // TODO: Body parts
        }

        public GiantSerpent2(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Greater; }
        }

        public override Poison HitPoison
        {
            get { return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly); }
        }

        public override bool DeathAdderCharmable
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 4; }
        }

        public override int Hides
        {
            get { return 15; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.10)
            {
                c.DropItem(new RareSerpentEgg2());
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

            if (BaseSoundID == -1)
                BaseSoundID = 219;
        }
    }

    [CorpseName("a giant serpent corpse")]
    [TypeAlias("Server.Mobiles.Serpant")]
    public class GiantSerpent3 : BaseCreature
    {
        [Constructable]
        public GiantSerpent3()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a giant serpent";
            Body = 0x15;
            Hue = Utility.RandomSnakeHue();
            BaseSoundID = 219;

            SetStr(186, 215);
            SetDex(56, 80);
            SetInt(66, 85);

            SetHits(112, 129);
            SetMana(0);

            SetDamage(7, 17);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Poison, 60);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 70, 90);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.Poisoning, 70.1, 100.0);
            SetSkill(SkillName.MagicResist, 25.1, 40.0);
            SetSkill(SkillName.Tactics, 65.1, 70.0);
            SetSkill(SkillName.Wrestling, 60.1, 80.0);

            Fame = 2500;
            Karma = -2500;

            VirtualArmor = 32;

            PackItem(new Bone());
            // TODO: Body parts
        }

        public GiantSerpent3(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Greater; }
        }

        public override Poison HitPoison
        {
            get { return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly); }
        }

        public override bool DeathAdderCharmable
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 4; }
        }

        public override int Hides
        {
            get { return 15; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.10)
            {
                c.DropItem(new RareSerpentEgg3());
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

            if (BaseSoundID == -1)
                BaseSoundID = 219;
        }
    }

    [CorpseName("a giant serpent corpse")]
    [TypeAlias("Server.Mobiles.Serpant")]
    public class GiantSerpent4 : BaseCreature
    {
        [Constructable]
        public GiantSerpent4()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a giant serpent";
            Body = 0x15;
            Hue = Utility.RandomSnakeHue();
            BaseSoundID = 219;

            SetStr(186, 215);
            SetDex(56, 80);
            SetInt(66, 85);

            SetHits(112, 129);
            SetMana(0);

            SetDamage(7, 17);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Poison, 60);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Fire, 5, 10);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 70, 90);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.Poisoning, 70.1, 100.0);
            SetSkill(SkillName.MagicResist, 25.1, 40.0);
            SetSkill(SkillName.Tactics, 65.1, 70.0);
            SetSkill(SkillName.Wrestling, 60.1, 80.0);

            Fame = 2500;
            Karma = -2500;

            VirtualArmor = 32;

            PackItem(new Bone());
            // TODO: Body parts
        }

        public GiantSerpent4(Serial serial)
            : base(serial)
        {
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Greater; }
        }

        public override Poison HitPoison
        {
            get { return (0.8 >= Utility.RandomDouble() ? Poison.Greater : Poison.Deadly); }
        }

        public override bool DeathAdderCharmable
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 4; }
        }

        public override int Hides
        {
            get { return 15; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.10)
            {
                c.DropItem(new RareSerpentEgg4());
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

            if (BaseSoundID == -1)
                BaseSoundID = 219;
        }
    }
}