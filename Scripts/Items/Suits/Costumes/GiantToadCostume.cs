namespace Server.Items
{
    public class GiantToadCostume : BaseCostume
    {
        [Constructable]
        public GiantToadCostume()
        {
            Name = "a giant toad halloween costume";
            CostumeBody = 80;
        }

        public GiantToadCostume(Serial serial) : base(serial)
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