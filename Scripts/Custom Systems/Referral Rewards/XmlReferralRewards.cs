#region References

using System;

#endregion

namespace Server.Engines.XmlSpawner2
{
	public class XmlReferralRewards : XmlAttachment
	{
        
        private int m_PointsAvailable;
		private int m_PointsSpent;
		private int m_RewardsChosen;
		private DateTime m_LastRewardChosen;

		[CommandProperty(AccessLevel.GameMaster)]
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

			if (AttachedTo is Mobile)
			{
				Mobile m = AttachedTo as Mobile;
				if (!m.Deleted)
				{
					Effects.PlaySound(m, m.Map, 958);
					m.SendMessage(String.Format("You are no longer eligable for referral rewards on Blood Oath."));
				}
			}
		}

        public override void OnAttach()
        {
            base.OnAttach();
        }

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			m_PointsAvailable = reader.ReadInt();
			m_PointsSpent = reader.ReadInt();
			m_RewardsChosen = reader.ReadInt();
			m_LastRewardChosen = reader.ReadDateTime();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);
			writer.Write(0); // version

			writer.Write(m_PointsAvailable);
			writer.Write(m_PointsSpent);
			writer.Write(m_RewardsChosen);
			writer.Write(m_LastRewardChosen);
		}
	}
}