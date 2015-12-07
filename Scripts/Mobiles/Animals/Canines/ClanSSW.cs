//Yrenwick Dragon Ultima IX pack, MiniQuest System & IceHound.cs created by Yrenwick Dragon (G. Younk)
//From the Ultima: Britannia shard  http://www.mac512.com/ultima/britannia/
//This script was created on 6/02/04

using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a clan scratch savage wolf corpse")]
    public class ClanSSW : BaseCreature
    {
        [Constructable]
        public ClanSSW()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Clan Scratch Savage Wolf";
            Body = 98;
            Hue = 0x2C;
            BaseSoundID = 229;

            SetStr(170);
            SetDex(244);
            SetInt(57);

            SetHits(65);

            SetDamage(8, 10);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Cold, 80);

            SetResistance(ResistanceType.Physical, 30, 35);
            SetResistance(ResistanceType.Cold, 40, 45);
            SetResistance(ResistanceType.Poison, 25, 30);
            SetResistance(ResistanceType.Energy, 20, 25);

            SetSkill(SkillName.Swords, 99.0, 100.0);
            SetSkill(SkillName.MagicResist, 41.5, 42.5);
            SetSkill(SkillName.Tactics, 65.1, 70.0);
            SetSkill(SkillName.Wrestling, 42.3, 45.5);

            Fame = 3400;
            Karma = -3400;

            VirtualArmor = 50;
        }

        public ClanSSW(Serial serial)
            : base(serial)
        {
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override FoodType FavoriteFood
        {
            get { return FoodType.Meat; }
        }

        public override PackInstinct PackInstinct
        {
            get { return PackInstinct.Canine; }
        }

        public override WeaponAbility GetWeaponAbility()
        {
            return WeaponAbility.ParalyzingBlow;
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Average);
            AddLoot(LootPack.Meager);
        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            SARegionDrops.GetSADrop(c);
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