using System;
using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Mobiles;

using VitaNex.SuperGumps;

namespace Server.Gumps.Compendium
{
    public class CompendiumPreviewPageGump : CompendiumPageGump
    {
        public CompendiumPreviewPageGump(Mobile from, CompendiumPageRenderer renderer)
            : base(from, renderer)
        {
            Disposable = false;
            Closable = false;
            Resizable = false;
        }

        protected override void CompileLayout(SuperGumpLayout layout)
        {
          Renderer.Render(this);
        }
    }
}