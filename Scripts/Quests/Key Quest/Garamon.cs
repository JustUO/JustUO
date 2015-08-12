using System;
using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("Garamons Corpse")]
    public class Garamon : Mobile
    {
        [Constructable]
        public Garamon()
        {
            Name = "Garamon";
            HairItemID = 0x2044;
            HairHue = 1153;
            FacialHairItemID = 0x204B;
            FacialHairHue = 1153;
            Body = 0x190;
            CantWalk = true;

            AddItem(new Sandals(927));
            AddItem(new Robe(927));

            Blessed = true;
        }

        public Garamon(Serial serial)
            : base(serial)
        {
        }

        public virtual bool IsInvulnerable
        {
            get { return true; }
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

        public override bool HandlesOnSpeech(Mobile from)
        {
            if (from.InRange(Location, 8))
                return true;

            return base.HandlesOnSpeech(from);
        }

        public override void OnSpeech(SpeechEventArgs e)
        {
            if (!e.Handled && e.Mobile.InRange(Location, 2))
            {
                var pm = e.Mobile as PlayerMobile;

                if (pm.AbyssEntry)
                {
                    pm.SendMessage("You have completed a Sacred quest already!");
                }
                else
                {
                    var keyword = e.Speech;

                    switch (keyword)
                    {
                        case "Hello":
                        {
                            Say(
                                String.Format(
                                    "Greetings Adventurer! If you are seeking to enter the Abyss, I may be of assitance to you."));
                            break;
                        }
                        case "hello":
                        {
                            Say(
                                String.Format(
                                    "Greetings Adventurer! If you are seeking to enter the Abyss, I may be of assitance to you."));
                            break;
                        }
                        case "Key":
                        {
                            Say(String.Format("It's three parts that you must find, and reunite as one!"));
                            break;
                        }
                        case "key":
                        {
                            Say(String.Format("It's three parts that you must find, and reunite as one!"));
                            break;
                        }
                        case "Abyss":
                        {
                            Say(
                                String.Format(
                                    "It's entrance is protected by stone guardians who will only grant passage to the carrier of a Tripartite Key!"));
                            break;
                        }
                        case "abyss":
                        {
                            Say(
                                String.Format(
                                    "It's entrance is protected by stone guardians who will only grant passage to the carrier of a Tripartite Key!"));
                            break;
                        }
                        /*case "Britain":
                    {
                    this.Direction =  GetDirectionTo( pm.Location );
                    Say( String.Format( "Britain is far North of here.. I have not been there since I was a child." ) );
                    break;
                    }
                    case "britain":
                    {
                    this.Direction =  GetDirectionTo( pm.Location );
                    Say( String.Format( "Britain is far North of here.. I have not been there since I was a child." ) );
                    break;
                    }
                    case "Moongate":
                    {
                    this.Direction =  GetDirectionTo( pm.Location );
                    Say( String.Format( "There is a Moongate South of Trinsic in the Forest." ) );
                    break;
                    }
                    case "moongate":
                    {
                    this.Direction =  GetDirectionTo( pm.Location );
                    Say( String.Format( "There is a Moongate South of Trinsic in the Forest." ) );
                    break;
                    }*/
                    }
                }
                base.OnSpeech(e);
            }
        }
    }
}