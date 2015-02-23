using System; 
using Server; 
using Server.Items;

namespace Server.Items
{
	public class WarriorBones : Bag
	{ 
		[Constructable]
		public WarriorBones() : this( 1 )
		{
			Name = "An Unknown Warrior's Skeleton";
			Movable = true;
			GumpID = 9;
			ItemID = 3792;
		}

		[Constructable]
		public WarriorBones( int amount )
		{
			DropItem( new LeatherChest() );
			DropItem( new Boots( 0x6B6 ) );
			DropItem( new Broadsword() );
			DropItem( new Gold( 260, 350 ) );
		}

		public WarriorBones( Serial serial ) : base( serial )
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

			if ( ItemID == 3787 )
				ItemID = 3792;
		}
	}

	public class HealerBones : Bag
	{ 
		[Constructable]
		public HealerBones() : this( 1 )
		{
			Name = "An Unknown Healer's Skeleton";
			Movable = true;
			GumpID = 9;
			ItemID = 3794;
		}

		[Constructable]
		public HealerBones( int amount )
		{
			DropItem( new Robe( Utility.RandomYellowHue() ) );
			DropItem( new Sandals( 0x6B6 ) );
			DropItem( new Bandage( Utility.RandomMinMax( 1, 5 ) ) );
			DropItem( new HealPotion() );
			DropItem( new CurePotion() );
		}

		public HealerBones( Serial serial ) : base( serial )
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

			if ( ItemID == 3787 )
				ItemID = 3794;
		}
	}

}
