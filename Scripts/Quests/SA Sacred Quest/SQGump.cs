using Server.Commands;
using Server.Network;

namespace Server.Gumps
{
    public class SQGump : Gump
    {
        public SQGump(Mobile owner)
            : base(50, 50)
        {
            //----------------------------------------------------------------------------------------------------
            AddPage(0);
            AddImageTiled(54, 33, 369, 400, 2624);
            AddAlphaRegion(54, 33, 369, 400);

            AddImageTiled(416, 39, 44, 389, 203);
            //--------------------------------------Window size bar--------------------------------------------

            AddImage(97, 49, 9005);
            AddImageTiled(58, 39, 29, 390, 10460);
            AddImageTiled(412, 37, 31, 389, 10460);
            AddLabel(140, 60, 1153, "Quest Offer");
            AddTextEntry(155, 110, 200, 20, 1163, 0, @"La Insep Ohm");
            AddTextEntry(107, 130, 200, 20, 1163, 0, @"Description");
            //AddLabel(175, 125, 200, 20, 1163, 0,"La Insep Ohm");
            //AddLabel(85, 135, 200, 20, 1163, 0, "Description");

            AddHtml(107, 155, 300, 230, "<BODY>" +
                                        //----------------------/----------------------------------------------/
                                        "<BASEFONT COLOR=WHITE>Repeating the mantra, you gradually enter a state of enlightened meditation.<br><br>As you contemplate your worthiness, an image of the Book of Circles comes into focus.<br><br>Perhaps you are ready for La Insep Om?<br>" +
                                        "</BODY>", false, true);

            AddImage(430, 9, 10441);
            AddImageTiled(40, 38, 17, 391, 9263);
            AddImage(6, 25, 10421);
            AddImage(34, 12, 10420);
            AddImageTiled(94, 25, 342, 15, 10304);
            AddImageTiled(40, 427, 415, 16, 10304);
            AddImage(-10, 314, 10402);
            AddImage(56, 150, 10411);
            AddImage(136, 84, 96);
            AddButton(315, 380, 12018, 12019, 1, GumpButtonType.Reply, 1);
            AddButton(114, 380, 12000, 12001, 0, GumpButtonType.Reply, 0);
        }

        public static void Initialize()
        {
            CommandSystem.Register("SQGump", AccessLevel.GameMaster, SQGump_OnCommand);
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            var from = state.Mobile;

            switch (info.ButtonID)
            {
                case 0:
                {
                    from.SendGump(new SQ1Gump(from));
                    from.CloseGump(typeof (SQGump));

                    break;
                }
                case 1:
                {
                    from.SendLocalizedMessage(1112683);
                    from.CloseGump(typeof (SQGump));
                    break;
                }
            }
        }

        private static void SQGump_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new SQGump(e.Mobile));
        }
    }
}