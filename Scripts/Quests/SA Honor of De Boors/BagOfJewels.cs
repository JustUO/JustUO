namespace Server.Items
{
    public class BagOfJewels : Bag
    {
        [Constructable]
        public BagOfJewels() : this(1)
        {
            LootType = LootType.Blessed;
            Weight = 4.0;
            Movable = false;
            Hue = 53;
        }

        [Constructable]
        public BagOfJewels(int amount)
        {
            DropItem(new Diamond(10));
            DropItem(new Ruby(10));
            DropItem(new Emerald(10));
        }

        public BagOfJewels(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1075307; // Bag of Jewels
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}