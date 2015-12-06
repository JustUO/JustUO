using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a slith corpse")]
    public class StoneSlith : BaseCreature
    {
        [Constructable]
        public StoneSlith()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a stone slith";
            Body = 734; 

            SetStr(250, 300);
            SetDex(76, 94);
            SetInt(56, 80);

            SetHits(157, 168);
            SetStam(76, 94);
            SetMana(45, 80);

            SetDamage(6, 24);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 30, 40);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 88.9, 96.7);
            SetSkill(SkillName.Tactics, 84.5, 97.2);
            SetSkill(SkillName.Wrestling, 76.3, 96.4);
            SetSkill(SkillName.Anatomy, 0.0);

            Tamable = true;
            ControlSlots = 2;
            MinTameSkill = 65.1;

            QLPoints = 20;
        }

        public StoneSlith(Serial serial)
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
                return 1;
            }
        }
        public override int DragonBlood{ get{ return 6; } }
        public override int Hides
        {
            get
            {
                return 12;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Spined;
            }
        }
        public override void GenerateLoot()
        {
            //PackItem(Gold(UtilityRandom(100, 200);
            //PackItem(SlithTongue);
            //PackItem(PotteryFragment);
            AddLoot(LootPack.Average, 2);
        }

        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.BleedAttack;
            //return WeaponAbility.LowerPhysicalResist;
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