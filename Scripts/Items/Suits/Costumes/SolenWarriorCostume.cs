namespace Server.Items
{
    public class SolenWarriorCostume : BaseCostume
    {
        [Constructable]
        public SolenWarriorCostume()
        {
            Name = "a solen warrior halloween costume";
            CostumeBody = 782;
        }

        public SolenWarriorCostume(Serial serial) : base(serial)
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