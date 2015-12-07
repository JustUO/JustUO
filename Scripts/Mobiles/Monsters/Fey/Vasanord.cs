using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a plant corpse")]
    public class Vasanord : BaseCreature
    {
        [Constructable]
        public Vasanord()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.6, 1.2)
        {
            Name = "Vasanord";
            Body = 780;
            Hue = 2071;

            SetStr(805, 869);
            SetDex(51, 64);
            SetInt(38, 48);

            SetHits(2553, 2626);
            SetMana(110);
            SetStam(51, 64);

            SetDamage(10, 23);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Fire, 20);
            SetDamageType(ResistanceType.Cold, 20);
            SetDamageType(ResistanceType.Poison, 20);
            SetDamageType(ResistanceType.Energy, 20);

            SetResistance(ResistanceType.Physical, 30, 50);
            SetResistance(ResistanceType.Fire, 20, 40);
            SetResistance(ResistanceType.Cold, 20, 50);
            SetResistance(ResistanceType.Poison, 100, 120);
            SetResistance(ResistanceType.Energy, 20, 50);

            SetSkill(SkillName.MagicResist, 72.8, 77.7);
            SetSkill(SkillName.Tactics, 50.7, 99.6);
            SetSkill(SkillName.Anatomy, 6.5, 17.1);
            SetSkill(SkillName.EvalInt, 92.5, 106.2);
            SetSkill(SkillName.Magery, 95.5, 106.9);
            SetSkill(SkillName.Wrestling, 93.6, 98.6);

            Fame = 8000;
            Karma = -8000;

            VirtualArmor = 88;

            PackItem(new DaemonBone(30));
            PackItem(new Board(10));
        }

        public Vasanord(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysMurderer
        {
            get { return true; }
        }

        public override bool BardImmune
        {
            get { return true; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Lethal; }
        }

        public override bool Unprovokable
        {
            get { return true; }
        }

        public override bool ReacquireOnMovement
        {
            get { return true; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich, 3);
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.MedScrolls, 2);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.6)
                c.DropItem(new TaintedSeeds(2));

            if (Utility.RandomDouble() < 0.5)
                c.DropItem(new VoidEssence(2));

            if (Utility.RandomDouble() < 0.10)
            {
                switch (Utility.Random(2))
                {
                    case 0:
                        AddToBackpack(new VoidOrb());
                        break;
                    case 1:
                        AddToBackpack(new VoidCore());
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