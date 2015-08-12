namespace Server.Items
{
    public class MudPuppy : Item
    {
        [Constructable]
        public MudPuppy() : base(0x9cc)
        {
            Hue = 643;
        }

        public MudPuppy(Serial serial) : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1095117; }
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