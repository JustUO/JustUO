using Server.Items;
using Server.Misc;

namespace Server.Mobiles
{
    [CorpseName("a glowing ratman corpse")]
    public class RatmanMage : BaseCreature
    {
        [Constructable]
        public RatmanMage()
            : base(AIType.AI_Mage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = NameList.RandomName("ratman");
            Body = 0x8F;
            BaseSoundID = 437;

            SetStr(146, 180);
            SetDex(101, 130);
            SetInt(186, 210);

            SetHits(88, 108);

            SetDamage(7, 14);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 40, 45);
            SetResistance(ResistanceType.Fire, 10, 20);
            SetResistance(ResistanceType.Cold, 10, 20);
            SetResistance(ResistanceType.Poison, 10, 20);
            SetResistance(ResistanceType.Energy, 10, 20);

            SetSkill(SkillName.EvalInt, 70.1, 80.0);
            SetSkill(SkillName.Magery, 70.1, 80.0);
            SetSkill(SkillName.MagicResist, 65.1, 90.0);
            SetSkill(SkillName.Tactics, 50.1, 75.0);
            SetSkill(SkillName.Wrestling, 50.1, 75.0);

            Fame = 7500;
            Karma = -7500;

            VirtualArmor = 44;

            PackReg(6);

            if (0.02 > Utility.RandomDouble())
                PackStatue();

            switch (Utility.Random(60))
            {
                case 0:
                    PackItem(new AnimateDeadScroll());
                    break;
                case 1:
                    PackItem(new BloodOathScroll());
                    break;
                case 2:
                    PackItem(new CorpseSkinScroll());
                    break;
                case 3:
                    PackItem(new CurseWeaponScroll());
                    break;
                case 4:
                    PackItem(new EvilOmenScroll());
                    break;
                case 5:
                    PackItem(new HorrificBeastScroll());
                    break;
                case 6:
                    PackItem(new MindRotScroll());
                    break;
                case 7:
                    PackItem(new PainSpikeScroll());
                    break;
                case 8:
                    PackItem(new WraithFormScroll());
                    break;
                case 9:
                    PackItem(new PoisonStrikeScroll());
                    break;
            }
        }

        public RatmanMage(Serial serial)
            : base(serial)
        {
        }

        public override InhumanSpeech SpeechType
        {
            get { return InhumanSpeech.Ratman; }
        }

        public override bool CanRummageCorpses
        {
            get { return true; }
        }

        public override int Meat
        {
            get { return 1; }
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
            AddLoot(LootPack.Rich);
            AddLoot(LootPack.LowScrolls);
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

            if (Body == 42)
            {
                Body = 0x8F;
                Hue = 0;
            }
        }
    }
}