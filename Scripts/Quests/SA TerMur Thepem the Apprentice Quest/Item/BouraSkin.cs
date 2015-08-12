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
    public class BouraSkin : Item
    {
        [Constructable]
        public BouraSkin()
            : base(0x11F4)
        {
            Name = "Boura Skin";
            LootType = LootType.Blessed;
            Weight = 1.0;
            Hue = 0x292;
        }

        public BouraSkin(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1113024; }
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