using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a goblin corpse")]
    public class GreenGoblinScout : BaseCreature
    {
        [Constructable]
        public GreenGoblinScout()
            : base(AIType.AI_OrcScout, FightMode.Closest, 10, 7, 0.2, 0.4)
        {
            Name = "an green goblin scout";
            Body = 723;
            BaseSoundID = 0x600;

            SetStr(261, 293);
            SetDex(62, 75);
            SetInt(107, 124);

            SetHits(167, 204);
            SetMana(107, 124);

            SetDamage(5, 7);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 47);
            SetResistance(ResistanceType.Fire, 30, 38);
            SetResistance(ResistanceType.Cold, 28, 31);
            SetResistance(ResistanceType.Poison, 13, 18);
            SetResistance(ResistanceType.Energy, 10, 15);

            SetSkill(SkillName.MagicResist, 92.8, 94.0);
            SetSkill(SkillName.Tactics, 82.1, 89.1);
            SetSkill(SkillName.Wrestling, 106.6, 113.1);
            SetSkill(SkillName.Anatomy, 80.3, 89.9);

            Fame = 15000;
            Karma = -15000;

            QLPoints = 10;

            PackItem(new Arrow(Utility.RandomMinMax(60, 70)));
            PackItem(new Bandage(Utility.RandomMinMax(1, 15)));

            if (0.1 > Utility.RandomDouble())
                AddItem(new OrcishBow());
            else
                AddItem(new Bow());
        }

        public GreenGoblinScout(Serial serial)
            : base(serial)
        {
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.SavagesAndOrcs; }
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
        }

        private Mobile FindTarget()
        {
            foreach (var m in GetMobilesInRange(10))
            {
                if (m.Player && m.Hidden && m.AccessLevel == AccessLevel.Player)
                {
                    return m;
                }
            }

            return null;
        }

        private void TryToDetectHidden()
        {
            var m = FindTarget();

            if (m != null)
            {
                if (Core.TickCount >= NextSkillTime && UseSkill(SkillName.DetectHidden))
                {
                    var targ = Target;

                    if (targ != null)
                        targ.Invoke(this, this);

                    Effects.PlaySound(Location, Map, 0x340);
                }
            }
        }

        public override void OnThink()
        {
            if (Utility.RandomDouble() < 0.2)
                TryToDetectHidden();
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