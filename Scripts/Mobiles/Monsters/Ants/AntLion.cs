using System;
using System.Collections;
using Server.Mobiles;
using Server.Items;
using Server.Targeting;

namespace Server.Mobiles
{
	[CorpseName( "an ant lion corpse" )]
    public class AntLion : BaseCreature    
	{
        private Item m_hole1;
		private Item m_hole2;
		private Item m_cracks1;
		private Item m_cracks2;
		[Constructable]
		public AntLion() : base( AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4 )
		{
			Name = "an ant lion";
			Body = 787;
			BaseSoundID = 1006;

			SetStr( 296, 320 );
			SetDex( 81, 105 );
			SetInt( 36, 60 );

			SetHits( 151, 162 );

			SetDamage( 7, 21 );

			SetDamageType( ResistanceType.Physical, 70 );
			SetDamageType( ResistanceType.Poison, 30 );

			SetResistance( ResistanceType.Physical, 45, 60 );
			SetResistance( ResistanceType.Fire, 25, 35 );
			SetResistance( ResistanceType.Cold, 30, 40 );
			SetResistance( ResistanceType.Poison, 40, 50 );
			SetResistance( ResistanceType.Energy, 30, 35 );

			SetSkill( SkillName.MagicResist, 70.0 );
			SetSkill( SkillName.Tactics, 90.0 );
			SetSkill( SkillName.Wrestling, 90.0 );
			Fame = 4500;
			Karma = -4500;

			VirtualArmor = 45;

			PackGem();
			PackGem();
			PackGold( 125, 175 );
			PackItem( new FertileDirt( Utility.RandomMinMax( 1, 5 ) ) );
			switch ( Utility.Random( 4 ) )
			{
				case 0: PackItem( new DullCopperOre( Utility.RandomMinMax( 1, 10 ) ) ); break;
				case 1: PackItem( new ShadowIronOre( Utility.RandomMinMax( 1, 10 ) ) ); break;
				case 2: PackItem( new CopperOre( Utility.RandomMinMax( 1, 10 ) ) ); break;
				case 3: PackItem( new BronzeOre( Utility.RandomMinMax( 1, 10 ) ) ); break;
			}
                        if ( 0.2 > Utility.RandomDouble() )
			{
                             switch ( Utility.Random( 7 ) )
                             {
                                 case 0: PackItem(new UnknownBardSkeleton()); break;
                                 case 1: PackItem(new UnknownRogueSkeleton()); break;
                                 case 2: PackItem(new UnknownMageSkeleton()); break;
                                 case 3: PackItem(new WarriorBones( ) );break;
                                 case 4: PackItem(new HealerBones( ) );break;
                             }
                        }
			
		}

private DateTime m_NextTunnel;
		private DateTime m_NextPoisonSpit;
		
		public override void OnDamage( int amount, Mobile attacker, bool willKill )
		{ 
			Mobile combatant = attacker; 

			if ( combatant == null || combatant.Deleted || combatant.Map != Map || !InRange( combatant, 12 ) || !CanBeHarmful( combatant ) || !InLOS( combatant ) ) 
				return; 
			
			if ( DateTime.Now > m_NextTunnel ) 
			{            
				m_NextTunnel = DateTime.Now + TimeSpan.FromSeconds( 20.0 + (10.0 * Utility.RandomDouble()) ); // 5-10 seconds 
				this.GetDirectionTo( combatant.Location ); //check direction 
				this.Say("*The ant lion begins to burrow*");
				this.PlaySound( GetAngerSound() ); // Sound
				this.Animate( 12, 5, 1, true, false, 0 ); //animation
				new InternalTimer( this, combatant, (Point3D)this.Location ).Start(); // start timer with saved current hits value            
			}
		}

		public override void OnActionCombat() 
		{ 
         
			Mobile combatant = Combatant; 

			if ( combatant == null || combatant.Deleted || combatant.Map != Map || !InRange( combatant, 12 ) || !CanBeHarmful( combatant ) || !InLOS( combatant ) ) 
				return; 
			if ( DateTime.Now > m_NextPoisonSpit ) 
			{ 
				m_NextPoisonSpit = DateTime.Now + TimeSpan.FromSeconds( 5.0 + (10.0 * Utility.RandomDouble()) ); // 5-15 seconds 
				this.GetDirectionTo( combatant.Location ); //check direction 
				this.Freeze( TimeSpan.FromSeconds( 2 ) ); //freeze for animation 
				this.Animate( 12, 5, 1, true, false, 0 ); //animation 
				this.PlaySound( GetAngerSound() ); // Sound 
				new PoisonTimer( this, combatant, (int)this.Hits ).Start(); // start timer with saved current hits value 
			} 
		}
  
		private class PoisonTimer : Timer 
		{ 
			private Mobile m_Mobile; 
			private Mobile a_Mobile; 
			private int m_hits; 

			public PoisonTimer( Mobile mobile, Mobile attacker, int hits ) : base( TimeSpan.FromSeconds( 1.5 ) ) 
			{ 
				Priority = TimerPriority.FiftyMS; 
				m_Mobile = mobile;
				a_Mobile = attacker;
			} 

