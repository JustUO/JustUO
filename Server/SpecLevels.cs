using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Server.Mobiles;

namespace Server
{
    class SpecLevels
    {

        double total;
        int level = 0;
        string currentClass;

        private void calculateLevel(Mobile player, double totalSkills)
        {
            double percentage; // later, gotta use total of all skills

            // if(percentage < blabla) then you cant have a class
            // else you can have a class
            double total = totalSkills;

            if (total >= 600 && total < 720) //spec 1
            {
                level = 1;
                
            }
            else if (total >= 720 && total < 840) //spec 2
            {
                level = 2;
            }
            else if (total >= 840 && total < 920) //spec 3
            {
                level = 3;
            }
            else if (total >= 920 && total < 1080) //spec 4
            {
                level = 4;
            }
            else if (total >= 1080 && total < 1200) //spec 5
            {
                level = 5;
            }
            else if (total >= 1200) //spec 6
            {
                level = 6;
            }
        }

        private void calculateClass(Mobile player)
        {

        }

        public string getClass(Mobile player)
        {
           // classes current = classes.Warrior;
            return "warrior";
        }

        public int getLevel(Mobile player)
        {
            double[] arrSpecCalc = { totalWarrior(player), totalBard(player), totalCrafter(player), totalMage(player), totalRanger(player), totalTheif(player) };
            


            total = totalWarrior(player);

            return level;
        }

        private double totalWarrior(Mobile player)
        {
            double skillsTotal = 0;
            skillsTotal += player.Skills.Anatomy.Value;
            skillsTotal += player.Skills.Fencing.Value;
            skillsTotal += player.Skills.Swords.Value;
            skillsTotal += player.Skills.Tactics.Value;
            skillsTotal += player.Skills.Healing.Value;
            skillsTotal += player.Skills.Macing.Value;
            skillsTotal += player.Skills.Parry.Value;
            skillsTotal += player.Skills.Wrestling.Value;
            return skillsTotal;
        }

        private double totalMage(Mobile player)
        {
            double skillsTotal = 0;
            skillsTotal += player.Skills.Magery.Value;
            skillsTotal += player.Skills.MagicResist.Value;
            skillsTotal += player.Skills.Inscribe.Value;
            skillsTotal += player.Skills.EvalInt.Value;
            skillsTotal += player.Skills.Alchemy.Value;
            skillsTotal += player.Skills.Meditation.Value;
            skillsTotal += player.Skills.SpiritSpeak.Value;
            skillsTotal += player.Skills.ItemID.Value;
            return skillsTotal;
        }

        private double totalCrafter(Mobile player)
        {
            double skillsTotal = 0;
            skillsTotal += player.Skills.Fletching.Value;
            skillsTotal += player.Skills.Mining.Value;
            skillsTotal += player.Skills.Lumberjacking.Value;
            skillsTotal += player.Skills.ArmsLore.Value;
            skillsTotal += player.Skills.Carpentry.Value;
            skillsTotal += player.Skills.Blacksmith.Value;
            skillsTotal += player.Skills.Tailoring.Value;
            skillsTotal += player.Skills.Tinkering.Value;
            return skillsTotal;
        }

        private double totalBard(Mobile player)
        {
            double skillsTotal = 0;
            skillsTotal += player.Skills.Begging.Value;
            skillsTotal += player.Skills.Cartography.Value;
            skillsTotal += player.Skills.Discordance.Value;
            skillsTotal += player.Skills.Peacemaking.Value;
            skillsTotal += player.Skills.Provocation.Value;
            skillsTotal += player.Skills.Herding.Value;
            skillsTotal += player.Skills.Musicianship.Value;
            skillsTotal += player.Skills.TasteID.Value;
            return skillsTotal;
        }

        private double totalRanger(Mobile player)
        {
            double skillsTotal = 0;
            skillsTotal += player.Skills.AnimalLore.Value;
            skillsTotal += player.Skills.AnimalTaming.Value;
            skillsTotal += player.Skills.Archery.Value;
            skillsTotal += player.Skills.Tracking.Value;
            skillsTotal += player.Skills.Fishing.Value;
            skillsTotal += player.Skills.Cooking.Value;
            skillsTotal += player.Skills.Veterinary.Value;
            skillsTotal += player.Skills.Camping.Value;
            return skillsTotal;
        }

        private double totalTheif(Mobile player)
        {
            double skillsTotal = 0;
            skillsTotal += player.Skills.DetectHidden.Value;
            skillsTotal += player.Skills.Hiding.Value;
            skillsTotal += player.Skills.Stealing.Value;
            skillsTotal += player.Skills.Stealth.Value;
            skillsTotal += player.Skills.Lockpicking.Value;
            skillsTotal += player.Skills.Snooping.Value;
            skillsTotal += player.Skills.Poisoning.Value;
            skillsTotal += player.Skills.RemoveTrap.Value; // not sure about this one
            return skillsTotal;
        }

    }
}
