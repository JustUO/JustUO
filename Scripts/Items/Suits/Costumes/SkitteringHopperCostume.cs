namespace Server.Items
{
    public class SkitteringHopperCostume : BaseCostume
    {
        [Constructable]
        public SkitteringHopperCostume()
        {
            Name = "a skittering hopper halloween costume";
            CostumeBody = 302;
        }

        public SkitteringHopperCostume(Serial serial) : base(serial)
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