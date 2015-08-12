namespace Server.Mobiles
{
    [CorpseName("a vollem corpse")]
    public class Vollem : BaseCreature
    {
        [Constructable]
        public Vollem()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.4, 0.8)
        {
            Name = "a vollem";
            Body = 293;
            //			BaseSoundID = 397;

            SetStr(225, 245);
            SetDex(80, 100);
            SetInt(30, 40);

            SetHits(151, 210);

            SetDamage(5, 10);

            SetDamageType(ResistanceType.Physical, 60);
            SetDamageType(ResistanceType.Poison, 40);

            SetResistance(ResistanceType.Physical, 80, 100);
            SetResistance(ResistanceType.Fire, 20, 30);
            SetResistance(ResistanceType.Cold, 60, 80);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 10, 25);

            SetSkill(SkillName.MagicResist, 30.1, 50.0);
            SetSkill(SkillName.Poisoning, 95.1, 100.0);
            SetSkill(SkillName.Tactics, 70.1, 90.0);
            SetSkill(SkillName.Wrestling, 50.1, 80.0);

            Fame = 3500;
            Karma = -3500;

            ControlSlots = 1;
        }

        public Vollem(Serial serial)
            : base(serial)
        {
        }

        public override bool IsScaredOfScaryThings
        {
            get { return false; }
        }

        public override bool IsScaryToPets
        {
            get { return true; }
        }

        public override bool IsBondable
        {
            get { return false; }
        }

        public override FoodType FavoriteFood
        {
            get { return FoodType.Meat; }
        }

        public override bool DeleteOnRelease
        {
            get { return true; }
        }

        public override bool AutoDispel
        {
            get { return !Controlled; }
        }

        public override bool BleedImmune
        {
            get { return true; }
        }

        public override bool BardImmune
        {
            get { return !Core.AOS || Controlled; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Lethal; }
        }

        public override void GenerateLoot()
        {
            AddLoot(LootPack.Meager, 2);
        }

        public override int GetAngerSound()
        {
            return 541;
        }

        public override int GetIdleSound()
        {
            if (!Controlled)
                return 542;

            return base.GetIdleSound();
        }

        public override int GetDeathSound()
        {
            if (!Controlled)
                return 545;

            return base.GetDeathSound();
        }

        public override int GetAttackSound()
        {
            return 562;
        }

        public override int GetHurtSound()
        {
            if (Controlled)
                return 320;

            return base.GetHurtSound();
        }

        /*
        public override void OnGaveMeleeAttack( Mobile defender )
        {
        base.OnGaveMeleeAttack( defender );

        if ( !m_Stunning && 0.3 > Utility.RandomDouble() )
        {
        m_Stunning = true;

        defender.Animate( 21, 6, 1, true, false, 0 );
        this.PlaySound( 0xEE );
        defender.LocalOverheadMessage( MessageType.Regular, 0x3B2, false, "You have been stunned by a colossal blow!" );

        BaseWeapon weapon = this.Weapon as BaseWeapon;
        if ( weapon != null )
        weapon.OnHit( this, defender );

        if ( defender.Alive )
        {
        defender.Frozen = true;
        Timer.DelayCall( TimeSpan.FromSeconds( 5.0 ), new TimerStateCallback( Recover_Callback ), defender );
        }
        }
        }
        */

        public override void OnDamage(int amount, Mobile from, bool willKill)
        {
            if (Controlled || Summoned)
            {
                var master = (ControlMaster);

                if (master == null)
                    master = SummonMaster;

                if (master != null && master.Player && master.Map == Map && master.InRange(Location, 20))
                {
                    if (master.Mana >= amount)
                    {
                        master.Mana -= amount;
                    }
                    else
                    {
                        amount -= master.Mana;
                        master.Mana = 0;
                        master.Damage(amount);
                    }
                }
            }

            base.OnDamage(amount, from, willKill);
        }

        public override void Serialize(GenericWriter writer)
        {
            base.Serialize(writer);
            writer.Write(0);
        }

        public override void Deserialize(GenericReader reader)
        {
            base.Deserialize(reader);
            var version = reader.ReadInt();
        }
    }
}