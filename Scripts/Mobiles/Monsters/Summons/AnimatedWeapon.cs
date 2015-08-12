namespace Server.Mobiles
{
    [CorpseName("the remains of a broken weapon")]
    public class AnimatedWeapon : BaseCreature
    {
        [Constructable]
        public AnimatedWeapon()
            : base(AIType.AI_Melee, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "Animated Weapon";
            Body = 692;

            SetStr(100);
            SetDex(100);
            SetInt(100);

            SetDamage(14, 21);

            SetDamageType(ResistanceType.Physical, 0);
            SetDamageType(ResistanceType.Poison, 100);

            SetResistance(ResistanceType.Physical, 45, 55);
            SetResistance(ResistanceType.Fire, 45, 55);
            SetResistance(ResistanceType.Cold, 45, 55);
            SetResistance(ResistanceType.Poison, 100);
            SetResistance(ResistanceType.Energy, 30, 40);

            SetSkill(SkillName.MagicResist, 90.1, 100.0);
            SetSkill(SkillName.Tactics, 100.0);
            SetSkill(SkillName.Wrestling, 100.0);

            VirtualArmor = 58;
            ControlSlots = 4;
        }

        public AnimatedWeapon(Serial serial)
            : base(serial)
        {
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
            return 0x64B;
        }

        public override int GetHurtSound()
        {
            return 0x64B;
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