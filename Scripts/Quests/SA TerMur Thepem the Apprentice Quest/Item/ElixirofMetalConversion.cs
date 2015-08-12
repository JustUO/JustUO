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
    public class ElixirofMetalConversion : Item
    {
        [Constructable]
        public ElixirofMetalConversion()
            : base(0x99B)
        {
            Hue = 1159;
            Movable = true;
        }

        public ElixirofMetalConversion(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113011; }
        }

        public override void OnDoubleClick(Mobile from)
        {
            var backpack = from.Backpack;

            var item1 = (IronIngot) backpack.FindItemByType(typeof (IronIngot));

            if (item1 != null)
            {
                BaseIngot m_Ore1 = item1;

                var toConsume = m_Ore1.Amount;

                if ((m_Ore1.Amount > 499) && (m_Ore1.Amount < 501))
                {
                    m_Ore1.Delete();

                    switch (Utility.Random(4))
                    {
                        case 0:
                            from.AddToBackpack(new DullCopperIngot(500));
                            break;
                        case 2:
                            from.AddToBackpack(new ShadowIronIngot(500));
                            break;
                        case 1:
                            from.AddToBackpack(new CopperIngot(500));
                            break;
                        case 3:
                            from.AddToBackpack(new BronzeIngot(500));
                            break;
                    }

                    from.SendMessage("You've successfully converted the Metal.");
                    Delete();
                }
                else if ((m_Ore1.Amount < 500) || (m_Ore1.Amount > 500))
                {
                    from.SendMessage("You can only convert 500 Iron Ingots at a time.");
                }
            }
            else
            {
                from.SendMessage("There isn't Iron Ingots in your Backpack.");
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