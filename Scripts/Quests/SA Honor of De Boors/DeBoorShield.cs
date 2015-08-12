namespace Server.Items
{
    public class DeBoorShield : Item
    {
        [Constructable]
        public DeBoorShield()
            : base(0x1B74)
        {
            LootType = LootType.Blessed;
            Weight = 7.0;
            Movable = false;
        }

        public DeBoorShield(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1075308; // Ancestral Shield
            }
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();

            if (Weight == 5.0)
                Weight = 7.0;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); //version
        }
    }
}