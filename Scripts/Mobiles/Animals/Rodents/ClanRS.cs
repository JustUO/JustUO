using Server.Items;
using Server.Services;

namespace Server.Mobiles
{
    [CorpseName("a clan ribbon supplicant corpse")]
    public class ClanRS : BaseCreature
    {
        //public override InhumanSpeech SpeechType{ get{ return InhumanSpeech.Ratman; } }
        [Constructable]
        public ClanRS()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Clan Ribbon Supplicant";
            Body = 42;
            Hue = 2952;
            BaseSoundID = 437;

            SetStr(173);
            SetDex(117);
            SetInt(207);

            SetHits(127);

            SetDamage(7, 14);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 55, 60);
            SetResistance(ResistanceType.Fire, 30, 35);
            SetResistance(ResistanceType.Cold, 80, 85);
            SetResistance(ResistanceType.Poison, 45, 50);
            SetResistance(ResistanceType.Energy, 25, 30);

            SetSkill(SkillName.MagicResist, 78.5, 80.0);
            SetSkill(SkillName.Tactics, 62.1, 65.0);
            SetSkill(SkillName.Wrestling, 56.5, 60.0);

            Fame = 1500;
            Karma = -1500;

            VirtualArmor = 48;
        }

        public ClanRS(Serial serial)
            : base(serial)
        {
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int Hides
        {
            get { return 8; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Rich, 3);
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