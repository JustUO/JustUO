using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a rotworm corpse")]
    public class RotWorm : BaseCreature
    {
        [Constructable]
        public RotWorm() : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a rotworm";
            Body = 732;

            SetStr(244);
            SetDex(80);
            SetInt(17);

            SetHits(215);

            SetDamage(1, 5);

            SetDamageType(ResistanceType.Physical, 100);
            //SetDamageType( ResistanceType.Poison, 40 );

            SetResistance(ResistanceType.Physical, 37);
            SetResistance(ResistanceType.Fire, 30);
            SetResistance(ResistanceType.Cold, 35);
            SetResistance(ResistanceType.Poison, 73);
            SetResistance(ResistanceType.Energy, 26);

            SetSkill(SkillName.MagicResist, 25.0);
            SetSkill(SkillName.Tactics, 25.0);
            SetSkill(SkillName.Wrestling, 50.0);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 16; //guess

            QLPoints = 10;

            //PackItem(new RawRotWormMeat(2));
        }

        public RotWorm(Serial serial) : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);
            var reg = Region.Find(c.GetWorldLocation(), c.Map);
            if (0.4 > Utility.RandomDouble() && reg.Name == "Ariel Writ Disaster")
                c.DropItem(new ArielHavenWritofMembership());
            c.DropItem(new BonePile());
        }

        public override int GetIdleSound()
        {
            return 1503;
        }

        public override int GetAngerSound()
        {
            return 1500;
        }

        public override int GetHurtSound()
        {
            return 1502;
        }

        public override int GetDeathSound()
        {
            return 1501;
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