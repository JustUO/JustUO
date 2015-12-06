using System;
using System.Collections;
using Server.Network;
using Server.Items;
using Server.Gumps;
using Server.Targeting;
using Server.Mobiles;

namespace Server.Engines.Help
{
	public class SelectComplaintTargetsGump : Gump
	{
		public override int TypeID { get { return 0x233E; } }

		private string text;
		private PageType m_Type;

		public SelectComplaintTargetsGump( string m_text, PageType type )
			: base( 0, 0 )
		{
			text = m_text;

			m_Type = type;

			AddBackground( 50, 50, 400, 400, 0xA28 );

			AddPage( 0 );

			AddHtmlLocalized( 165, 70, 200, 20, 1070961, false, false ); // Select Complaint Targets

			AddHtmlLocalized( 75, 95, 350, 145, 1074790, false, false ); // You may select up to three players as the targets of this complaint.  Please select one of the options below to select the players by either targeting them, typing their names or selecting their names from a list.  Since their may be several players with the same name, targeting the players is the most accurate way to identify the players.

			AddHtmlLocalized( 110, 240, 350, 145, 1074789, false, false ); // Target Involved Players
			AddButton( 80, 240, 0xD0, 0xD1, 1, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 110, 270, 350, 145, 1074785, false, false ); // Type Names of Involved Players
			AddButton( 80, 270, 0xD0, 0xD1, 2, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 110, 300, 350, 145, 1074786, false, false ); // Select Names of Involved Players From List
			AddButton( 80, 300, 0xD0, 0xD1, 3, GumpButtonType.Reply, 0 );

			AddButton( 320, 360, 0x819, 0x818, 0, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			switch ( info.ButtonID )
			{
				case 0:
					{
						from.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.

						break;
					}
				case 1:
					{
						from.SendGump( new TargetInvolvedPlayersGump( text, m_Type, "", "", "" ) );

						break;
					}
				case 2:
					{
						from.SendGump( new TypeNamesOfInvolvedPlayersGump( text, m_Type, "", "", "" ) );

						break;
					}
				case 3:
					{
						from.SendGump( new SelectNamesOfInvolvedPlayersFromListGump( text, m_Type ) );

						break;
					}
			}
		}
	}

	public class TargetInvolvedPlayersGump : Gump
	{
		public override int TypeID { get { return 0x233B; } }

		private string text;
		private PageType m_Type;

		private string first, second, third;

		public TargetInvolvedPlayersGump( string m_text, PageType type, string m_first, string m_second, string m_third )
			: base( 0, 0 )
		{
			text = m_text;

			m_Type = type;

			first = m_first;

			second = m_second;

			third = m_third;

			AddBackground( 50, 50, 400, 300, 0xA28 );

			AddPage( 0 );

			AddHtmlLocalized( 165, 70, 200, 20, 1070961, false, false ); // Select Complaint Targets

			AddHtmlLocalized( 75, 95, 350, 145, 1070960, false, false ); // You may select up to three players as the targets of this complaint.

			AddHtmlLocalized( 110, 180, 350, 145, 1070962, false, false ); // Complaint Target: 
			AddButton( 80, 180, 0xD0, 0xD1, 1, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 110, 210, 350, 145, 1070962, false, false ); // Complaint Target: 
			AddButton( 80, 210, 0xD0, 0xD1, 2, GumpButtonType.Reply, 0 );

			AddHtmlLocalized( 110, 240, 350, 145, 1070962, false, false ); // Complaint Target: 
			AddButton( 80, 240, 0xD0, 0xD1, 3, GumpButtonType.Reply, 0 );

			AddButton( 125, 290, 0x81A, 0x81B, 4, GumpButtonType.Reply, 0 );

			AddButton( 320, 290, 0x819, 0x818, 0, GumpButtonType.Reply, 0 );

			AddLabel( 220, 180, 0xA28, first );

			AddLabel( 220, 210, 0xA28, second );

			AddLabel( 220, 240, 0xA28, third );
		}

        public override void OnResponse(NetState state, RelayInfo info)
		{
			Mobile from = state.Mobile;

			switch ( info.ButtonID )
			{
				case 0:
					{
						from.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.

						break;
					}
				case 1:
					{
						from.SendLocalizedMessage( 1070956 ); // Which player is this complaint about?

						from.Target = new SelectTarget( text, m_Type, first, second, third, 1 );

						break;
					}
				case 2:
					{
						from.SendLocalizedMessage( 1070956 ); // Which player is this complaint about?

						from.Target = new SelectTarget( text, m_Type, first, second, third, 2 );

						break;
					}
				case 3:
					{
						from.SendLocalizedMessage( 1070956 ); // Which player is this complaint about?

						from.Target = new SelectTarget( text, m_Type, first, second, third, 3 );

						break;
					}
				case 4:
					{
						string information = String.Format( " Involved Players: \n {0} \n {1} \n {2}", first, second, third );
						text += information;

						PageQueue.Enqueue( new PageEntry( from, text, m_Type ) );

						break;
					}
			}
		}
	}

