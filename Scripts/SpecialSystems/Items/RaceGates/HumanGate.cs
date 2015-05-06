using System;
using Server.Gumps;
using Server.Gumps.RaceGumps;

namespace Server.Items
{
    public class HumanGate : Item
    {
        [Constructable]
        public HumanGate()
            : base(0xF6C)
        {
            this.Movable = false;
            this.Hue = 0;
            this.Light = LightType.Circle300;
        }

        public HumanGate(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "a human race gate";
            }
        }
        public override bool OnMoveOver(Mobile m)
        {
            m.CloseGump(typeof(HumanGump));
            m.SendGump(new HumanGump(m));
            return false;
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