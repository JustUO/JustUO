//By: Monolith - 10/10/2011

using Server.Mobiles;

namespace Server.Items
{
    public class UnderworldTele : Teleporter
    {
        [Constructable]
        public UnderworldTele()
        {
        }

        public UnderworldTele(Serial serial)
            : base(serial)
        {
        }

        public override bool OnMoveOver(Mobile m)
        {
            if (m is PlayerMobile)
            {
                var player = (PlayerMobile) m;

                if (player.AbyssEntry)
                {
                    m.SendMessage("You Enter the Stygian Abyss");
                    return base.OnMoveOver(m);
                }
                m.SendMessage("You have not obtained entry to the Abyss");
            }
            return true;
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0); // version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}