			protected override void OnTick() 
			{ 
				AntLion lion = m_Mobile as AntLion; 
                  	
				if ( lion != null )
				{
					if ( a_Mobile != null && a_Mobile.Alive ) 
					{ 
						a_Mobile.ApplyPoison( lion, Poison.Greater );
						lion.MovingEffect( a_Mobile, 0x36D4, 7, 0, false, false, 0x238, 1 ); 
						a_Mobile.FixedParticles( 0x374A, 10, 15, 5021, EffectLayer.Waist );
						a_Mobile.PlaySound( 0x474 );
                      	 
						Stop(); 
					}
				}
			}
		}
		public override int GetAngerSound()
		{
			return 0x5A;
		}

		public override int GetIdleSound()
		{
			return 0x5A;
		}

		public override int GetAttackSound()
		{
			return 0x164;
		}

		public override int GetHurtSound()
		{
			return 0x187;
		}

		public override int GetDeathSound()
		{
			return 0x1BA;
		}

		public AntLion( Serial serial ) : base( serial )
		{
		}
private class InternalTimer : Timer 
		{ 
			private AntLion m_Ant; 
			private Mobile m_Comb; 
			private Point3D m_Locale; 
			private int ticktock;

			public InternalTimer( AntLion mobile, Mobile c, Point3D locale ) : base( TimeSpan.FromSeconds( 1.5 ),TimeSpan.FromSeconds( 1.5 ) ) 
			{ 
				Priority = TimerPriority.FiftyMS; 
				m_Ant = mobile; 
				m_Comb = c;
				m_Locale = locale;
				ticktock = 0;      
			} 

			protected override void OnTick() 
			{
				if( m_Ant != null )
				{
					if ( ticktock == 0 ) 
					{ 
						if ( m_Comb != null && m_Comb.Alive )
						{
							Item cracks = new Item(Utility.Random(6913,6));
							cracks.Movable = false;
							cracks.Map=m_Ant.Map;
							m_Ant.m_cracks1=cracks;
							cracks.Location = m_Ant.Location;
							Item hole = new Item( 7026 );
							hole.Movable=false;
							hole.Name="hole";
							hole.Hue = 1;
							m_Ant.m_hole1 = hole;
							hole.Map = m_Ant.Map;
							hole.Location = m_Ant.Location;
							m_Ant.Location = new Point3D(357, 198, -36);
						}
						else
						{
							m_Ant.Location = m_Locale;
							//this.Stop();
						}
					}
					else if ( ticktock == 1 )
					{
						m_Ant.Hits += 5;
						if(m_Ant.m_hole1 != null)
							m_Ant.m_hole1.Delete();
					}
					else if ( ticktock == 2 )
					{
						m_Ant.Hits += 5;
						if(m_Ant.m_cracks1!=null)
							m_Ant.m_cracks1.Delete();
					}
					else if ( ticktock==3 ) 
					{ 
						if ( m_Comb != null && m_Comb.Alive )
						{
							Item cracks = new Item(Utility.Random(6913,6));
							cracks.Movable = false;
							cracks.Map = m_Comb.Map;
							m_Ant.m_cracks2 = cracks;
							cracks.Location = m_Comb.Location;
							Item hole = new Item(7026);
							hole.Movable = false;
							hole.Name = "hole";
							hole.Hue = 1;
							m_Ant.m_hole2 = hole;
							hole.Map = m_Comb.Map;
							hole.Location = m_Comb.Location;
							m_Ant.Location = m_Comb.Location;
						}
						else
						{
							m_Ant.Location = m_Locale;
							//this.Stop();
						}
					}
					else if ( ticktock == 4 )
					{	
						if(m_Ant.m_hole2!=null)
							m_Ant.m_hole2.Delete();
					}
					else if ( ticktock == 5 )
					{
						if(m_Ant.m_cracks2!=null)
							m_Ant.m_cracks2.Delete();
						
						//m_Ant.Delete();
						this.Stop();
					} 
					ticktock += 1;
				}
			}
		}
		
		public override bool OnBeforeDeath()
		{
			if(m_hole1!=null)
				m_hole1.Delete();
			if(m_hole2!=null)
				m_hole2.Delete();
			if(m_cracks1!=null)
				m_cracks1.Delete();
			if(m_cracks2!=null)
				m_cracks2.Delete();
			return true;
		}
			
		public override void OnAfterDelete()
		{
			if(m_hole1!=null)
				m_hole1.Delete();
			if(m_hole2!=null)
				m_hole2.Delete();
			if(m_cracks1!=null)
				m_cracks1.Delete();
			if(m_cracks2!=null)
				m_cracks2.Delete();
			
			base.OnAfterDelete();
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );
			writer.Write( (int) 0 );
            writer.Write( (Item) m_hole1 );
			writer.Write( (Item) m_hole2 );
			writer.Write( (Item) m_cracks1 );
			writer.Write( (Item) m_cracks2 );
		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );
			int version = reader.ReadInt();
            m_hole1 = reader.ReadItem();
			m_hole2 = reader.ReadItem();
			m_cracks1 = reader.ReadItem();
			m_cracks2 = reader.ReadItem();
		}
	}
}
