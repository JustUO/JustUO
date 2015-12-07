using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("an orcish corpse")]
    public class OrcBrute : BaseCreature
    {
        [Constructable]
        public OrcBrute()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Body = 189;

            Name = "an orc brute";
            BaseSoundID = 0x45A;

            SetStr(767, 945);
            SetDex(66, 75);
            SetInt(46, 70);

            SetHits(476, 552);

            SetDamage(20, 25);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 40, 50);
            SetResistance(ResistanceType.Cold, 25, 35);
            SetResistance(ResistanceType.Poison, 25, 35);
            SetResistance(ResistanceType.Energy, 25, 35);

            SetSkill(SkillName.Macing, 90.1, 100.0);
            SetSkill(SkillName.MagicResist, 125.1, 140.0);
            SetSkill(SkillName.Tactics, 90.1, 100.0);
            SetSkill(SkillName.Wrestling, 90.1, 100.0);

            Fame = 15000;
            Karma = -15000;

            VirtualArmor = 50;

            Item ore = new ShadowIronOre(25);
            ore.ItemID = 0x19B9;
            PackItem(ore);
            PackItem(new IronIngot(10));

            if (0.05 > Utility.RandomDouble())
                PackItem(new OrcishKinMask());

            if (0.2 > Utility.RandomDouble())
                PackItem(new BolaBall());
        }

        public OrcBrute(Serial serial)
            : base(serial)
        {
        }

        public override bool BardImmune
        {
            get { return !Core.AOS; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Lethal; }
        }

        public override int Meat
        {
            get { return 2; }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.SavagesAndOrcs; }
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override bool AutoDispel
        {
            get { return true; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.FilthyRich);
            AddLoot(LootPack.Rich);
        }

        public override bool IsEnemy(Mobile m)
        {
            if (m.Player && m.FindItemOnLayer(Layer.Helm) is OrcishKinMask)
                return false;

            return base.IsEnemy(m);
        }

        public override void AggressiveAction(Mobile aggressor, bool criminal)
        {
            base.AggressiveAction(aggressor, criminal);

            var item = aggressor.FindItemOnLayer(Layer.Helm);

            if (item is OrcishKinMask)
            {
                AOS.Damage(aggressor, 50, 0, 100, 0, 0, 0);
                item.Delete();
                aggressor.FixedParticles(0x36BD, 20, 10, 5044, EffectLayer.Head);
                aggressor.PlaySound(0x307);
            }
        }

        public override void OnDamagedBySpell(Mobile caster)
        {
            if (caster == this)
                return;

            SpawnOrcLord(caster);
        }

        public void SpawnOrcLord(Mobile target)
        {
            var map = target.Map;

            if (map == null)
                return;

            var orcs = 0;

            foreach (var m in GetMobilesInRange(10))
            {
                if (m is OrcishLord)
                    ++orcs;
            }

            if (orcs < 10)
            {
                BaseCreature orc = new SpawnedOrcishLord();

                orc.Team = Team;

                var loc = target.Location;
                var validLocation = false;

                for (var j = 0; !validLocation && j < 10; ++j)
                {
                    var x = target.X + Utility.Random(3) - 1;
                    var y = target.Y + Utility.Random(3) - 1;
                    var z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);
                }

                orc.MoveToWorld(loc, map);

                orc.Combatant = target;
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