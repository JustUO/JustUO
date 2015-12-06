using System;
using Server;
using Server.Engines.VeteranRewards;
using Server.Mobiles;
using Server.Network;

namespace Server.Gumps
{
	public class StatRewardGump : Gump
	{
		public override int TypeID { get { return 0x193; } }

		private PlayerMobile m_Mobile;

		public StatRewardGump( PlayerMobile pm )
			: base( 200, 200 )
		{
			m_Mobile = pm;

			AddPage( 0 );

			AddBackground( 0, 0, 291, 173, 0x13BE );
			AddImageTiled( 5, 5, 280, 140, 0xA40 );

			/*
			 * <B>Ultima Online Rewards Program</B>
			 * 
			 * Thank you for being part of the Ultima
			 * Online community for over 6 months. As a
			 * token of our appreciation, your stat cap
			 * will be increased. 
			 */
			AddHtmlLocalized( 9, 9, 272, 140, 1076664, 0x7FFF, false, false );

			AddButton( 160, 147, 0xFB7, 0xFB8, 1, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 195, 149, 120, 20, 1006044, 0x7FFF, false, false ); // OK

			AddButton( 5, 147, 0xFB1, 0xFB2, 0, GumpButtonType.Reply, 0 );
			AddHtmlLocalized( 40, 149, 100, 20, 1060051, 0x7FFF, false, false ); // CANCEL
		}

		public override void OnResponse( NetState sender, RelayInfo info )
		{
			if ( info.ButtonID == 1 && RewardSystem.IsEligibleForStatReward( m_Mobile ) )
			{
				m_Mobile.HasStatReward = true;
				m_Mobile.StatCap += 5;
				m_Mobile.SendLocalizedMessage( 1062312 ); // Your stat cap has been increased.
			}
		}
	}
}
