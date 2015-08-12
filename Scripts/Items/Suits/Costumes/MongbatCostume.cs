namespace Server.Items
{
    public class MongbatCostume : BaseCostume
    {
        [Constructable]
        public MongbatCostume()
        {
            Name = "a Mongbat halloween Costume";
            CostumeBody = 39;
        }

        public MongbatCostume(Serial serial) : base(serial)
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