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

using System;
using Server.Items;

namespace Server.Engines.Quests
{
    public class Dugan : MondainQuester
    {
        [Constructable]
        public Dugan()
            : base("Elder Dugan", "the Prospector")
        {
        }

        public Dugan(Serial serial)
            : base(serial)
        {
        }

        public override Type[] Quests
        {
            get
            {
                return new[]
                {
                    typeof (Missing)
                };
            }
        }

        public override void InitBody()
        {
            InitStats(100, 100, 25);

            Female = false;
            Race = Race.Human;
            Body = 0x190;

            Hue = 0x83EA;
            HairItemID = 0x203C;
        }

        public override void InitOutfit()
        {
            AddItem(new Backpack());
            AddItem(new Shoes());
            AddItem(new LeatherArms());
            AddItem(new LeatherChest());
            AddItem(new LeatherLegs());
            AddItem(new LeatherGloves());
            AddItem(new GnarledStaff());
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