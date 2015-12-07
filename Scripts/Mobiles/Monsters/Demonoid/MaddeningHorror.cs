using Server.Items;

namespace Server.Mobiles
{
    [CorpseName("a maddening horror corpse")]
    public class MaddeningHorror : BaseCreature
    {
        [Constructable]
        public MaddeningHorror() : base(AIType.AI_NecroMage, FightMode.Closest, 10, 1, 0.2, 0.4)
        {
            Name = "a maddening horror";
            Body = 721;

            SetStr(271);
            SetDex(98);
            SetInt(850);

            SetHits(660);

            SetDamage(15, 27);

            SetDamageType(ResistanceType.Physical, 20);
            SetDamageType(ResistanceType.Cold, 40);
            SetDamageType(ResistanceType.Energy, 40);

            SetResistance(ResistanceType.Physical, 55, 65);
            SetResistance(ResistanceType.Fire, 29, 30);
            SetResistance(ResistanceType.Cold, 50, 60);
            SetResistance(ResistanceType.Poison, 40, 50);
            SetResistance(ResistanceType.Energy, 50, 60);

            SetSkill(SkillName.EvalInt, 118, 125.9);
            SetSkill(SkillName.Magery, 120.4, 125);
            SetSkill(SkillName.Meditation, 100.8);
            SetSkill(SkillName.MagicResist, 180.5, 195);
            SetSkill(SkillName.Tactics, 94.0, 100);
            SetSkill(SkillName.Wrestling, 82.4, 95);
            SetSkill(SkillName.SpiritSpeak, 118, 125.9);
            SetSkill(SkillName.Necromancy, 120.4, 125);
        }

        public MaddeningHorror(Serial serial) : base(serial)
        {
        }

        public override bool DeleteCorpseOnDeath
        {
            get { return true; }
        }

        protected override BaseAI ForcedAI
        {
            get { return new OmniAI(this); }
        }

        public override bool OnBeforeDeath()
        {
            if (Utility.RandomDouble() < 0.05)
            {
                if (!base.OnBeforeDeath())
                    return false;

                //VileTentacles VT = new VileTentacles();
                //VT.MoveToWorld(Location, Map);
            }

            if (!base.OnBeforeDeath())
                return false;

            var gold = new Gold(Utility.RandomMinMax(1767, 1800));
            gold.MoveToWorld(Location, Map);

            Effects.SendLocationEffect(Location, Map, 0x376A, 10, 1);
            return true;
        }

        public override int GetIdleSound()
        {
            return 1553;
        }

        public override int GetAngerSound()
        {
            return 1550;
        }

        public override int GetHurtSound()
        {
            return 1552;
        }

        public override int GetDeathSound()
        {
            return 1551;
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