namespace Server.Items
{
    public class WispCostume : BaseCostume
    {
        [Constructable]
        public WispCostume()
        {
            Name = "a wisp halloween costume";
            CostumeBody = 58;
        }

        public WispCostume(Serial serial) : base(serial)
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