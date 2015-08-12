namespace Server.Items
{
    public class ElixirofVeriteConversion : Item
    {
        [Constructable]
        public ElixirofVeriteConversion()
            : base(0x99B)
        {
            Name = "Elixir of Verite Conversion";

            Hue = 2207;
            Movable = true;
        }

        public ElixirofVeriteConversion(Serial serial)
            : base(serial)
        {
        }

        public override void OnDoubleClick(Mobile from)
        {
            var backpack = from.Backpack;

            var item1 = (CopperIngot) backpack.FindItemByType(typeof (CopperIngot));

            if (item1 != null)
            {
                BaseIngot m_Ore1 = item1;

                var toConsume = m_Ore1.Amount;

                if ((m_Ore1.Amount > 499) && (m_Ore1.Amount < 501))
                {
                    m_Ore1.Delete();
                    from.SendMessage("You've successfully converted the Metal.");
                    from.AddToBackpack(new VeriteIngot(500));
                    Delete();
                }
                else if ((m_Ore1.Amount < 500) || (m_Ore1.Amount > 500))
                {
                    from.SendMessage("You can only convert 500 Copper Ingots at a time.");
                }
            }
            else
            {
                from.SendMessage("There isn't Copper Ingots in your Backpack.");
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