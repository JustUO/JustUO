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
	public class CorruptedCrystalPortal : Item, ISecurable
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
		public CorruptedCrystalPortal() : base()
		{	
			//Weight= 20;
			ItemID= 18059;
			Hue= 1164;
			Name= "Corrupted Crystal Portal";
			Movable= true;
			LootType = LootType.Blessed;
			m_Level = SecureLevel.Anyone;
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
			else {
				list.Add( "Double-click to open help menu" );
			}
		}
	
		public CorruptedCrystalPortal( Serial serial ) : base( serial ) { }
		
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
					else if (e.Speech.ToLower() == "dungeon covetous") {
						DoTeleport( e.Mobile, 2498, 921, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon deceit") {
						DoTeleport( e.Mobile, 4111, 434, 5, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon despise") {
						DoTeleport( e.Mobile, 1301, 1080, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon destard") {
						DoTeleport( e.Mobile, 1176, 2640, 2, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon ice") {
						DoTeleport( e.Mobile, 1999, 81, 4, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon fire") {
						DoTeleport( e.Mobile, 2923, 3409, 8, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon hythloth") {
						DoTeleport( e.Mobile, 4721, 3824, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon orc") {
						DoTeleport( e.Mobile, 1017, 1429, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon shame") {
						DoTeleport( e.Mobile, 511, 1565, 0, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon wrong") {
						DoTeleport( e.Mobile, 2043, 238, 10, Map.Trammel ); }
					else if (e.Speech.ToLower() == "dungeon wind") {
						DoTeleport( e.Mobile, 1361, 895, 0, Map.Trammel ); }
/// Malas
					else if (e.Speech.ToLower() == "dungeon doom") {
						DoTeleport( e.Mobile, 2368, 1267, -85, Map.Malas ); }
// is this the right citadel? uoguide.com says it is //
// Tokuno
					else if (e.Speech.ToLower() == "dungeon citadel") {
						DoTeleport( e.Mobile, 1345, 769, 19, Map.Tokuno ); }
					else if (e.Speech.ToLower() == "dungeon citadel") {
						DoTeleport( e.Mobile, 1345, 769, 19, Map.Tokuno ); }
					else if (e.Speech.ToLower() == "dungeon fandancer") {
						DoTeleport( e.Mobile, 970, 222, 23, Map.Tokuno ); }
					else if (e.Speech.ToLower() == "dungeon mines") {
						DoTeleport( e.Mobile, 257, 786, 63, Map.Tokuno ); }
/// Malas
					else if (e.Speech.ToLower() == "dungeon bedlam") {
						DoTeleport( e.Mobile, 2068, 1372, -75, Map.Malas ); }
					else if (e.Speech.ToLower() == "dungeon labyrinth") {
						DoTeleport( e.Mobile, 1732, 975, -75, Map.Malas ); }
// Ter Mur
					else if (e.Speech.ToLower() == "dungeon underworld") {
						DoTeleport( e.Mobile, 1143, 1085, -37, Map.TerMur ); }
					else if (e.Speech.ToLower() == "dungeon abyss") {
						DoTeleport( e.Mobile, 946, 71, 72, Map.TerMur ); }
// fel
					else if (e.Speech.ToLower() == "fel dungeon covetous") {
						DoTeleport( e.Mobile, 2498, 921, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon deceit") {
						DoTeleport( e.Mobile, 4111, 434, 5, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon despise") {
						DoTeleport( e.Mobile, 1301, 1080, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon destard") {
						DoTeleport( e.Mobile, 1176, 2640, 2, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon fire") {
						DoTeleport( e.Mobile, 2923, 3409, 8, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon hythloth") {
						DoTeleport( e.Mobile, 4721, 3824, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon ice") {
						DoTeleport( e.Mobile, 1999, 81, 4, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon orc") {
						DoTeleport( e.Mobile, 1017, 1429, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon shame") {
						DoTeleport( e.Mobile, 511, 1565, 0, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon wrong") {
						DoTeleport( e.Mobile, 2043, 238, 10, Map.Felucca ); }
					else if (e.Speech.ToLower() == "fel dungeon wind") {
						DoTeleport( e.Mobile, 1361, 895, 0, Map.Felucca ); }
			}
		} // end of OnSpeech
	}
}


