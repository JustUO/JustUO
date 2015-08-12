namespace Server.Items
{
    public class MinotaurCostume : BaseCostume
    {
        [Constructable]
        public MinotaurCostume()
        {
            Name = "a minotaur halloween costume";
            CostumeBody = 263;
        }

        public MinotaurCostume(Serial serial) : base(serial)
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