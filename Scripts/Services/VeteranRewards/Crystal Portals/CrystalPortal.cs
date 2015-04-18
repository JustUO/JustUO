/*
Many thanks to Milva, Enroq, and daat99 for their help in the support forums. 

If you want to edit locations or add your own, look at the sections starting around line 150. 
Just remember, the text that has to be spoken must be all lower case. 

*/

using System;
using Server;
using Server.Network;
using Server.Mobiles;
using Server.Engines.VeteranRewards;
using Server.Gumps;
using Server.Multis;
using Server.ContextMenus;
using System.Collections.Generic;



namespace Server.Items
{
	public class CrystalPortal : Item, ISecurable
	{
	
		private Point3D m_PointDest;
		private Map m_MapDest;
		private SecureLevel m_Level;
		
		public Point3D PointDest{ get{ return m_PointDest; } }
		public Map MapDest{ get{ return m_MapDest; } }
		
		[CommandProperty(AccessLevel.GameMaster)]
		public SecureLevel Level 
		{
			get { return this.m_Level; }
			set { this.m_Level = value; }
		}
		

		[Constructable]
		public CrystalPortal() : base()
		{	
			//Weight= 20;
			ItemID= 18059;
			Name= "Crystal Portal";
			Movable= true;
			LootType = LootType.Blessed;
		}

		public override void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			base.GetContextMenuEntries(from, list);
			SetSecureLevelEntry.AddTo(from, this, list);
		}
		
		public bool CheckAccess(Mobile m)
		{
			BaseHouse house = BaseHouse.FindHouseAt(this);
			
			if (house != null && (house.Public ? house.IsBanned(m) : !house.HasAccess(m)))
				return false;
				
			return (house != null && house.HasSecureAccess(m, this.m_Level));
		}
		
