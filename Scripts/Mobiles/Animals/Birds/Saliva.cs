using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a saliva corpse")]
    public class Saliva : Harpy
    {
        [Constructable]
        public Saliva()
        {
            Name = "a saliva";
            Hue = 0x11E;

            SetStr(136, 206);
            SetDex(123, 222);
            SetInt(118, 127);

            SetHits(409, 842);

            SetDamage(19, 28);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 46, 47);
            SetResistance(ResistanceType.Fire, 32, 40);
            SetResistance(ResistanceType.Cold, 34, 49);
            SetResistance(ResistanceType.Poison, 40, 48);
            SetResistance(ResistanceType.Energy, 35, 39);

            SetSkill(SkillName.Wrestling, 106.4, 128.8);
            SetSkill(SkillName.Tactics, 129.9, 141.0);
            SetSkill(SkillName.MagicResist, 84.3, 90.1);
        }

        public Saliva(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.AosUltraRich, 2);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            c.DropItem(new SalivasFeather());

            if (Utility.RandomDouble() < 0.1)
                c.DropItem(new ParrotItem());
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}