using Server.Items;
using Server.Network;

namespace Server.Gumps
{
    public class ClockworkStartGump : Gump
    {
        private readonly ClockworkMechanism m_Item;

        public ClockworkStartGump(ClockworkMechanism item)
            : base(200, 200)
        {
            m_Item = item;

            Resizable = false;

            AddPage(0);
            AddBackground(0, 0, 297, 115, 9200);

            AddImageTiled(5, 10, 285, 25, 2624);
            AddHtmlLocalized(10, 15, 275, 25, 1112855, 0x7FFF, false, false);

            AddImageTiled(5, 40, 285, 40, 2624);
            AddHtmlLocalized(10, 40, 275, 40, 1112856, 0x7FFF, false, false);

            AddButton(5, 85, 4017, 4018, 0, GumpButtonType.Reply, 0);
            AddHtmlLocalized(40, 87, 80, 25, 1011012, 0x7FFF, false, false);

            AddButton(215, 85, 4023, 4024, 1, GumpButtonType.Reply, 0);
            AddHtmlLocalized(250, 87, 80, 25, 1006044, 0x7FFF, false, false);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            if (0 != info.ButtonID && null != m_Item && !m_Item.Deleted)
                m_Item.BeginMechanismAssembly(state.Mobile);
        }
    }
}