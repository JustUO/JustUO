using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;

using Server.Commands;
using Server.Gumps;

namespace Server.Gumps.Compendium
{
    public static class LinqHelper
    {
        /// Taken from: http://stackoverflow.com/questions/2471588/how-to-get-index-using-linq
        ///<summary>Finds the index of the first item matching an expression in an enumerable.</summary>
        ///<param name="items">The enumerable to search.</param>
        ///<param name="predicate">The expression to test the items against.</param>
        ///<returns>The index of the first matching item, or -1 if no items match.</returns>
        public static int FindIndex<T>(this IEnumerable<T> items, Func<T, bool> predicate)
        {
            if (items == null) throw new ArgumentNullException("items");
            if (predicate == null) throw new ArgumentNullException("predicate");

            int retVal = 0;
            foreach (var item in items)
            {
                if (predicate(item)) return retVal;
                retVal++;
            }
            return -1;
        }
    }

    public class Compendium
    {
        public const AccessLevel ACCESS_LEVEL_REQUIRED_TO_EDIT_ARTICLES = AccessLevel.Developer;
        public const string COMPENDIUM_ROOT_FOLDER_NAME = "Compendium";
        public const string YELLOW_TEXT_WEB_COLOR = "#FDBF3E";
        public const string CONTENT_TEXT_WEB_COLOR = "#111111";

        public static Dictionary<string, CompendiumPageRenderer> g_CompendiumRenderers = new Dictionary<string, CompendiumPageRenderer>();

        public static void Configure()
        {
            EventSink.WorldLoad += new WorldLoadEventHandler(EventSink_WorldLoad);
        }

        public static void EventSink_WorldLoad()
        {
            Console.WriteLine("Loading Compendium Page Renderers");
            if (!Directory.Exists(CompendiumRootPath))
            {
                Directory.CreateDirectory(CompendiumRootPath);
                return;
            }

            string[] availableFiles = Directory.GetFiles(CompendiumRootPath, "*.xml");

            foreach (string filename in availableFiles)
            {
                try
                {
                    XDocument document = XDocument.Load(Path.Combine(CompendiumRootPath, filename));
                    CompendiumPageRenderer renderer = CompendiumPageRenderer.Deserialize(document);
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
            get
            {
                return Path.Combine(Core.BaseDirectory, COMPENDIUM_ROOT_FOLDER_NAME);
            }
        }

        public static void Initialize()
        {
            CommandSystem.Register("ViewPage", AccessLevel.GameMaster, new CommandEventHandler(_OnCommand));
        }

        [Usage("")]
        [Description("Open the Main MOTD landingpage")]
        public static void _OnCommand(CommandEventArgs e)
        {
            Mobile caller = e.Mobile;

            if (caller.HasGump(typeof(CompendiumPageRenderer)))
                caller.CloseGump(typeof(CompendiumPageRenderer));

            if (e.Arguments.Length > 0)
            {
                if (e.Arguments[0].IndexOfAny(System.IO.Path.GetInvalidFileNameChars()) >= 0)
                {
                    caller.SendMessage("That page name has illegal characters in it.");
                    return;
                }

                if (g_CompendiumRenderers.ContainsKey(e.Arguments[0]))
                {
                    CompendiumPageGump gump = new CompendiumPageGump(caller, g_CompendiumRenderers[e.Arguments[0]]);
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
