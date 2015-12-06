using System;

namespace Server.Mobiles
{
    [CorpseName("a raptor corpse")]
    public class Raptor : BaseCreature
    {
        [Constructable]
        public Raptor()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a raptor";
            Body = 730; 

            SetStr(401, 464);
            SetDex(131, 154);
            SetInt(102, 150);

            SetHits(342, 400);
			SetMana(102, 150);
			SetStam(131, 154);

            SetDamage(11, 17);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 50);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 40, 50);
            SetResistance(ResistanceType.Poison, 20, 30);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 75.5, 89.0);
            SetSkill(SkillName.Tactics, 80.3, 93.8);
            SetSkill(SkillName.Wrestling, 66.9, 81.5);

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 107.1;

            QLPoints = 20;
        }

        public Raptor(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get
            {
                return 7;
            }
        }
        public override int Hides
        {
            get
            {
                return 11;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 2);
        }

        public override int GetIdleSound()
        {
            return 1573;
        }

        public override int GetAngerSound()
        {
            return 1570;
        }

        public override int GetHurtSound()
        {
            return 1572;
        }

        public override int GetDeathSound()
        {
            return 1571;
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