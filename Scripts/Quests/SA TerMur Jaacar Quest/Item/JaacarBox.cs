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

using Reward = Server.Engines.Quests.BaseReward;

namespace Server.Items
{
    public class JaacarBox : WoodenBox
    {
        [Constructable]
        public JaacarBox()
        {
            Movable = true;
            Hue = 1266;

            DropItem(Reward.CookRecipe());
        }

        public JaacarBox(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get { return "Jaacar Reward Box"; }
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