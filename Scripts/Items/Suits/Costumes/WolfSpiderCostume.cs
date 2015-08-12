namespace Server.Items
{
    public class WolfSpiderCostume : BaseCostume
    {
        [Constructable]
        public WolfSpiderCostume()
        {
            Name = "a wolf spider halloween costume";
            CostumeBody = 376;
        }

        public WolfSpiderCostume(Serial serial) : base(serial)
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