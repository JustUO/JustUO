namespace Server.Items
{
    public class GazerCostume : BaseCostume
    {
        [Constructable]
        public GazerCostume()
        {
            Name = "a gazer halloween costume";
            CostumeBody = 22;
        }

        public GazerCostume(Serial serial) : base(serial)
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