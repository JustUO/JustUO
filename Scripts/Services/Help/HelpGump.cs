using System;
using Server.Gumps;
using Server.Menus.Questions;
using Server.Mobiles;
using Server.Multis;
using Server.Network;

namespace Server.Engines.Help
{
    public class ContainedMenu : QuestionMenu
    {
        private readonly Mobile m_From;
        public ContainedMenu(Mobile from)
            : base("You already have an open help request. We will have someone assist you as soon as possible.  What would you like to do?", new string[] { "Leave my old help request like it is.", "Remove my help request from the queue." })
        {
            this.m_From = from;
        }

        public override void OnCancel(NetState state)
        {
            this.m_From.SendLocalizedMessage(1005306, "", 0x35); // Help request unchanged.
        }

        public override void OnResponse(NetState state, int index)
        {
            if (index == 0)
            {
                this.m_From.SendLocalizedMessage(1005306, "", 0x35); // Help request unchanged.
            }
            else if (index == 1)
            {
                PageEntry entry = PageQueue.GetEntry(this.m_From);

                if (entry != null && entry.Handler == null)
                {
                    this.m_From.SendLocalizedMessage(1005307, "", 0x35); // Removed help request.
                    entry.AddResponse(entry.Sender, "[Canceled]");
                    PageQueue.Remove(entry);
                }
                else
                {
                    this.m_From.SendLocalizedMessage(1005306, "", 0x35); // Help request unchanged.
                }
            }
        }
    }

    public class HelpGump : Gump
    {
        public override int TypeID { get { return 0x29A; } }

