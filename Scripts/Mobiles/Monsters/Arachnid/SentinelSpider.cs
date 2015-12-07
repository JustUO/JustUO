using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a sentinel spider corpse")]
    public class SentinelSpider : BaseCreature
    {
        [Constructable]
        public SentinelSpider()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a sentinel spider";
            Body = 11;
            Hue = 1663;
            BaseSoundID = 1170;

            SetStr(90, 113);
            SetDex(135, 150);
            SetInt(40, 88);

            SetHits(250, 336);

            SetDamage(15, 22);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 50);
            SetResistance(ResistanceType.Fire, 30, 40);
            SetResistance(ResistanceType.Cold, 25, 35);
            SetResistance(ResistanceType.Poison, 70, 80);
            SetResistance(ResistanceType.Energy, 30, 35);

            SetSkill(SkillName.Anatomy, 90.1, 100.0);
            SetSkill(SkillName.Poisoning, 100.1, 110.0);
            SetSkill(SkillName.MagicResist, 85.1, 90.0);
            SetSkill(SkillName.Tactics, 101.1, 110.0);
            SetSkill(SkillName.Wrestling, 105.1, 120.0);

            Fame = 18900;
            Karma = -18900;

            VirtualArmor = 36;

            PackItem(new SpidersSilk(8));
        }

        public SentinelSpider(Serial serial) : base(serial)
        {
        }

        public override bool GivesMLMinorArtifact
        {
            get { return true; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Lethal; }
        }

        public override Poison HitPoison
        {
            get { return Poison.Lethal; }
        }

        public override int TreasureMapLevel
        {
            get { return 5; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.15)
                c.DropItem(new BottleIchor());

            if (Utility.RandomDouble() < 0.05)
                c.DropItem(new SpiderCarapace());

            if (Utility.RandomDouble() < 0.025)
            {
                switch (Utility.Random(18))
                {
                    case 0:
                        c.DropItem(new AssassinChest());
                        break;
                    case 1:
                        c.DropItem(new AssassinArms());
                        break;
                    case 2:
                        c.DropItem(new DeathChest());
                        break;
                    case 3:
                        c.DropItem(new MyrmidonArms());
                        break;
                    case 4:
                        c.DropItem(new MyrmidonLegs());
                        break;
                    case 5:
                        c.DropItem(new MyrmidonGorget());
                        break;
                    case 6:
                        c.DropItem(new LeafweaveGloves());
                        break;
                    case 7:
                        c.DropItem(new LeafweaveLegs());
                        break;
                    case 8:
                        c.DropItem(new LeafweavePauldrons());
                        break;
                    case 9:
                        c.DropItem(new PaladinGloves());
                        break;
                    case 10:
                        c.DropItem(new PaladinGorget());
                        break;
                    case 11:
                        c.DropItem(new PaladinArms());
                        break;
                    case 12:
                        c.DropItem(new HunterArms());
                        break;
                    case 13:
                        c.DropItem(new HunterGloves());
                        break;
                    case 14:
                        c.DropItem(new HunterLegs());
                        break;
                    case 15:
                        c.DropItem(new HunterChest());
                        break;
                    case 16:
                        c.DropItem(new GreymistArms());
                        break;
                    case 17:
                        c.DropItem(new GreymistGloves());
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