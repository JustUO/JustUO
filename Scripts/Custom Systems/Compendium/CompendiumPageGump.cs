#region References
using System;
using System.Collections.Generic;
using System.Linq;

using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace Server.Gumps.Compendium
{
	public class CompendiumPageGump : SuperGump
	{
		protected Mobile m_caller;

		private int m_hyperlinkId = 10;
		private readonly Dictionary<string, int> m_hyperlinkRegistry = new Dictionary<string, int>();

		public int RegisterHyperlink(string link)
		{
			var idToReturn = m_hyperlinkId;

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

		public virtual void onHyperlinkClick(GumpButton button, int param)
		{
			if (button != null)
			{
				try
				{
					var linkName = m_hyperlinkRegistry.First(element => element.Value == param).Key;

					if (Compendium.g_CompendiumRenderers.ContainsKey(linkName))
					{
						var gump = new CompendiumPageGump(m_caller, Compendium.g_CompendiumRenderers[linkName]);
						gump.Send();
					}
				}
				catch (Exception e)
				{
					Console.WriteLine("Bad CompendiumPageGump link");
					Console.WriteLine(e);
					Refresh();
				}
			}
			//lookup the hyperlink from the param and open the gump
		}

		public virtual void onEditPageButtonClick(GumpButton button, object param)
		{
			if (m_caller.AccessLevel < AccessLevel.Administrator)
			{
				return;
			}

			try
			{
				var state = new CompendiumEditorState
				{
					PageName = Renderer.Name,
					Caller = (PlayerMobile)m_caller,
					RendererToEdit = (CompendiumPageRenderer)Renderer.Clone()
				};

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