namespace Server.Items
{
    public class LetterOfIntroduction : Item
    {
        [Constructable]
        public LetterOfIntroduction()
            : base(0xEC0)
        {
            LootType = LootType.Blessed;
            Weight = 1.0;
        }

        public LetterOfIntroduction(Serial serial) : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113243; }
        } // Laifem's Letter of Introduction

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }

    public class MasteringWeaving : Item
    {
        [Constructable]
        public MasteringWeaving()
            : base(0x0FF0)
        {
            LootType = LootType.Blessed;
            Weight = 1.0;
        }

        public MasteringWeaving(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113244; }
        } // Mastering the Art of Weaving

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}