namespace Server.Items
{
    public class PixieCostume : BaseCostume
    {
        [Constructable]
        public PixieCostume()
        {
            Name = "a pixie halloween costume";
            CostumeBody = 128;
        }

        public PixieCostume(Serial serial) : base(serial)
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