using System;
using Server.Gumps;
using Server.Gumps.RaceGumps;

namespace Server.Items
{
    public class BarbarianGate : Item
    {
        [Constructable]
        public BarbarianGate()
            : base(0xF6C)
        {
            this.Movable = false;
            this.Hue = 33804;
            this.Light = LightType.Circle300;
        }

        public BarbarianGate(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "a barbarian race gate";
            }
        }
        public override bool OnMoveOver(Mobile m)
        {
            m.CloseGump(typeof(BarbarianGump));
            m.SendGump(new BarbarianGump(m));
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