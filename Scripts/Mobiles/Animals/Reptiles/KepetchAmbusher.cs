using System;

namespace Server.Mobiles
{
    [CorpseName("a kepetch corpse")]
    public class KepetchAmbusher : BaseCreature
    {
        [Constructable]
        public KepetchAmbusher()
            : base(AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4)
        {
            Name = "a kepetch ambusher";
            Body = 726;

            SetStr(451, 455);
            SetDex(258, 267);
            SetInt(43, 47);

            SetHits(511, 543);
			SetStam(258, 267);
			SetMana(43, 47);

            SetDamage(7, 17);

            SetDamageType(ResistanceType.Physical, 80);
            SetDamageType(ResistanceType.Poison, 20);

            SetResistance(ResistanceType.Physical, 60, 75);
            SetResistance(ResistanceType.Fire, 50, 60);
            SetResistance(ResistanceType.Cold, 45, 55);
            SetResistance(ResistanceType.Poison, 50, 70);
            SetResistance(ResistanceType.Energy, 55, 75);

            SetSkill(SkillName.Anatomy, 110.2, 112.4);
            SetSkill(SkillName.MagicResist, 95.8, 97.5);
            SetSkill(SkillName.Tactics, 107.9, 112.4);
            SetSkill(SkillName.Wrestling, 103.7, 109.3);

            QLPoints = 25;
        }

        public KepetchAmbusher(Serial serial)
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
                return 14;
            }
        }
        public override HideType HideType
        {
            get
            {
                return HideType.Horned;
            }
        }
        // add fur drop
        public override FoodType FavoriteFood
        {
            get
            {
                return FoodType.FruitsAndVegies | FoodType.GrainsAndHay;
            }
        }
        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average, 2);
        }

        public override int GetIdleSound()
        {
            return 1545;
        }

        public override int GetAngerSound()
        {
            return 1542;
        }

        public override int GetHurtSound()
        {
            return 1544;
        }

        public override int GetDeathSound()
        {
            return 1543;
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