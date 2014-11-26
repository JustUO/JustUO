#region Header
//   Vorspire    _,-'/-'/  AttributesExt.cs
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
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Reflection;

using Server.Items;
#endregion

namespace Server
{
	public static class AttributesExtUtility
	{
		#region Defaults
		public static List<SlayerName> SuperSlayers = new List<SlayerName>
		{
			SlayerName.Repond,
			SlayerName.ReptilianDeath,
			SlayerName.DaemonDismissal,
			SlayerName.ElementalBan,
			SlayerName.Exorcism,
			SlayerName.ArachnidDoom,
			SlayerName.Fey
		};

		public static List<TalismanSlayerName> SuperTalismanSlayers = new List<TalismanSlayerName>
		{
			TalismanSlayerName.Bat,
			TalismanSlayerName.Bear,
			TalismanSlayerName.Beetle,
			TalismanSlayerName.Bird,
			TalismanSlayerName.Bovine,
			TalismanSlayerName.Flame,
			TalismanSlayerName.Ice,
			TalismanSlayerName.Mage,
			TalismanSlayerName.Vermin
		};
		#endregion Defaults

		#region Fields & Properties
		/// <summary>
		///     (InstanceOf:BaseAttributes).GetValue multiple parameter support.
		///     Some instances of RunUO may have extended attribute enum flags beyond the Int32 capacity of 31 entries.
		/// </summary>
		private static readonly MethodInfo _GetValueImpl;

		private static readonly byte _GetValueSupport;

		public static readonly Type TypeOfBaseAttributes = typeof(BaseAttributes);
		public static readonly Type TypeOfAosAttribute = typeof(AosAttribute);
		public static readonly Type TypeOfAosArmorAttribute = typeof(AosArmorAttribute);
		public static readonly Type TypeOfAosWeaponAttribute = typeof(AosWeaponAttribute);
		public static readonly Type TypeOfAosElementAttribute = typeof(AosElementAttribute);
		public static readonly Type TypeOfSkillName = typeof(SkillName);
		public static readonly Type TypeOfSlayerName = typeof(SlayerName);
		public static readonly Type TypeOfTalismanSlayerName = typeof(TalismanSlayerName);

		public static Dictionary<AosAttribute, AttributeFactors> AttrFactors { get; private set; }
		public static Dictionary<AosArmorAttribute, AttributeFactors> ArmorAttrFactors { get; private set; }
		public static Dictionary<AosWeaponAttribute, AttributeFactors> WeaponAttrFactors { get; private set; }
		public static Dictionary<AosElementAttribute, AttributeFactors> ElementAttrFactors { get; private set; }
		public static Dictionary<SkillName, AttributeFactors> SkillBonusAttrFactors { get; private set; }
		public static Dictionary<SlayerName, AttributeFactors> SlayerAttrFactors { get; private set; }
		public static Dictionary<TalismanSlayerName, AttributeFactors> TalismanSlayerAttrFactors { get; private set; }
		#endregion Fields & Properties

