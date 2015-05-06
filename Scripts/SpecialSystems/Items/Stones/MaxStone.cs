using System;

namespace Server.Items
{
    public class MaxStone : Item
    {
        [Constructable]
        public MaxStone()
            : base(0xED4)
        {
            this.Movable = false;
            this.Hue = 1160;
        }

        public MaxStone(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "an Max Skills stone";
            }
        }
        public override void OnDoubleClick(Mobile from)
        {
            from.SetAllSkills(130);
            from.Str = 130;
            from.Dex = 130;
            from.Int = 130;

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