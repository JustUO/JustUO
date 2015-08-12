namespace Server.Items
{
    public class FireElementalCostume : BaseCostume
    {
        [Constructable]
        public FireElementalCostume()
        {
            Name = "a fire elemental halloween costume";
            CostumeBody = 15;
        }

        public FireElementalCostume(Serial serial) : base(serial)
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