	public class SelectTarget : Target
	{
		private int number;
		private string first;
		private string second;
		private string third;

		private string text;
		private PageType m_Type;

		public SelectTarget( string m_text, PageType type, string m_first, string m_second, string m_third, int m_number )
			: base( 8, false, TargetFlags.None )
		{
			number = m_number;
			first = m_first;
			second = m_second;
			third = m_third;

			text = m_text;
			m_Type = type;
		}

		protected override void OnTarget( Mobile from, object targeted )
		{
			if ( targeted is PlayerMobile )
			{
				PlayerMobile pm = targeted as PlayerMobile;

				switch ( number )
				{
					case 1:
						{
							first = pm.Name;

							break;
						}
					case 2:
						{
							second = pm.Name;

							break;
						}
					case 3:
						{
							third = pm.Name;

							break;
						}
				}
			}

			from.SendGump( new TargetInvolvedPlayersGump( text, m_Type, first, second, third ) );
		}

		protected override void OnTargetCancel( Mobile from, TargetCancelType cancelType )
		{
			from.SendGump( new TargetInvolvedPlayersGump( text, m_Type, first, second, third ) );
		}
	}

	public class TypeNamesOfInvolvedPlayersGump : Gump
	{
		public override int TypeID { get { return 0x233C; } }

		private string text;
		private PageType m_Type;

		private string first, second, third;

		public TypeNamesOfInvolvedPlayersGump( string m_text, PageType type, string m_first, string m_second, string m_third )
			: base( 0, 0 )
		{
			text = m_text;

			m_Type = type;

			first = m_first;
			second = m_second;
			third = m_third;

			AddBackground( 50, 50, 440, 300, 0xA28 );

			AddPage( 0 );

			AddHtmlLocalized( 165, 70, 200, 20, 1074785, false, false ); // Type Names of Involved Players

			AddHtmlLocalized( 75, 95, 350, 145, 1074787, false, false ); // Type the names of up to three players involved in this complaint.

			AddButton( 125, 290, 0x81A, 0x81B, 4, GumpButtonType.Reply, 0 );

			AddButton( 320, 290, 0x819, 0x818, 0, GumpButtonType.Reply, 0 );

			AddBackground( 75, 175, 385, 30, 0xBB8 );
			AddTextEntry( 80, 180, 375, 30, 0x481, 0, first );

			AddBackground( 75, 205, 385, 30, 0xBB8 );
			AddTextEntry( 80, 210, 375, 30, 0x481, 1, second );

			AddBackground( 75, 235, 385, 30, 0xBB8 );
			AddTextEntry( 80, 240, 375, 30, 0x481, 2, third );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			switch ( info.ButtonID )
			{
				case 0:
					{
						from.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.

						break;
					}
				case 4:
					{
						TextRelay entry = info.GetTextEntry( 0 );
						first = ( entry == null ? "" : entry.Text.Trim() );

						entry = info.GetTextEntry( 1 );
						second = ( entry == null ? "" : entry.Text.Trim() );

						entry = info.GetTextEntry( 2 );
						third = ( entry == null ? "" : entry.Text.Trim() );

						string information = String.Format( " Involved Players: \n {0} \n {1} \n {2}", first, second, third );
						text += information;

						PageQueue.Enqueue( new PageEntry( from, text, m_Type ) );

						break;
					}
			}
		}
	}

	public class SelectNamesOfInvolvedPlayersFromListGump : Gump
	{
		public override int TypeID { get { return 0x233D; } }

		private string text;
		private PageType m_Type;

		public SelectNamesOfInvolvedPlayersFromListGump( string m_text, PageType type )
			: base( 0, 0 )
		{
			text = m_text;

			m_Type = type;

			AddBackground( 50, 50, 400, 360, 0xA28 );

			AddPage( 0 );

			AddHtmlLocalized( 165, 70, 200, 20, 1074786, false, false ); // Select Names of Involved Players From List

			AddHtmlLocalized( 75, 95, 350, 145, 1074788, false, false ); // Select the names of up to three players involved in this complaint from the list below.

			AddButton( 125, 350, 0x81A, 0x81B, 1, GumpButtonType.Reply, 0 );

			AddButton( 320, 350, 0x819, 0x818, 0, GumpButtonType.Reply, 0 );
		}

		public override void OnResponse( NetState state, RelayInfo info )
		{
			Mobile from = state.Mobile;

			switch ( info.ButtonID )
			{
				case 0:
					{
						from.SendLocalizedMessage( 501235, "", 0x35 ); // Help request aborted.

						break;
					}
				case 1:
					{
						PageQueue.Enqueue( new PageEntry( from, text, m_Type ) );

						break;
					}
			}
		}
	}
}