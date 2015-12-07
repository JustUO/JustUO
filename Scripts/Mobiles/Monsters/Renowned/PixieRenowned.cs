using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("Pixie [Renowned] corpse")]
    public class PixieRenowned : BaseRenowned
    {
        [Constructable]
        public PixieRenowned()
            : base(AIType.AI_Mage)
        {
            Name = "Pixie";
            Title = "[Renowned]";
            Body = 128;
            BaseSoundID = 0x467;

            SetStr(-350, 380);
            SetDex(450, 600);
            SetInt(700, 8500);

            SetHits(9100, 9200);
            SetStam(450, 600);
            SetMana(700, 800);

            SetDamage(9, 15);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 70, 90);
            SetResistance(ResistanceType.Fire, 60, 70);
            SetResistance(ResistanceType.Cold, 70, 80);
            SetResistance(ResistanceType.Poison, 60, 70);
            SetResistance(ResistanceType.Energy, 60, 70);

            SetSkill(SkillName.EvalInt, 100.0, 100.0);
            SetSkill(SkillName.Magery, 90.1, 110.0);
            SetSkill(SkillName.Meditation, 100.0, 100.0);
            SetSkill(SkillName.MagicResist, 110.5, 150.0);
            SetSkill(SkillName.Tactics, 100.1, 120.0);
            SetSkill(SkillName.Wrestling, 100.1, 120.0);

            Fame = 7000;
            Karma = 7000;

            VirtualArmor = 100;

            PackItem(new EssenceFeeling());

            if (0.02 > Utility.RandomDouble())
                PackStatue();
        }

        public PixieRenowned(Serial serial)
            : base(serial)
        {
        }

        public override Type[] UniqueSAList
        {
            get { return new[] {typeof (DemonHuntersStandard)}; }
        }

        public override Type[] SharedSAList
        {
            get
            {
                return new[]
                {typeof (SwordOfShatteredHopes), typeof (FairyDragonWing), typeof (FeyWings), typeof (PillarOfStrength)};
            }
        }

        public override bool InitialInnocent
        {
            get { return true; }
        }

        public override HideType HideType
        {
            get { return HideType.Spined; }
        }

        public override int Hides
        {
            get { return 5; }
        }

        public override int Meat
        {
            get { return 1; }
        }

        public override OppositionGroup OppositionGroup
        {
            get { return OppositionGroup.FeyAndUndead; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.LowScrolls);
            AddLoot(LootPack.Gems, 2);
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