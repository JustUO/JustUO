using Server.Gumps.Zulugumps;
using System;

namespace Server.Items
{
    public class HolyBook : Item
    {
        [Constructable]
        public HolyBook()
            : base(0xFF2) //0xFF2
        {
            this.Movable = true;
            this.Hue = 0x49E;
            this.LootType = LootType.Blessed;
        }

        public HolyBook(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "Codex Caelestis";
            }
        }
        public override void OnDoubleClick(Mobile from)
        {

            from.CloseGump(typeof(ebookgump));
            from.SendGump(new ebookgump(from));
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