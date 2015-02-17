using System;

namespace Server.Mobiles
{
    [CorpseName("a slith corpse")]
    public class ToxicSlith : BaseCreature
    {
        [Constructable]
        public ToxicSlith()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a toxic slith";
            Body = 734; 

            SetStr(219, 330);
            SetDex(46, 65);
            SetInt(25, 38);

            SetHits(182, 209);
            SetStam(230, 279);
			SetMana(0, 3);

            SetDamage(6, 24);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 36, 44);
            SetResistance(ResistanceType.Fire, 6, 10);
            SetResistance(ResistanceType.Cold, 6, 10);
            SetResistance(ResistanceType.Poison, 100, 100);
            SetResistance(ResistanceType.Energy, 6, 10);

            SetSkill(SkillName.MagicResist, 95.4, 98.9);
            SetSkill(SkillName.Tactics, 84.3, 91.5);
            SetSkill(SkillName.Wrestling, 89.3, 97.9);

            Tamable = false;
            ControlSlots = 1;
            MinTameSkill = 80.7;

            QLPoints = 30;
        }

        public ToxicSlith(Serial serial)
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
        public override int DragonBlood{ get{ return 6; } }
        public override int Hides
        {
            get
            {
                return 11;
            }
        }
        public override void GenerateLoot()
        {
            //PackItem(Gold(UtilityRandom(400, 500);
            //PackItem(ToxicVenomSac);
            //PackItem(SlithTongue);
            //PackItem(PotteryFragment);
            //PackItem(TatteredScroll);
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