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

    public class statBoostGump : Gump
    {

        // stat increase
        const int STAT_INC = 100;

        public statBoostGump(Mobile owner)
            : this(owner, ResurrectMessage.Generic, false)
        {
        }


        public statBoostGump(Mobile owner, ResurrectMessage msg, bool fromSacrifice)
            : this(owner, msg)
        {
        }

        public enum Buttons
        {
            Checkbox1,
            Checkbox2,
            Checkbox3,
            Button1,
        }

        public statBoostGump(Mobile owner, ResurrectMessage msg)
            : base(100, 0)
        {


            this.Closable = false;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;
            this.AddPage(0);
            this.AddBackground(100, 51, 197, 198, 9200);
            this.AddLabel(162, 63, 0, @"Boost stat");
            this.AddLabel(110, 100, 0, @"Strength");
            this.AddLabel(110, 135, 0, @"Dexterity");
            this.AddLabel(110, 170, 0, @"Intellect");
            this.AddCheck(260, 100, 210, 211, false, (int)Buttons.Checkbox1);
            this.AddCheck(260, 135, 210, 211, false, (int)Buttons.Checkbox2);
            this.AddCheck(260, 170, 210, 211, false, (int)Buttons.Checkbox3);
            this.AddButton(167, 207, 247, 248, (int)Buttons.Button1, GumpButtonType.Reply, 0);

            owner.Frozen = true;
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;
           
            if (info.IsSwitched((int)Buttons.Checkbox1) == true && info.IsSwitched((int)Buttons.Checkbox2) == false && info.IsSwitched((int)Buttons.Checkbox3) == false && from.statBoost == false) // str
            {
                from.Str = STAT_INC;
                from.statBoost = true;
                from.Frozen = false;
            }
            else if (info.IsSwitched((int)Buttons.Checkbox2) == true && info.IsSwitched((int)Buttons.Checkbox1) == false && info.IsSwitched((int)Buttons.Checkbox3) == false && from.statBoost == false) // dex
            {
                from.Dex = STAT_INC;
                from.statBoost = true;
                from.Frozen = false;
            }
            else if (info.IsSwitched((int)Buttons.Checkbox3) == true && info.IsSwitched((int)Buttons.Checkbox2) == false && info.IsSwitched((int)Buttons.Checkbox1) == false && from.statBoost == false) // int
            {
                from.Int = STAT_INC;
                from.statBoost = true;
                from.Frozen = false;
            }

            from.SendMessage("Please select only one stat");
            
          
                from.Frozen = false;
          
        }
    }
}