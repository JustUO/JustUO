#region Header
//   Vorspire    _,-'/-'/  SkillExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
#endregion

namespace Server
{
	public static class SkillExtUtility
	{
		public static bool IsLocked(this Skill skill, SkillLock locked)
		{
			return skill.Lock == locked;
		}

		public static bool IsCapped(this Skill skill)
		{
			return skill.Base >= skill.Cap;
		}

		public static bool IsZero(this Skill skill)
		{
			return skill.Base <= 0;
		}

		public static bool IsZeroOrCapped(this Skill skill)
		{
			return IsZero(skill) || IsCapped(skill);
		}

		public static bool WillCap(this Skill skill, double value, bool isEqual = true)
		{
			return isEqual ? (skill.Base + value >= skill.Cap) : (skill.Base + value > skill.Cap);
		}

		public static bool WillZero(this Skill skill, double value, bool isEqual = true)
		{
			return isEqual ? (skill.Base - value <= 0) : (skill.Base - value < 0);
		}

		public static bool DecreaseBase(this Skill skill, double value, bool ignoreZero = false, bool trim = true)
		{
			if (trim)
			{
				value = Math.Min(skill.Base, value);
			}

			if (ignoreZero || (!IsZero(skill) && !WillZero(skill, value, false)))
			{
				skill.Base -= value;
				return true;
			}

			return false;
		}

		public static bool IncreaseBase(this Skill skill, double value, bool ignoreCap = false, bool trim = true)
		{
			if (trim)
			{
				value = Math.Min(skill.Cap - skill.Base, value);
			}

			if (ignoreCap || (!IsCapped(skill) && !WillCap(skill, value, false)))
			{
				skill.Base += value;
				return true;
			}

			return false;
		}

		public static bool SetBase(this Skill skill, double value, bool ignoreLimits = false, bool trim = true)
		{
			if (trim)
			{
				value = Math.Max(0, Math.Min(skill.Cap, value));
			}

			if (ignoreLimits || (value < skill.Base && !IsZero(skill) && !WillZero(skill, skill.Base - value, false)) ||
				(value > skill.Base && !IsCapped(skill) && !WillCap(skill, value - skill.Base, false)))
			{
				skill.Base = value;
				return true;
			}

			return false;
		}

		public static void DecreaseCap(this Skill skill, double value)
		{
			SetCap(skill, skill.Cap - value);
		}

		public static void IncreaseCap(this Skill skill, double value)
		{
			SetCap(skill, skill.Cap + value);
		}

		public static void SetCap(this Skill skill, double value)
		{
			skill.Cap = Math.Max(0, value);
			Normalize(skill);
		}

		public static void Normalize(this Skill skill)
		{
			if (IsCapped(skill))
			{
				skill.BaseFixedPoint = skill.CapFixedPoint;
			}

			if (IsZero(skill))
			{
				skill.BaseFixedPoint = 0;
			}
		}

		public static readonly SkillName[] CombatSkills = new[]
		{
			SkillName.Archery, SkillName.Chivalry, SkillName.Fencing, SkillName.Focus, SkillName.Macing, SkillName.Parry,
			SkillName.Swords, SkillName.Tactics, SkillName.Wrestling, SkillName.Bushido
		};

		public static readonly SkillName[] HealingSkills = new[] {SkillName.Healing, SkillName.Veterinary};

		public static readonly SkillName[] MagicSkills = new[]
		{
			SkillName.Alchemy, SkillName.EvalInt, SkillName.Inscribe, SkillName.Magery, SkillName.Meditation,
			SkillName.Necromancy, SkillName.MagicResist, SkillName.Spellweaving, SkillName.SpiritSpeak
		};

		public static readonly SkillName[] BardicSkills = new[]
		{SkillName.Discordance, SkillName.Musicianship, SkillName.Peacemaking, SkillName.Provocation};

		public static readonly SkillName[] RogueSkills = new[]
		{
			SkillName.Begging, SkillName.DetectHidden, SkillName.Hiding, SkillName.Lockpicking, SkillName.Poisoning,
			SkillName.RemoveTrap, SkillName.Snooping, SkillName.Stealing, SkillName.Stealth, SkillName.Ninjitsu
		};

		public static readonly SkillName[] KnowledgeSkills = new[]
		{
			SkillName.Anatomy, SkillName.AnimalLore, SkillName.AnimalTaming, SkillName.ArmsLore, SkillName.Camping,
			SkillName.Forensics, SkillName.Herding, SkillName.ItemID, SkillName.TasteID, SkillName.Tracking
		};

		public static readonly SkillName[] CraftSkills = new[]
		{
			SkillName.Blacksmith, SkillName.Fletching, SkillName.Carpentry, SkillName.Cooking, SkillName.Cartography,
			SkillName.Tailoring, SkillName.Tinkering, SkillName.Imbuing
		};

		public static readonly SkillName[] HarvestSkills = new[]
		{SkillName.Fishing, SkillName.Mining, SkillName.Lumberjacking};

		public static bool IsCombat(this SkillName skill)
		{
			return CombatSkills.Contains(skill);
		}

		public static bool IsHealing(this SkillName skill)
		{
			return HealingSkills.Contains(skill);
		}

		public static bool IsMagic(this SkillName skill)
		{
			return MagicSkills.Contains(skill);
		}

		public static bool IsBardic(this SkillName skill)
		{
			return BardicSkills.Contains(skill);
		}

		public static bool IsRogue(this SkillName skill)
		{
			return RogueSkills.Contains(skill);
		}

		public static bool IsKnowledge(this SkillName skill)
		{
			return KnowledgeSkills.Contains(skill);
		}

		public static bool IsCraft(this SkillName skill)
		{
			return CraftSkills.Contains(skill);
		}

		public static bool IsHarvest(this SkillName skill)
		{
			return HarvestSkills.Contains(skill);
		}
	}
}