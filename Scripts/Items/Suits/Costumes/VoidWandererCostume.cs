namespace Server.Items
{
    public class VoidWandererCostume : BaseCostume
    {
        [Constructable]
        public VoidWandererCostume()
        {
            Name = "a wanderer of the void halloween costume";
            CostumeBody = 316;
        }

        public VoidWandererCostume(Serial serial) : base(serial)
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