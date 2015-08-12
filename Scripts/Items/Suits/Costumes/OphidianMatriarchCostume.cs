namespace Server.Items
{
    public class OphidianMatriarchCostume : BaseCostume
    {
        [Constructable]
        public OphidianMatriarchCostume()
        {
            Name = "an ophidian matriarch halloween costume";
            CostumeBody = 87;
        }

        public OphidianMatriarchCostume(Serial serial) : base(serial)
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