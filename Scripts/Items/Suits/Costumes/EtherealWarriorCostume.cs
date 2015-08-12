namespace Server.Items
{
    public class EtherealWarriorCostume : BaseCostume
    {
        [Constructable]
        public EtherealWarriorCostume()
        {
            Name = "an ethereal warrior halloween costume";
            CostumeBody = 123;
        }

        public EtherealWarriorCostume(Serial serial) : base(serial)
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