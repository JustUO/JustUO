using System;
using Server.Multis;
using System.Collections;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

namespace Server.Items
{
	public class EnhancedBandage : Bandage
    {
        [Constructable]
        public EnhancedBandage()
            : this(1)
        {
        }

        [Constructable]
        public EnhancedBandage(int amount)
            : base(amount)
        {
            this.Hue = 0x8A5;
        }

        public EnhancedBandage(Serial serial)
            : base(serial)
        {
        }

        public static int HealingBonus
        {
            get
            {
                return 10;
            }
        }
        public override int LabelNumber
        {
            get
            {
                return 1152441;
            }
        }// enhanced bandage
        public override bool Dye(Mobile from, DyeTub sender)
        {
            return false;
        }

        public override void AddNameProperties(ObjectPropertyList list)
        {
            base.AddNameProperties(list);

            list.Add(1075216); // these bandages have been enhanced
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);

            writer.WriteEncodedInt(0); //version
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);

            int version = reader.ReadEncodedInt();
        }
    }
	
	
	[Flipable( 0x2AC0, 0x2AC3 )]
	public class FountainOfLife : BaseContainer, ISecurable
	{
		public override int LabelNumber { get { return 1075197; } } // Fountain Of Life

		public override int DefaultMaxItems { get { return 125; } }
		public override int DefaultGumpID { get { return 0x484; } }
		public override int DefaultDropSound { get { return 0x42; } }

		private SecureLevel m_Level;

		public static int MaxCharges { get { return 10; } }
		public static TimeSpan RechargeTime { get { return TimeSpan.FromDays( 1.0 ); } }

		private int m_Charges;

		[CommandProperty( AccessLevel.GameMaster )]
		public SecureLevel Level
		{
			get { return m_Level; }
			set { m_Level = value; }
		}

		[CommandProperty( AccessLevel.GameMaster )]
		public int Charges
		{
			get { return m_Charges; }
			set { m_Charges = Math.Min( value, MaxCharges ); InvalidateProperties(); }
		}

		private Timer m_Timer;

		[Constructable]
		public FountainOfLife()
			: this( MaxCharges )
		{
		}

		[Constructable]
		public FountainOfLife( int charges )
			: base( 0x2AC0 )
		{
			m_Charges = charges;
			m_Timer = Timer.DelayCall( RechargeTime, RechargeTime, new TimerCallback( Recharge ) );
		}

		public FountainOfLife( Serial serial )
			: base( serial )
		{
		}

		public override void OnDoubleClick( Mobile from )
		{
			if ( from.AccessLevel > AccessLevel.Player || from.InRange( this.GetWorldLocation(), 2 ) || this.RootParent is PlayerVendor )
			{
				Open( from );
				Effects.PlaySound( Location, Map, 0x23E );
			}
			else
			{
				from.LocalOverheadMessage( MessageType.Regular, 0x3B2, 1019045 ); // I can't reach that.
			}
		}

		public override bool OnDragDrop( Mobile from, Item dropped )
		{
			if ( dropped is Bandage && !( dropped is EnhancedBandage ) )
			{
				return base.OnDragDrop( from, dropped );
			}
			else
			{
				from.SendLocalizedMessage( 1075209 ); // Only bandages may be dropped into the fountain.
				return false;
			}
		}

		public override bool OnDragDropInto( Mobile from, Item item, Point3D p, byte gridloc )
		{
			if ( !base.OnDragDropInto( from, item, p, gridloc ) )
				return false;

			if ( item is Bandage && !( item is EnhancedBandage ) )
			{
				bool allow = base.OnDragDropInto( from, item, p, gridloc );

				if ( allow )
					Enhance();

				return allow;
			}
			else
			{
				from.SendLocalizedMessage( 1075209 ); // Only bandages may be dropped into the fountain.
				return false;
			}
		}

		public override void GetProperties( ObjectPropertyList list )
		{
            base.GetProperties(list);
			list.Add( 1075217, m_Charges.ToString() ); // ~1_val~ charges remaining
		}

		public override void OnDelete()
		{
			if ( m_Timer != null )
				m_Timer.Stop();

			base.OnDelete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.WriteEncodedInt( 0 ); // version

			writer.Write( m_Charges );
			writer.Write( (DateTime) m_Timer.Next );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadEncodedInt();

			m_Charges = reader.ReadInt();
			DateTime next = reader.ReadDateTime();

			if ( next < DateTime.UtcNow )
				m_Timer = Timer.DelayCall( TimeSpan.Zero, RechargeTime, new TimerCallback( Recharge ) );
			else
				m_Timer = Timer.DelayCall( next - DateTime.UtcNow, RechargeTime, new TimerCallback( Recharge ) );
		}

		private void Recharge()
		{
			m_Charges = MaxCharges;

			Enhance();
		}

		private void Enhance()
		{
			for ( int i = Items.Count - 1; i >= 0 && m_Charges > 0; i-- )
			{
				Item item = (Item) Items[i];

				if ( item is Bandage && !( item is EnhancedBandage ) )
				{
					if ( item.Amount > m_Charges )
					{
						item.Amount -= m_Charges;
						DropItem( new EnhancedBandage( m_Charges ) );
						m_Charges = 0;
					}
					else
					{
						DropItem( new EnhancedBandage( item.Amount ) );
						m_Charges -= item.Amount;
						item.Delete();
					}
				}
			}

			InvalidateProperties();
		}
	}
}