		#region Initialization
		static AttributesExtUtility()
		{
			_GetValueImpl = typeof(BaseAttributes).GetMethod("GetValue", BindingFlags.Instance | BindingFlags.Public);
			_GetValueSupport = 0x0;

			if (_GetValueImpl != null)
			{
				var p = _GetValueImpl.GetParameters();

				if (p.Length == 1)
				{
					if (p[0].ParameterType.IsEqual(typeof(uint)))
					{
						_GetValueSupport = 0x1;
					}
					else if (p[0].ParameterType.IsEqual(typeof(long)))
					{
						_GetValueSupport = 0x2;
					}
					else if (p[0].ParameterType.IsEqual(typeof(ulong)))
					{
						_GetValueSupport = 0x3;
					}
				}
			}

			AttrFactors = new Dictionary<AosAttribute, AttributeFactors>
			{
				{AosAttribute.AttackChance, new AttributeFactors(1.3, 0, 15, 3)},
				{AosAttribute.BonusDex, new AttributeFactors(1.1, 0, 8)},
				{AosAttribute.BonusHits, new AttributeFactors(1.1, 0, 5)},
				{AosAttribute.BonusInt, new AttributeFactors(1.1, 0, 8)},
				{AosAttribute.BonusMana, new AttributeFactors(1.1, 0, 8)},
				{AosAttribute.BonusStam, new AttributeFactors(1.1, 0, 8)},
				{AosAttribute.BonusStr, new AttributeFactors(1.1, 0, 8)},
				{AosAttribute.CastRecovery, new AttributeFactors(1.2, 0, 3)},
				{AosAttribute.CastSpeed, new AttributeFactors(1.4)},
				{AosAttribute.DefendChance, new AttributeFactors(1.1, 0, 15, 3)},
				{AosAttribute.EnhancePotions, new AttributeFactors(1.0, 0, 25, 5)},
				{AosAttribute.IncreasedKarmaLoss, new AttributeFactors()},
				{AosAttribute.LowerManaCost, new AttributeFactors(1.1, 0, 15, 3)},
				{AosAttribute.LowerRegCost, new AttributeFactors(1.0, 0, 20, 2)},
				{AosAttribute.Luck, new AttributeFactors(1.0, 0, 100, 10)},
				{AosAttribute.NightSight, new AttributeFactors()},
				{AosAttribute.ReflectPhysical, new AttributeFactors(1.0, 0, 15, 3)},
				{AosAttribute.RegenHits, new AttributeFactors(1.0, 0, 2)},
				{AosAttribute.RegenMana, new AttributeFactors(1.0, 0, 2)},
				{AosAttribute.RegenStam, new AttributeFactors(1.0, 0, 3)},
				{AosAttribute.SpellChanneling, new AttributeFactors()},
				{AosAttribute.SpellDamage, new AttributeFactors(1.0, 0, 12, 2)},
				{AosAttribute.WeaponDamage, new AttributeFactors(1.0, 0, 50, 5)},
				{AosAttribute.WeaponSpeed, new AttributeFactors(1.1, 0, 30, 5)}
			};

			ArmorAttrFactors = new Dictionary<AosArmorAttribute, AttributeFactors>
			{
				{AosArmorAttribute.DurabilityBonus, new AttributeFactors(1.0, 0, 250, 25)},
				{AosArmorAttribute.LowerStatReq, new AttributeFactors(1.0, 0, 100, 10)},
				{AosArmorAttribute.MageArmor, new AttributeFactors()},
				{AosArmorAttribute.SelfRepair, new AttributeFactors(1.0, 0, 5)}
			};

			WeaponAttrFactors = new Dictionary<AosWeaponAttribute, AttributeFactors>
			{
				{AosWeaponAttribute.DurabilityBonus, new AttributeFactors(1.0, 0, 250, 25)},
				{AosWeaponAttribute.HitColdArea, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitDispel, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitEnergyArea, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitFireArea, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitFireball, new AttributeFactors(1.4, 0, 50, 5)},
				{AosWeaponAttribute.HitHarm, new AttributeFactors(1.1, 0, 50, 5)},
				{AosWeaponAttribute.HitLeechHits, new AttributeFactors(1.1, 0, 100, 10)},
				{AosWeaponAttribute.HitLeechMana, new AttributeFactors(1.1, 0, 100, 10)},
				{AosWeaponAttribute.HitLeechStam, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitLightning, new AttributeFactors(1.4, 0, 50, 5)},
				{AosWeaponAttribute.HitLowerAttack, new AttributeFactors(1.1, 0, 50, 5)},
				{AosWeaponAttribute.HitLowerDefend, new AttributeFactors(1.3, 0, 50, 5)},
				{AosWeaponAttribute.HitMagicArrow, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitPhysicalArea, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.HitPoisonArea, new AttributeFactors(1.0, 0, 50, 5)},
				{AosWeaponAttribute.LowerStatReq, new AttributeFactors(1.0, 0, 100, 5)},
				{AosWeaponAttribute.MageWeapon, new AttributeFactors(1.0, -30, 0, 3)},
				{AosWeaponAttribute.ResistColdBonus, new AttributeFactors(1.0, 0, 15, 3)},
				{AosWeaponAttribute.ResistEnergyBonus, new AttributeFactors(1.0, 0, 15, 3)},
				{AosWeaponAttribute.ResistFireBonus, new AttributeFactors(1.0, 15, 0, 3)},
				{AosWeaponAttribute.ResistPhysicalBonus, new AttributeFactors(1.0, 0, 15, 3)},
				{AosWeaponAttribute.ResistPoisonBonus, new AttributeFactors(1.0, 0, 15, 3)},
				{AosWeaponAttribute.SelfRepair, new AttributeFactors(1.0, 0, 5)},
				{AosWeaponAttribute.UseBestSkill, new AttributeFactors(1.4)}
			};

			ElementAttrFactors = new Dictionary<AosElementAttribute, AttributeFactors>
			{
				{AosElementAttribute.Direct, new AttributeFactors()},
				{AosElementAttribute.Chaos, new AttributeFactors()},
				{AosElementAttribute.Cold, new AttributeFactors(1.0, 0, 15, 3)},
				{AosElementAttribute.Energy, new AttributeFactors(1.0, 0, 15, 3)},
				{AosElementAttribute.Fire, new AttributeFactors(1.0, 0, 15, 3)},
				{AosElementAttribute.Physical, new AttributeFactors(1.0, 0, 15, 3)},
				{AosElementAttribute.Poison, new AttributeFactors(1.0, 0, 15, 3)}
			};

			SkillBonusAttrFactors = new Dictionary<SkillName, AttributeFactors>();
			SlayerAttrFactors = new Dictionary<SlayerName, AttributeFactors>();
			TalismanSlayerAttrFactors = new Dictionary<TalismanSlayerName, AttributeFactors>();

			//All skill bonuses have a weight of 1.4 and intensity cap of 15%
			foreach (SkillName attr in Enum.GetValues(TypeOfSkillName))
			{
				SkillBonusAttrFactors.Add(attr, new AttributeFactors(1.4, 0, 15, 3));
			}

			//All standard slayers have a weight of 1.1 and intensity cap of 1
			//If a standard slayer is considered a super slayer, its weight is 1.3
			foreach (SlayerName attr in Enum.GetValues(TypeOfSlayerName))
			{
				SlayerAttrFactors.Add(attr, new AttributeFactors(attr.IsSuper() ? 1.3 : 1.1));
			}

			//All talisman slayers have a weight of 1.1 and intensity cap of 1
			//If a talisman slayer is considered a super slayer, its weight is 1.3
			foreach (TalismanSlayerName attr in Enum.GetValues(TypeOfTalismanSlayerName))
			{
				TalismanSlayerAttrFactors.Add(attr, new AttributeFactors(attr.IsSuper() ? 1.3 : 1.1));
			}
		}
		#endregion Initialization

