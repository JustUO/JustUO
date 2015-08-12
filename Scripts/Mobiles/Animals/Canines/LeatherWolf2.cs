using System;
using Server;
using Server.Items;
using Server.Mobiles;
using Server.Network;

namespace Server.Mobiles
{
	[CorpseName( "a leather wolf corpse" )]
	public class LeatherWolf2 : BaseCreature
	{

        private DateTime recoverDelay;
        bool firstSummoned = true;

		[Constructable]
		public LeatherWolf2() : base( AIType.AI_Animal, FightMode.Aggressor, 10, 1, 0.2, 0.4 )
		{
			Name = "a Leather Wolf";
			Body = 739;

			SetStr( 104, 104 );
			SetDex( 111, 111 );
			SetInt( 22, 22 );

			SetHits( 221, 221 );
			SetStam( 111, 111);
			SetMana( 22, 22);

			SetDamage( 9, 20 );

			SetDamageType( ResistanceType.Physical, 100 );

			SetResistance( ResistanceType.Physical, 0, 40 );
			SetResistance( ResistanceType.Fire, 0, 19 );
			SetResistance( ResistanceType.Cold, 0, 25 );
			SetResistance( ResistanceType.Poison, 0, 16 );
			SetResistance( ResistanceType.Energy, 0, 11 );

			SetSkill( SkillName.Anatomy, 0.0, 0.0 );
			SetSkill( SkillName.MagicResist, 65.2, 70.1 );
			SetSkill( SkillName.Tactics, 55.2, 71.5 );
			SetSkill( SkillName.Wrestling, 60.7, 70.9 );
			
			Fame = 4500;
			Karma = -4500;

			VirtualArmor = 25;
		}

		public override void GenerateLoot()
		{
			AddLoot( LootPack.Meager, 2 );
		}

		public override int Meat{ get{ return 1; } }
		public override PackInstinct PackInstinct{ get { return PackInstinct.Canine; } }
		public override int Hides{ get{ return 7; } }
		public override FoodType FavoriteFood{ get{ return FoodType.Meat; } }


		public override int GetIdleSound() { return 1545; } 
		public override int GetAngerSound() { return 1542; } 
		public override int GetHurtSound() { return 1544; } 
		public override int GetDeathSound()	{ return 1543; }

		public override WeaponAbility GetWeaponAbility()
		{
			return WeaponAbility.BleedAttack;
		}

        public void SpawnLeatherWolf2s(Mobile target)
        {
            Map map = this.Map;

            if (map == null)
                return;

            int newLeatherWolf2s = Utility.RandomMinMax(1, 2);

            for (int i = 0; i < newLeatherWolf2s; ++i)
            {
                LeatherWolf2 LeatherWolf2 = new LeatherWolf2();

                LeatherWolf2.Team = this.Team;
                LeatherWolf2.FightMode = FightMode.Closest;

                bool validLocation = false;
                Point3D loc = this.Location;

                for (int j = 0; !validLocation && j < 10; ++j)
                {
                    int x = X + Utility.Random(3) - 1;
                    int y = Y + Utility.Random(3) - 1;
                    int z = map.GetAverageZ(x, y);

                    if (validLocation = map.CanFit(x, y, this.Z, 16, false, false))
                        loc = new Point3D(x, y, Z);
                    else if (validLocation = map.CanFit(x, y, z, 16, false, false))
                        loc = new Point3D(x, y, z);
                }

                LeatherWolf2.MoveToWorld(loc, map);
                LeatherWolf2.Combatant = target;
            }
        }

        public void DoSpecialAbility(Mobile target)
        {

            if (0.03 >= Utility.RandomDouble())
                SpawnLeatherWolf2s(target);
        }

        public override void OnGaveMeleeAttack(Mobile defender)
        {
            base.OnGaveMeleeAttack(defender);

            DoSpecialAbility(defender);

            defender.Damage(Utility.Random(20, 10), this);
            defender.Stam -= Utility.Random(20, 10);
            defender.Mana -= Utility.Random(20, 10);
        }

        public override void OnGotMeleeAttack(Mobile attacker)
        {
            base.OnGotMeleeAttack(attacker);

            DoSpecialAbility(attacker);

        }

        public override void OnDeath(Container c)
        {
            base.OnDeath(c);

            if (Utility.RandomDouble() < 0.25)
            {
                c.DropItem(new LeatherWolfSkin());
            }

            if (Utility.RandomDouble() < 0.05)
            {
                c.DropItem(new ReflectiveWolfEye());
            }
        }

		public LeatherWolf2( Serial serial ) : base( serial )
		{
		}

		public override void Serialize( GenericWriter writer )
		{
			base.Serialize( writer );

			writer.Write( (int) 0 );

		}

		public override void Deserialize( GenericReader reader )
		{
			base.Deserialize( reader );

			int version = reader.ReadInt();

		}
	}
}