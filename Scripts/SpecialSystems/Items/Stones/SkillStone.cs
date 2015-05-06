using Server.Gumps.Zulugumps;
using System;

/*
 *  Author Oscar Ternström
 */

namespace Server.Items
{
    public class SkillStone : Item
    {

        const int NUMBER_OF_BOOSTS = 3;

        [Constructable]
        public SkillStone()
            : base(0xED4)
        {
            this.Movable = false;
            this.Hue = 1160;
        }

        public SkillStone(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "a skills stone";
            }
        }
        public override void OnDoubleClick(Mobile from)
        {

            if (from.statBoost == false) { 
            from.CloseGump(typeof(statBoostGump));
            from.SendGump(new statBoostGump(from));
            }
            else if (from.skillBoost != NUMBER_OF_BOOSTS)
            {
                from.CloseGump(typeof(skillBoostGump));
                from.SendGump(new skillBoostGump(from));
            }
            else if (from.skillBoost == NUMBER_OF_BOOSTS)
            {
                from.SendMessage("You have already boosted your skills!");
            }
            else if (from.statBoost == true)
            {
                from.SendMessage("You have already boosted your stat!");
            }
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.Write((int)0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadInt();
        }
    }
}