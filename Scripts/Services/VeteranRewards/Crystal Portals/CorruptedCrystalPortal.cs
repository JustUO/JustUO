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
	    //Weight = 20;
	    ItemID = 18059;
	    Hue = 1164;
	    Name = "Corrupted Crystal Portal";
	    Movable = true;
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
		
	public virtual void DoTeleport( Mobile m, Point3D dest, Map map ) {
	    if (m == null || dest == Point3D.Zero || map == null || map == Map.Internal) {
		return; //no soup for you
	    }
	    //Vorpsire is correct here:  Play sounds when they leave and also when they arrive.
	    Effects.SendLocationEffect( m.Location, m.Map, 0x3728, 10, 10 );
	    Effects.PlaySound( m.Location, m.Map, 0x1FE );
		    
	    BaseCreature.TeleportPets( m, loc, map );
	    m.MoveToWorld( new Point3D(x, y, z), map );

	    Effects.SendLocationEffect( m.Location, m.Map, 0x3728, 10, 10 );
	    Effects.PlaySound( m.Location, m.Map, 0x1FE );
	}
		
	public override bool HandlesOnSpeech{ get{ return true; } }

        public override void OnSpeech( SpeechEventArgs e )
        {
	    if( !this.CheckAccess(e.Mobile) || e.Handled || e.Blocked || !e.Mobile.InRange(Location, 2) ) {
		return;
	    }

	    Point3D dest = Point3D.Zero;
	    Map map = null;

	    ResolveDest(e.Speech.Trim(), ref dest, ref map); //we want exclusivity in teleporting ergo use a switch not multiple ifs

	    if (dest == Point3D.Zero || map == null, map == Map.Internal) {
		return;
	    }
	    
	    e.Handled = true;
	    //make sure they aren't in combat, carrying a sigil, etc.
	    if (ValidateUse(e.Mobile, true) ) {
		DoTeleport(e.Mobile, dest, map);
	    }   
	}

	public static void ResolveDest(string name, ref Point3D loc, ref Map map)
	{
	    if (String.IsNullOrWhiteSpace(name))
	    {
		return;
	    }
	    
	    switch (name.Trim().ToLower())
	    {
		case "dungeon covetous":
		    {
			loc = new Point3D( 2498, 921, 0 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon deceit":
		    {
			loc = new Point3D( 4111, 434, 5 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon despise":
		    {
			loc = new Point3D( 1301, 1080, 0 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon destard":
		    {
			loc = new Point3D( 1176, 2640, 2 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon ice":
		    {
			loc = new Point3D( 1999, 81, 4 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon fire":
		    {
			loc = new Point3D( 2923, 3409, 8 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon hythloth":
		    {
			loc = new Point3D( 4721, 3824, 0 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon orc":
		    {
			loc = new Point3D( 1017, 1429, 0 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon shame":
		    {
			loc = new Point3D( 511, 1565, 0 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon wrong":
		    {
			loc = new Point3D( 2043, 238, 10 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon wind":
		    {
			loc = new Point3D( 1361, 895, 0 );
			map = Map.Trammel;
		    }
		    break;
		case "dungeon doom":
		    {
			loc = new Point3D( 2368, 1267, -85 );
			map = Map.Malas;
		    }
		    break;
		    
		    // is this the right citadel? uoguide.com says it is
		case "dungeon citadel":
		    {
			loc = new Point3D( 1345, 769, 19 );
			map = Map.Tokuno;
		    }
		    break;
		case "dungeon fandancer":
		    {
			loc = new Point3D( 970, 222, 23 );
			map = Map.Tokuno;
		    }
		    break;
		case "dungeon mines":
		    {
			loc = new Point3D( 257, 786, 63 );
			map = Map.Tokuno;
		    }
		    break;
		case "dungeon bedlam":
		    {
			loc = new Point3D( 2068, 1372, -75 );
			map = Map.Malas;
		    }
		    break;
		case "dungeon labyrinth":
		    {
			loc = new Point3D( 1732, 975, -75 );
			map = Map.Malas;
		    }
		    break;
		case "dungeon underworld":
		    {
			loc = new Point3D( 1143, 1085, -37 );
			map = Map.TerMur;
		    }
		    break;
		case "dungeon abyss":
		    {
			loc = new Point3D( 946, 71, 72 );
			map = Map.TerMur;
		    }
		    break;
		    // fel
		case "fel dungeon covetous":
		    {
			loc = new Point3D( 2498, 921, 0 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon deceit":
		    {
			loc = new Point3D( 4111, 434, 5 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon despise":
		    {
			loc = new Point3D( 1301, 1080, 0 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon destard":
		    {
			loc = new Point3D( 1176, 2640, 2 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon fire":
		    {
			loc = new Point3D( 2923, 3409, 8 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon hythloth":
		    {
			loc = new Point3D( 4721, 3824, 0 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon ice":
		    {
			loc = new Point3D( 1999, 81, 4 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon orc":
		    {
			loc = new Point3D( 1017, 1429, 0 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon shame":
		    {
			loc = new Point3D( 511, 1565, 0 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon wrong":
		    {
			loc = new Point3D( 2043, 238, 10 );
			map = Map.Felucca;
		    }
		    break;
		case "fel dungeon wind":
		    {
			loc = new Point3D( 1361, 895, 0 );
			map = Map.Felucca;
		    }
		    break;
	    } //switch
	}// resolvedest

	public virtual bool ValidateUse(Mobile m, bool message) {
	    if (Movable) {
		if (message) {
		    m.SendMessage("This must be locked down in a house to use!");
		}
		return false;
	    }

	    if (Sigil.ExistsOn(m)) {
		if (message) {
		    m.SendLocalizedMessage(1061632); // You can't do that while carrying the sigil.
		}

		return false;
	    }

	    if (WeightOverloading.IsOverloaded(m)) {
		if (message) {
		    m.SendLocalizedMessage(502359, "", 0x22); // Thou art too encumbered to move.
		}
		
		return false;
	    }
	    
	    if (m.Criminal) {
		if (message) {
		    m.SendLocalizedMessage(1005561, "", 0x22); // Thou'rt a criminal and cannot escape so easily.
		}
		
		return false;
	    }
	    
	    if (SpellHelper.CheckCombat(m)) {
		if (message) {
		    m.SendLocalizedMessage(1005564, "", 0x22); // Wouldst thou flee during the heat of battle??
		}
		
		return false;
	    }
	    
	    if (m.Spell != null) {
		if (message) {
		    m.SendLocalizedMessage(1049616); // You are too busy to do that at the moment.
		}
		
		return false;
	    }
	    
	    return true;
	}
    }
}
    
    
    