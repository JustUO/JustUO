using System;
using Server.Gumps;
using Server.Gumps.RaceGumps;

namespace Server.Items
{
    public class GoblinGate : Item
    {
        [Constructable]
        public GoblinGate()
            : base(0xF6C)
        {
            this.Movable = false;
            this.Hue = 34186;
            this.Light = LightType.Circle300;
        }

        public GoblinGate(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "a goblin race gate";
            }
        }
        public override bool OnMoveOver(Mobile m)
        {
            m.CloseGump(typeof(GoblinGump));
            m.SendGump(new GoblinGump(m));
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