using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a sentinel spider corpse")]
    public class SentinelSpider : BaseCreature
    {
        [Constructable]
        public SentinelSpider()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            this.Name = "a Sentinel spider";
            this.Body = 11;
            this.Hue = 0x8FF;
            this.BaseSoundID = 1170;

            this.SetStr(95, 100);
            this.SetDex(142, 145);
            this.SetInt(43, 45);

            this.SetHits(264, 265);
            this.SetStam(142, 145);
            this.SetMana(43, 45);

            this.SetDamage(15, 22);

            this.SetDamageType(ResistanceType.Physical, 100);

            this.SetResistance(ResistanceType.Physical, 48);
            this.SetResistance(ResistanceType.Fire, 32);
            this.SetResistance(ResistanceType.Cold, 33);
            this.SetResistance(ResistanceType.Poison, 71);
            this.SetResistance(ResistanceType.Energy, 30);

            this.SetSkill(SkillName.Wrestling, 119.7);
            this.SetSkill(SkillName.Tactics, 102.9);
            this.SetSkill(SkillName.MagicResist, 88.5);
            this.SetSkill(SkillName.Poisoning, 101.0);
			
            this.PackItem(new SpidersSilk(8));

            this.Fame = 18900;
            this.Karma = -18900;
        }

        public SentinelSpider(Serial serial)
            : base(serial)
        {
        }

        public override bool GivesMLMinorArtifact
        {
            get
            {
                return true;
            }
        }
        public override Poison PoisonImmune
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override Poison HitPoison
        {
            get
            {
                return Poison.Lethal;
            }
        }
        public override void GenerateLoot()
        {
            this.AddLoot(LootPack.AosUltraRich, 4);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);		
			
            if (Utility.RandomDouble() < 0.025)
            {
                switch ( Utility.Random(2) )
                {
                    case 0:
                        c.DropItem(new LuckyCoin());
                        break;
                    case 1:
                        c.DropItem(new SpiderCarapace());
                        break;
                }
            }
				
            if (Utility.RandomDouble() < 0.15)
                c.DropItem(new BottleIchor());
        }

        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.ArmorIgnore;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
			
            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
			
            int version = reader.ReadInt();
        }
    }
}