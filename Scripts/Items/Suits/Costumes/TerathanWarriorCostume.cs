namespace Server.Items
{
    public class TerathanWarriorCostume : BaseCostume
    {
        [Constructable]
        public TerathanWarriorCostume()
        {
            Name = "a terathan warrior halloween costume";
            CostumeBody = 70;
        }

        public TerathanWarriorCostume(Serial serial) : base(serial)
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