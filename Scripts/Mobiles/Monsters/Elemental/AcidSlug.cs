using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("an acid slug corpse")]
    public class AcidSlug : BaseCreature
    {
        [Constructable]
        public AcidSlug()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "an acid slug";
            Body = 51;
            Hue = 44;

            SetStr(213, 294);
            SetDex(80, 82);
            SetInt(18, 22);

            SetHits(333, 370);

            SetDamage(21, 28);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 10, 15);
            SetResistance(ResistanceType.Fire, 0);
            SetResistance(ResistanceType.Cold, 10, 15);
            SetResistance(ResistanceType.Poison, 60, 70);
            SetResistance(ResistanceType.Energy, 10, 15);

            SetSkill(SkillName.MagicResist, 25.0);
            SetSkill(SkillName.Tactics, 30.0, 50.0);
            SetSkill(SkillName.Wrestling, 30.0, 80.0);
        }

        public AcidSlug(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            SARegionDrops.GetSADrop(c);

            if (Utility.RandomDouble() < 0.25)
            {
                c.DropItem(new VialOfVitriol());
            }

            if (Utility.RandomDouble() < 0.25)
            {
                c.DropItem(new CongealedSlugAcid());
            }

            if (Utility.RandomDouble() < 0.75)
            {
                c.DropItem(new AcidSac());
            }
        }

        public override int GetIdleSound()
        {
            return 1499;
        }

        public override int GetAngerSound()
        {
            return 1496;
        }

        public override int GetHurtSound()
        {
            return 1498;
        }

        public override int GetDeathSound()
        {
            return 1497;
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