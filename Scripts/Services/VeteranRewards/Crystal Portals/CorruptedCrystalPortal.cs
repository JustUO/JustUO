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




namespace Server.Items
{
	public class CorruptedCrystalPortal : Item
	{
	
		private Point3D m_PointDest;
		private Map m_MapDest;
		
		public Point3D PointDest{ get{ return m_PointDest; } }
		public Map MapDest{ get{ return m_MapDest; } }
		

		[Constructable]
		public CorruptedCrystalPortal() : base()
		{	
			//Weight= 20;
			ItemID= 18059;
			Hue= 1164;
			Name= "Corrupted Crystal Portal";
			Movable= true;
			LootType = LootType.Blessed;
		}
		
		 public override void OnDoubleClick( Mobile from )
		{
			
            if( from.InRange( Location, 3 ) )
            {
				if( this.Movable )
				{
					from.SendMessage("This must be locked down in a house to use!");
					// from.SendGump( new CorruptedCrystalPortalGump( from ) ); 
				}
				else
				{
					from.SendGump( new CorruptedCrystalPortalGump( from ) ); 
				}
			}
			else
				from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.

		}
		
		public override void GetProperties( ObjectPropertyList list )
		{
			base.GetProperties( list );
			
			if( this.Movable )
			{
				list.Add( "This must be locked down in a house to use!" );
			}
			else
			{
				list.Add( "Double-click to open help menu" );
			}
		}
	
		public CorruptedCrystalPortal( Serial serial ) : base( serial )
		{
			
		}
		
		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 ); // version
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();
		}
		
		public virtual void DoTeleport( Mobile m )
		{
			Effects.SendLocationEffect( m.Location, m.Map, 0x3728, 10, 10 );
			m.PlaySound( 0x1FE );
		}
		

		public override bool HandlesOnSpeech{ get{ return true; } }

        public override void OnSpeech( SpeechEventArgs e )
        {
            if (!e.Handled && e.Mobile.InRange(this.Location, 2))
                {
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
					else if (e.Speech.ToLower() == "dungeon covetous")
					{
						/// location = new Point3D( x, y, z );
						Point3D loc = new Point3D( 2498, 921, 0 );
						/// what map is the location above on?
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon deceit")
					{
						Point3D loc = new Point3D( 4111, 434, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon despise")
					{
						Point3D loc = new Point3D( 1301, 1080, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon destard")
					{
						Point3D loc = new Point3D( 1176, 2640, 2 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon ice")
					{
						Point3D loc = new Point3D( 1999, 81, 4 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon fire")
					{
						Point3D loc = new Point3D( 2923, 3409, 8 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon hythloth")
					{
						Point3D loc = new Point3D( 4721, 3824, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon orc")
					{
						Point3D loc = new Point3D( 1017, 1429, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon shame")
					{
						Point3D loc = new Point3D( 511, 1565, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon wrong")
					{
						Point3D loc = new Point3D( 2043, 238, 10 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon wind")
					{
						Point3D loc = new Point3D( 1361, 895, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon doom")
					{
						Point3D loc = new Point3D( 2368, 1267, -85 );
						Map map = Map.Malas;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
		// is this the right citadel? uoguide.com says it is //
					else if (e.Speech.ToLower() == "dungeon citadel")
					{
						Point3D loc = new Point3D( 1345, 769, 19 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon citadel")
					{
						Point3D loc = new Point3D( 1345, 769, 19 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon fandancer")
					{
						Point3D loc = new Point3D( 970, 222, 23 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon mines")
					{
						Point3D loc = new Point3D( 257, 786, 63 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon bedlam")
					{
						Point3D loc = new Point3D( 2068, 1372, -75 );
						Map map = Map.Malas;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon labyrinth")
					{
						Point3D loc = new Point3D( 1732, 975, -75 );
						Map map = Map.Malas;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon underworld")
					{
						Point3D loc = new Point3D( 1143, 1085, -37 );
						Map map = Map.TerMur;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "dungeon abyss")
					{
						Point3D loc = new Point3D( 946, 71, 72 );
						Map map = Map.TerMur;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
		// fel //
					else if (e.Speech.ToLower() == "fel dungeon covetous")
					{
						Point3D loc = new Point3D( 2498, 921, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon deceit")
					{
						Point3D loc = new Point3D( 4111, 434, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon despise")
					{
						Point3D loc = new Point3D( 1301, 1080, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon destard")
					{
						Point3D loc = new Point3D( 1176, 2640, 2 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon fire")
					{
						Point3D loc = new Point3D( 2923, 3409, 8 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon hythloth")
					{
						Point3D loc = new Point3D( 4721, 3824, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon ice")
					{
						Point3D loc = new Point3D( 1999, 81, 4 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon orc")
					{
						Point3D loc = new Point3D( 1017, 1429, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon shame")
					{
						Point3D loc = new Point3D( 511, 1565, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon wrong")
					{
						Point3D loc = new Point3D( 2043, 238, 10 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel dungeon wind")
					{
						Point3D loc = new Point3D( 1361, 895, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
				}
		}
	}
}


