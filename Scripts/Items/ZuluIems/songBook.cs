using Server.Gumps.Zulugumps;
using System;

namespace Server.Items
{
    public class songBook : Item
    {
        [Constructable]
        public songBook()
            : base(7187) //0xFF2
        {
            this.Movable = true;
            this.Hue = 1165;
            this.LootType = LootType.Blessed;
        }

        public songBook(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "Book of Songs";
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