using System;

namespace Server.Items
{
    public class SerpentArrow : WeaponAbility
    {
        public SerpentArrow()
        {
        }

        public override int BaseMana
        {
            get
            {
                return 25;
            }
        }
        public override void OnHit(Mobile attacker, Mobile defender, int damage)
        {
            if (!this.Validate(attacker) || !this.CheckMana(attacker, true))
                return;

            ClearCurrentAbility(attacker);

            int level;

            double total = attacker.Skills[SkillName.Poisoning].Value + attacker.Skills[SkillName.Archery].Value + attacker.Dex;

            if (total >= 275.0 && 2 > Utility.Random(10))
                level = 3;
            else if (total > 200.0 && 4 > Utility.Random(10))
                level = 2;
            else if (total > 150.0 && 8 > Utility.Random(10))
                level = 1;
            else
                level = 0;

            if (level > 0)
            {
                attacker.SendMessage("You poisoned the target!");
                defender.SendMessage("You are poisoned!");

                defender.ApplyPoison(attacker, Poison.GetPoison(level));

                defender.FixedParticles(0x374A, 10, 15, 5021, EffectLayer.Waist);
                defender.PlaySound(0x474);

                //Possible to get Poisoning Gain
                attacker.CheckSkill(SkillName.Poisoning, 0, 100);
            }
            else
            {
                attacker.SendMessage("Your poison had no effect!");
            }
        }
    }
}