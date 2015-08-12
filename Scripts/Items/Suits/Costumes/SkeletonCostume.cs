namespace Server.Items
{
    public class SkeletonCostume : BaseCostume
    {
        [Constructable]
        public SkeletonCostume()
        {
            Name = "a skeleton halloween costume";
            CostumeBody = 50;
        }

        public SkeletonCostume(Serial serial) : base(serial)
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