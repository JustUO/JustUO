using System;
using System.Collections.Generic;
using Server.ContextMenus;
using Server.Items;

namespace Server.Mobiles
{
    public class Greeter1 : BaseGuildmaster
    {
        private static bool m_Talked; // flag to prevent spam 

        private readonly string[] kfcsay =
        {
            "Greetings Adventurer! If you are seeking to enter the Abyss, I may be of assitance to you."
        };

        [Constructable]
        public Greeter1()
            : base("Greeter1")
        {
        }

        public Greeter1(Serial serial)
            : base(serial)
        {
        }

        public override NpcGuild NpcGuild
        {
            get { return NpcGuild.TailorsGuild; }
        }

        public override bool ClickTitle
        {
            get { return false; }
        }

        public override void InitBody()
        {
            InitStats(100, 100, 100);
            Name = "Garamon";
            Body = 0x190;
        }

        public override void InitOutfit()
        {
            AddItem(new Robe(1));
            AddItem(new Sandals(1));

            HairItemID = 0x203B;
            HairHue = 0;
        }

        public override void OnMovement(Mobile m, Point3D oldLocation)
        {
            if (m_Talked == false)
            {
                if (m.InRange(this, 2))
                {
                    m_Talked = true;
                    SayRandom(kfcsay, this);
                    Move(GetDirectionTo(m.Location));

                    var t = new SpamTimer();
                    t.Start();
                }
            }
        }

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (from.InRange(Location, 2))
                return true;

            return base.HandlesOnSpeech(from);
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            var from = e.Mobile;

            if (!e.Handled && from is PlayerMobile && from.InRange(Location, 2) && e.Speech.Contains("abyss"))
            {
                var pm = (PlayerMobile) from;

                if (e.Speech.Contains("abyss"))
                    SayTo(from,
                        "It's entrance is protected by stone guardians, who will only grant passage to the carrier of a Tripartite Key!");

                else if (e.Speech.Contains("key"))
                    SayTo(from, "It's three parts you must find and re-unite as one!");

                e.Handled = true;
            }
            base.OnSpeech(e);
        }

        public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
        {
            base.GetContextMenuEntries(from, list);
            list.Add(new Greeter1Entry(from, this));
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }

        private static void SayRandom(string[] say, Mobile m)
        {
            m.Say(say[Utility.Random(say.Length)]);
        }

        public class Greeter1Entry : ContextMenuEntry
        {
            private readonly Mobile m_Giver;
            private readonly Mobile m_Mobile;

            public Greeter1Entry(Mobile from, Mobile giver)
                : base(6146, 3)
            {
                m_Mobile = from;
                m_Giver = giver;
            }
        }

        private class SpamTimer : Timer
        {
            public SpamTimer()
                : base(TimeSpan.FromSeconds(90))
            {
                Priority = TimerPriority.OneSecond;
            }

            protected override void OnTick()
            {
                m_Talked = false;
            }
        }
    }
}