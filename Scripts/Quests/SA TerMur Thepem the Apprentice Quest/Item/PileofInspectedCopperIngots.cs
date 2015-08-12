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
    public class PileofInspectedCopperIngots : Item
    {
        [Constructable]
        public PileofInspectedCopperIngots()
            : base(0x1BEA)
        {
            Hue = 2413;
        }

        public PileofInspectedCopperIngots(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113023; }
        } //Pile of Inspected Copper Ingots

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