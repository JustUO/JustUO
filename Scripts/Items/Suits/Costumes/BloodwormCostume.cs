namespace Server.Items
{
    public class BloodwormCostume : BaseCostume
    {
        [Constructable]
        public BloodwormCostume()
        {
            Name = "a bloodWorm halloween costume";
            CostumeBody = 287;
        }

        public BloodwormCostume(Serial serial) : base(serial)
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