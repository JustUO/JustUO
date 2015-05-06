using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps.RaceGumps
{

    public class BarbarianGump : Gump
    {
        private readonly Mobile m_Healer;
        private readonly int m_Price;
        private readonly bool m_FromSacrifice;
        private readonly double m_HitsScalar;
        private readonly ResurrectMessage m_Msg;

        public BarbarianGump(Mobile owner)
            : this(owner, owner, ResurrectMessage.Generic, false)
        {
        }


        public BarbarianGump(Mobile owner, Mobile healer, ResurrectMessage msg, bool fromSacrifice)
            : this(owner, healer, msg, fromSacrifice, 0.0)
        {
        }

        public BarbarianGump(Mobile owner, Mobile healer, ResurrectMessage msg, bool fromSacrifice, double hitsScalar)
            : base(100, 0)
        {
            this.m_Healer = healer;
            this.m_FromSacrifice = fromSacrifice;
            this.m_HitsScalar = hitsScalar;

            this.m_Msg = msg;

            this.AddPage(0);

            this.AddBackground(0, 0, 400, 350, 2600);

            //   this.AddHtml(0, 20, 400, 35, "Dark-Elf race", false, false);

            this.AddHtml(50, 55, 300, 140, "Enter the world with the Barbarian race?", true, true);

            this.AddButton(200, 227, 4005, 4007, 0, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(235, 230, 110, 35, 1011012, false, false); // CANCEL

            this.AddButton(65, 227, 4005, 4007, 1, GumpButtonType.Reply, 0);
            this.AddHtmlLocalized(100, 230, 110, 35, 1011011, false, false); // CONTINUE
            owner.Frozen = true;
        }


        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            from.CloseGump(typeof(BarbarianGump));
            from.Frozen = false;
            if (info.ButtonID == 1 || info.ButtonID == 2)
            {
                from.MoveToWorld(new Point3D(1475, 1645, 20), Map.Trammel); // move player to Britain center
                from.Hue = 33804;

                //from.Name = from.Name + " [Dark-Elf]";  // not sure about this one, its kinda ugly
                from.Frozen = false;
            }
        }
    }
}