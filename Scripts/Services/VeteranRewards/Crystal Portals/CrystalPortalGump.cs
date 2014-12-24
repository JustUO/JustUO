/*
[cliloc(s) 1113946 - 1113950]

This crystal portal allows you to teleport directly to a bank or a moongate.<br><br>

For Trammel ruleset, say the city's name followed by "mint" (e.g. "minoc mint"). For a moongate, say the gate's name and "moongate" (eg. "minoc moongate").<br><br>

For Felucca, say "fel" then same rules as above. So "fel minoc mint" or "fel minoc moongate".
CITY NAMES:<br>
britain, bucs, cove, delucia, haven, jhelom, magincia, minoc, moonglow, nujelm, ocllo, papua, serpent, skara, trinsic, vesper, wind, yew, luna, umbra, zento, termur, ilshenar
MOONGATE NAMES<br>
moonglow, britain, jhelom, yew, minoc, trinsic, skara, magincia, haven, bucs, vesper, compassion, honesty, honor, humility, justice, sacrifice, spirituality, valor, chaos, luna, umbra, isamu, makoto, homare, termur<br><br><br>


The same teleportation rules apply regarding criminal flagging, weight, etc.
*/


using System; 
using Server; 
using Server.Gumps; 
using Server.Network;
using Server.Items;
using Server.Mobiles;
using Server.Multis;
using Server.Regions;

namespace Server.Gumps
{
    public class CrystalPortalGump : Gump
    {
        private Mobile m_From;

        public CrystalPortalGump( Mobile from ) : base( 25,25 )
        {
			m_From = from;
			m_From.CloseGump( typeof( CrystalPortalGump ) );
		
            this.Closable=true;
			this.Disposable=false;
			this.Dragable=true;
			this.Resizable=false;

			AddPage(0);
			AddBackground(0, 2, 373, 515, 9200);
			AddHtml( 14, 15, 346, 484, @"<br><br>This crystal portal allows you to teleport directly to a bank or a moongate.<br><br>For Trammel ruleset, say the city's name followed by 'mint' (e.g. 'minoc mint'). For a moongate, say the gate's name and 'moongate' (eg. 'minoc moongate').<br><br>For Felucca, say 'fel' then same rules as above. So 'fel minoc mint' or 'fel minoc moongate'.<br><br>CITY NAMES:<br>britain, bucs, cove, delucia, haven, jhelom, magincia, minoc, moonglow, nujelm, papua, serpent, skara, trinsic, vesper, wind, luna, umbra, zento, ilshenar<br><br>FEL CITY NAMES:<br>britain, bucs, cove, ocllo, jhelom, magincia, minoc, moonglow, nujelm, serpent, skara, trinsic, vesper, wind<br><br>MOONGATE NAMES<br>moonglow, britain, jhelom, yew, minoc, trinsic, skara, magincia, haven, bucs, vesper, compassion, honesty, honor, humility, justice, sacrifice, spirituality, valor, chaos, luna, umbra, isamu, makoto, homare, termur<br><br>FEL MOONGATE NAMES<br>britain, bucs, jhelom, magincia, minoc, moonglow, skara, trinsic, vesper<br><br>The same teleportation rules apply regarding criminal flagging, weight, etc.<br><br>", (bool)true, (bool)true); 
        }

        

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            Mobile from = sender.Mobile;

            switch(info.ButtonID)
            {
                				case 0:
				{

					break;
				}

            }
        }
    }
}