		#region Helper Methods
		private static bool SupportsAttributes(Item item, string name, out BaseAttributes attrs)
		{
			PropertyInfo pi = item.GetType().GetProperty(name);

			if (pi == null)
			{
				attrs = null;
				return false;
			}

			return (attrs = pi.GetValue(item, null) as BaseAttributes) != null;
		}

		private static bool HasAttribute(Item item, string name, ulong attr, out int value)
		{
			value = 0;

			PropertyInfo pi = item.GetType().GetProperty(name);

			if (pi == null)
			{
				return false;
			}

			var attrs = pi.GetValue(item, null) as BaseAttributes;

			if (attrs == null)
			{
				return false;
			}

			if (attrs is AosSkillBonuses)
			{
				var sb = (AosSkillBonuses)attrs;
				var sk = (SkillName)attr;

				for (int i = 0; i < 5; i++)
				{
					if (sb.GetSkill(i) == sk)
					{
						value += (int)(sb.GetBonus(i) * 10);
					}
				}

				return value != 0;
			}

			if (_GetValueImpl != null)
			{
				switch (_GetValueSupport)
				{
					case 0x0:
						return (value = (int)_GetValueImpl.Invoke(attrs, new object[] {(int)attr})) != 0;
					case 0x1:
						return (value = (int)_GetValueImpl.Invoke(attrs, new object[] {(uint)attr})) != 0;
					case 0x2:
						return (value = (int)_GetValueImpl.Invoke(attrs, new object[] {(long)attr})) != 0;
					case 0x3:
						return (value = (int)_GetValueImpl.Invoke(attrs, new object[] {attr})) != 0;
				}
			}

			return false;
		}

