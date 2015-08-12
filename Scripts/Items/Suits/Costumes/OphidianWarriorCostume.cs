namespace Server.Items
{
    public class OphidianWarriorCostume : BaseCostume
    {
        [Constructable]
        public OphidianWarriorCostume()
        {
            Name = "a ophidian warrior halloween costume";
            CostumeBody = 86;
        }

        public OphidianWarriorCostume(Serial serial) : base(serial)
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