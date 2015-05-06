using System;
using Server.Gumps;
using Server.Gumps.RaceGumps;

namespace Server.Items
{
    public class DwarfGate : Item
    {
        [Constructable]
        public DwarfGate()
            : base(0xF6C)
        {
            this.Movable = false;
            this.Hue = 33888;
            
            //this.Light = LightType.Circle300;
        }

        public DwarfGate(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "a dwarf race gate";
            }
        }
        public override bool OnMoveOver(Mobile m)
        {
            m.CloseGump(typeof(DwarfGump));
            m.SendGump(new DwarfGump(m));
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