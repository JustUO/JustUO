namespace Server.Items
{
    public class OniCostume : BaseCostume
    {
        [Constructable]
        public OniCostume()
        {
            Name = "an oni halloween costume";
            CostumeBody = 241;
        }

        public OniCostume(Serial serial) : base(serial)
        {
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