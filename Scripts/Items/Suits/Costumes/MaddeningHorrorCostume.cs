namespace Server.Items
{
    public class MaddeningHorrorCostume : BaseCostume
    {
        [Constructable]
        public MaddeningHorrorCostume()
        {
            Name = "a maddening horror halloween costume";
            CostumeBody = 721;
        }

        public MaddeningHorrorCostume(Serial serial) : base(serial)
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