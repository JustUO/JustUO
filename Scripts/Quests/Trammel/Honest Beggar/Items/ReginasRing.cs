namespace Server.Items
{
    public class ReginasRing : SilverRing
    {
        [Constructable]
        public ReginasRing()
        {
            LootType = LootType.Blessed;
            Weight = 1;
        }

        public ReginasRing(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1075305; }
        } // Regina's Ring

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