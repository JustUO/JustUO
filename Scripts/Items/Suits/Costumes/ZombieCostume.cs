namespace Server.Items
{
    public class ZombieCostume : BaseCostume
    {
        [Constructable]
        public ZombieCostume()
        {
            Name = "a zombie costume";
            CostumeBody = 3;
        }

        public ZombieCostume(Serial serial) : base(serial)
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