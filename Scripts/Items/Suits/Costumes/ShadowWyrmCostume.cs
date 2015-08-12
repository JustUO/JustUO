namespace Server.Items
{
    public class ShadowWyrmCostume : BaseCostume
    {
        [Constructable]
        public ShadowWyrmCostume()
        {
            Name = "a shadow wyrm halloween Costume";
            CostumeBody = 106;
        }

        public ShadowWyrmCostume(Serial serial) : base(serial)
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