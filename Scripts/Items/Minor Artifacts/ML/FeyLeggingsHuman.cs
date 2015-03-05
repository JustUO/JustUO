namespace Server.Items
{
    public class FeyLeggingsHuman : FeyLeggings
    {
        [Constructable]
        public FeyLeggingsHuman()
        {
            
        }

        public override Race RequiredRace
        {
            get
            {
                return Race.Human;
            }
        }
        public FeyLeggingsHuman(Serial serial)
            : base(serial)
        {
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.WriteEncodedInt(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            int version = reader.ReadEncodedInt();
        }
    }
}