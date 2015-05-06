using System;
using Server.Mobiles;

namespace Server.Commands
{
    public class ShowClasse
    {
        public static void Initialize()
        {
            CommandSystem.Register("Showclasse", AccessLevel.Player, new CommandEventHandler(Classe_OnCommand));
        }

        [Usage("Showclasse")]
        [Description("Returns the players class level.")]
        private static void Classe_OnCommand(CommandEventArgs e)
        {
            string retString;
            Mobile m = e.Mobile;
            
            var currentSpecLevel = m.getSpec();
            if (Convert.ToInt32(currentSpecLevel) == 0) // remove this later
            {
                retString = "You aren´t in any class";
            }
            retString = "You aren´t in any class";
            e.Mobile.SendMessage(retString);

        }
    }
}