        public HelpGump(Mobile from)
            : base(0, 0)
        {
            from.CloseGump(typeof(HelpGump));

            bool isYoung = IsYoung(from);

            AddBackground(50, 25, 540, 430, 2600);

            AddPage(0);

            AddHtmlLocalized(150, 50, 360, 40, 1001002, false, false); // <CENTER><U>Ultima Online Help Menu</U></CENTER>
            AddButton(425, 415, 2073, 2072, 0, GumpButtonType.Reply, 0); // Close

            AddPage(1);

            if (isYoung)
            {
                AddButton(80, 75, 5540, 5541, 10, GumpButtonType.Reply, 2);
                AddHtmlLocalized(110, 75, 450, 60, 1041525, true, true); // <BODY><BASEFONT COLOR=BLACK><u>Young Player Haven Transport.</u> Select this option if you want to be transported to Haven <BASEFONT COLOR=BLACK>.</BODY>

                AddButton(80, 141, 5540, 5541, 1, GumpButtonType.Page, 2);
                AddHtmlLocalized(110, 141, 450, 60, 1001003, true, true); // <U>General question about Ultima Online</U>: Select this option if you are having difficulties learning to use a skill, if you are lost, or if you have a general gameplay question.

                AddButton(80, 207, 5540, 5541, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 207, 450, 60, 1001004, true, true); // <U>My character is physically stuck or cannot continue to play</U>: This choice covers cases where your character is in a location they cannot move out of.

                AddButton(80, 273, 5540, 5541, 3, GumpButtonType.Page, 3);
                AddHtmlLocalized(110, 273, 450, 60, 1001005, true, true); // <U>Another player is harassing me</U>: Another player is harassing your character, be it by verbal or physical means, or is breaking the Terms of Service Agreement. To see what constitutes harassment please visit <A HREF="http://support.owo.com/gm_harass.html">http://support.owo.com/gm_harass.html</A>.

                AddButton(80, 339, 5540, 5541, 4, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 339, 450, 60, 1001006, true, true); // <U>Other</U>: If you are experiencing a problem in the game that does not fall into one of the other categories or is not addressed on the Support web page (located at <A HREF="http://support.owo.com">http://support.owo.com</A>), and requires in-game assistance, use this option.
            }
            else
            {
                AddButton(80, 75, 5540, 5541, 1, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 75, 450, 60, 1001003, true, true); // <U>General question about Ultima Online</U>: Select this option if you are having difficulties learning to use a skill, if you have a general gameplay question, or you would like to search the UO Knowledge Base.

                AddButton(80, 141, 5540, 5541, 2, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 141, 450, 60, 1001004, true, true); // <U>My character is physically stuck</U>: This choice only covers cases where your character is physically stuck in a location they cannot move out of.

                AddButton(80, 207, 5540, 5541, 0, GumpButtonType.Page, 3);
                AddHtmlLocalized(110, 207, 450, 60, 1001005, true, true); // <U>Another player is harassing me</U>: Another player is harassing me verbally or physically, or is breaking the Terms of Service Agreement.  To see what constitutes harassment please visit <A HREF="http://uo.custhelp.com/cgi-bin/uo.cfg/php/enduser/std_adp.php?p_faqid=47">- How do I report someone for Harassment in UO? -</A>.

                AddButton(80, 273, 5540, 5541, 0, GumpButtonType.Page, 2);
                AddHtmlLocalized(110, 273, 450, 60, 1001006, true, true); // <U>Other</U>: If you are experiencing a problem in the game that does not fall into one of the other categories or is not addressed on the Support web page (located at <A HREF="http://uo.custhelp.com/">http://uo.custhelp.com/</A>) and requires in-game assistance please use this option.
            }

            AddPage(2);

            if (isYoung)
            {
                AddButton(80, 75, 5540, 5541, 8, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 75, 450, 60, 1001008, true, true); // <U>Visit the Ultima Online web site</U>: You can learn about Ultima Online, browse the UO Playguide, and keep abreast of current issues.  This selection will launch your web browser and take you the web site.

                AddButton(80, 141, 5540, 5541, 5, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 141, 450, 60, 1074796, true, true); // <U>Visit the Ultima Online Knowledge Base</U>: You can find detailed answers to many of the most frequently asked questions in our Knowledge Base.  This selection will launch your web browser and take you to those answers.

                AddButton(80, 207, 5540, 5541, 6, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 207, 450, 60, 1001009, true, true); // <U>Report a bug</U>: Use this option to launch your web browser submit a bug report.  Your report will be read by our Quality Assurance staff.  We apologize for not being able to reply to individual bug reports.

                AddButton(80, 273, 5540, 5541, 7, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 273, 450, 60, 1001010, true, true); // <U>Contact a Counselor</U>: A Counselor is an experienced Ultima Online player who has volunteered their time to help answer general gameplay questions.  Selecting this option will let you send a message to a Counselor.  Please remember that Counselors are volunteers and may not be available at all times so please be patient and remember to check the website for additional help.

                AddButton(80, 339, 5540, 5541, 4, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 339, 450, 60, 1001006, true, true); // <U>Other</U>: If you are experiencing a problem in the game that does not fall into one of the other categories or is not addressed on the Support web page (located at <A HREF="http://uo.custhelp.com/">http://uo.custhelp.com/</A>) and requires in-game assistance please use this option.

                AddButton(80, 405, 5540, 5541, 0, GumpButtonType.Page, 1);
                AddHtmlLocalized(110, 405, 450, 29, 1001011, true, false); // <U>Return to the help menu</U>
            }
            else
            {
                AddButton(80, 75, 5540, 5541, 8, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 75, 450, 60, 1001008, true, true); // <U>Visit the Ultima Online web site</U>: You can learn about Ultima Online, browse the UO Playguide, and keep abreast of current issues.  This selection will launch your web browser and take you the web site.

                AddButton(80, 141, 5540, 5541, 5, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 141, 450, 60, 1074796, true, true); // <U>Visit the Ultima Online Knowledge Base</U>: You can find detailed answers to many of the most frequently asked questions in our Knowledge Base.  This selection will launch your web browser and take you to those answers.

                AddButton(80, 207, 5540, 5541, 6, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 207, 450, 60, 1001009, true, true); // <U>Report a bug</U>: Use this option to launch your web browser submit a bug report.  Your report will be read by our Quality Assurance staff.  We apologize for not being able to reply to individual bug reports.

                AddButton(80, 273, 5540, 5541, 4, GumpButtonType.Reply, 0);
                AddHtmlLocalized(110, 273, 450, 60, 1001006, true, true); // <U>Other</U>: If you are experiencing a problem in the game that does not fall into one of the other categories or is not addressed on the Support web page (located at <A HREF="http://uo.custhelp.com/">http://uo.custhelp.com/</A>) and requires in-game assistance please use this option.

                AddButton(80, 339, 5540, 5541, 0, GumpButtonType.Page, 1);
                AddHtmlLocalized(110, 339, 450, 29, 1001011, true, false); // <U>Return to the help menu</U>
            }

            AddPage(3);

            AddButton(80, 75, 5540, 5541, 3, GumpButtonType.Reply, 0);
            AddHtmlLocalized(110, 75, 450, 164, 1062572, true, true); // <U><CENTER>Another player is harassing me (or Exploiting).</CENTER></U><BR>VERBAL HARASSMENT<BR>Use this option when another player is verbally harassing your character. Verbal harassment behaviors include but are not limited to, using bad language, threats etc.. Before you submit a complaint be sure you understand what constitutes harassment <A HREF="http://uo.custhelp.com/cgi-bin/uo.cfg/php/enduser/std_adp.php?p_faqid=40">– what is verbal harassment? -</A> and that you have followed these steps:<BR>1. You have asked the player to stop and they have continued.<BR>2. You have tried to remove yourself from the situation.<BR>3. You have done nothing to instigate or further encourage the harassment.<BR>4. You have added the player to your ignore list. <A HREF="http://uo.custhelp.com/cgi-bin/uo.cfg/php/enduser/std_adp.php?p_faqid=138">- How do I ignore a player?</A><BR>5. You have read and understand Origin’s definition of harassment.<BR>6. Your account information is up to date. (Including a current email address)<BR>*If these steps have not been taken, GMs may be unable to take action against the offending player.<BR>**A chat log will be review by a GM to assess the validity of this complaint. Abuse of this system is a violation of the Rules of Conduct.<BR>EXPLOITING<BR>Use this option to report someone who may be exploiting or cheating. <A HREF="http://uo.custhelp.com/cgi-bin/uo.cfg/php/enduser/std_adp.php?p_faqid=41">– What constitutes an exploit?</a>

            AddButton(80, 245, 5540, 5541, 12, GumpButtonType.Reply, 0);
            AddHtmlLocalized(110, 245, 450, 164, 1062573, true, true); // <U><CENTER>Another player is harassing me using game mechanics.</CENTER></U><BR><BR>PHYSICAL HARASSMENT<BR>Use this option when another player is harassing your character using game mechanics. Physical harassment includes but is not limited to luring and any act that causes a players death in Trammel. Before you submit a complaint be sure you understand what constitutes harassment <A HREF="http://uo.custhelp.com/cgi-bin/uo.cfg/php/enduser/std_adp.php?p_faqid=59">- What is physical harassment? -</A> and that you have followed these steps:<BR>1. You have asked the player to stop and they have continued.<BR>2. You have tried to remove yourself from the situation.<BR>3. You have done nothing to instigate or further encourage the harassment.<BR>4. You have added the player to your ignore list. <A HREF="http://uo.custhelp.com/cgi-bin/uo.cfg/php/enduser/std_adp.php?p_faqid=138">- How do I ignore a player? -</A><BR>5. You have read and understand Origin’s definition of harassment.<BR>6. Your account information is up to date. (Including a current email address)<BR>*If these steps have not been taken, GMs may be unable to take action against the offending player.<BR>**This issue will be reviewed by a GM to assess the validity of this complaint. Abuse of this system is a violation of the Rules of Conduct.

            AddButton(80, 415, 5540, 5541, 0, GumpButtonType.Page, 1);
            AddHtmlLocalized(110, 415, 450, 29, 1001011, true, false); // <U>Return to the help menu</U>
        }

