#region References

using System;
using System.Collections.Generic;
using System.Net;

#endregion

namespace Server.Engines.XmlSpawner2
{
	public class XmlReferralRewards : XmlAttachment
	{
        
        private int m_PointsAvailable;
		private int m_PointsSpent;
		private int m_RewardsChosen;
		private DateTime m_LastRewardChosen;
		public List<IPAddress> ReferredList = new List<IPAddress>();  //something im about to do, but now we test git.

		public int PointsAvailable
		{
			get { return m_PointsAvailable; }
			set { m_PointsAvailable = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int PointsSpent
		{
			get { return m_PointsSpent; }
			set { m_PointsSpent = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int RewardsChosen
		{
			get { return m_RewardsChosen; }
			set { m_RewardsChosen = value; }
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastRewardChosen
		{
			get { return m_LastRewardChosen; }
			set { m_LastRewardChosen = value; }
		}

		public XmlReferralRewards(ASerial serial)
			: base(serial)
		{
		}

		[Attachable]
		public XmlReferralRewards()
		{
		}

		public override void OnDelete()
		{
			base.OnDelete();

		    if (!(AttachedTo is Mobile))
		    {
		        return;
		    }

		    Mobile m = AttachedTo as Mobile;

		    if (m.Deleted)
		    {
		        return;
		    }

		    Effects.PlaySound(m, m.Map, 958);
		    m.SendMessage(String.Format("You are no longer eligable for referral rewards."));
		}

        public static void Configure()
        {
            // Register our event handler
            EventSink.CharacterCreated += new CharacterCreatedEventHandler(EventSink_CharacterCreated);
        }

	    private static void EventSink_CharacterCreated(CharacterCreatedEventArgs args)
	    {
	        Mobile from = args.Mobile;

	        if (from != null)
	        {
                XmlAttach.AttachTo(from, new XmlReferralRewards());
	        }
	    }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

		    switch (version)
		    {
		        case 1:
		        {
                    int referredcount = reader.ReadInt();

                    if (referredcount > 0)
                    {
                        ReferredList = new List<IPAddress>();

                        for (int i = 0; i < referredcount; i++)
                        {
                            IPAddress r = reader.ReadIPAddress();
                            ReferredList.Add(r);
                        }
                    }

		            goto case 0;
		        }
                case 0:
		        {
                    m_PointsAvailable = reader.ReadInt();
                    m_PointsSpent = reader.ReadInt();
                    m_RewardsChosen = reader.ReadInt();
                    m_LastRewardChosen = reader.ReadDateTime();

		            break;
		        }
		    }
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(1); // version

            writer.Write(ReferredList.Count);

            foreach (IPAddress ip in ReferredList)
            {
                writer.Write(ip);
            }

			writer.Write(m_PointsAvailable);
			writer.Write(m_PointsSpent);
			writer.Write(m_RewardsChosen);
			writer.Write(m_LastRewardChosen);
		}
	}
}