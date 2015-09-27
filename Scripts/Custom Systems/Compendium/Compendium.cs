#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Xml.Linq;

using Server.Commands;
#endregion

namespace Server.Gumps.Compendium
{
	public class Compendium
	{
		public const AccessLevel ACCESS_LEVEL_REQUIRED_TO_EDIT_ARTICLES = AccessLevel.Developer;
		public const string COMPENDIUM_ROOT_FOLDER_NAME = "Compendium";
		public const string YELLOW_TEXT_WEB_COLOR = "#FDBF3E";
		public const string CONTENT_TEXT_WEB_COLOR = "#111111";

		public static Dictionary<string, CompendiumPageRenderer> g_CompendiumRenderers =
			new Dictionary<string, CompendiumPageRenderer>();

		public static void Configure()
		{
			EventSink.WorldLoad += EventSink_WorldLoad;
		}

		public static void EventSink_WorldLoad()
		{
			Console.WriteLine("Loading Compendium Page Renderers");
			if (!Directory.Exists(CompendiumRootPath))
			{
				Directory.CreateDirectory(CompendiumRootPath);
				return;
			}

			var availableFiles = Directory.GetFiles(CompendiumRootPath, "*.xml");

			foreach (var filename in availableFiles)
			{
				try
				{
					var document = XDocument.Load(Path.Combine(CompendiumRootPath, filename));
					var renderer = CompendiumPageRenderer.Deserialize(document);
					g_CompendiumRenderers.Add(renderer.Name, renderer);
				}
				catch (Exception e)
				{
					Console.WriteLine(e);
				}
			}
		}

		public static string CompendiumRootPath
		{
			get { return Path.Combine(Core.BaseDirectory, COMPENDIUM_ROOT_FOLDER_NAME); }
		}

		public static void Initialize()
		{
			CommandSystem.Register("ViewPage", AccessLevel.GameMaster, _OnCommand);
		}

		[Usage(""), Description("Open the Main MOTD landingpage")]
		public static void _OnCommand(CommandEventArgs e)
		{
			var caller = e.Mobile;

			if (caller.HasGump(typeof(CompendiumPageRenderer)))
			{
				caller.CloseGump(typeof(CompendiumPageRenderer));
			}

			if (e.Arguments.Length > 0)
			{
				if (e.Arguments[0].IndexOfAny(Path.GetInvalidFileNameChars()) >= 0)
				{
					caller.SendMessage("That page name has illegal characters in it.");
					return;
				}

				if (g_CompendiumRenderers.ContainsKey(e.Arguments[0]))
				{
					var gump = new CompendiumPageGump(caller, g_CompendiumRenderers[e.Arguments[0]]);
					gump.Send();
				}
				else
				{
					caller.SendMessage("That page does not exist.");
				}
			}
		}
	}
}