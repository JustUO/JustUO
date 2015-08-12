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

using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Neville : BaseEscort
    {
        [Constructable]
        public Neville()
        {
            Name = "Neville Brightwhistle";
        }

        public Neville(Serial serial)
            : base(serial)
        {
        }

        public override bool InitialInnocent
        {
            get { return true; }
        }

        public override bool IsInvulnerable
        {
            get { return true; }
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (EscortToDugan)
                };
            }
        }

        public override bool CanBeDamaged()
        {
            return false;
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            Race = Race.Human;

            Hue = 0x8412;
            HairItemID = 0x2047;
            HairHue = 0x465;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Shoes(0x1BB));
            AddItem(new LongPants(0x901));
            AddItem(new Tunic(0x70A));
            AddItem(new Cloak(0x675));
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