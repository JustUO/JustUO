using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;

/*
 *  Author Oscar Ternström
 */

namespace Server.Gumps.Zulugumps
{

    public class ebookgump : Gump
    {

        public ebookgump(Mobile owner)
            : this(owner, ResurrectMessage.Generic, false)
        {
        }


        public ebookgump(Mobile owner, ResurrectMessage msg, bool fromSacrifice)
            : this(owner, msg)
        {
        }


        public ebookgump(Mobile owner, ResurrectMessage msg)
            : base(100, 0)
        {

            this.Closable = true;
            this.Disposable = false;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddImageTiled(77, 124, 417, 247, 500);
           
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

        }
    }
}