namespace Server.Items
{
    public class SatyrCostume : BaseCostume
    {
        [Constructable]
        public SatyrCostume()
        {
            Name = "an satyr halloween costume";
            CostumeBody = 271;
        }

        public SatyrCostume(Serial serial) : base(serial)
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