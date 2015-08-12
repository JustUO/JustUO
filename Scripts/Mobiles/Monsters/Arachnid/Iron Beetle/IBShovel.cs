using Server.Engines.Harvest;

namespace Server.Items
{
    public class IBShovel : BaseHarvestTool
    {
        [Constructable]
        public IBShovel() : this(1000)
        {
        }

        [Constructable]
        public IBShovel(int uses) : base(uses, 0xF39)
        {
            Name = "Iron Beetle Mining Shovel";
            Weight = 5.0;
            Hue = 1150;
        }

        public IBShovel(Serial serial) : base(serial)
        {
        }

        public override HarvestSystem HarvestSystem
        {
            get { return Mining.System; }
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