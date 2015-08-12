namespace Server.Mobiles
{
    [CorpseName("the remains of a rising colossus")]
    public class RisingColossus : BaseCreature
    {
        [Constructable]
        public RisingColossus()
            : base(AIType.AI_Melee, FightMode.Strongest, 10, 1, 0.4, 0.5)
        {
            Name = "Rising Colossus";
            Body = 829;

            SetStr(100);
            SetDex(100);
            SetInt(100);

            SetDamage(17, 25);

            SetDamageType(ResistanceType.Physical, 100);

            SetResistance(ResistanceType.Physical, 100, 105);
            SetResistance(ResistanceType.Fire, 100, 105);
            SetResistance(ResistanceType.Cold, 100, 105);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 90.1, 100.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 100.0);

            VirtualArmor = 58;
            ControlSlots = 5;
        }

        public RisingColossus(Serial serial)
            : base(serial)
        {
        }
		
		public override bool AlwaysMurderer
        {
            get { return true; }
		}
		
        public override double DispelDifficulty
        {
            get { return 125.0; }
        }

        public override double DispelFocus
        {
            get { return 45.0; }
        }

        public override bool BleedImmune
        {
            get { return true; }
        }

        public override Poison PoisonImmune
        {
            get { return Poison.Lethal; }
        } // Immune to poison?

        public override int GetAttackSound()
        {
            return 0x627;
        }

        public override int GetHurtSound()
        {
            return 0x629;
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