using System;
using Server.Mobiles;

namespace Server.Spells.Mystic
{
    public class RisingColossusSpell : MysticSpell
    {
        private static readonly SpellInfo m_Info = new SpellInfo(
            "Rising Colossus", "Kal Vas Xen Corp Ylem",
            230,
            9022,
            Reagent.DaemonBone,
            Reagent.DragonBlood,
            Reagent.FertileDirt,
            Reagent.Nightshade);
			
        private static readonly int[] m_Offsets =
        {
            -1, -1,
            -1, 0,
            -1, 1,
            0, -1,
            0, 1,
            1, -1,
            1, 0,
            1, 1
        };

        public RisingColossusSpell(Mobile caster, Item scroll)
            : base(caster, scroll, m_Info)
        {
        }

        // When this spell is invoked, a weapon is conjured and animated. This weapon attacks nearby foes. 
        // Shame you cannot target a weapon/armor and animate it Diablo II's Summon Steel Golem style, it would be retardly simple too, just equip the mobile with the item and mark it unmovable.
        public override int RequiredMana
        {
            get { return 50; }
        }

        public override double RequiredSkill
        {
            get { return 83.0; }
        }

        public override bool CheckCast()
        {
            if (!base.CheckCast())
                return false;

            if (Caster.Followers + 5 > Caster.FollowersMax)
            {
                Caster.SendLocalizedMessage(1049645); // You have too many followers to summon that creature.
                return false;
            }

            return true;
        }

        public override void OnCast()
        {
            if (CheckSequence())
            {	
				var duration = TimeSpan.FromSeconds(5);
				
				if (Caster.Skills[SkillName.Imbuing].Fixed > Caster.Skills[SkillName.Focus].Fixed)
				{
					duration = TimeSpan.FromSeconds((2*Caster.Skills[SkillName.Imbuing].Fixed)/5);
					//SpellHelper.Summon(new RisingColossus(), Caster, 0x216, duration, false, false);
				}
				else
				{
					duration = TimeSpan.FromSeconds((2*Caster.Skills[SkillName.Focus].Fixed)/5);
					//SpellHelper.Summon(new RisingColossus(), Caster, 0x216, duration, false, false);					
				}
				
				var p = new Point3D(Caster);

				//Check for a valid location
				if (Caster.Map == null) //sanity
					return;

				var offset = Utility.Random(8)*2;

				for (var i = 0; i < m_Offsets.Length; i += 2)
				{
					var x = p.X + m_Offsets[(offset + i)%m_Offsets.Length];
					var y = p.Y + m_Offsets[(offset + i + 1)%m_Offsets.Length];

					if (Caster.Map.CanSpawnMobile(x, y, p.Z))
					{
						p = new Point3D(x, y, p.Z);
						BaseCreature.Summon(new RisingColossus(), false, Caster, p, 0x216, duration);
						return;
					}
					var z = Caster.Map.GetAverageZ(x, y);

					if (Caster.Map.CanSpawnMobile(x, y, z))
					{
						p = new Point3D(x, y, z);
						BaseCreature.Summon(new RisingColossus(), false, Caster, p, 0x216, duration);
						return;
					}
				}
			}

            FinishSequence();
        }
    }
}