        public static void Initialize()
        {
            EventSink.HelpRequest += new HelpRequestEventHandler(EventSink_HelpRequest);
        }

        private static void EventSink_HelpRequest(HelpRequestEventArgs e)
        {
            foreach (Gump g in e.Mobile.NetState.Gumps)
            {
                if (g is HelpGump)
                    return;
            }

            if (!PageQueue.CheckAllowedToPage(e.Mobile))
                return;

            if (PageQueue.Contains(e.Mobile))
                e.Mobile.SendMenu(new ContainedMenu(e.Mobile));
            else
                e.Mobile.SendGump(new HelpGump(e.Mobile));
        }

        private static bool IsYoung(Mobile m)
        {
            if (m is PlayerMobile)
                return ((PlayerMobile)m).Young;

            return false;
        }

        public static bool CheckCombat(Mobile m)
        {
            for (int i = 0; i < m.Aggressed.Count; ++i)
            {
                AggressorInfo info = m.Aggressed[i];

                if (DateTime.UtcNow - info.LastCombatTime < TimeSpan.FromSeconds(30.0))
                    return true;
            }

            return false;
        }

        public override void OnResponse(NetState state, RelayInfo info)
        {
            Mobile from = state.Mobile;

            PageType type = (PageType)(-1);

            switch ( info.ButtonID )
            {
                case 0: // Close/Cancel
                    {
                        from.SendLocalizedMessage(501235, "", 0x35); // Help request aborted.

                        break;
                    }
                case 1: // General question about Ultima Online
                    {
                        from.LaunchBrowser("http://www.uo.com");
                        break;
                    }
                case 2: // Stuck
                    {
                        var house = BaseHouse.FindHouseAt(from);

                        if (house != null && house.IsAosRules && !from.Region.IsPartOf(typeof(Engines.ConPVP.SafeZone))) // Dueling
                        {
                            from.Location = house.BanLocation;
                        }
                        else if (from.Region.IsPartOf(typeof(Regions.Jail)))
                        {
                            from.SendLocalizedMessage(1041530, "", 0x35); // You'll need a better jailbreak plan then that!
                        }
                        else if (Factions.Sigil.ExistsOn(from))
                        {
                            from.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
                        }
                        else if (from.CanUseStuckMenu() && from.Region.CanUseStuckMenu(from) && !CheckCombat(from) && !from.Frozen && !from.Criminal)
                        {
                            StuckMenu menu = new StuckMenu(from, from, true);

                            menu.BeginClose();

                            from.SendGump(menu);
                        }
                        else
                        {
                            type = PageType.Stuck;
                        }

                        break;
                    }
                case 3: // Harassment: Verbal
                    {
                        type = PageType.VerbalHarassment;
                        break;
                    }
                case 4: // Other
                    {
                        type = PageType.Other;
                        break;
                    }
                case 5: // Visit the OWO web page
                    {
                        from.LaunchBrowser("http://www.uoguide.com/");
                        break;
                    }
                case 6: // Report a bug or contact Origin
                    {
                        from.LaunchBrowser("http://www.uo.com");
                        break;
                    }
                case 7: // Contact a Councelor
                    {
                        type = PageType.Question;
                        break;
                    }
                case 8: // Visit the Ultima Online web site
                    {
                        from.LaunchBrowser("http://www.uo.com");
                        break;
                    }
                case 10: // Young player transport
                    {
                        if (IsYoung(from))
                        {
                            if (from.Region.IsPartOf(typeof(Regions.Jail)))
                            {
                                from.SendLocalizedMessage(1041530, "", 0x35); // You'll need a better jailbreak plan then that!
                            }
                            else if (from.Region.Name == "Haven" || from.Region.Name == "New Haven")
                            {
                                from.SendLocalizedMessage(1041529); // You're already in Haven
                            }
                            else
                            {
                                if (from.Alive)
                                    from.MoveToWorld(new Point3D(3503, 2574, 14), Map.Trammel);
                                else
                                    from.MoveToWorld(new Point3D(3469, 2559, 36), Map.Trammel);
                            }
                        }

                        break;
                    }
                case 11: // Promotional Code
                    {
                        //if (PromotionalSystem.Enabled)
                        //{
                        //    from.SendGump(new PromotionalCodeGump());
                        //}
                        //else
                        //{
                            from.SendLocalizedMessage(1062904); // The promo code redemption system is currently unavailable. Please try again later.

                            from.SendLocalizedMessage(501235, "", 0x35); // Help request aborted.
                        //}

                        break;
                    }
                case 12: // Harassment: Physical
                    {
                        type = PageType.PhysicalHarassment;
                        break;
                    }
            }

            if (type != (PageType)(-1) && PageQueue.CheckAllowedToPage(from))
            {
                from.SendGump(new PagePromptGump(from, type));
            }
        }
    }
}