		public override void OnDoubleClick( Mobile from )
		{
			// Players who do not have proper house privileges will not be able to open the gump	
			if ( !this.CheckAccess(from)) { 
				from.SendLocalizedMessage(1061637); /*You are not allowed to access this.*/ 
				return;
			}
			if( from.InRange( Location, 3 ) ) {
				if( this.Movable ) {
					from.SendMessage("This must be locked down in a house to use!");
				}
				else{ from.SendGump( new CorruptedCrystalPortalGump( from ) ); }
			}
			else{ from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); /* I can't reach that.*/ }
		}
		
		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			
			if( this.Movable ) {
				list.Add( "This must be locked down in a house to use!" );
			}
			else{
				list.Add( "Double-click to open help menu" );
			}
		}
	
		public CrystalPortal( Serial serial ) : base( serial ) { }
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
			
			writer.WriteEncodedInt((int)this.m_Level);
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
			
			this.m_Level = (SecureLevel)reader.ReadEncodedInt();
		}
		
		public virtual void DoTeleport( Mobile m, int x, int y, int z, Map map )
		{
			Point3D loc = new Point3D( x, y ,z );
			BaseCreature.TeleportPets( m, loc, map );
			
			m.MoveToWorld( new Point3D(x, y, z), map );
			
			Effects.SendLocationEffect( m.Location, m.Map, 0x3728, 10, 10 );
			m.PlaySound( 0x1FE );
		 }
		

		public override bool HandlesOnSpeech{ get{ return true; } }

        public override void OnSpeech( SpeechEventArgs e )
        {
			// Players who do not have proper house access privileges will not be able to use this for travel
			if( !this.CheckAccess(e.Mobile) ) { return; }
			
            if (!e.Handled && e.Mobile.InRange(this.Location, 2)) {
					if (this.Movable)
						e.Mobile.SendMessage("This must be locked down in a house to use!");
					else if ( Factions.Sigil.ExistsOn( e.Mobile ) )
						e.Mobile.SendLocalizedMessage( 1061632 ); // You can't do that while carrying the sigil.
					else if ( Server.Misc.WeightOverloading.IsOverloaded( e.Mobile ) )
						e.Mobile.SendLocalizedMessage( 502359, "", 0x22 ); // Thou art too encumbered to move.
					else if ( e.Mobile.Criminal )
						e.Mobile.SendLocalizedMessage( 1005561, "", 0x22 ); // Thou'rt a criminal and cannot escape so easily.
					else if ( Server.Spells.SpellHelper.CheckCombat( e.Mobile ) )
						e.Mobile.SendLocalizedMessage( 1005564, "", 0x22 ); // Wouldst thou flee during the heat of battle??
					else if ( e.Mobile.Spell != null )
						e.Mobile.SendLocalizedMessage( 1049616 ); // You are too busy to do that at the moment.
/*  Begin Speech Entries  */
// tram banks //
					else if (e.Speech.ToLower() == "britain mint") {
						DoTeleport( e.Mobile, 1434, 1699, 2, Map.Trammel ); }
					else if (e.Speech.ToLower() == "bucs mint") {
						DoTeleport( e.Mobile, 2724, 2192, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "cove mint") {
						DoTeleport( e.Mobile, 2238, 1195, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "delucia mint") {
						DoTeleport( e.Mobile, 5274, 3991, 37, Map.Trammel ); }
					//osi lists new haven listed as simply 'haven'. probably because there's no bank in 'old haven'//
					else if (e.Speech.ToLower() == "haven mint") {
						DoTeleport( e.Mobile, 3500, 2571, 14, Map.Trammel ); }
					else if (e.Speech.ToLower() == "jhelom mint") {
						DoTeleport( e.Mobile, 1417, 3821, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "magincia mint") {
						DoTeleport( e.Mobile, 3728, 2164, 20, Map.Trammel ); }
					else if (e.Speech.ToLower() == "minoc mint") {
						DoTeleport( e.Mobile, 2498, 561, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "moonglow mint") {
						DoTeleport( e.Mobile, 4471, 1177, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "nujelm mint") {
						DoTeleport( e.Mobile, 3770, 1308, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "papua mint") {
						DoTeleport( e.Mobile, 5675, 3144, 12, Map.Trammel ); }
					else if (e.Speech.ToLower() == "serpent mint") {
						DoTeleport( e.Mobile, 2895, 3479, 15, Map.Trammel ); }
					else if (e.Speech.ToLower() == "skara mint") {
						DoTeleport( e.Mobile, 596, 2138, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "trinsic mint") {
						DoTeleport( e.Mobile, 1823, 2821, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "vesper mint") {
						DoTeleport( e.Mobile, 2899, 676, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "wind mint") {
						DoTeleport( e.Mobile, 5345, 93, 15, Map.Trammel ); }
					else if (e.Speech.ToLower() == "luna mint") {
						DoTeleport( e.Mobile, 1015, 527, -65, Map.Trammel ); }
					else if (e.Speech.ToLower() == "zento mint") {
						DoTeleport( e.Mobile,741, 1261, 30, Map.Trammel); }
					else if (e.Speech.ToLower() == "ilshenar mint") {
						DoTeleport( e.Mobile, 1232, 557, -19, Map.Trammel ); }
	// fel banks  //
					else if (e.Speech.ToLower() == "fel britain mint") {
						DoTeleport( e.Mobile, 1434, 1699, 2, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel bucs mint") {
						DoTeleport( e.Mobile, 2724, 2192, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel cove mint") {
						DoTeleport( e.Mobile, 2238, 1195, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel ocllo mint") {
						DoTeleport( e.Mobile, 3687, 2523, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel jhelom mint") {
						DoTeleport( e.Mobile, 1417, 3821, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel magincia mint") {
						DoTeleport( e.Mobile, 3728, 2164, 20, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel minoc mint") {
						DoTeleport( e.Mobile, 2498, 561, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel moonglow mint") {
						DoTeleport( e.Mobile, 4471, 1177, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel nujelm mint") {
						DoTeleport( e.Mobile, 3770, 1308, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel serpent mint") {
						DoTeleport( e.Mobile, 2895, 3479, 15, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel skara mint") {
						DoTeleport( e.Mobile, 596, 2138, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel trinsic mint") {
						DoTeleport( e.Mobile, 1823, 2821, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel vesper mint") {
						DoTeleport( e.Mobile, 2899, 676, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel wind mint") {
						DoTeleport( e.Mobile, 1361, 895, 0, Map.Felucca ); }
// tram moongates //
					else if (e.Speech.ToLower() == "britain moongate") {
						DoTeleport( e.Mobile, 1336, 1997, 5, Map.Trammel ); }
					else if (e.Speech.ToLower() == "haven moongate") {
						DoTeleport( e.Mobile, 3763, 2771, 50, Map.Trammel ); }
					else if (e.Speech.ToLower() == "jhelom moongate") {
						DoTeleport( e.Mobile, 1330, 3780, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "magincia moongate") {
						DoTeleport( e.Mobile, 3563, 2139, 34, Map.Trammel ); }
					else if (e.Speech.ToLower() == "minoc moongate") {
						DoTeleport( e.Mobile, 2701, 692, 5, Map.Trammel ); }
					else if (e.Speech.ToLower() == "moonglow moongate") {
						DoTeleport( e.Mobile, 4467, 1283, 5, Map.Trammel ); }
					else if (e.Speech.ToLower() == "skara moongate") {
						DoTeleport( e.Mobile,643, 2067, 5, Map.Trammel ); }
					else if (e.Speech.ToLower() == "trinsic moongate") {
						DoTeleport( e.Mobile, 1828, 2948, -20, Map.Trammel ); }
			/* vesper doesn't have it's own moongate, it shares one with minoc. but osi has an entry for one, clilocs confirm this.*/		
					else if (e.Speech.ToLower() == "vesper moongate") {
						DoTeleport( e.Mobile, 2701, 692, 5, Map.Trammel ); }
		// yew moongate not included in osi, but i'm adding it. //
					else if (e.Speech.ToLower() == "yew moongate") {
						DoTeleport( e.Mobile, 771, 752, 5, Map.Trammel ); }
///  Ilshenar
					else if (e.Speech.ToLower() == "compassion moongate") {
						DoTeleport( e.Mobile,1215, 467, -13, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "honesty moongate") {
						DoTeleport( e.Mobile, 722, 1366, -60, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "honor moongate") {
						DoTeleport( e.Mobile,744, 724, -28, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "humility moongate") {
						DoTeleport( e.Mobile, 281, 1016, 0, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "justice moongate") {
						DoTeleport( e.Mobile, 987, 1011, -32, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "sacrifice moongate") {
						DoTeleport( e.Mobile, 1174, 1286, -30, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "spirituality moongate") {
						DoTeleport( e.Mobile, 1532, 1340, -3, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "valor moongate") {
						DoTeleport( e.Mobile, 528, 216, -45, Map.Ilshenar ); }
					else if (e.Speech.ToLower() == "chaos moongate") {
						DoTeleport( e.Mobile, 1721, 218, 96, Map.Ilshenar ); }
/// Malas
					else if (e.Speech.ToLower() == "luna moongate") {
						DoTeleport( e.Mobile, 1015, 527, -65, Map.Malas ); }
					else if (e.Speech.ToLower() == "umbra moongate") {
						DoTeleport( e.Mobile, 1997, 1386, -85, Map.Malas ); }
/// Ter Mur
					else if (e.Speech.ToLower() == "termur moongate") {
						DoTeleport( e.Mobile, 851, 3526, 0, Map.TerMur ); }
// Tokuno
					else if (e.Speech.ToLower() == "isamu moongate") {
						DoTeleport( e.Mobile, 1169, 998, 41, Map.Tokuno ); }
					else if (e.Speech.ToLower() == "makoto moongate") {
						DoTeleport( e.Mobile, 802, 1204, 25, Map.Tokuno ); }
					else if (e.Speech.ToLower() == "homare moongate") {
						DoTeleport( e.Mobile, 270, 628, 15, Map.Tokuno ); }
/// fel moongates  
					else if (e.Speech.ToLower() == "fel britain moongate") {
						DoTeleport( e.Mobile, 1336, 1997, 5, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel bucs moongate") {
						DoTeleport( e.Mobile, 2711, 2234, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel jhelom moongate") {
						DoTeleport( e.Mobile, 1330, 3780, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel magincia moongate") {
						DoTeleport( e.Mobile, 3563, 2139, 34, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel minoc moongate") {
						DoTeleport( e.Mobile, 2701, 692, 5, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel moonglow moongate") {
						DoTeleport( e.Mobile, 4467, 1283, 5, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel skara moongate") {
						DoTeleport( e.Mobile,643, 2067, 5, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel trinsic moongate") {
						DoTeleport( e.Mobile,1828, 2948, -20, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel yew moongate") {
						DoTeleport( e.Mobile, 771, 752, 5, Map.Felucca); }
					else if (e.Speech.ToLower() == "fel vesper moongate") {
						DoTeleport( e.Mobile, 2701, 692, 5, Map.Felucca ); }
			}
		} //end of OnSpeech method
	}
}


