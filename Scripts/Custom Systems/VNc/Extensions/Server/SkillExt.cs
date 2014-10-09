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
			return (skill.Base >= skill.Cap);
		}

		public static bool IsZero(this Skill skill)
		{
			return (skill.Base <= 0);
		}

		public static bool IsZeroOrCapped(this Skill skill)
		{
			return (skill.IsZero() || skill.IsCapped());
		}

		public static bool WillCap(this Skill skill, double value, bool isEqual = true)
		{
			return (isEqual ? (skill.Base + value >= skill.Cap) : (skill.Base + value > skill.Cap));
		}

		public static bool WillZero(this Skill skill, double value, bool isEqual = true)
		{
			return (isEqual ? (skill.Base - value <= 0) : (skill.Base - value < 0));
		}

		public static bool DecreaseBase(this Skill skill, double value, bool ignoreZero = false, bool trim = true)
		{
			if (trim)
			{
				value = Math.Min(skill.Base, value);
			}

			if (ignoreZero || (!skill.IsZero() && !skill.WillZero(value, false)))
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

			if (ignoreCap || (!skill.IsCapped() && !skill.WillCap(value, false)))
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

			if (ignoreLimits || (value < skill.Base && !skill.IsZero() && !skill.WillZero(skill.Base - value, false)) ||
				(value > skill.Base && !skill.IsCapped() && !skill.WillCap(value - skill.Base, false)))
			{
				skill.Base = value;
				return true;
			}

			return false;
		}

		public static void DecreaseCap(this Skill skill, double value)
		{
			skill.SetCap(skill.Cap - value);
		}

		public static void IncreaseCap(this Skill skill, double value)
		{
			skill.SetCap(skill.Cap + value);
		}

		public static void SetCap(this Skill skill, double value)
		{
			value = Math.Max(0, value);
			skill.Cap = value;
			skill.Normalize();
		}

		public static void Normalize(this Skill skill)
		{
			if (skill.IsCapped())
			{
				skill.BaseFixedPoint = skill.CapFixedPoint;
			}

			if (skill.IsZero())
			{
				skill.BaseFixedPoint = 0;
			}
		}
	}
}