		public static ulong[] AttrMasks = ((AosAttribute)0).GetValues<ulong>();
		public static ulong[] ArmorAttrMasks = ((AosArmorAttribute)0).GetValues<ulong>();
		public static ulong[] WeaponAttrMasks = ((AosWeaponAttribute)0).GetValues<ulong>();
		public static ulong[] ElementAttrMasks = ((AosElementAttribute)0).GetValues<ulong>();
		public static ulong[] SkillBonusMasks = ((SkillName)0).GetValues<ulong>();
		public static ulong[] SlayerAttrMasks = ((SlayerName)0).GetValues<ulong>();
		public static ulong[] TalismanSlayerAttrMasks = ((TalismanSlayerName)0).GetValues<ulong>();

		public static ulong[][] AttributeMasks = new[]
		{
			AttrMasks, ArmorAttrMasks, WeaponAttrMasks, ElementAttrMasks, SkillBonusMasks, SlayerAttrMasks,
			TalismanSlayerAttrMasks
		};

		public static int GetAttributeCount(
			this Item item,
			bool normal = true,
			bool armor = true,
			bool weapon = true,
			bool element = true,
			bool skills = true,
			bool slayers = true)
		{
			int total = 0, value;

			if (normal)
			{
				total += AttributeMasks[0].Count(a => item.HasAttribute((AosAttribute)a, out value));
			}

			if (armor)
			{
				total += AttributeMasks[1].Count(a => item.HasAttribute((AosArmorAttribute)a, out value));
			}

			if (weapon)
			{
				total += AttributeMasks[2].Count(a => item.HasAttribute((AosWeaponAttribute)a, out value));
			}

			if (element)
			{
				total += AttributeMasks[3].Count(a => item.HasAttribute((AosElementAttribute)a, out value));
			}

			if (skills)
			{
				total += AttributeMasks[4].Count(a => item.HasSkillBonus((SkillName)a, out value));
			}

			if (slayers)
			{
				total += AttributeMasks[5].Count(a => item.HasSlayer((SlayerName)a));
				total += AttributeMasks[6].Count(a => item.HasSlayer((TalismanSlayerName)a));
			}

			return total;
		}

		public static string GetPropertyString(double value, bool html = true)
		{
			string s = value.ToString("#,0");

			if (html)
			{
				s = s.WrapUOHtmlColor(value > 0 ? Color.LimeGreen : value < 0 ? Color.Red : Color.Yellow);
			}

			return s;
		}
		#endregion

		#region Base Attributes
		public static string ToString(this BaseAttributes attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Standard Attributes
		public static bool HasAttribute(this Item item, AosAttribute attr, out int value)
		{
			return HasAttribute(item, "Attributes", (ulong)attr, out value);
		}

		public static bool SupportsAttribute(this Item item, out AosAttributes attrs)
		{
			BaseAttributes a;

			if (SupportsAttributes(item, "Attributes", out a))
			{
				attrs = (AosAttributes)a;
				return true;
			}

			attrs = null;
			return false;
		}

		public static double GetWeight(this AosAttribute attr)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(attr, new AttributeFactors());
			}

			return AttrFactors[attr].Weight;
		}

