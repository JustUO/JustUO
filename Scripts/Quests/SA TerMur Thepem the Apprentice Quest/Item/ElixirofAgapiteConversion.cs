/*                                                             .---.
/  .  \
|\_/|   |
|   |  /|
.----------------------------------------------------------------' |
/  .-.                                                              |
|  /   \         Contribute To The Orbsydia SA Project               |
| |\_.  |                                                            |
|\|  | /|                        By Lotar84                          |
| `---' |                                                            |
|       |       (Orbanised by Orb SA Core Development Team)          | 
|       |                                                           /
|       |----------------------------------------------------------'
\       |
\     /
`---'
*/

namespace Server.Items
{
    public class ElixirofAgapiteConversion : Item
    {
        [Constructable]
        public ElixirofAgapiteConversion()
            : base(0x99B)
        {
            Hue = 2425;
            Movable = true;
        }

        public ElixirofAgapiteConversion(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113008; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            var backpack = from.Backpack;

            var item1 = (ShadowIronIngot) backpack.FindItemByType(typeof (ShadowIronIngot));

            if (item1 != null)
            {
                BaseIngot m_Ore1 = item1;

                var toConsume = m_Ore1.Amount;

                if ((m_Ore1.Amount > 499) && (m_Ore1.Amount < 501))
                {
                    m_Ore1.Delete();
                    from.SendMessage("You've successfully converted the Metal.");
                    from.AddToBackpack(new AgapiteIngot(500));
                    Delete();
                }
                else if ((m_Ore1.Amount < 500) || (m_Ore1.Amount > 500))
                {
                    from.SendMessage("You can only convert 500 ShadowIron Ingots at a time.");
                }
            }
            else
            {
                from.SendMessage("There isn't ShadowIron Ingots in your Backpack.");
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