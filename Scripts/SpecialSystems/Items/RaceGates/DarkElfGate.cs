using System;
using Server.Gumps;
using Server.Gumps.RaceGumps;

namespace Server.Items
{
    public class DarkElfGate : Item
    {
        [Constructable]
        public DarkElfGate()
            : base(0xF6C)
        {
            this.Movable = false;
            this.Hue = 33877;
            this.Light = LightType.Circle300;
        }

        public DarkElfGate(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "a dark-elf race gate";
            }
        }
        public override bool OnMoveOver(Mobile m)
        {
            m.CloseGump(typeof(DarkElfGump));
            m.SendGump(new DarkElfGump(m));
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