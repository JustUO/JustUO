/*                                                             .---.
/  .  \
|\_/|   |
|   |  /|
.----------------------------------------------------------------' |
/  .-.                                                              |
|  /   \            Contribute To The Orbsydia SA Project            |
| |\_.  |                                                            |
|\|  | /|                        By Lotar84                          |
| `---' |                                                            |
|       |         (Orbanised by Orb SA Core Development Team)        | 
|       |                                                           /
|       |----------------------------------------------------------'
\       |
\     /
`---'

*/

namespace Server.Items
{
    public class CandlewoodTorch : BaseShield
    {
        [Constructable]
        public CandlewoodTorch()
            : base(0xF6B)
        {
            Attributes.SpellChanneling = 1;
            Attributes.CastSpeed = -1;
        }

        public CandlewoodTorch(Serial serial)
            : base(serial)
        {
        }

        public override int LabelNumber
        {
            get { return 1094957; }
        } //Candlewood Torch

        public override void OnDoubleClick(Mobile from)
        {
            if (!IsChildOf(from.Backpack))
            {
                from.SendLocalizedMessage(1042001); // That must be in your pack for you to use it.
            }
            else
            {
                if (ItemID == 0xF6B)
                {
                    ItemID = 0xA12;
                }
                else if (ItemID == 0xA12)
                {
                    ItemID = 0xF6B;
                }
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            var version = reader.ReadEncodedInt();
        }
    }
}