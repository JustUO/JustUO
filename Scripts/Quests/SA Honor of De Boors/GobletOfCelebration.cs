namespace Server.Items
{
    public class GobletOfCelebration : BaseBeverage
    {
        [Constructable]
        public GobletOfCelebration()
        {
            LootType = LootType.Blessed;
            Weight = 1.0;
            Hue = 1158;
        }

        [Constructable]
        public GobletOfCelebration(BeverageType type) : base(type)
        {
            LootType = LootType.Blessed;
            Weight = 1.0;
            Hue = 1158;
        }

        public GobletOfCelebration(Serial serial) : base(serial)
        {
        }

        public override int MaxQuantity
        {
            get { return 5; }
        }

        public override int LabelNumber
        {
            get { return 1075430; }
        } // Goblet of Celebration
        //1075272 = You drink from the goblet of celebration
        public override int ComputeItemID()
        {
            if (ItemID == 0x99A || ItemID == 0x9B3 || ItemID == 0x9BF || ItemID == 0x9CB)
                return ItemID;

            return 0x99A;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write(1); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadInt();
        }
    }
}