		public static void SetWeight(this AosAttribute attr, double weight)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				AttrFactors[attr].Weight = weight;
			}
		}

		public static int GetMin(this AosAttribute attr)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(attr, new AttributeFactors());
			}

			return AttrFactors[attr].Min;
		}

		public static void SetMin(this AosAttribute attr, int min)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				AttrFactors[attr].Min = min;
			}
		}

		public static int GetMax(this AosAttribute attr)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(attr, new AttributeFactors());
			}

			return AttrFactors[attr].Max;
		}

		public static void SetMax(this AosAttribute attr, int max)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				AttrFactors[attr].Max = max;
			}
		}

		public static int GetInc(this AosAttribute attr)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(attr, new AttributeFactors());
			}

			return AttrFactors[attr].Inc;
		}

		public static void SetInc(this AosAttribute attr, int inc)
		{
			if (!AttrFactors.ContainsKey(attr))
			{
				AttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Inc = inc
					});
			}
			else
			{
				AttrFactors[attr].Inc = inc;
			}
		}

		public static string ToString(this AosAttribute attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Armor Attributes
		public static bool HasAttribute(this Item item, AosArmorAttribute attr, out int value)
		{
			return (HasAttribute(item, "ArmorAttributes", (ulong)attr, out value) ||
					HasAttribute(item, "ClothingAttributes", (ulong)attr, out value));
		}

		public static bool SupportsAttribute(this Item item, out AosArmorAttributes attrs)
		{
			BaseAttributes a;

			if (SupportsAttributes(item, "ArmorAttributes", out a) || SupportsAttributes(item, "ClothingAttributes", out a))
			{
				attrs = (AosArmorAttributes)a;
				return true;
			}

			attrs = null;
			return false;
		}

		public static double GetWeight(this AosArmorAttribute attr)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(attr, new AttributeFactors());
			}

			return ArmorAttrFactors[attr].Weight;
		}

		public static void SetWeight(this AosArmorAttribute attr, double weight)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				ArmorAttrFactors[attr].Weight = weight;
			}
		}

		public static int GetMin(this AosArmorAttribute attr)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(attr, new AttributeFactors());
			}

			return ArmorAttrFactors[attr].Min;
		}

		public static void SetMin(this AosArmorAttribute attr, int min)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				ArmorAttrFactors[attr].Min = min;
			}
		}

		public static int GetMax(this AosArmorAttribute attr)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(attr, new AttributeFactors());
			}

			return ArmorAttrFactors[attr].Max;
		}

		public static void SetMax(this AosArmorAttribute attr, int max)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				ArmorAttrFactors[attr].Max = max;
			}
		}

		public static int GetInc(this AosArmorAttribute attr)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(attr, new AttributeFactors());
			}

			return ArmorAttrFactors[attr].Inc;
		}

		public static void SetInc(this AosArmorAttribute attr, int inc)
		{
			if (!ArmorAttrFactors.ContainsKey(attr))
			{
				ArmorAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Inc = inc
					});
			}
			else
			{
				ArmorAttrFactors[attr].Inc = inc;
			}
		}

		public static string ToString(this AosArmorAttribute attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Weapon Attributes
		public static bool HasAttribute(this Item item, AosWeaponAttribute attr, out int value)
		{
			return HasAttribute(item, "WeaponAttributes", (ulong)attr, out value);
		}

		public static bool SupportsAttribute(this Item item, out AosWeaponAttributes attrs)
		{
			BaseAttributes a;

			if (SupportsAttributes(item, "WeaponAttributes", out a))
			{
				attrs = (AosWeaponAttributes)a;
				return true;
			}

			attrs = null;
			return false;
		}

		public static double GetWeight(this AosWeaponAttribute attr)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(attr, new AttributeFactors());
			}

			return WeaponAttrFactors[attr].Weight;
		}

		public static void SetWeight(this AosWeaponAttribute attr, double weight)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				WeaponAttrFactors[attr].Weight = weight;
			}
		}

		public static int GetMin(this AosWeaponAttribute attr)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(attr, new AttributeFactors());
			}

			return WeaponAttrFactors[attr].Min;
		}

		public static void SetMin(this AosWeaponAttribute attr, int min)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				WeaponAttrFactors[attr].Min = min;
			}
		}

		public static int GetMax(this AosWeaponAttribute attr)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(attr, new AttributeFactors());
			}

			return WeaponAttrFactors[attr].Max;
		}

		public static void SetMax(this AosWeaponAttribute attr, int max)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				WeaponAttrFactors[attr].Max = max;
			}
		}

		public static int GetInc(this AosWeaponAttribute attr)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(attr, new AttributeFactors());
			}

			return WeaponAttrFactors[attr].Inc;
		}

		public static void SetInc(this AosWeaponAttribute attr, int inc)
		{
			if (!WeaponAttrFactors.ContainsKey(attr))
			{
				WeaponAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Inc = inc
					});
			}
			else
			{
				WeaponAttrFactors[attr].Inc = inc;
			}
		}

		public static string ToString(this AosWeaponAttribute attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Element Attributes
		public static bool HasAttribute(this Item item, AosElementAttribute attr, out int value)
		{
			return (HasAttribute(item, "Resistances", (ulong)attr, out value) /*||
					HasAttribute(item, "AosElementDamages", (ulong)attr, out value)*/);
		}

		public static bool SupportsAttribute(this Item item, out AosElementAttributes attrs)
		{
			BaseAttributes a;

			if (SupportsAttributes(item, "Resistances", out a) /* || SupportsAttributes(item, "AosElementDamages", out a)*/)
			{
				attrs = (AosElementAttributes)a;
				return true;
			}

			attrs = null;
			return false;
		}

		public static double GetWeight(this AosElementAttribute attr)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(attr, new AttributeFactors());
			}

			return ElementAttrFactors[attr].Weight;
		}

		public static void SetWeight(this AosElementAttribute attr, double weight)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				ElementAttrFactors[attr].Weight = weight;
			}
		}

		public static int GetMin(this AosElementAttribute attr)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(attr, new AttributeFactors());
			}

			return ElementAttrFactors[attr].Min;
		}

		public static void SetMin(this AosElementAttribute attr, int min)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				ElementAttrFactors[attr].Min = min;
			}
		}

		public static int GetMax(this AosElementAttribute attr)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(attr, new AttributeFactors());
			}

			return ElementAttrFactors[attr].Max;
		}

		public static void SetMax(this AosElementAttribute attr, int max)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				ElementAttrFactors[attr].Max = max;
			}
		}

		public static int GetInc(this AosElementAttribute attr)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(attr, new AttributeFactors());
			}

			return ElementAttrFactors[attr].Inc;
		}

		public static void SetInc(this AosElementAttribute attr, int inc)
		{
			if (!ElementAttrFactors.ContainsKey(attr))
			{
				ElementAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Inc = inc
					});
			}
			else
			{
				ElementAttrFactors[attr].Inc = inc;
			}
		}

		public static string ToString(this AosElementAttribute attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Skill Bonus Attributes
		public static bool HasSkillBonus(this Item item, SkillName attr, out int value)
		{
			return HasAttribute(item, "SkillBonuses", (ulong)attr, out value);
		}

		public static bool SupportsSkillBonus(this Item item, out AosSkillBonuses attrs)
		{
			BaseAttributes a;

			if (SupportsAttributes(item, "SkillBonuses", out a))
			{
				attrs = (AosSkillBonuses)a;
				return true;
			}

			attrs = null;
			return false;
		}

		public static double GetBonusWeight(this SkillName attr)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(attr, new AttributeFactors());
			}

			return SkillBonusAttrFactors[attr].Weight;
		}

		public static void SetBonusWeight(this SkillName attr, double weight)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				SkillBonusAttrFactors[attr].Weight = weight;
			}
		}

		public static int GetBonusMin(this SkillName attr)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(attr, new AttributeFactors());
			}

			return SkillBonusAttrFactors[attr].Min;
		}

		public static void SetBonusMin(this SkillName attr, int min)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				SkillBonusAttrFactors[attr].Min = min;
			}
		}

		public static int GetBonusMax(this SkillName attr)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(attr, new AttributeFactors());
			}

			return SkillBonusAttrFactors[attr].Max;
		}

		public static void SetBonusMax(this SkillName attr, int max)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				SkillBonusAttrFactors[attr].Max = max;
			}
		}

		public static int GetBonusInc(this SkillName attr)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(attr, new AttributeFactors());
			}

			return SkillBonusAttrFactors[attr].Inc;
		}

		public static void SetBonusInc(this SkillName attr, int inc)
		{
			if (!SkillBonusAttrFactors.ContainsKey(attr))
			{
				SkillBonusAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Inc = inc
					});
			}
			else
			{
				SkillBonusAttrFactors[attr].Inc = inc;
			}
		}

		public static string ToString(this SkillName attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Slayer Attributes
		private static bool SupportsSlayers(Item item, string name, out PropertyInfo prop)
		{
			prop = item.GetType().GetProperty(name, TypeOfSlayerName);

			return prop != null;
		}

		public static bool SupportsSlayer(this Item item, out PropertyInfo[] props)
		{
			var p = new List<PropertyInfo>();
			PropertyInfo pi;

			if (SupportsSlayers(item, "Slayer", out pi))
			{
				p.Add(pi);
			}

			if (SupportsSlayers(item, "Slayer1", out pi))
			{
				p.Add(pi);
			}

			if (SupportsSlayers(item, "Slayer2", out pi))
			{
				p.Add(pi);
			}

			if (SupportsSlayers(item, "Slayer3", out pi))
			{
				p.Add(pi);
			}

			if (SupportsSlayers(item, "Slayer4", out pi))
			{
				p.Add(pi);
			}

			props = p.ToArray();
			return props.Length > 0;
		}

		private static bool HasSlayer(Item item, string name, SlayerName attr)
		{
			PropertyInfo pi = item.GetType().GetProperty(name, TypeOfSlayerName);

			if (pi == null)
			{
				return false;
			}

			return ((SlayerName)pi.GetValue(item, null) == attr);
		}

		public static bool HasSlayer(this Item item, SlayerName attr)
		{
			return (HasSlayer(item, "Slayer", attr) || HasSlayer(item, "Slayer1", attr) || HasSlayer(item, "Slayer2", attr) ||
					HasSlayer(item, "Slayer3", attr) || HasSlayer(item, "Slayer4", attr));
		}

		public static double GetWeight(this SlayerName attr)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return SlayerAttrFactors[attr].Weight;
		}

		public static void SetWeight(this SlayerName attr, double weight)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				SlayerAttrFactors[attr].Weight = weight;
			}
		}

		public static int GetMin(this SlayerName attr)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return SlayerAttrFactors[attr].Min;
		}

		public static void SetMin(this SlayerName attr, int min)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				SlayerAttrFactors[attr].Min = min;
			}
		}

		public static int GetMax(this SlayerName attr)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return SlayerAttrFactors[attr].Max;
		}

		public static void SetMax(this SlayerName attr, int max)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				SlayerAttrFactors[attr].Max = max;
			}
		}

		public static int GetInc(this SlayerName attr)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return SlayerAttrFactors[attr].Inc;
		}

		public static void SetInc(this SlayerName attr, int inc)
		{
			if (!SlayerAttrFactors.ContainsKey(attr))
			{
				SlayerAttrFactors.Add(attr, new AttributeFactors(1.0, 1, inc));
			}
			else
			{
				SlayerAttrFactors[attr].Inc = inc;
			}
		}

		public static bool IsSuper(this SlayerName attr)
		{
			return SuperSlayers.Contains(attr);
		}

		public static string ToString(this SlayerName attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion

		#region Talisman Slayer Attributes
		private static bool SupportsTalismanSlayers(Item item, string name, out PropertyInfo prop)
		{
			prop = item.GetType().GetProperty(name, TypeOfTalismanSlayerName);

			return prop != null;
		}

		public static bool SupportsTalismanSlayer(this Item item, out PropertyInfo[] props)
		{
			var p = new List<PropertyInfo>();
			PropertyInfo pi;

			if (SupportsTalismanSlayers(item, "Slayer", out pi))
			{
				p.Add(pi);
			}

			if (SupportsTalismanSlayers(item, "Slayer1", out pi))
			{
				p.Add(pi);
			}

			if (SupportsTalismanSlayers(item, "Slayer2", out pi))
			{
				p.Add(pi);
			}

			if (SupportsTalismanSlayers(item, "Slayer3", out pi))
			{
				p.Add(pi);
			}

			if (SupportsTalismanSlayers(item, "Slayer4", out pi))
			{
				p.Add(pi);
			}

			props = p.ToArray();
			return props.Length > 0;
		}

		private static bool HasTalismanSlayer(Item item, string name, TalismanSlayerName attr)
		{
			PropertyInfo pi = item.GetType().GetProperty(name, TypeOfTalismanSlayerName);

			if (pi == null)
			{
				return false;
			}

			return ((TalismanSlayerName)pi.GetValue(item, null) == attr);
		}

		public static bool HasSlayer(this Item item, TalismanSlayerName attr)
		{
			return (HasTalismanSlayer(item, "Slayer", attr) || HasTalismanSlayer(item, "Slayer1", attr) ||
					HasTalismanSlayer(item, "Slayer2", attr) || HasTalismanSlayer(item, "Slayer3", attr) ||
					HasTalismanSlayer(item, "Slayer4", attr));
		}

		public static double GetWeight(this TalismanSlayerName attr)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return TalismanSlayerAttrFactors[attr].Weight;
		}

		public static void SetWeight(this TalismanSlayerName attr, double weight)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Weight = weight
					});
			}
			else
			{
				TalismanSlayerAttrFactors[attr].Weight = weight;
			}
		}

		public static int GetMin(this TalismanSlayerName attr)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return TalismanSlayerAttrFactors[attr].Min;
		}

		public static void SetMin(this TalismanSlayerName attr, int min)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Min = min
					});
			}
			else
			{
				TalismanSlayerAttrFactors[attr].Min = min;
			}
		}

		public static int GetMax(this TalismanSlayerName attr)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return TalismanSlayerAttrFactors[attr].Max;
		}

		public static void SetMax(this TalismanSlayerName attr, int max)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Max = max
					});
			}
			else
			{
				TalismanSlayerAttrFactors[attr].Max = max;
			}
		}

		public static int GetInc(this TalismanSlayerName attr)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(attr, new AttributeFactors());
			}

			return TalismanSlayerAttrFactors[attr].Inc;
		}

		public static void SetInc(this TalismanSlayerName attr, int inc)
		{
			if (!TalismanSlayerAttrFactors.ContainsKey(attr))
			{
				TalismanSlayerAttrFactors.Add(
					attr,
					new AttributeFactors
					{
						Inc = inc
					});
			}
			else
			{
				TalismanSlayerAttrFactors[attr].Inc = inc;
			}
		}

		public static bool IsSuper(this TalismanSlayerName attr)
		{
			return SuperTalismanSlayers.Contains(attr);
		}

		public static string ToString(this TalismanSlayerName attr, double val, bool html = true)
		{
			return GetPropertyString(val, html);
		}
		#endregion
	}

	[PropertyObject]
	public sealed class AttributeFactors
	{
		[CommandProperty(AccessLevel.Administrator)]
		public double Weight { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public int Min { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public int Max { get; set; }

		[CommandProperty(AccessLevel.Administrator)]
		public int Inc { get; set; }

		public AttributeFactors(double weight = 1.0, int min = 0, int max = 1, int inc = 1)
		{
			Weight = weight;
			Min = min;
			Max = max;
			Inc = inc;
		}

		public AttributeFactors(GenericReader reader)
		{
			Deserialize(reader);
		}

		public double GetIntensity(int value)
		{
			value = Math.Max(Min, Math.Min(Max, value));

			if (value > 0)
			{
				return value / Math.Max(1.0, Max);
			}

			if (value < 0)
			{
				return value / Math.Min(-1.0, Min);
			}

			return 0;
		}

		public double GetWeight(int value)
		{
			return GetIntensity(value) * Weight;
		}

		public void Serialize(GenericWriter writer)
		{
			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					writer.Write(Min);
					goto case 0;
				case 0:
					{
						writer.Write(Weight);
						writer.Write(Max);
						writer.Write(Inc);
					}
					break;
			}
		}

		public void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
					Min = reader.ReadInt();
					goto case 0;
				case 0:
					{
						Weight = reader.ReadDouble();
						Max = reader.ReadInt();
						Inc = reader.ReadInt();
					}
					break;
			}
		}
	}
}