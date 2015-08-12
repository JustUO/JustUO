namespace Server.Items
{
    public class ChronicleGargoyleQueenVolI : Item
    {
        [Constructable]
        public ChronicleGargoyleQueenVolI()
            : base(0xFF2)
        {
            Weight = 1.0;
        }

        public ChronicleGargoyleQueenVolI(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1150914; }
        } //Chronicle of the Gargoyle Queen Vol. I

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