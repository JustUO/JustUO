// Created by Peoharen for the Mobile Abilities Package.

using System;

namespace Server.Mobiles
{
    [CorpseName("a corpse")]
    public class BaseSummoned : BaseCreature
    {
        private DateTime m_DecayTime;

        public BaseSummoned(AIType aitype, FightMode fightmode, int spot, int meleerange, double passivespeed,
            double activespeed)
            : base(aitype, fightmode, spot, meleerange, passivespeed, activespeed)
        {
            m_DecayTime = DateTime.UtcNow + m_Delay;
        }

        public BaseSummoned(Serial serial)
            : base(serial)
        {
        }

        public override bool AlwaysAttackable
        {
            get { return true; }
        }

        public override bool DeleteCorpseOnDeath
        {
            get { return true; }
        }

        public override double DispelDifficulty
        {
            get { return 117.5; }
        }

        public override double DispelFocus
        {
            get { return 45.0; }
        }

        public override bool IsDispellable
        {
            get { return true; }
        }

        public virtual TimeSpan m_Delay
        {
            get { return TimeSpan.FromMinutes(2.0); }
        }

        public override void OnThink()
        {
            if (DateTime.UtcNow > m_DecayTime)
            {
                FixedParticles(14120, 10, 15, 5012, EffectLayer.Waist);
                PlaySound(510);
                Delete();
            }
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

            m_DecayTime = DateTime.UtcNow + TimeSpan.FromMinutes(1.0);
        }
    }
}