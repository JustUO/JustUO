using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;

namespace Server.Misc
{
    class OpenUOHelper
    {

        private static OpenUOSDK _OpenUOSDK;

        public static void Configure()
        {
           _OpenUOSDK = new OpenUOSDK();
        }

            
        public static Bitmap GetBitmap(int itemID)
        {
            try
            {
                return OpenUOSDK.ArtFactory.GetStatic<Bitmap>(itemID);
            }
            catch
            {
                Utility.PushColor(ConsoleColor.Red);
                Console.WriteLine("Error: Not able to read client files.");
                Utility.PopColor();
            }

            return null;
        }
    }
}
