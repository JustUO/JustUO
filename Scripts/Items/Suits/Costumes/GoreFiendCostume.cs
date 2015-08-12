namespace Server.Items
{
    public class GoreFiendCostume : BaseCostume
    {
        [Constructable]
        public GoreFiendCostume()
        {
            Name = "a gore fiend halloween costume";
            CostumeBody = 305;
        }

        public GoreFiendCostume(Serial serial) : base(serial)
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