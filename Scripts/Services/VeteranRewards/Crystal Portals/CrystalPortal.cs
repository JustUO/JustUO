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
	public class CrystalPortal : Item
	{
	
		private Point3D m_PointDest;
		private Map m_MapDest;
		
		public Point3D PointDest{ get{ return m_PointDest; } }
		public Map MapDest{ get{ return m_MapDest; } }
		

		[Constructable]
		public CrystalPortal() : base()
		{	
			//Weight= 20;
			ItemID= 18059;
			Name= "Crystal Portal";
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
					// from.SendGump( new CrystalPortalGump( from ) ); 
				}
				else
				{
					from.SendGump( new CrystalPortalGump( from ) ); 
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
	
		public CrystalPortal( Serial serial ) : base( serial )
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
					else if (e.Speech.ToLower() == "britain mint")
					{
						/// location = new Point3D( x, y, z );
						Point3D loc = new Point3D( 1434, 1699, 2 );
						/// what map is the location above on?
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "bucs mint")
					{
						Point3D loc = new Point3D( 2724, 2192, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "cove mint")
					{
						Point3D loc = new Point3D( 2238, 1195, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "delucia mint")
					{
						Point3D loc = new Point3D( 5274, 3991, 37 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					//osi lists new haven listed as simply 'haven'. probably because there's no bank in 'old haven'//
					else if (e.Speech.ToLower() == "haven mint")
					{
						Point3D loc = new Point3D( 3500, 2571, 14 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "jhelom mint")
					{
						Point3D loc = new Point3D( 1417, 3821, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "magincia mint")
					{
						Point3D loc = new Point3D( 3728, 2164, 20 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "minoc mint")
					{
						Point3D loc = new Point3D( 2498, 561, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "moonglow mint")
					{
						Point3D loc = new Point3D( 4471, 1177, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "nujelm mint")
					{
						Point3D loc = new Point3D( 3770, 1308, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "papua mint")
					{
						Point3D loc = new Point3D( 5675, 3144, 12 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "serpent mint")
					{
						Point3D loc = new Point3D( 2895, 3479, 15 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "skara mint")
					{
						Point3D loc = new Point3D( 596, 2138, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "trinsic mint")
					{
						Point3D loc = new Point3D( 1823, 2821, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "vesper mint")
					{
						Point3D loc = new Point3D( 2899, 676, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "wind mint")
					{
						Point3D loc = new Point3D( 5345, 93, 15 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "luna mint")
					{
						Point3D loc = new Point3D( 1015, 527, -65 );
						Map map = Map.Malas;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "zento mint")
					{
						Point3D loc = new Point3D( 741, 1261, 30 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "ilshenar mint")
					{
						Point3D loc = new Point3D( 1232, 557, -19 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
				
	// fel banks  //

					else if (e.Speech.ToLower() == "fel britain mint")
					{
						Point3D loc = new Point3D( 1434, 1699, 2 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel bucs mint")
					{
						Point3D loc = new Point3D( 2724, 2192, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel cove mint")
					{
						Point3D loc = new Point3D( 2238, 1195, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel ocllo mint")
					{
						Point3D loc = new Point3D( 3687, 2523, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
					else if (e.Speech.ToLower() == "fel jhelom mint")
					{
						Point3D loc = new Point3D( 1417, 3821, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel magincia mint")
					{
						Point3D loc = new Point3D( 3728, 2164, 20 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel minoc mint")
					{
						Point3D loc = new Point3D( 2498, 561, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel moonglow mint")
					{
						Point3D loc = new Point3D( 4471, 1177, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel nujelm mint")
					{
						Point3D loc = new Point3D( 3770, 1308, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
					else if (e.Speech.ToLower() == "fel serpent mint")
					{
						Point3D loc = new Point3D( 2895, 3479, 15 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel skara mint")
					{
						Point3D loc = new Point3D( 596, 2138, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel trinsic mint")
					{
						Point3D loc = new Point3D( 1823, 2821, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel vesper mint")
					{
						Point3D loc = new Point3D( 2899, 676, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel wind mint")
					{
						Point3D loc = new Point3D( 1361, 895, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}

// tram moongates //

					else if (e.Speech.ToLower() == "britain moongate")
					{
						Point3D loc = new Point3D( 1336, 1997, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "haven moongate")
					{
						Point3D loc = new Point3D( 3763, 2771, 50 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "jhelom moongate")
					{
						Point3D loc = new Point3D( 1330, 3780, 0 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "magincia moongate")
					{
						Point3D loc = new Point3D( 3563, 2139, 34 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "minoc moongate")
					{
						Point3D loc = new Point3D( 2701, 692, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "moonglow moongate")
					{
						Point3D loc = new Point3D( 4467, 1283, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "skara moongate")
					{
						Point3D loc = new Point3D( 643, 2067, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "trinsic moongate")
					{
						Point3D loc = new Point3D( 1828, 2948, -20 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
			/* vesper doesn't have it's own moongate, it shares one with minoc. but osi has an entry for one, clilocs confirm this.*/		
					else if (e.Speech.ToLower() == "vesper moongate")
					{
						Point3D loc = new Point3D( 2701, 692, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
		// yew moongate not included in osi, but i'm adding it. //
					else if (e.Speech.ToLower() == "yew moongate")
					{
						Point3D loc = new Point3D( 771, 752, 5 );
						Map map = Map.Trammel;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
					else if (e.Speech.ToLower() == "compassion moongate")
					{
						Point3D loc = new Point3D( 1215, 467, -13 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "honesty moongate")
					{
						Point3D loc = new Point3D( 722, 1366, -60 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "honor moongate")
					{
						Point3D loc = new Point3D( 744, 724, -28 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "humility moongate")
					{
						Point3D loc = new Point3D( 281, 1016, 0 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "justice moongate")
					{
						Point3D loc = new Point3D( 987, 1011, -32 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "sacrifice moongate")
					{
						Point3D loc = new Point3D( 1174, 1286, -30 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "spirituality moongate")
					{
						Point3D loc = new Point3D( 1532, 1340, -3 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "valor moongate")
					{
						Point3D loc = new Point3D( 528, 216, -45 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "chaos moongate")
					{
						Point3D loc = new Point3D( 1721, 218, 96 );
						Map map = Map.Ilshenar;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
					else if (e.Speech.ToLower() == "luna moongate")
					{
						Point3D loc = new Point3D( 1015, 527, -65 );
						Map map = Map.Malas;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "umbra moongate")
					{
						Point3D loc = new Point3D( 1997, 1386, -85 );
						Map map = Map.Malas;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "termur moongate")
					{
						Point3D loc = new Point3D( 851, 3526, 0 );
						Map map = Map.TerMur;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "isamu moongate")
					{
						Point3D loc = new Point3D( 1169, 998, 41 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "makoto moongate")
					{
						Point3D loc = new Point3D( 802, 1204, 25 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "homare moongate")
					{
						Point3D loc = new Point3D( 270, 628, 15 );
						Map map = Map.Tokuno;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
	
	/////  fel moongates  ////////
	
					
					else if (e.Speech.ToLower() == "fel britain moongate")
					{
						Point3D loc = new Point3D( 1336, 1997, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel bucs moongate")
					{
						Point3D loc = new Point3D( 2711, 2234, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel jhelom moongate")
					{
						Point3D loc = new Point3D( 1330, 3780, 0 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel magincia moongate")
					{
						Point3D loc = new Point3D( 3563, 2139, 34 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel minoc moongate")
					{
						Point3D loc = new Point3D( 2701, 692, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel moonglow moongate")
					{
						Point3D loc = new Point3D( 4467, 1283, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel skara moongate")
					{
						Point3D loc = new Point3D( 643, 2067, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel trinsic moongate")
					{
						Point3D loc = new Point3D( 1828, 2948, -20 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel yew moongate")
					{
						Point3D loc = new Point3D( 771, 752, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					else if (e.Speech.ToLower() == "fel vesper moongate")
					{
						Point3D loc = new Point3D( 2701, 692, 5 );
						Map map = Map.Felucca;
						
						BaseCreature.TeleportPets( e.Mobile, loc, map );
						e.Mobile.MoveToWorld( loc, map );
						DoTeleport( e.Mobile );
					}
					
					
				}
		}
	}
}


