using System;
using System.Collections.Generic;
using System.Linq;

using Server;
using Server.Gumps;
using Server.Network;
using Server.Commands;
using Server.Mobiles;

using VitaNex.SuperGumps;

namespace Server.Gumps.Compendium
{
    public class CompendiumPageGump : SuperGump
    {
        protected Mobile m_caller;

        private int m_hyperlinkId = 10;
        private Dictionary<string, int> m_hyperlinkRegistry = new Dictionary<string, int>();

        public int RegisterHyperlink(string link)
        {
            int idToReturn = m_hyperlinkId;

            if (m_hyperlinkRegistry.ContainsKey(link))
            {
                idToReturn = m_hyperlinkRegistry[link];
            }
            else
            {
                m_hyperlinkRegistry.Add(link, idToReturn);
                m_hyperlinkId++;
            }

            return idToReturn;
        }

        public CompendiumPageRenderer Renderer { get; set; }

        public CompendiumPageGump(Mobile from, CompendiumPageRenderer renderer)
            : base(from as PlayerMobile)
        {
            m_caller = from;
            Renderer = renderer;
            Disposable = true;
            Closable = true;
            Resizable = false;
            Dragable = true;
        }

        protected override void CompileLayout(SuperGumpLayout layout)
        {
            Renderer.Render(this);
        }

        public virtual void onHyperlinkClick(IGumpComponent gumpComponent, object param)
        {
            GumpButton button = gumpComponent as GumpButton;

            if (button != null)
            {
                try
                {
                  string linkName = m_hyperlinkRegistry.Where(element => element.Value == button.Param).First().Key;
                  
                  if (Compendium.g_CompendiumRenderers.ContainsKey(linkName))
                  {
                      CompendiumPageGump gump = new CompendiumPageGump(m_caller, Compendium.g_CompendiumRenderers[linkName]);
                      gump.Send();
                  }
                }
                catch (Exception e)
                {
                    Console.WriteLine("Bad CompendiumPageGump link");
                    Console.WriteLine(e);
                    this.Refresh();
                }
            }
            //lookup the hyperlink from the param and open the gump
        }

        public void onEditPageButtonClick(IGumpComponent gumpComponent, object param)
        {
            if (m_caller.AccessLevel < AccessLevel.Administrator)
            {
                return;
            }

            try
            {
                CompendiumEditorState state = new CompendiumEditorState();
                state.PageName = Renderer.Name;
                state.Caller = (PlayerMobile)m_caller;
                state.RendererToEdit = (CompendiumPageRenderer)Renderer.Clone();

                state.RendererToEdit.SelectedElement = null;
                state.EditorInstance = new CompendiumPageEditor(state);
                state.RendererToEdit.State = state;
                state.ElementListGump = new ElementListGump(state);

                state.PreviewGump = new CompendiumPreviewPageGump(state.Caller, state.RendererToEdit);

                state.EditorInstance.Send();
                state.ElementListGump.Send();
                state.PreviewGump.Send();
            }
            catch (Exception e)
            {
                Console.WriteLine("An exception was caught while trying to edit Compendium page.");
                Console.WriteLine(e);
            }
        }
    }
}