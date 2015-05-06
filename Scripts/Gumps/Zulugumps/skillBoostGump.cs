using System;
using System.Collections.Generic;
using Server.Items;
using Server.Mobiles;
using Server.Network;

/*
 *  Author Oscar Ternström
 */

namespace Server.Gumps.Zulugumps
{

    public class skillBoostGump : Gump
    {

        // skill increase
        const double SKILL_INC = 100;
        // number of skill boosts
        const int NUMBER_OF_BOOSTS = 3;

        public skillBoostGump(Mobile owner)
            : this(owner, ResurrectMessage.Generic, false)
        {
        }


        public skillBoostGump(Mobile owner, ResurrectMessage msg, bool fromSacrifice)
            : this(owner, msg)
        {
        }

        public skillBoostGump(Mobile owner, ResurrectMessage msg)
            : base(100, 0)
        {

            this.Closable = true;
            this.Disposable = true;
            this.Dragable = true;
            this.Resizable = false;

            AddPage(0);
            AddBackground(90, 30, 450, 538, 9200);
            AddLabel(299, 50, 0, @"Skill Boost");
            AddItem(255, 50, 4029);

            AddLabel(101, 100, 0, @"Archery");
            AddLabel(101, 130, 0, @"Fencing");
            AddLabel(101, 160, 0, @"Mace Fighting");
            AddLabel(101, 190, 0, @"Parrying");
            AddLabel(101, 220, 0, @"Swordsmanship");
            AddLabel(101, 250, 0, @"Tactics");
            AddLabel(101, 280, 0, @"Wrestling");
            AddLabel(101, 310, 0, @"Healing");
            AddLabel(101, 340, 0, @"Vetrinary");
            AddLabel(100, 370, 0, @"Alchemy");
            AddLabel(101, 400, 0, @"Inscription");
            AddLabel(101, 430, 0, @"Magery");
            AddLabel(101, 460, 0, @"Meditation");
            AddLabel(101, 490, 0, @"Resist");
            AddLabel(101, 520, 0, @"Spirit Speak");


            AddLabel(247, 100, 0, @"Begging");
            AddLabel(247, 130, 0, @"Detect Hidden");
            AddLabel(247, 160, 0, @"Hiding");
            AddLabel(247, 190, 0, @"Lockpick");
            AddLabel(247, 220, 0, @"Poisoning");
            AddLabel(247, 250, 0, @"Remove Trap");
            AddLabel(247, 280, 0, @"Snooping");
            AddLabel(247, 310, 0, @"Stealing");
            AddLabel(247, 340, 0, @"Stealth");
            AddLabel(247, 370, 0, @"Anatomy");
            AddLabel(247, 400, 0, @"Animal Lore");
            AddLabel(247, 430, 0, @"Animal Taming");
            AddLabel(247, 460, 0, @"Camping");
            AddLabel(247, 490, 0, @"Herding");
            AddLabel(247, 520, 0, @"Taste ID");


            AddLabel(400, 100, 0, @"Tracking");
            AddLabel(400, 130, 0, @"Arms Lore");
            AddLabel(400, 160, 0, @"Blacksmithy");
            AddLabel(400, 190, 0, @"Bowcraft");
            AddLabel(400, 220, 0, @"Carpentry");
            AddLabel(400, 250, 0, @"Cooking");
            AddLabel(400, 280, 0, @"Item ID");
            AddLabel(400, 310, 0, @"Cartography");
            AddLabel(400, 340, 0, @"Tailoring");
            AddLabel(400, 370, 0, @"Tinkering");
            AddLabel(400, 400, 0, @"Fishing");
            AddLabel(400, 430, 0, @"Mining");
            AddLabel(400, 460, 0, @"Lumberjacking");

           /* AddLabel(400, 490, 0, @"New Label");
            AddLabel(400, 520, 0, @"New Label");
            AddLabel(400, 540, 0, @"New Label");
            AddLabel(530, 100, 0, @"New Label");
            AddLabel(530, 130, 0, @"New Label");
            AddLabel(530, 160, 0, @"New Label");
            AddLabel(530, 190, 0, @"New Label");
            AddLabel(530, 220, 0, @"New Label");
            AddLabel(530, 250, 0, @"New Label");
            AddLabel(530, 310, 0, @"New Label");
            AddLabel(530, 340, 0, @"New Label");
            AddLabel(530, 370, 0, @"New Label");
            AddLabel(530, 400, 0, @"New Label");
            AddLabel(530, 430, 0, @"New Label");
            AddLabel(530, 460, 0, @"New Label"); */

            AddButton(195, 100, 210, 211, 1, GumpButtonType.Reply, 0,"Archery"); // done
            AddButton(195, 130, 210, 211, 2, GumpButtonType.Reply, 0,"Fencing");
            AddButton(195, 160, 210, 211, 3, GumpButtonType.Reply, 0,"Mace Fighting");
            AddButton(195, 190, 210, 211, 4, GumpButtonType.Reply, 0,"Parrying");
            AddButton(195, 220, 210, 211, 5, GumpButtonType.Reply, 0,"Swordsmanship");
            AddButton(195, 250, 210, 211, 6, GumpButtonType.Reply, 0,"Tactics");
            AddButton(195, 280, 210, 211, 7, GumpButtonType.Reply, 0,"Wrestling");
            AddButton(195, 310, 210, 211, 8, GumpButtonType.Reply, 0,"Healing");
            AddButton(195, 340, 210, 211, 9, GumpButtonType.Reply, 0,"Vetrinary");
            AddButton(195, 370, 210, 211, 10, GumpButtonType.Reply, 0,"Alchemy");
            AddButton(195, 400, 210, 211, 11, GumpButtonType.Reply, 0,"Inscription");
            AddButton(195, 430, 210, 211, 12, GumpButtonType.Reply, 0,"Magery");
            AddButton(195, 460, 210, 211, 13, GumpButtonType.Reply, 0,"Meditation");
            AddButton(195, 490, 210, 211, 14, GumpButtonType.Reply, 0,"Resist");
            AddButton(195, 520, 210, 211, 15, GumpButtonType.Reply, 0,"Spirit Speak");
           // AddButton(195, 520, 210, 211, 0, GumpButtonType.Reply, 0,"Begging");


            AddButton(345, 100, 210, 211, 16, GumpButtonType.Reply, 0,"Begging");
            AddButton(345, 130, 210, 211, 17, GumpButtonType.Reply, 0,"Detect Hidden");
            AddButton(345, 160, 210, 211, 18, GumpButtonType.Reply, 0,"Hiding");
            AddButton(345, 190, 210, 211, 19, GumpButtonType.Reply, 0,"Lockpick");
            AddButton(345, 220, 210, 211, 20, GumpButtonType.Reply, 0,"Poisoning");
            AddButton(345, 250, 210, 211, 21, GumpButtonType.Reply, 0,"Remove Trap");
            AddButton(345, 280, 210, 211, 22, GumpButtonType.Reply, 0,"Snooping");
            AddButton(345, 310, 210, 211, 23, GumpButtonType.Reply, 0,"Stealing");
            AddButton(345, 340, 210, 211, 24, GumpButtonType.Reply, 0,"Stealth");
            AddButton(345, 370, 210, 211, 25, GumpButtonType.Reply, 0,"Anatomy");
            AddButton(345, 400, 210, 211, 26, GumpButtonType.Reply, 0,"Animal Lore");
            AddButton(345, 430, 210, 211, 27, GumpButtonType.Reply, 0,"Animal Taming");
            AddButton(345, 460, 210, 211, 28, GumpButtonType.Reply, 0,"Camping");
            AddButton(345, 490, 210, 211, 29, GumpButtonType.Reply, 0,"Herding");
            AddButton(345, 520, 210, 211, 30, GumpButtonType.Reply, 0,"Taste ID");
            //AddButton(345, 520, 210, 211, 0, GumpButtonType.Reply, 0,"Arms Lore");



            AddButton(490, 100, 210, 211, 31, GumpButtonType.Reply, 0, "Tracking");
            AddButton(490, 130, 210, 211, 32, GumpButtonType.Reply, 0, "Arms Lore");
            AddButton(490, 160, 210, 211, 33, GumpButtonType.Reply, 0, "Blacksmithy");
            AddButton(490, 190, 210, 211, 34, GumpButtonType.Reply, 0, "Bowcraft");
            AddButton(490, 220, 210, 211, 35, GumpButtonType.Reply, 0, "Carpentry");
            AddButton(490, 250, 210, 211, 36, GumpButtonType.Reply, 0, "Cooking");
            AddButton(490, 280, 210, 211, 37, GumpButtonType.Reply, 0, "Item ID");
            AddButton(490, 310, 210, 211, 38, GumpButtonType.Reply, 0, "Cartography");
            AddButton(490, 340, 210, 211, 39, GumpButtonType.Reply, 0, "Tailoring");
            AddButton(490, 370, 210, 211, 40, GumpButtonType.Reply, 0, "Tinkering");
            AddButton(490, 400, 210, 211, 41, GumpButtonType.Reply, 0, "Fishing");
            AddButton(490, 430, 210, 211, 42, GumpButtonType.Reply, 0, "Mining");
            AddButton(490, 460, 210, 211, 43, GumpButtonType.Reply, 0, "Lumberjacking");
           // AddButton(490, 490, 210, 211, 0, GumpButtonType.Reply, 0,"");
          //  AddButton(480, 520, 210, 211, 0, GumpButtonType.Reply, 0);
           // AddButton(480, 520, 210, 211, 0, GumpButtonType.Reply, 0);



           /* AddButton(612, 114, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(613, 178, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(613, 146, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(613, 207, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(613, 236, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(613, 265, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(613, 297, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(615, 333, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(614, 364, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(615, 428, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(615, 396, 210, 211, 0, GumpButtonType.Reply, 0);
            AddButton(615, 457, 210, 211, 0, GumpButtonType.Reply, 0); */
			

            owner.Frozen = true;
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;
            
            switch (info.ButtonID)
            {
                case 1:
                    from.Skills.Archery.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    { 
                    from.CloseGump(typeof(skillBoostGump));
                    from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 2:
                    from.Skills.Fencing.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 3:
                    from.Skills.Macing.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 4:
                    from.Skills.Parry.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 5:
                    from.Skills.Swords.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 6:
                    from.Skills.Tactics.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 7:
                    from.Skills.Wrestling.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 8:
                    from.Skills.Healing.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 9:
                    from.Skills.Veterinary.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 10:
                    from.Skills.Alchemy.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 11:
                    from.Skills.Inscribe.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 12:
                    from.Skills.Magery.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 13:
                    from.Skills.Meditation.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 14:
                    from.Skills.MagicResist.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 15:
                    from.Skills.SpiritSpeak.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 16:
                    from.Skills.Begging.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 17:
                    from.Skills.DetectHidden.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 18:
                    from.Skills.Hiding.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 19:
                    from.Skills.Lockpicking.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 20:
                    from.Skills.Poisoning.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 21:
                    from.Skills.RemoveTrap.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 22:
                    from.Skills.Snooping.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 23:
                    from.Skills.Stealing.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 24:
                    from.Skills.Stealth.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 25:
                    from.Skills.Anatomy.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 26:
                    from.Skills.AnimalLore.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 27:
                    from.Skills.AnimalTaming.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 28:
                    from.Skills.Camping.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 29:
                    from.Skills.Herding.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 30:
                    from.Skills.TasteID.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 31:
                    from.Skills.Tracking.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 32:
                    from.Skills.ArmsLore.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 33:
                    from.Skills.Blacksmith.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 34:
                    from.Skills.Fletching.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 35:
                    from.Skills.Carpentry.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 36:
                    from.Skills.Cooking.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 37:
                    from.Skills.ItemID.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 38:
                    from.Skills.Cartography.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 39:
                    from.Skills.Tailoring.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 40:
                    from.Skills.Tinkering.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 41:
                    from.Skills.Fishing.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 42:
                    from.Skills.Mining.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
                case 43:
                    from.Skills.Lumberjacking.Base = SKILL_INC;
                    from.skillBoost = from.skillBoost + 1;
                    if (from.skillBoost != NUMBER_OF_BOOSTS)
                    {
                        from.CloseGump(typeof(skillBoostGump));
                        from.SendGump(new skillBoostGump(from));
                    }
                    break;
            }
            

            from.SendMessage("Can´t boost more skills");
            

            from.Frozen = false;

        }
    }
}