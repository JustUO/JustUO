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
    public class PileofInspectedShadowIronIngots : Item
    {
        [Constructable]
        public PileofInspectedShadowIronIngots()
            : base(0x1BEA)
        {
            Hue = 2406;
        }

        public PileofInspectedShadowIronIngots(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113022; }
        } //Pile of Inspected Shadow Iron Ingots

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