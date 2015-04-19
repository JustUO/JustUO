#region References

using System;
using System.Collections;
using System.Linq;
using System.Net;
using Server.Accounting;
using Server.Commands;
using Server.Engines.XmlSpawner2;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
using Server.Targeting;

#endregion

namespace Server
{
    public class TellAFriend
    {
        public static bool Enabled = false;
        private const string TAFShardName = "Your Shard Name";

        public static void Initialize()
        {
            if (!Enabled)
            {
                return;
            }

            CommandSystem.Register("referral", AccessLevel.Player, Referral_OnCommand);
            CommandSystem.Register("referralrewards", AccessLevel.Player, ReferralRewards_OnCommand);
            EventSink.Login += EventSink_Login;
        }

        private static void EventSink_Login(LoginEventArgs args)
        {
            Mobile m = args.Mobile;
            PlayerMobile pm = m as PlayerMobile;

            Account acct = (Account) m.Account;

            bool hasreferred = Convert.ToBoolean(acct.GetTag("ToldAFriend"));

            if (!hasreferred && m.AccessLevel < AccessLevel.Counselor)
            {
                m.SendGump(new TAFGump(m));
            }
        }

        [Usage("referral")]
        [Description("Opens the referral system.")]
        public static void Referral_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            bool toldfriend = ToldAFriend(m);

            if (!toldfriend)
            {
                m.SendGump(new TAFGump(m));
            }
        }

        [Usage("referralrewards")]
        [Description("Opens the referral rewards system.")]
        private static void ReferralRewards_OnCommand(CommandEventArgs e)
        {
            Mobile m = e.Mobile;
            m.SendGump(new ReferralRewardsGump(m, 0));
        }

        private class TAFGump : Gump
        {
            private const int LabelColor32 = 0xFFFFFF;

            private static string Center(string text)
            {
                return String.Format("<CENTER>{0}</CENTER>", text);
            }

            private static string Color(string text, int color)
            {
                return String.Format("<BASEFONT COLOR=#{0:X6}>{1}</BASEFONT>", color, text);
            }

            public TAFGump(Mobile from, string initialText = "") : base(30, 20)
            {
                if (from == null)
                {
                    return;
                }

                this.AddPage(1);
                this.AddBackground(50, 0, 479, 309, 9270);
                this.AddImage(0, 0, 10400);
                this.AddImage(0, 225, 10402);
                this.AddImage(495, 0, 10410);
                this.AddImage(495, 225, 10412);
                this.AddImage(60, 15, 5536);

                AddHtml(170, 20, 315, 165,
                    String.Format(
                        "Welcome to the {0} Referral System!  Click the button below to target the character that invited you to play, and you will both receive a referral point to spend in the Referral Rewards Shop.  You cannot set your own accounts as your referrer.  You can access the shop by saying [referralrewards.  You can only purchase one referral gift per day.  For every player that marks you as their referrer, you will receive another Referral Point.  Thank you for playing {1}!",
                        TAFShardName, TAFShardName),
                    true, true);


                bool toldfriend = ToldAFriend(from);
                if (!toldfriend)
                {
                    this.AddLabel(205, 195, 50, String.Format("Who referred you to {0}?", TAFShardName));


                    this.AddLabel(205, 230, 88, "Target Your Referrer:");
                    this.AddButton(365, 230, 4023, 4025, 2, GumpButtonType.Reply, 0); //Target player button
                }
                else
                {
                    this.AddLabel(205, 200, 88, "You already entered a referrer.");
                }
                this.AddHtml(205, 255, 205, 56, Color(Center(initialText), 0xFF0000), false, false);
            }

            public override void OnResponse(NetState state, RelayInfo info)
            {
                Mobile from = state.Mobile;
                int id = info.ButtonID;


                if (id == 2)
                {
                    from.BeginTarget(10, false, TargetFlags.None, TAFTarget);
                    from.SendMessage(String.Format("Please target the character that referred you to {0}.", TAFShardName));
                }
                else
                {
                    from.SendMessage("You can open this gump again by saying [referral.");
                }
            }

            private static void TAFTarget(Mobile from, object target)
            {
                if (from == null)
                {
                    return;
                }

                string initialText = "";
                if (target is PlayerMobile)
                {
                    Mobile friend = (Mobile) target;
                    Account fracct = (Account) friend.Account;
                    ArrayList tafip_List = new ArrayList(fracct.LoginIPs);

                    Account acct = (Account) from.Account;
                    ArrayList ip_List = new ArrayList(acct.LoginIPs);
                    PlayerMobile pm = from as PlayerMobile;


                    bool toldu = Convert.ToBoolean(acct.GetTag("ToldAFriend"));

                    if (fracct == acct)
                    {
                        initialText = "You can't be your own referrer!";
                    }
                    else
                    {
                        bool uniqueIP = true;

                        foreach (object t in ip_List)
                        {
                            if (tafip_List.Contains(t))
                            {
                                uniqueIP = false;
                            }
                        }

                        XmlReferralRewards re =
                            (XmlReferralRewards) XmlAttach.FindAttachment(friend, typeof (XmlReferralRewards));
                        XmlReferralRewards me =
                            (XmlReferralRewards) XmlAttach.FindAttachment(from, typeof (XmlReferralRewards));

                        //We wrote the method below to prevent players from referring multiple accounts of the same friend.
                        if (uniqueIP)
                        {
                            if (pm != null)
                            {
                                if (me != null)
                                {
                                    if (me.ReferredList.Any(tafip_List.Contains))
                                    {
                                        @from.SendMessage("You've already referred someone that uses their IP address.");
                                        return;
                                    }
                                }
                            }

                            if (!toldu)
                            {
                                initialText = String.Format("{0} marked as referrer.", friend.Name);

                                if (pm != null && me != null)
                                {
                                    foreach (IPAddress ip in tafip_List)
                                    {
                                        me.ReferredList.Add(ip);
                                    }
                                }

                                if (re != null && me != null)
                                {
                                    re.PointsAvailable++;
                                    me.PointsAvailable++;
                                    friend.SendMessage(
                                        "{0} has just marked you as their referrer.  A referral point has been added to your account.  You may spend these by using the [referralrewards command.",
                                        from.Name);
                                    from.SendMessage("You were given a bonus referral point for marking your referrer!");
                                }
                                else
                                {
                                    friend.SendMessage(39, "You are not eligable for referral rewards on {0}.",
                                        TAFShardName);
                                }

                                acct.SetTag("ToldAFriend", "true");
                                acct.SetTag("Referrer", fracct.ToString());
                            }
                            else
                            {
                                from.SendMessage("You have already set a referrer.");
                                return;
                            }
                        }
                        else
                        {
                            from.SendMessage("You can't refer another account you own.");
                            return;
                        }
                    }
                }
                else
                {
                    initialText = "Please select a player character.";
                }
                from.SendGump(new TAFGump(from, initialText));
            }
        }

        private static bool ToldAFriend(Mobile m)
        {
            Account acct = (Account) m.Account;
            bool told = Convert.ToBoolean(acct.GetTag("ToldAFriend"));
            return told;
        }
    }
}
