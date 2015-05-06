using Server.Gumps.Zulugumps;
using System;

namespace Server.Items
{
    public class NecroBook : Item
    {
        [Constructable]
        public NecroBook()
            : base(0x1C13) //0xFF2
        {
            this.Movable = true;
            this.Hue = 0x66D;
            this.LootType = LootType.Blessed;
          //  this.        
        }

        public NecroBook(Serial serial)
            : base(serial)
        {
        }

        public override string DefaultName
        {
            get
            {
                return "Codex Damnorum";
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