namespace Server.Items
{
    public class LadyOfTheSnowCostume : BaseCostume
    {
        [Constructable]
        public LadyOfTheSnowCostume()
        {
            Name = "a lady of the snow halloween costume";
            CostumeBody = 252;
        }

        public LadyOfTheSnowCostume(Serial serial) : base(serial)
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