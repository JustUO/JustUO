namespace Server.Items
{
    public class RedHerring : Item
    {
        [Constructable]
        public RedHerring() : base(0x9cc)
        {
            Hue = 337;
        }

        public RedHerring(Serial serial) : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1095046; }
        } // Britain Crown Fish

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