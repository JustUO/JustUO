using Server.Commands;
using Server.Network;

namespace Server.Gumps
{
    public class SQ2Gump : Gump
    {
        public SQ2Gump(Mobile owner)
            : base(50, 50)
        {
            Closable = true;
            Disposable = true;
            Dragable = true;
            Resizable = false;

            AddPage(0);
            AddBackground(65, 65, 386, 294, 9200);
            AddTextEntry(159, 81, 200, 26, 1163, 0, @"La Insep Ohm");
            AddButton(82, 304, 2152, 248, 2, GumpButtonType.Reply, 2);
            AddButton(82, 249, 2151, 248, 1, GumpButtonType.Reply, 1);
            AddButton(82, 194, 2151, 248, 0, GumpButtonType.Reply, 0);
            AddTextEntry(95, 114, 337, 19, 1422, 0, @"Amongst all else, of how many virtues does the circle");
            AddTextEntry(137, 198, 257, 20, 1422, 0, @"Seven");
            AddTextEntry(137, 252, 257, 20, 1422, 0, @"Five");
            AddTextEntry(0, 0, 200, 20, 1422, 0, @"");
            AddTextEntry(136, 307, 258, 20, 1422, 0, @"Eight");
            AddTextEntry(97, 137, 200, 20, 1422, 0, @"consist?");
        }

        public static void Initialize()
        {
            CommandSystem.Register("SQ2Gump", AccessLevel.GameMaster, SQ2Gump_OnCommand);
        }

        public override void OnResponse(NetState sender, RelayInfo info)
        {
            var from = sender.Mobile;

            switch (info.ButtonID)
            {
                case 0:
                {
                    from.SendLocalizedMessage(1112680);
                    from.CloseGump(typeof (SQ2Gump));
                    break;
                }
                case 1:
                {
                    from.SendLocalizedMessage(1112680);
                    from.CloseGump(typeof (SQ2Gump));
                    break;
                }
                case 2:
                {
                    from.SendGump(new SQ3Gump(from));
                    from.CloseGump(typeof (SQ2Gump));
                    break;
                }
            }
        }

        private static void SQ2Gump_OnCommand(CommandEventArgs e)
        {
            e.Mobile.SendGump(new SQ2Gump(e.Mobile));
        }
    }
}