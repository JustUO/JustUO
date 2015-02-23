using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a fire ant corpse")]
    public class FireAnt : BaseCreature
    {
        [Constructable]
        public FireAnt()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a fire ant";
            Body = 738; 

            SetStr(201, 246);
            SetDex(103, 115);
            SetInt(16, 29);

            SetHits(254, 289);
			SetMana(16, 29);
			SetStam(103, 121);

            SetDamage(15, 18);

            SetDamageType(ResistanceType.Physical, 40);
            SetDamageType(ResistanceType.Fire, 60);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 95, 97);
            SetResistance(ResistanceType.Cold, 36, 42);
            SetResistance(ResistanceType.Poison, 37, 45);
            SetResistance(ResistanceType.Energy, 36, 44);

            SetSkill(SkillName.Anatomy, 0);
            SetSkill(SkillName.MagicResist, 46.7, 58.2);
            SetSkill(SkillName.Tactics, 71.9, 82.8);
            SetSkill(SkillName.Wrestling, 71.5, 83.4);
            QLPoints = 2;
        }

        public FireAnt(Serial serial)
            : base(serial)
        {
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
        }
        public override void OnDeath(Container c)
        {

            base.OnDeath(c);
            Region reg = Region.Find(c.GetWorldLocation(), c.Map);
            if (1.0 > Utility.RandomDouble() && reg.Name == "Crimson Veins")
            {
                if (Utility.RandomDouble() < 0.6)
                    c.DropItem(new EssencePrecision());
                
            }
            
            if (1.0 > Utility.RandomDouble() && reg.Name == "Fire Temple Ruins")
            {
                if (Utility.RandomDouble() < 0.6)
                    c.DropItem(new EssenceOrder());
            }
            if (1.0 > Utility.RandomDouble() && reg.Name == "Lava Caldera")
            {
                if (Utility.RandomDouble() < 0.6)
                    c.DropItem(new EssencePassion());
            }
        }
        public override int GetIdleSound()
        {
            return 846;
        }

        public override int GetAngerSound()
        {
            return 849;
        }

        public override int GetHurtSound()
        {
            return 852;
        }

        public override int GetDeathSound()
        {
            return 850;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write((int)0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadInt();
        }
    }
}