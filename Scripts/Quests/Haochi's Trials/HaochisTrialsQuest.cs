using System;
using Server.Mobiles;

namespace Server.Engines.Quests.Samurai
{
    public class HaochisTrialsQuest : QuestSystem
    {
        private static readonly Type[] m_TypeReferenceTable =
        {
            typeof (AcceptConversation),
            typeof (RadarConversation),
            typeof (FirstTrialIntroConversation),
            typeof (FirstTrialKillConversation),
            typeof (GainKarmaConversation),
            typeof (SecondTrialIntroConversation),
            typeof (SecondTrialAttackConversation),
            typeof (ThirdTrialIntroConversation),
            typeof (ThirdTrialKillConversation),
            typeof (FourthTrialIntroConversation),
            typeof (FourthTrialCatsConversation),
            typeof (FifthTrialIntroConversation),
            typeof (FifthTrialReturnConversation),
            typeof (LostSwordConversation),
            typeof (SixthTrialIntroConversation),
            typeof (SeventhTrialIntroConversation),
            typeof (EndConversation),
            typeof (FindHaochiObjective),
            typeof (FirstTrialIntroObjective),
            typeof (FirstTrialKillObjective),
            typeof (FirstTrialReturnObjective),
            typeof (SecondTrialIntroObjective),
            typeof (SecondTrialAttackObjective),
            typeof (SecondTrialReturnObjective),
            typeof (ThirdTrialIntroObjective),
            typeof (ThirdTrialKillObjective),
            typeof (ThirdTrialReturnObjective),
            typeof (FourthTrialIntroObjective),
            typeof (FourthTrialCatsObjective),
            typeof (FourthTrialReturnObjective),
            typeof (FifthTrialIntroObjective),
            typeof (FifthTrialReturnObjective),
            typeof (SixthTrialIntroObjective),
            typeof (SixthTrialReturnObjective),
            typeof (SeventhTrialIntroObjective),
            typeof (SeventhTrialReturnObjective)
        };

        private bool m_SentRadarConversion;

        public HaochisTrialsQuest(PlayerMobile from)
            : base(from)
        {
        }

        // Serialization
        public HaochisTrialsQuest()
        {
        }

        public override Type[] TypeReferenceTable
        {
            get { return m_TypeReferenceTable; }
        }

        public override object Name
        {
            get
            {
                // Haochi's Trials
                return 1063022;
            }
        }

        public override object OfferMessage
        {
            get
            {
                /* <i>As you enter the courtyard you notice a faded sign.
                * It reads: </i><br><br>
                * 
                * Welcome to your new home, Samurai.<br><br>
                * 
                * Though your skills are only a shadow of what they can be some day,
                * you must prove your adherence to the code of the Bushido.<br><br>
                * 
                * Seek Daimyo Haochi for guidance.<br><br>
                * 
                * <i>Will you accept the challenge?</i>
                */
                return 1063023;
            }
        }

        public override TimeSpan RestartDelay
        {
            get { return TimeSpan.MaxValue; }
        }

        public override bool IsTutorial
        {
            get { return true; }
        }

        public override int Picture
        {
            get { return 0x15D7; }
        }

        public static bool HasLostHaochisKatana(Mobile from)
        {
            var pm = from as PlayerMobile;

            if (pm == null)
                return false;

            var qs = pm.Quest;

            if (qs is HaochisTrialsQuest)
            {
                if (qs.IsObjectiveInProgress(typeof (FifthTrialReturnObjective)))
                {
                    var pack = from.Backpack;

                    return (pack == null || pack.FindItemByType(typeof (HaochisKatana)) == null);
                }
            }

            return false;
        }

        public override void Accept()
        {
            base.Accept();

            AddConversation(new AcceptConversation());
        }

        public override void Slice()
        {
            if (!m_SentRadarConversion &&
                (From.Map != Map.Malas || From.X < 360 || From.X > 400 || From.Y < 760 || From.Y > 780))
            {
                m_SentRadarConversion = true;
                AddConversation(new RadarConversation());
            }

            base.Slice();
        }

        public override void ChildDeserialize(GenericReader reader)
        {
            var version = reader.ReadEncodedInt();

            m_SentRadarConversion = reader.ReadBool();
        }

        public override void ChildSerialize(GenericWriter writer)
        {
            writer.WriteEncodedInt(0); // version

            writer.Write(m_SentRadarConversion);
        }
    }
}