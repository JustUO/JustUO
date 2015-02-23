using System;

namespace Server.Mobiles
{
    [CorpseName("a slith corpse")]
    public class Slith : BaseCreature
    {
        [Constructable]
        public Slith()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a slith";
            Body = 734; 

            SetStr(133, 146);
            SetDex(59, 67);
            SetInt(12, 20);

            SetHits(84, 94);

            SetDamage(6, 24);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 35, 45);
            SetResistance(ResistanceType.Fire, 30, 45);
            SetResistance(ResistanceType.Poison, 25, 35);
            SetResistance(ResistanceType.Energy, 25, 35);

            SetSkill(SkillName.MagicResist, 59.2, 67.9);
            SetSkill(SkillName.Tactics, 66.8, 78.4);
            SetSkill(SkillName.Wrestling, 63.1, 77.6);

            Tamable = true;
            ControlSlots = 1;
            MinTameSkill = 80.7;

            QLPoints = 15;
        }

        public Slith(Serial serial)
            : base(serial)
        {
        }

        public override bool HasBreath
        {
            get
            {
                return true;
            }
        }// fire breath enabled
        public override int Meat
        {
            get
            {
                return 6;
            }
        }
        public override int Hides
        {
            get
            {
                return 10;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
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