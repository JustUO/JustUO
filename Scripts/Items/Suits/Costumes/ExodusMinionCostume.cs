namespace Server.Items
{
    public class ExodusMinionCostume : BaseCostume
    {
        [Constructable]
        public ExodusMinionCostume()
        {
            Name = "a exodus minion halloween costume";
            CostumeBody = 757;
        }

        public ExodusMinionCostume(Serial serial) : base(serial)
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