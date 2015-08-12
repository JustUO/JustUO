using System;
using System.Collections.Generic;
using Server.Spells.Seventh;

namespace Server.Spells.Mystic
{
    public class StoneFormSpell : MysticSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Stone Form", "In Rel Ylem",
            230,
            9022,
            Reagent.Bloodmoss,
            Reagent.FertileDirt,
            Reagent.Garlic
            );

        private static readonly Dictionary<Mobile, List<ResistanceMod>> m_Table =
            new Dictionary<Mobile, List<ResistanceMod>>();

        public StoneFormSpell(Mobile caster, Item scroll) : base(caster, scroll, m_Info)
        {
        }

        public override TimeSpan CastDelayBase
        {
            get { return TimeSpan.FromSeconds(2.0); }
        }

        public override double RequiredSkill
        {
            get { return 33.0; }
        }

        public override int RequiredMana
        {
            get { return 11; }
        }

        public static bool HasEffect(Mobile m)
        {
            return m_Table.ContainsKey(m);
        }

        public static void RemoveEffect(Mobile m)
        {
            if (!m_Table.ContainsKey(m))
                return;

            var mods = m_Table[m];

            for (var i = 0; i < m_Table[m].Count; i++)
            {
                m.RemoveResistanceMod(mods[i]);
            }

            Enhancement.SetValue(m, AosAttribute.CastSpeed, 2, "Stone Form");
            Enhancement.SetValue(m, AosAttribute.WeaponSpeed, 10, "Stone Form");

            m_Table.Remove(m);
            m.EndAction(typeof (StoneFormSpell));
            m.PlaySound(0x201);
            m.FixedParticles(0x3728, 1, 13, 9918, 92, 3, EffectLayer.Head);
            m.BodyMod = 0;
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
            {
                return false;
            }
            if (!Caster.CanBeginAction(typeof (StoneFormSpell)))
            {
                RemoveEffect(Caster);
                Caster.SendMessage("You are no longer in Stone Form.");
                return false;
            }
            if (Caster.BodyMod != 0)
            {
                Caster.SendMessage("You cannot transform while in that form.");
                return false;
            }
            if (Caster.BodyMod == 183 || Caster.BodyMod == 184)
            {
                Caster.SendMessage("You cannot transform while wearing body paint.");
                return false;
            }
            if (!Caster.CanBeginAction(typeof (PolymorphSpell)))
            {
                Caster.SendMessage("You cannot transform while polymorphed.");
                return false;
            }
            /* else if ( !Caster.CanBeginAction( typeof( StoneFormSpell ) ) )
            {
                StoneFormSpell.RemoveEffect( Caster );
                Caster.SendMessage( "You are no longer in Stone Form." );
				return false;
			}*/

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {
                // Values
                var bonus1 = 2 + (int) (Caster.Skills[SkillName.Mysticism].Value/20);
                var bonus = 1 + (int) (Caster.Skills[SkillName.Focus].Value/20);

                // Mount
                var mount = Caster.Mount;

                if (mount != null)
                    mount.Rider = null;

                // Resists
                var mods = new List<ResistanceMod>();
                mods.Add(new ResistanceMod(ResistanceType.Physical, bonus1 + bonus));
                mods.Add(new ResistanceMod(ResistanceType.Fire, bonus1 + bonus));
                mods.Add(new ResistanceMod(ResistanceType.Cold, bonus1 + bonus));
                mods.Add(new ResistanceMod(ResistanceType.Poison, bonus1 + bonus));
                mods.Add(new ResistanceMod(ResistanceType.Energy, bonus1 + bonus));

                for (var i = 0; i < mods.Count; i++)
                    Caster.AddResistanceMod(mods[i]);

                // Effects
                Caster.BodyMod = 705;
                Caster.PlaySound(0x65A);
                Caster.FixedParticles(0x3728, 1, 13, 9918, 92, 3, EffectLayer.Head);

                m_Table.Add(Caster, mods);

                Enhancement.SetValue(Caster, AosAttribute.CastSpeed, -2, "Stone Form");
                Enhancement.SetValue(Caster, AosAttribute.WeaponSpeed, -10, "Stone Form");

                Caster.BeginAction(typeof (StoneFormSpell));
            }

            FinishSequence();
        }
    }
}