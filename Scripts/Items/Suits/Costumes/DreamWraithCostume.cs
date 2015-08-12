namespace Server.Items
{
    public class DreamWraithCostume : BaseCostume
    {
        [Constructable]
        public DreamWraithCostume()
        {
            Name = "a dream wraith halloween Costume";
            CostumeBody = 740;
        }

        public DreamWraithCostume(Serial serial) : base(serial)
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