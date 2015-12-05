#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] Mobile.cs
// ************************************/
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using CustomsFramework;

using Server.Accounting;
using Server.Commands;
using Server.ContextMenus;
using Server.Guilds;
using Server.Gumps;
using Server.HuePickers;
using Server.Items;
using Server.Menus;
using Server.Mobiles;
using Server.Network;
using Server.Prompts;
using Server.Targeting;
#endregion

namespace Server
{

	#region Callbacks
	public delegate void TargetCallback(Mobile from, object targeted);

	public delegate void TargetStateCallback(Mobile from, object targeted, object state);

	public delegate void TargetStateCallback<T>(Mobile from, object targeted, T state);

	public delegate void PromptCallback(Mobile from, string text);

	public delegate void PromptStateCallback(Mobile from, string text, object state);

	public delegate void PromptStateCallback<T>(Mobile from, string text, T state);
	#endregion

	#region [...]Mods
	public class TimedSkillMod : SkillMod
	{
		private readonly DateTime _Expire;

		public TimedSkillMod(SkillName skill, bool relative, double value, TimeSpan delay)
			: this(skill, relative, value, DateTime.UtcNow + delay)
		{ }

		public TimedSkillMod(SkillName skill, bool relative, double value, DateTime expire)
			: base(skill, relative, value)
		{
			_Expire = expire;
		}

		public override bool CheckCondition()
		{
			return (DateTime.UtcNow < _Expire);
		}
	}

	public class EquipedSkillMod : SkillMod
	{
		private readonly Item _Item;
		private readonly Mobile _Mobile;

		public EquipedSkillMod(SkillName skill, bool relative, double value, Item item, Mobile mobile)
			: base(skill, relative, value)
		{
			_Item = item;
			_Mobile = mobile;
		}

		public override bool CheckCondition()
		{
			return (!_Item.Deleted && !_Mobile.Deleted && _Item.Parent == _Mobile);
		}
	}

	public class DefaultSkillMod : SkillMod
	{
		public DefaultSkillMod(SkillName skill, bool relative, double value)
			: base(skill, relative, value)
		{ }

		public override bool CheckCondition()
		{
			return true;
		}
	}

	public abstract class SkillMod
	{
		private Mobile _Owner;
		private SkillName _Skill;
		private bool _Relative;
		private double _Value;
		private bool _ObeyCap;

		protected SkillMod(SkillName skill, bool relative, double value)
		{
			_Skill = skill;
			_Relative = relative;
			_Value = value;
		}

		public bool ObeyCap
		{
			get { return _ObeyCap; }
			set
			{
				_ObeyCap = value;

				if (_Owner != null)
				{
					Skill sk = _Owner.Skills[_Skill];

					if (sk != null)
					{
						sk.Update();
					}
				}
			}
		}

		public Mobile Owner
		{
			get { return _Owner; }
			set
			{
				if (_Owner != value)
				{
					if (_Owner != null)
					{
						_Owner.RemoveSkillMod(this);
					}

					_Owner = value;

					if (_Owner != value)
					{
						_Owner.AddSkillMod(this);
					}
				}
			}
		}

		public void Remove()
		{
			Owner = null;
		}

		public SkillName Skill
		{
			get { return _Skill; }
			set
			{
				if (_Skill != value)
				{
					Skill oldUpdate = (_Owner != null ? _Owner.Skills[_Skill] : null);

					_Skill = value;

					if (_Owner != null)
					{
						Skill sk = _Owner.Skills[_Skill];

						if (sk != null)
						{
							sk.Update();
						}
					}

					if (oldUpdate != null)
					{
						oldUpdate.Update();
					}
				}
			}
		}

		public bool Relative
		{
			get { return _Relative; }
			set
			{
				if (_Relative != value)
				{
					_Relative = value;

					if (_Owner != null)
					{
						Skill sk = _Owner.Skills[_Skill];

						if (sk != null)
						{
							sk.Update();
						}
					}
				}
			}
		}

		public bool Absolute
		{
			get { return !_Relative; }
			set
			{
				if (_Relative == value)
				{
					_Relative = !value;

					if (_Owner != null)
					{
						Skill sk = _Owner.Skills[_Skill];

						if (sk != null)
						{
							sk.Update();
						}
					}
				}
			}
		}

		public double Value
		{
			get { return _Value; }
			set
			{
				if (_Value != value)
				{
					_Value = value;

					if (_Owner != null)
					{
						Skill sk = _Owner.Skills[_Skill];

						if (sk != null)
						{
							sk.Update();
						}
					}
				}
			}
		}

		public abstract bool CheckCondition();
	}

	public class ResistanceMod
	{
		private Mobile _Owner;
		private ResistanceType _Type;
		private int _Offset;

		public Mobile Owner { get { return _Owner; } set { _Owner = value; } }

		public ResistanceType Type
		{
			get { return _Type; }
			set
			{
				if (_Type != value)
				{
					_Type = value;

					if (_Owner != null)
					{
						_Owner.UpdateResistances();
					}
				}
			}
		}

		public int Offset
		{
			get { return _Offset; }
			set
			{
				if (_Offset != value)
				{
					_Offset = value;

					if (_Owner != null)
					{
						_Owner.UpdateResistances();
					}
				}
			}
		}

		public ResistanceMod(ResistanceType type, int offset)
		{
			_Type = type;
			_Offset = offset;
		}
	}

	public class StatMod
	{
		private readonly StatType _Type;
		private readonly string _Name;
		private readonly int _Offset;
		private readonly TimeSpan _Duration;
		private readonly DateTime _Added;

		public StatType Type { get { return _Type; } }
		public string Name { get { return _Name; } }
		public int Offset { get { return _Offset; } }

		public bool HasElapsed()
		{
			if (_Duration == TimeSpan.Zero)
			{
				return false;
			}

			return (DateTime.UtcNow - _Added) >= _Duration;
		}

		public StatMod(StatType type, string name, int offset, TimeSpan duration)
		{
			_Type = type;
			_Name = name;
			_Offset = offset;
			_Duration = duration;
			_Added = DateTime.UtcNow;
		}
	}
	#endregion

	public class DamageEntry
	{
		private readonly Mobile _Damager;
		private DateTime _LastDamage;

		public Mobile Damager { get { return _Damager; } }
		public int DamageGiven { get; set; }
		public DateTime LastDamage { get { return _LastDamage; } set { _LastDamage = value; } }
		public bool HasExpired { get { return (DateTime.UtcNow > (_LastDamage + _ExpireDelay)); } }
		public List<DamageEntry> Responsible { get; set; }

		private static TimeSpan _ExpireDelay = TimeSpan.FromMinutes(2.0);

		public static TimeSpan ExpireDelay { get { return _ExpireDelay; } set { _ExpireDelay = value; } }

		public DamageEntry(Mobile damager)
		{
			_Damager = damager;
		}
	}

	#region Enums
	[Flags]
	public enum StatType
	{
		Str = 1,
		Dex = 2,
		Int = 4,
		All = 7
	}

	public enum StatLockType : byte
	{
		Up,
		Down,
		Locked
	}

	[CustomEnum(new[] {"North", "Right", "East", "Down", "South", "Left", "West", "Up"}), Flags]
	public enum Direction : byte
	{
		North = 0x0,
		Right = 0x1,
		East = 0x2,
		Down = 0x3,
		South = 0x4,
		Left = 0x5,
		West = 0x6,
		Up = 0x7,

		Mask = 0x7,
		Running = 0x80,
		ValueMask = 0x87
	}

	[Flags]
	public enum MobileDelta
	{
		None = 0x00000000,
		Name = 0x00000001,
		Flags = 0x00000002,
		Hits = 0x00000004,
		Mana = 0x00000008,
		Stam = 0x00000010,
		Stat = 0x00000020,
		Noto = 0x00000040,
		Gold = 0x00000080,
		Weight = 0x00000100,
		Direction = 0x00000200,
		Hue = 0x00000400,
		Body = 0x00000800,
		Armor = 0x00001000,
		StatCap = 0x00002000,
		GhostUpdate = 0x00004000,
		Followers = 0x00008000,
		Properties = 0x00010000,
		TithingPoints = 0x00020000,
		Resistances = 0x00040000,
		WeaponDamage = 0x00080000,
		Hair = 0x00100000,
		FacialHair = 0x00200000,
		Race = 0x00400000,
		HealthbarYellow = 0x00800000,
		HealthbarPoison = 0x01000000,
        #region Enhance Client
        Face = 0x08000000,
        #endregion
		Attributes = 0x0000001C
	}

	public enum AccessLevel
	{
		Player,
		VIP,
		Counselor,
		Decorator,
		Spawner,
		GameMaster,
		Seer,
		Administrator,
		Developer,
		CoOwner,
		Owner
	}

	public enum VisibleDamageType
	{
		None,
		Related,
		Everyone
	}

	public enum ResistanceType
	{
		Physical,
		Fire,
		Cold,
		Poison,
		Energy
	}

	public enum ApplyPoisonResult
	{
		Poisoned,
		Immune,
		HigherPoisonActive,
		Cured
	}
	#endregion

	[Serializable]
	public sealed class MobileNotConnectedException : Exception
	{
		public MobileNotConnectedException(Mobile source, string message)
			: base(message)
		{
			Source = source.ToString();
		}
	}

	#region Delegates
	public delegate bool SkillCheckTargetHandler(
		Mobile from, SkillName skill, object target, double minSkill, double maxSkill);

	public delegate bool SkillCheckLocationHandler(Mobile from, SkillName skill, double minSkill, double maxSkill);

	public delegate bool SkillCheckDirectTargetHandler(Mobile from, SkillName skill, object target, double chance);

	public delegate bool SkillCheckDirectLocationHandler(Mobile from, SkillName skill, double chance);

	public delegate TimeSpan RegenRateHandler(Mobile from);

	public delegate bool AllowBeneficialHandler(Mobile from, Mobile target);

	public delegate bool AllowHarmfulHandler(Mobile from, Mobile target);

	public delegate Container CreateCorpseHandler(
		Mobile from, HairInfo hair, FacialHairInfo facialhair, List<Item> initialContent, List<Item> equipedItems);

	public delegate int AosStatusHandler(Mobile from, int index);
	#endregion

	/// <summary>
	///     Base class representing players, npcs, and creatures.
	/// </summary>
	public class Mobile : IHued, IComparable<Mobile>, ISerializable, ISpawnable
	{
		#region CompareTo(...)
		public int CompareTo(IEntity other)
		{
			if (other == null)
			{
				return -1;
			}

			return _Serial.CompareTo(other.Serial);
		}

		public int CompareTo(Mobile other)
		{
			return CompareTo((IEntity)other);
		}

		public int CompareTo(object other)
		{
			if (other == null || other is IEntity)
			{
				return CompareTo((IEntity)other);
			}

			throw new ArgumentException();
		}
		#endregion

		#region Customs Framework
		private List<BaseModule> _Modules = new List<BaseModule>();

		[CommandProperty(AccessLevel.Developer)]
		public List<BaseModule> Modules { get { return _Modules; } set { _Modules = value; } }

		//public List<BaseModule> Modules { get; private set; }

		public BaseModule GetModule(string name)
		{
			return Modules.FirstOrDefault(mod => mod.Name == name);
		}

		public BaseModule GetModule(Type type)
		{
			return Modules.FirstOrDefault(mod => mod.GetType() == type);
		}

		public List<BaseModule> GetModules(string name)
		{
			return Modules.Where(mod => mod.Name == name).ToList();
		}

		public List<BaseModule> SearchModules(string search)
		{
			string[] keywords = search.ToLower().Split(' ');
			var modules = new List<BaseModule>();

			foreach (BaseModule mod in Modules)
			{
				bool match = true;
				string name = mod.Name.ToLower();

				foreach (string keyword in keywords)
				{
					if (name.IndexOf(keyword, StringComparison.Ordinal) == -1)
					{
						match = false;
					}
				}

				if (match)
				{
					modules.Add(mod);
				}
			}

			return modules;
		}
		#endregion

		private static bool _DragEffects = true;

		public static bool DragEffects { get { return _DragEffects; } set { _DragEffects = value; } }

		#region Handlers
		public static AllowBeneficialHandler AllowBeneficialHandler { get; set; }

		public static AllowHarmfulHandler AllowHarmfulHandler { get; set; }

		private static SkillCheckTargetHandler _SkillCheckTargetHandler;
		private static SkillCheckLocationHandler _SkillCheckLocationHandler;
		private static SkillCheckDirectTargetHandler _SkillCheckDirectTargetHandler;
		private static SkillCheckDirectLocationHandler _SkillCheckDirectLocationHandler;

		public static SkillCheckTargetHandler SkillCheckTargetHandler { get { return _SkillCheckTargetHandler; } set { _SkillCheckTargetHandler = value; } }

		public static SkillCheckLocationHandler SkillCheckLocationHandler { get { return _SkillCheckLocationHandler; } set { _SkillCheckLocationHandler = value; } }

		public static SkillCheckDirectTargetHandler SkillCheckDirectTargetHandler { get { return _SkillCheckDirectTargetHandler; } set { _SkillCheckDirectTargetHandler = value; } }

		public static SkillCheckDirectLocationHandler SkillCheckDirectLocationHandler { get { return _SkillCheckDirectLocationHandler; } set { _SkillCheckDirectLocationHandler = value; } }

		private static AosStatusHandler _AosStatusHandler;

		public static AosStatusHandler AosStatusHandler { get { return _AosStatusHandler; } set { _AosStatusHandler = value; } }
		#endregion

		#region Regeneration
		private static RegenRateHandler _HitsRegenRate, _StamRegenRate, _ManaRegenRate;
		private static TimeSpan _DefaultHitsRate, _DefaultStamRate, _DefaultManaRate;

		public static RegenRateHandler HitsRegenRateHandler { get { return _HitsRegenRate; } set { _HitsRegenRate = value; } }

		public static TimeSpan DefaultHitsRate { get { return _DefaultHitsRate; } set { _DefaultHitsRate = value; } }

		public static RegenRateHandler StamRegenRateHandler { get { return _StamRegenRate; } set { _StamRegenRate = value; } }

		public static TimeSpan DefaultStamRate { get { return _DefaultStamRate; } set { _DefaultStamRate = value; } }

		public static RegenRateHandler ManaRegenRateHandler { get { return _ManaRegenRate; } set { _ManaRegenRate = value; } }

		public static TimeSpan DefaultManaRate { get { return _DefaultManaRate; } set { _DefaultManaRate = value; } }

		public static TimeSpan GetHitsRegenRate(Mobile m)
		{
		    if (_HitsRegenRate == null)
			{
				return _DefaultHitsRate;
			}
		    return _HitsRegenRate(m);
			}

		public static TimeSpan GetStamRegenRate(Mobile m)
		{
	        if (_StamRegenRate == null)
			{
				return _DefaultStamRate;
			}
	        return _StamRegenRate(m);
			}

		public static TimeSpan GetManaRegenRate(Mobile m)
		{
	        if (_ManaRegenRate == null)
			{
				return _DefaultManaRate;
			}
	        return _ManaRegenRate(m);
			}
		#endregion

		private class MovementRecord
		{
		    private long _End;

			private static readonly Queue<MovementRecord> _InstancePool = new Queue<MovementRecord>();

			public static MovementRecord NewInstance(long end)
			{
				MovementRecord r;

				if (_InstancePool.Count > 0)
				{
					r = _InstancePool.Dequeue();

					r._End = end;
				}
				else
				{
					r = new MovementRecord(end);
				}

				return r;
			}

			private MovementRecord(long end)
			{
				_End = end;
			}

			public bool Expired()
			{
				bool v = (Core.TickCount - _End >= 0);

				if (v)
				{
					_InstancePool.Enqueue(this);
				}

				return v;
			}
		}

		#region Var declarations
		private Serial _Serial;
		private Map _Map;
		private Point3D _Location;
		private Direction _Direction;
		private Body _Body;
		private int _Hue;
		private Poison _Poison;
		private Timer _PoisonTimer;
		private BaseGuild _Guild;
		private string _GuildTitle;
		private bool _Criminal;
		private string _Name;
		private int _Kills, _ShortTermMurders;
		private int _SpeechHue, _EmoteHue, _WhisperHue, _YellHue;
		private string _Language;
		private NetState _NetState;
		private bool _Female, _Warmode, _Hidden, _Blessed, _Flying;
		private int _StatCap;
		private int _Str, _Dex, _Int;
		private int _Hits, _Stam, _Mana;
		private int _Fame, _Karma;
		private AccessLevel _AccessLevel;
		private Skills _Skills;
		private List<Item> _Items;
		private bool _Player;
		private string _Title;
		private string _Profile;
		private bool _ProfileLocked;
		private int _LightLevel;
		private int _TotalGold, _TotalItems, _TotalWeight;
		private List<StatMod> _StatMods;
		private ISpell _Spell;
		private Target _Target;
		private Prompt _Prompt;
		private ContextMenu _ContextMenu;
		private List<AggressorInfo> _Aggressors, _Aggressed;
		private Mobile _Combatant;
		private List<Mobile> _Stabled;
		private bool _AutoPageNotify;
		private bool _CanHearGhosts;
		private bool _CanSwim, _CantWalk;
		private int _TithingPoints;
		private bool _DisplayGuildTitle;
		private Mobile _GuildFealty;
		private DateTime[] _StuckMenuUses;
		private Timer _ExpireCombatant;
		private Timer _ExpireCriminal;
		private Timer _ExpireAggrTimer;
		private Timer _LogoutTimer;
		private Timer _CombatTimer;
		private Timer _ManaTimer, _HitsTimer, _StamTimer;
		private long _NextSkillTime;
		private long _NextActionMessage;
		private bool _Paralyzed;
		private ParalyzedTimer _ParaTimer;
		private bool _Sleep;
		private SleepTimer _SleepTimer;
		private bool _Frozen;
		private FrozenTimer _FrozenTimer;
		private int _AllowedStealthSteps;
		private int _Hunger;
		private int _NameHue = -1;
		private Region _Region;
		private bool _DisarmReady, _StunReady;
		private int _BaseSoundId;
		private int _VirtualArmor;
		private bool _Squelched;
		private int _MagicDamageAbsorb;
		private int _Followers, _FollowersMax;
		private List<object> _Actions; // prefer List<object> over ArrayList for more specific profiling information
		private Queue<MovementRecord> _MoveRecords;
		private int _WarmodeChanges;
		private DateTime _NextWarmodeChange;
		private WarmodeTimer _WarmodeTimer;
		private int _Thirst, _BAC;
		private int _VirtualArmorMod;
		private VirtueInfo _Virtues;
		private object _Party;
		private List<SkillMod> _SkillMods;
		private Body _BodyMod;
		private DateTime _LastStrGain;
		private DateTime _LastIntGain;
		private DateTime _LastDexGain;
		private Race _Race;
		#endregion

		private static readonly TimeSpan _WarmodeSpamCatch = TimeSpan.FromSeconds((Core.SE ? 1.0 : 0.5));
		private static readonly TimeSpan _WarmodeSpamDelay = TimeSpan.FromSeconds((Core.SE ? 4.0 : 2.0));

		private const int WarmodeCatchCount = 4;
		// Allow four warmode changes in 0.5 seconds, any more will be delay for two seconds

		[CommandProperty(AccessLevel.Decorator)]
		public Race Race
		{
			get
			{
				if (_Race == null)
				{
					_Race = Race.DefaultRace;
				}

				return _Race;
			}
			set
			{
				Race oldRace = Race;

				_Race = value;

				if (_Race == null)
				{
					_Race = Race.DefaultRace;
				}

				Body = _Race.Body(this);
				UpdateResistances();

				Delta(MobileDelta.Race);

				OnRaceChange(oldRace);
			}
		}

		protected virtual void OnRaceChange(Race oldRace)
		{ }

		public virtual double RacialSkillBonus { get { return 0; } }

        public virtual bool CanBeRevealed { get { return true; } }

		private List<ResistanceMod> _ResistMods;

		private int[] _Resistances;

		protected List<string> _SlayerVulnerabilities = new List<string>();
		protected bool _SpecialSlayerMechanics = false;

		public List<String> SlayerVulnerabilities { get { return _SlayerVulnerabilities; } }

		[CommandProperty(AccessLevel.Decorator)]
		public bool SpecialSlayerMechanics { get { return _SpecialSlayerMechanics; } }

		public int[] Resistances { get { return _Resistances; } }

		public virtual int BasePhysicalResistance { get { return 0; } }
		public virtual int BaseFireResistance { get { return 0; } }
		public virtual int BaseColdResistance { get { return 0; } }
		public virtual int BasePoisonResistance { get { return 0; } }
		public virtual int BaseEnergyResistance { get { return 0; } }

		public virtual void ComputeLightLevels(out int global, out int personal)
		{
			ComputeBaseLightLevels(out global, out personal);

			if (_Region != null)
			{
				_Region.AlterLightLevel(this, ref global, ref personal);
			}
		}

		public virtual void ComputeBaseLightLevels(out int global, out int personal)
		{
			global = 0;
			personal = _LightLevel;
		}

		public virtual void CheckLightLevels(bool forceResend)
		{ }

		[CommandProperty(AccessLevel.Counselor)]
		public virtual int PhysicalResistance { get { return GetResistance(ResistanceType.Physical); } }

		[CommandProperty(AccessLevel.Counselor)]
		public virtual int FireResistance { get { return GetResistance(ResistanceType.Fire); } }

		[CommandProperty(AccessLevel.Counselor)]
		public virtual int ColdResistance { get { return GetResistance(ResistanceType.Cold); } }

		[CommandProperty(AccessLevel.Counselor)]
		public virtual int PoisonResistance { get { return GetResistance(ResistanceType.Poison); } }

		[CommandProperty(AccessLevel.Counselor)]
		public virtual int EnergyResistance { get { return GetResistance(ResistanceType.Energy); } }

		public virtual void UpdateResistances()
		{
			if (_Resistances == null)
			{
				_Resistances = new int[] {int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue};
			}

			bool delta = false;

			for (int i = 0; i < _Resistances.Length; ++i)
			{
				if (_Resistances[i] != int.MinValue)
				{
					_Resistances[i] = int.MinValue;
					delta = true;
				}
			}

			if (delta)
			{
				Delta(MobileDelta.Resistances);
			}
		}

		public virtual int GetResistance(ResistanceType type)
		{
			if (_Resistances == null)
			{
				_Resistances = new int[] {int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue};
			}

			var v = (int)type;

			if (v < 0 || v >= _Resistances.Length)
			{
				return 0;
			}

			int res = _Resistances[v];

			if (res == int.MinValue)
			{
				ComputeResistances();
				res = _Resistances[v];
			}

			return res;
		}

		public List<ResistanceMod> ResistanceMods { get { return _ResistMods; } set { _ResistMods = value; } }

		public virtual void AddResistanceMod(ResistanceMod toAdd)
		{
			if (_ResistMods == null)
			{
				_ResistMods = new List<ResistanceMod>();
			}

			_ResistMods.Add(toAdd);
			UpdateResistances();
		}

		public virtual void RemoveResistanceMod(ResistanceMod toRemove)
		{
			if (_ResistMods != null)
			{
				_ResistMods.Remove(toRemove);

				if (_ResistMods.Count == 0)
				{
					_ResistMods = null;
				}
			}

			UpdateResistances();
		}

		private static int _MaxPlayerResistance = 70;

		public static int MaxPlayerResistance { get { return _MaxPlayerResistance; } set { _MaxPlayerResistance = value; } }

		public virtual void ComputeResistances()
		{
			if (_Resistances == null)
			{
				_Resistances = new int[] {int.MinValue, int.MinValue, int.MinValue, int.MinValue, int.MinValue};
			}

			for (int i = 0; i < _Resistances.Length; ++i)
			{
				_Resistances[i] = 0;
			}

			_Resistances[0] += BasePhysicalResistance;
			_Resistances[1] += BaseFireResistance;
			_Resistances[2] += BaseColdResistance;
			_Resistances[3] += BasePoisonResistance;
			_Resistances[4] += BaseEnergyResistance;

			for (int i = 0; _ResistMods != null && i < _ResistMods.Count; ++i)
			{
				ResistanceMod mod = _ResistMods[i];
				var v = (int)mod.Type;

				if (v >= 0 && v < _Resistances.Length)
				{
					_Resistances[v] += mod.Offset;
				}
			}

			for (int i = 0; i < _Items.Count; ++i)
			{
				Item item = _Items[i];

				if (item.CheckPropertyConfliction(this))
				{
					continue;
				}

				_Resistances[0] += item.PhysicalResistance;
				_Resistances[1] += item.FireResistance;
				_Resistances[2] += item.ColdResistance;
				_Resistances[3] += item.PoisonResistance;
				_Resistances[4] += item.EnergyResistance;
			}

			for (int i = 0; i < _Resistances.Length; ++i)
			{
				int min = GetMinResistance((ResistanceType)i);
				int max = GetMaxResistance((ResistanceType)i);

				if (max < min)
				{
					max = min;
				}

				if (_Resistances[i] > max)
				{
					_Resistances[i] = max;
				}
				else if (_Resistances[i] < min)
				{
					_Resistances[i] = min;
				}
			}
		}

		public virtual int GetMinResistance(ResistanceType type)
		{
			return int.MinValue;
		}

		public virtual int GetMaxResistance(ResistanceType type)
		{
			if (_Player)
			{
				return _MaxPlayerResistance;
			}

			return int.MaxValue;
		}

		public int GetAosStatus(int index)
		{
			return (_AosStatusHandler == null) ? 0 : _AosStatusHandler(this, index);
		}

		public virtual void SendPropertiesTo(Mobile from)
		{
			from.Send(PropertyList);
		}

		public virtual void OnAosSingleClick(Mobile from)
		{
			ObjectPropertyList opl = PropertyList;

			if (opl.Header > 0)
			{
				int hue;

				if (_NameHue != -1)
				{
					hue = _NameHue;
				}
				else if (IsStaff())
				{
					hue = 11;
				}
				else
				{
					hue = Notoriety.GetHue(Notoriety.Compute(from, this));
				}

				from.Send(new MessageLocalized(_Serial, Body, MessageType.Label, hue, 3, opl.Header, Name, opl.HeaderArgs));
			}
		}

		public virtual string ApplyNameSuffix(string suffix)
		{
			return suffix;
		}

		public virtual void AddNameProperties(ObjectPropertyList list)
		{
			string name = Name;

			if (name == null)
			{
				name = String.Empty;
			}

			string prefix = "";

			if (ShowFameTitle && (_Player || _Body.IsHuman) && _Fame >= 10000)
			{
				prefix = _Female ? "Lady" : "Lord";
			}

			string suffix = "";

			if (PropertyTitle && Title != null && Title.Length > 0)
			{
				suffix = Title;
			}

			BaseGuild guild = _Guild;

			if (guild != null && (_Player || _DisplayGuildTitle))
			{
				if (suffix.Length > 0)
				{
					suffix = String.Format("{0} [{1}]", suffix, Utility.FixHtml(guild.Abbreviation));
				}
				else
				{
					suffix = String.Format("[{0}]", Utility.FixHtml(guild.Abbreviation));
				}
			}

			suffix = ApplyNameSuffix(suffix);

			list.Add(1050045, "{0} \t{1}\t {2}", prefix, name, suffix); // ~1_PREFIX~~2_NAME~~3_SUFFIX~

			if (guild != null && (_DisplayGuildTitle || (_Player && guild.Type != GuildType.Regular)))
			{
				string type;

				if (guild.Type >= 0 && (int)guild.Type < _GuildTypes.Length)
				{
					type = _GuildTypes[(int)guild.Type];
				}
				else
				{
					type = "";
				}

				string title = GuildTitle;

				if (title == null)
				{
					title = "";
				}
				else
				{
					title = title.Trim();
				}

				if (NewGuildDisplay && title.Length > 0)
				{
					list.Add("{0}, {1}", Utility.FixHtml(title), Utility.FixHtml(guild.Name));
				}
				else
				{
					if (title.Length > 0)
					{
						list.Add("{0}, {1} Guild{2}", Utility.FixHtml(title), Utility.FixHtml(guild.Name), type);
					}
					else
					{
						list.Add(Utility.FixHtml(guild.Name));
					}
				}
			}
		}

		public virtual bool NewGuildDisplay { get { return false; } }

		public virtual void GetProperties(ObjectPropertyList list)
		{
			AddNameProperties(list);
		}

		public virtual void GetChildProperties(ObjectPropertyList list, Item item)
		{ }

		public virtual void GetChildNameProperties(ObjectPropertyList list, Item item)
		{ }

		private void UpdateAggrExpire()
		{
			if (_Deleted || (_Aggressors.Count == 0 && _Aggressed.Count == 0))
			{
				StopAggrExpire();
			}
			else if (_ExpireAggrTimer == null)
			{
				_ExpireAggrTimer = new ExpireAggressorsTimer(this);
				_ExpireAggrTimer.Start();
			}
		}

		private void StopAggrExpire()
		{
			if (_ExpireAggrTimer != null)
			{
				_ExpireAggrTimer.Stop();
			}

			_ExpireAggrTimer = null;
		}

		private void CheckAggrExpire()
		{
			for (int i = _Aggressors.Count - 1; i >= 0; --i)
			{
				if (i >= _Aggressors.Count)
				{
					continue;
				}

				AggressorInfo info = _Aggressors[i];

				if (info.Expired)
				{
					Mobile attacker = info.Attacker;
					attacker.RemoveAggressed(this);

					_Aggressors.RemoveAt(i);
					info.Free();

					if (_NetState != null && CanSee(attacker) && Utility.InUpdateRange(_Location, attacker._Location))
					{
						_NetState.Send(MobileIncoming.Create(_NetState, this, attacker));
					}
				}
			}

			for (int i = _Aggressed.Count - 1; i >= 0; --i)
			{
				if (i >= _Aggressed.Count)
				{
					continue;
				}

				AggressorInfo info = _Aggressed[i];

				if (info.Expired)
				{
					Mobile defender = info.Defender;
					defender.RemoveAggressor(this);

					_Aggressed.RemoveAt(i);
					info.Free();

					if (_NetState != null && CanSee(defender) && Utility.InUpdateRange(_Location, defender._Location))
					{
						_NetState.Send(MobileIncoming.Create(_NetState, this, defender));
					}
				}
			}

			UpdateAggrExpire();
		}

		public List<Mobile> Stabled { get { return _Stabled; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public VirtueInfo Virtues { get { return _Virtues; } set { } }

		public object Party { get { return _Party; } set { _Party = value; } }
		public List<SkillMod> SkillMods { get { return _SkillMods; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VirtualArmorMod
		{
			get { return _VirtualArmorMod; }
			set
			{
				if (_VirtualArmorMod != value)
				{
					_VirtualArmorMod = value;

					Delta(MobileDelta.Armor);
				}
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked when <paramref name="skill" /> changes in some way.
		/// </summary>
		public virtual void OnSkillInvalidated(Skill skill)
		{ }

		public virtual void UpdateSkillMods()
		{
			ValidateSkillMods();

			for (int i = 0; i < _SkillMods.Count; ++i)
			{
				SkillMod mod = _SkillMods[i];

				Skill sk = _Skills[mod.Skill];

				if (sk != null)
				{
					sk.Update();
				}
			}
		}

		public virtual void ValidateSkillMods()
		{
			for (int i = 0; i < _SkillMods.Count;)
			{
				SkillMod mod = _SkillMods[i];

				if (mod.CheckCondition())
				{
					++i;
				}
				else
				{
					InternalRemoveSkillMod(mod);
				}
			}
		}

		public virtual void AddSkillMod(SkillMod mod)
		{
			if (mod == null)
			{
				return;
			}

			ValidateSkillMods();

			if (!_SkillMods.Contains(mod))
			{
				_SkillMods.Add(mod);
				mod.Owner = this;

				Skill sk = _Skills[mod.Skill];

				if (sk != null)
				{
					sk.Update();
				}
			}
		}

		public virtual void RemoveSkillMod(SkillMod mod)
		{
			if (mod == null)
			{
				return;
			}

			ValidateSkillMods();

			InternalRemoveSkillMod(mod);
		}

		private void InternalRemoveSkillMod(SkillMod mod)
		{
			if (_SkillMods.Contains(mod))
			{
				_SkillMods.Remove(mod);
				mod.Owner = null;

				Skill sk = _Skills[mod.Skill];

				if (sk != null)
				{
					sk.Update();
				}
			}
		}

		private class WarmodeTimer : Timer
		{
			private readonly Mobile _Mobile;
			private bool _Value;

			public bool Value { get { return _Value; } set { _Value = value; } }

			public WarmodeTimer(Mobile m, bool value)
				: base(_WarmodeSpamDelay)
			{
				_Mobile = m;
				_Value = value;
			}

			protected override void OnTick()
			{
				_Mobile.Warmode = _Value;
				_Mobile._WarmodeChanges = 0;

				_Mobile._WarmodeTimer = null;
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked when a client, <paramref name="from" />, invokes a 'help request' for the Mobile. Seemingly no longer functional in newer clients.
		/// </summary>
		public virtual void OnHelpRequest(Mobile from)
		{ }

		public void DelayChangeWarmode(bool value)
		{
			if (_WarmodeTimer != null)
			{
				_WarmodeTimer.Value = value;
				return;
			}

			if (_Warmode == value)
			{
				return;
			}

			DateTime now = DateTime.UtcNow, next = _NextWarmodeChange;

			if (now > next || _WarmodeChanges == 0)
			{
				_WarmodeChanges = 1;
				_NextWarmodeChange = now + _WarmodeSpamCatch;
			}
			else if (_WarmodeChanges == WarmodeCatchCount)
			{
				_WarmodeTimer = new WarmodeTimer(this, value);
				_WarmodeTimer.Start();

				return;
			}
			else
			{
				++_WarmodeChanges;
			}

			Warmode = value;
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int MeleeDamageAbsorb { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public int MagicDamageAbsorb { get { return _MagicDamageAbsorb; } set { _MagicDamageAbsorb = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillsTotal { get { return _Skills == null ? 0 : _Skills.Total; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int SkillsCap
		{
			get { return _Skills == null ? 0 : _Skills.Cap; }
			set
			{
				if (_Skills != null)
				{
					_Skills.Cap = value;
				}
			}
		}

		public bool InLOS(Mobile target)
		{
			if (_Deleted || _Map == null)
			{
				return false;
			}
		    if (target == this || IsStaff())
			{
				return true;
			}

		    return _Map.LineOfSight(this, target);
		}

		public bool InLOS(object target)
		{
			if (_Deleted || _Map == null)
			{
				return false;
			}
		    if (target == this || IsStaff())
			{
				return true;
			}
		    if (target is Item && ((Item)target).RootParent == this)
			{
				return true;
			}

		    return _Map.LineOfSight(this, target);
		}

		public bool InLOS(Point3D target)
		{
			if (_Deleted || _Map == null)
			{
				return false;
			}
		    if (IsStaff())
			{
				return true;
			}

		    return _Map.LineOfSight(this, target);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int BaseSoundID { get { return _BaseSoundId; } set { _BaseSoundId = value; } }

		public long NextCombatTime { get { return _NextCombatTime; } set { _NextCombatTime = value; } }

		public bool BeginAction(object toLock)
		{
			if (_Actions == null)
			{
				_Actions = new List<object>();

				_Actions.Add(toLock);

				return true;
			}
		    if (!_Actions.Contains(toLock))
			{
		        _Actions.Add(toLock);

				return true;
			}

			return false;
		}

		public bool CanBeginAction(object toLock)
		{
			return (_Actions == null || !_Actions.Contains(toLock));
		}

		public void EndAction(object toLock)
		{
			if (_Actions != null)
			{
				_Actions.Remove(toLock);

				if (_Actions.Count == 0)
				{
					_Actions = null;
				}
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public int NameHue { get { return _NameHue; } set { _NameHue = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int Hunger
		{
			get { return _Hunger; }
			set
			{
				int oldValue = _Hunger;

				if (oldValue != value)
				{
					_Hunger = value;

					EventSink.InvokeHungerChanged(new HungerChangedEventArgs(this, oldValue));
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Thirst { get { return _Thirst; } set { _Thirst = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public int BAC { get { return _BAC; } set { _BAC = value; } }

		private long _LastMoveTime;

		/// <summary>
		///     Gets or sets the number of steps this player may take when hidden before being revealed.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int AllowedStealthSteps { get { return _AllowedStealthSteps; } set { _AllowedStealthSteps = value; } }

		/* Logout:
		* 
		* When a client logs into mobile x
		*  - if ( x is Internalized ) move x to logout location and map
		* 
		* When a client attached to a mobile disconnects
		*  - LogoutTimer is started
		*	   - Delay is taken from Region.GetLogoutDelay to allow insta-logout regions.
		*     - OnTick : Location and map are stored, and mobile is internalized
		* 
		* Some things to consider:
		*  - An internalized person getting killed (say, by poison). Where does the body go?
		*  - Regions now have a GetLogoutDelay( Mobile m ); virtual function (see above)
		*/
		private Point3D _LogoutLocation;
		private Map _LogoutMap;

		public virtual TimeSpan GetLogoutDelay()
		{
			return Region.GetLogoutDelay(this);
		}

		private StatLockType _StrLock, _DexLock, _IntLock;

		private Item _Holding;

		public Item Holding
		{
			get { return _Holding; }
			set
			{
				if (_Holding != value)
				{
					if (_Holding != null)
					{
						UpdateTotal(_Holding, TotalType.Weight, -(_Holding.TotalWeight + _Holding.PileWeight));

						if (_Holding.HeldBy == this)
						{
							_Holding.HeldBy = null;
						}
					}

					if (value != null && _Holding != null)
					{
						DropHolding();
					}

					_Holding = value;

					if (_Holding != null)
					{
						UpdateTotal(_Holding, TotalType.Weight, _Holding.TotalWeight + _Holding.PileWeight);

						if (_Holding.HeldBy == null)
						{
							_Holding.HeldBy = this;
						}
					}
				}
			}
		}

		public long LastMoveTime { get { return _LastMoveTime; } set { _LastMoveTime = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Paralyzed
		{
			get { return _Paralyzed; }
			set
			{
				if (_Paralyzed != value)
				{
					_Paralyzed = value;
					Delta(MobileDelta.Flags);

					SendLocalizedMessage(_Paralyzed ? 502381 : 502382);

					if (_ParaTimer != null)
					{
						_ParaTimer.Stop();
						_ParaTimer = null;
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool Asleep
		{
			get { return _Sleep; }
			set
			{
				if (_Sleep != value)
				{
					_Sleep = value;

					if (_SleepTimer != null)
					{
						Send(SpeedControl.Disable);
						_SleepTimer.Stop();
						_SleepTimer = null;
					}
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool DisarmReady
		{
			get { return _DisarmReady; }
			set
			{
				_DisarmReady = value;
				//SendLocalizedMessage( value ? 1019013 : 1019014 );
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool StunReady
		{
			get { return _StunReady; }
			set
			{
				_StunReady = value;
				//SendLocalizedMessage( value ? 1019011 : 1019012 );
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public bool Frozen
		{
			get { return _Frozen; }
			set
			{
				if (_Frozen != value)
				{
					_Frozen = value;
					Delta(MobileDelta.Flags);

					if (_FrozenTimer != null)
					{
						_FrozenTimer.Stop();
						_FrozenTimer = null;
					}
				}
			}
		}

		public void Paralyze(TimeSpan duration)
		{
			if (!_Paralyzed)
			{
				Paralyzed = true;

				_ParaTimer = new ParalyzedTimer(this, duration);
				_ParaTimer.Start();
			}
		}

		public void Sleep(TimeSpan duration)
		{
			if (!_Sleep)
			{
				Asleep = true;
				Send(SpeedControl.WalkSpeed);

				_SleepTimer = new SleepTimer(this, duration);
				_SleepTimer.Start();
			}
		}

		public void Freeze(TimeSpan duration)
		{
			if (!_Frozen)
			{
				Frozen = true;

				_FrozenTimer = new FrozenTimer(this, duration);
				_FrozenTimer.Start();
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawStr" /> property.
		/// </summary>
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public StatLockType StrLock
		{
			get { return _StrLock; }
			set
			{
				if (_StrLock != value)
				{
					_StrLock = value;

					if (_NetState != null)
					{
						_NetState.Send(new StatLockInfo(this));
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawDex" /> property.
		/// </summary>
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public StatLockType DexLock
		{
			get { return _DexLock; }
			set
			{
				if (_DexLock != value)
				{
					_DexLock = value;

					if (_NetState != null)
					{
						_NetState.Send(new StatLockInfo(this));
					}
				}
			}
		}

		/// <summary>
		///     Gets or sets the <see cref="StatLockType">lock state</see> for the <see cref="RawInt" /> property.
		/// </summary>
		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public StatLockType IntLock
		{
			get { return _IntLock; }
			set
			{
				if (_IntLock != value)
				{
					_IntLock = value;

					if (_NetState != null)
					{
						_NetState.Send(new StatLockInfo(this));
					}
				}
			}
		}

		public override string ToString()
		{
			return String.Format("0x{0:X} \"{1}\"", _Serial.Value, Name);
		}

		public long NextActionTime { get; set; }

		public long NextActionMessage { get { return _NextActionMessage; } set { _NextActionMessage = value; } }

		private static int _ActionMessageDelay = 125;

		public static int ActionMessageDelay { get { return _ActionMessageDelay; } set { _ActionMessageDelay = value; } }

		public virtual void SendSkillMessage()
		{
			if (_NextActionMessage - Core.TickCount >= 0)
			{
				return;
			}

			_NextActionMessage = Core.TickCount + _ActionMessageDelay;

			SendLocalizedMessage(500118); // You must wait a few moments to use another skill.
		}

		public virtual void SendActionMessage()
		{
			if (_NextActionMessage - Core.TickCount >= 0)
			{
				return;
			}

			_NextActionMessage = Core.TickCount + _ActionMessageDelay;

			SendLocalizedMessage(500119); // You must wait to perform another action.
		}

		public virtual void ClearHands()
		{
			ClearHand(FindItemOnLayer(Layer.OneHanded));
			ClearHand(FindItemOnLayer(Layer.TwoHanded));
		}

		public virtual void ClearHand(Item item)
		{
			if (item != null && item.Movable && !item.AllowEquipedCast(this))
			{
				Container pack = Backpack;

				if (pack == null)
				{
					AddToBackpack(item);
				}
				else
				{
					pack.DropItem(item);
				}
			}
		}

		private static bool _GlobalRegenThroughPoison = true;

		public static bool GlobalRegenThroughPoison { get { return _GlobalRegenThroughPoison; } set { _GlobalRegenThroughPoison = value; } }

		public virtual bool RegenThroughPoison { get { return _GlobalRegenThroughPoison; } }

		public virtual bool CanRegenHits { get { return Alive && (RegenThroughPoison || !Poisoned); } }
		public virtual bool CanRegenStam { get { return Alive; } }
		public virtual bool CanRegenMana { get { return Alive; } }

		#region Timers
		private class ManaTimer : Timer
		{
			private readonly Mobile _Owner;

			public ManaTimer(Mobile m)
				: base(GetManaRegenRate(m), GetManaRegenRate(m))
			{
				Priority = TimerPriority.FiftyMS;
				_Owner = m;
			}

			protected override void OnTick()
			{
				if (_Owner.CanRegenMana) // _Owner.Alive )
				{
					_Owner.Mana++;
				}

				Delay = Interval = GetManaRegenRate(_Owner);
			}
		}

		private class HitsTimer : Timer
		{
			private readonly Mobile _Owner;

			public HitsTimer(Mobile m)
				: base(GetHitsRegenRate(m), GetHitsRegenRate(m))
			{
				Priority = TimerPriority.FiftyMS;
				_Owner = m;
			}

			protected override void OnTick()
			{
				if (_Owner.CanRegenHits) // _Owner.Alive && !_Owner.Poisoned )
				{
					_Owner.Hits++;
				}

				Delay = Interval = GetHitsRegenRate(_Owner);
			}
		}

		private class StamTimer : Timer
		{
			private readonly Mobile _Owner;

			public StamTimer(Mobile m)
				: base(GetStamRegenRate(m), GetStamRegenRate(m))
			{
				Priority = TimerPriority.FiftyMS;
				_Owner = m;
			}

			protected override void OnTick()
			{
				if (_Owner.CanRegenStam) // _Owner.Alive )
				{
					_Owner.Stam++;
				}

				Delay = Interval = GetStamRegenRate(_Owner);
			}
		}

		private class LogoutTimer : Timer
		{
			private readonly Mobile _Mobile;

			public LogoutTimer(Mobile m)
				: base(TimeSpan.FromDays(1.0))
			{
				Priority = TimerPriority.OneSecond;
				_Mobile = m;
			}

			protected override void OnTick()
			{
				if (_Mobile._Map != Map.Internal)
				{
					EventSink.InvokeLogout(new LogoutEventArgs(_Mobile));

					_Mobile._LogoutLocation = _Mobile._Location;
					_Mobile._LogoutMap = _Mobile._Map;

					_Mobile.Internalize();
				}
			}
		}

		private class ParalyzedTimer : Timer
		{
			private readonly Mobile _Mobile;

			public ParalyzedTimer(Mobile m, TimeSpan duration)
				: base(duration)
			{
				Priority = TimerPriority.TwentyFiveMS;
				_Mobile = m;
			}

			protected override void OnTick()
			{
				_Mobile.Paralyzed = false;
			}
		}

		private class SleepTimer : Timer
		{
			private readonly Mobile _Mobile;

			public SleepTimer(Mobile m, TimeSpan duration)
				: base(duration)
			{
				Priority = TimerPriority.TwentyFiveMS;
				_Mobile = m;
			}

			protected override void OnTick()
			{
				_Mobile.Asleep = false;
			}
		}

		private class FrozenTimer : Timer
		{
			private readonly Mobile _Mobile;

			public FrozenTimer(Mobile m, TimeSpan duration)
				: base(duration)
			{
				Priority = TimerPriority.TwentyFiveMS;
				_Mobile = m;
			}

			protected override void OnTick()
			{
				_Mobile.Frozen = false;
			}
		}

		private class CombatTimer : Timer
		{
			private readonly Mobile _Mobile;

			public CombatTimer(Mobile m)
				: base(TimeSpan.FromSeconds(0.0), TimeSpan.FromSeconds(0.01), 0)
			{
				_Mobile = m;

				if (!_Mobile._Player && _Mobile._Dex <= 100)
				{
					Priority = TimerPriority.FiftyMS;
				}
			}

			protected override void OnTick()
			{
				if (Core.TickCount - _Mobile._NextCombatTime >= 0)
				{
					Mobile combatant = _Mobile.Combatant;

					// If no combatant, wrong map, one of us is a ghost, or cannot see, or deleted, then stop combat
					if (combatant == null || combatant._Deleted || _Mobile._Deleted || combatant._Map != _Mobile._Map ||
						!combatant.Alive || !_Mobile.Alive || !_Mobile.CanSee(combatant) || combatant.IsDeadBondedPet ||
						_Mobile.IsDeadBondedPet)
					{
						_Mobile.Combatant = null;
						return;
					}

					IWeapon weapon = _Mobile.Weapon;

					if (!_Mobile.InRange(combatant, weapon.MaxRange))
					{
						return;
					}

					if (_Mobile.InLOS(combatant))
					{
						weapon.OnBeforeSwing(_Mobile, combatant); //OnBeforeSwing for checking in regards to being hidden and whatnot
						_Mobile.RevealingAction();
						_Mobile._NextCombatTime = Core.TickCount + (int)weapon.OnSwing(_Mobile, combatant).TotalMilliseconds;
					}
				}
			}
		}

		private class ExpireCombatantTimer : Timer
		{
			private readonly Mobile _Mobile;

			public ExpireCombatantTimer(Mobile m)
				: base(TimeSpan.FromMinutes(1.0))
			{
				Priority = TimerPriority.FiveSeconds;
				_Mobile = m;
			}

			protected override void OnTick()
			{
				_Mobile.Combatant = null;
			}
		}

		private static TimeSpan _ExpireCriminalDelay = TimeSpan.FromMinutes(2.0);

		public static TimeSpan ExpireCriminalDelay { get { return _ExpireCriminalDelay; } set { _ExpireCriminalDelay = value; } }

		private class ExpireCriminalTimer : Timer
		{
			private readonly Mobile _Mobile;

			public ExpireCriminalTimer(Mobile m)
				: base(_ExpireCriminalDelay)
			{
				Priority = TimerPriority.FiveSeconds;
				_Mobile = m;
			}

			protected override void OnTick()
			{
				_Mobile.Criminal = false;
			}
		}

		private class ExpireAggressorsTimer : Timer
		{
			private readonly Mobile _Mobile;

			public ExpireAggressorsTimer(Mobile m)
				: base(TimeSpan.FromSeconds(5.0), TimeSpan.FromSeconds(5.0))
			{
				_Mobile = m;
				Priority = TimerPriority.FiveSeconds;
			}

			protected override void OnTick()
			{
				if (_Mobile.Deleted || (_Mobile.Aggressors.Count == 0 && _Mobile.Aggressed.Count == 0))
				{
					_Mobile.StopAggrExpire();
				}
				else
				{
					_Mobile.CheckAggrExpire();
				}
			}
		}
		#endregion

		private long _NextCombatTime;

		public long NextSkillTime { get { return _NextSkillTime; } set { _NextSkillTime = value; } }

		public List<AggressorInfo> Aggressors { get { return _Aggressors; } }

		public List<AggressorInfo> Aggressed { get { return _Aggressed; } }

		private int _ChangingCombatant;

		public bool ChangingCombatant { get { return (_ChangingCombatant > 0); } }

		public virtual void Attack(Mobile m)
		{
			if (CheckAttack(m))
			{
				Combatant = m;
			}
		}

		public virtual bool CheckAttack(Mobile m)
		{
			return (Utility.InUpdateRange(this, m) && CanSee(m) && InLOS(m));
		}

		/// <summary>
		///     Overridable. Gets or sets which Mobile that this Mobile is currently engaged in combat with.
		///     <seealso cref="OnCombatantChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual Mobile Combatant
		{
			get { return _Combatant; }
			set
			{
				if (_Deleted)
				{
					return;
				}

				if (_Combatant != value && value != this)
				{
					Mobile old = _Combatant;

					++_ChangingCombatant;
					_Combatant = value;

					if ((_Combatant != null && !CanBeHarmful(_Combatant, false)) || !Region.OnCombatantChange(this, old, _Combatant))
					{
						_Combatant = old;
						--_ChangingCombatant;
						return;
					}

					if (_NetState != null)
					{
						_NetState.Send(new ChangeCombatant(_Combatant));
					}

					if (_Combatant == null)
					{
						if (_ExpireCombatant != null)
						{
							_ExpireCombatant.Stop();
						}

						if (_CombatTimer != null)
						{
							_CombatTimer.Stop();
						}

						_ExpireCombatant = null;
						_CombatTimer = null;
					}
					else
					{
						if (_ExpireCombatant == null)
						{
							_ExpireCombatant = new ExpireCombatantTimer(this);
						}

						_ExpireCombatant.Start();

						if (_CombatTimer == null)
						{
							_CombatTimer = new CombatTimer(this);
						}

						_CombatTimer.Start();
					}

					if (_Combatant != null && CanBeHarmful(_Combatant, false))
					{
						DoHarmful(_Combatant);

						if (_Combatant != null)
						{
							_Combatant.PlaySound(_Combatant.GetAngerSound());
						}
					}

					OnCombatantChange();
					--_ChangingCombatant;
				}
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked after the <see cref="Combatant" /> property has changed.
		///     <seealso cref="Combatant" />
		/// </summary>
		public virtual void OnCombatantChange()
		{ }

		public double GetDistanceToSqrt(Point3D p)
		{
			int xDelta = _Location.m_X - p.m_X;
			int yDelta = _Location.m_Y - p.m_Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public double GetDistanceToSqrt(Mobile m)
		{
			int xDelta = _Location.m_X - m._Location.m_X;
			int yDelta = _Location.m_Y - m._Location.m_Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public double GetDistanceToSqrt(IPoint2D p)
		{
			int xDelta = _Location.m_X - p.X;
			int yDelta = _Location.m_Y - p.Y;

			return Math.Sqrt((xDelta * xDelta) + (yDelta * yDelta));
		}

		public virtual void AggressiveAction(Mobile aggressor)
		{
			AggressiveAction(aggressor, false);
		}

		public virtual void AggressiveAction(Mobile aggressor, bool criminal)
		{
			if (aggressor == this)
			{
				return;
			}

			AggressiveActionEventArgs args = AggressiveActionEventArgs.Create(this, aggressor, criminal);

			EventSink.InvokeAggressiveAction(args);

			args.Free();

			if (Combatant == aggressor)
			{
				if (_ExpireCombatant == null)
				{
					_ExpireCombatant = new ExpireCombatantTimer(this);
				}
				else
				{
					_ExpireCombatant.Stop();
				}

				_ExpireCombatant.Start();
			}

			bool addAggressor = true;

			List<AggressorInfo> list = _Aggressors;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (info.Attacker == aggressor)
				{
					info.Refresh();
					info.CriminalAggression = criminal;
					info.CanReportMurder = criminal;

					addAggressor = false;
				}
			}

			list = aggressor._Aggressors;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (info.Attacker == this)
				{
					info.Refresh();

					addAggressor = false;
				}
			}

			bool addAggressed = true;

			list = _Aggressed;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (info.Defender == aggressor)
				{
					info.Refresh();

					addAggressed = false;
				}
			}

			list = aggressor._Aggressed;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (info.Defender == this)
				{
					info.Refresh();
					info.CriminalAggression = criminal;
					info.CanReportMurder = criminal;

					addAggressed = false;
				}
			}

			bool setCombatant = false;

			if (addAggressor)
			{
				_Aggressors.Add(AggressorInfo.Create(aggressor, this, criminal));
				// new AggressorInfo( aggressor, this, criminal, true ) );

				if (CanSee(aggressor) && _NetState != null)
				{
					_NetState.Send(MobileIncoming.Create(_NetState, this, aggressor));
				}

				if (Combatant == null)
				{
					setCombatant = true;
				}

				UpdateAggrExpire();
			}

			if (addAggressed)
			{
				aggressor._Aggressed.Add(AggressorInfo.Create(aggressor, this, criminal));
				// new AggressorInfo( aggressor, this, criminal, false ) );

				if (CanSee(aggressor) && _NetState != null)
				{
					_NetState.Send(MobileIncoming.Create(_NetState, this, aggressor));
				}

				if (Combatant == null)
				{
					setCombatant = true;
				}

				UpdateAggrExpire();
			}

			if (setCombatant)
			{
				Combatant = aggressor;
			}

			Region.OnAggressed(aggressor, this, criminal);
		}

		public void RemoveAggressed(Mobile aggressed)
		{
			if (_Deleted)
			{
				return;
			}

			List<AggressorInfo> list = _Aggressed;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (info.Defender == aggressed)
				{
					_Aggressed.RemoveAt(i);
					info.Free();

					if (_NetState != null && CanSee(aggressed))
					{
						_NetState.Send(MobileIncoming.Create(_NetState, this, aggressed));
					}

					break;
				}
			}

			UpdateAggrExpire();
		}

		public void RemoveAggressor(Mobile aggressor)
		{
			if (_Deleted)
			{
				return;
			}

			List<AggressorInfo> list = _Aggressors;

			for (int i = 0; i < list.Count; ++i)
			{
				AggressorInfo info = list[i];

				if (info.Attacker == aggressor)
				{
					_Aggressors.RemoveAt(i);
					info.Free();

					if (_NetState != null && CanSee(aggressor))
					{
						_NetState.Send(MobileIncoming.Create(_NetState, this, aggressor));
					}

					break;
				}
			}

			UpdateAggrExpire();
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalGold { get { return GetTotal(TotalType.Gold); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalItems { get { return GetTotal(TotalType.Items); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int TotalWeight { get { return GetTotal(TotalType.Weight); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int TithingPoints
		{
			get { return _TithingPoints; }
			set
			{
				if (_TithingPoints != value)
				{
					_TithingPoints = value;

					Delta(MobileDelta.TithingPoints);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int Followers
		{
			get { return _Followers; }
			set
			{
				if (_Followers != value)
				{
					_Followers = value;

					Delta(MobileDelta.Followers);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int FollowersMax
		{
			get { return _FollowersMax; }
			set
			{
				if (_FollowersMax != value)
				{
					_FollowersMax = value;

					Delta(MobileDelta.Followers);
				}
			}
		}

		public virtual int GetTotal(TotalType type)
		{
			switch (type)
			{
				case TotalType.Gold:
					return _TotalGold;

				case TotalType.Items:
					return _TotalItems;

				case TotalType.Weight:
					return _TotalWeight;
			}

			return 0;
		}

		public virtual void UpdateTotal(Item sender, TotalType type, int delta)
		{
			if (delta == 0 || sender.IsVirtualItem)
			{
				return;
			}

			switch (type)
			{
				case TotalType.Gold:
					_TotalGold += delta;
					Delta(MobileDelta.Gold);
					break;

				case TotalType.Items:
					_TotalItems += delta;
					break;

				case TotalType.Weight:
					_TotalWeight += delta;
					Delta(MobileDelta.Weight);
					OnWeightChange(_TotalWeight - delta);
					break;
			}
		}

		public virtual void UpdateTotals()
		{
			if (_Items == null)
			{
				return;
			}

			int oldWeight = _TotalWeight;

			_TotalGold = 0;
			_TotalItems = 0;
			_TotalWeight = 0;

			foreach (Item item in _Items)
			{
				item.UpdateTotals();

				if (item.IsVirtualItem)
				{
					continue;
				}

				_TotalGold += item.TotalGold;
				_TotalItems += item.TotalItems + 1;
				_TotalWeight += item.TotalWeight + item.PileWeight;
			}

			if (_Holding != null)
			{
				_TotalWeight += _Holding.TotalWeight + _Holding.PileWeight;
			}

			if (_TotalWeight != oldWeight)
			{
				OnWeightChange(oldWeight);
			}
		}

		public void ClearQuestArrow()
		{
			_QuestArrow = null;
		}

		public void ClearTarget()
		{
			_Target = null;
		}

		private bool _TargetLocked;

		public bool TargetLocked { get { return _TargetLocked; } set { _TargetLocked = value; } }

		private class SimpleTarget : Target
		{
			private readonly TargetCallback _Callback;

			public SimpleTarget(int range, TargetFlags flags, bool allowGround, TargetCallback callback)
				: base(range, allowGround, flags)
			{
				_Callback = callback;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (_Callback != null)
				{
					_Callback(from, targeted);
				}
			}
		}

		public Target BeginTarget(int range, bool allowGround, TargetFlags flags, TargetCallback callback)
		{
			Target t = new SimpleTarget(range, flags, allowGround, callback);

			Target = t;

			return t;
		}

		private class SimpleStateTarget : Target
		{
			private readonly TargetStateCallback _Callback;
			private readonly object _State;

			public SimpleStateTarget(int range, TargetFlags flags, bool allowGround, TargetStateCallback callback, object state)
				: base(range, allowGround, flags)
			{
				_Callback = callback;
				_State = state;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (_Callback != null)
				{
					_Callback(from, targeted, _State);
				}
			}
		}

		public Target BeginTarget(int range, bool allowGround, TargetFlags flags, TargetStateCallback callback, object state)
		{
			Target t = new SimpleStateTarget(range, flags, allowGround, callback, state);

			Target = t;

			return t;
		}

		private class SimpleStateTarget<T> : Target
		{
			private readonly TargetStateCallback<T> _Callback;
			private readonly T _State;

			public SimpleStateTarget(int range, TargetFlags flags, bool allowGround, TargetStateCallback<T> callback, T state)
				: base(range, allowGround, flags)
			{
				_Callback = callback;
				_State = state;
			}

			protected override void OnTarget(Mobile from, object targeted)
			{
				if (_Callback != null)
				{
					_Callback(from, targeted, _State);
				}
			}
		}

		public Target BeginTarget<T>(int range, bool allowGround, TargetFlags flags, TargetStateCallback<T> callback, T state)
		{
			Target t = new SimpleStateTarget<T>(range, flags, allowGround, callback, state);

			Target = t;

			return t;
		}

		public Target Target
		{
			get { return _Target; }
			set
			{
				Target oldTarget = _Target;
				Target newTarget = value;

				if (oldTarget == newTarget)
				{
					return;
				}

				_Target = null;

				if (oldTarget != null && newTarget != null)
				{
					oldTarget.Cancel(this, TargetCancelType.Overriden);
				}

				_Target = newTarget;

				if (newTarget != null && _NetState != null && !_TargetLocked)
				{
					_NetState.Send(newTarget.GetPacketFor(_NetState));
				}

				OnTargetChange();
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked after the <see cref="Target">Target property</see> has changed.
		/// </summary>
		protected virtual void OnTargetChange()
		{ }

		public ContextMenu ContextMenu
		{
			get { return _ContextMenu; }
			set
			{
				_ContextMenu = value;

				if (_ContextMenu != null && _NetState != null)
				{
					// Old packet is preferred until assistants catch up
					if (_NetState.NewHaven && _ContextMenu.RequiresNewPacket)
					{
						Send(new DisplayContextMenu(_ContextMenu));
					}
					else
					{
						Send(new DisplayContextMenuOld(_ContextMenu));
					}
				}
			}
		}

		public virtual bool CheckContextMenuDisplay(IEntity target)
		{
			return true;
		}

		#region Prompts
		private class SimplePrompt : Prompt
		{
			private readonly PromptCallback _Callback;
			private readonly PromptCallback _CancelCallback;
			private readonly bool _CallbackHandlesCancel;

			public SimplePrompt(PromptCallback callback, PromptCallback cancelCallback)
			{
				_Callback = callback;
				_CancelCallback = cancelCallback;
			}

			public SimplePrompt(PromptCallback callback, bool callbackHandlesCancel)
			{
				_Callback = callback;
				_CallbackHandlesCancel = callbackHandlesCancel;
			}

			public SimplePrompt(PromptCallback callback)
				: this(callback, false)
			{ }

			public override void OnResponse(Mobile from, string text)
			{
				if (_Callback != null)
				{
					_Callback(from, text);
				}
			}

			public override void OnCancel(Mobile from)
			{
				if (_CallbackHandlesCancel && _Callback != null)
				{
					_Callback(from, "");
				}
				else if (_CancelCallback != null)
				{
					_CancelCallback(from, "");
				}
			}
		}

		public Prompt BeginPrompt(PromptCallback callback, PromptCallback cancelCallback)
		{
			Prompt p = new SimplePrompt(callback, cancelCallback);

			Prompt = p;
			return p;
		}

		public Prompt BeginPrompt(PromptCallback callback, bool callbackHandlesCancel)
		{
			Prompt p = new SimplePrompt(callback, callbackHandlesCancel);

			Prompt = p;
			return p;
		}

		public Prompt BeginPrompt(PromptCallback callback)
		{
			return BeginPrompt(callback, false);
		}

		private class SimpleStatePrompt : Prompt
		{
			private readonly PromptStateCallback _Callback;
			private readonly PromptStateCallback _CancelCallback;

			private readonly bool _CallbackHandlesCancel;

			private readonly object _State;

			public SimpleStatePrompt(PromptStateCallback callback, PromptStateCallback cancelCallback, object state)
			{
				_Callback = callback;
				_CancelCallback = cancelCallback;
				_State = state;
			}

			public SimpleStatePrompt(PromptStateCallback callback, bool callbackHandlesCancel, object state)
			{
				_Callback = callback;
				_State = state;
				_CallbackHandlesCancel = callbackHandlesCancel;
			}

			public SimpleStatePrompt(PromptStateCallback callback, object state)
				: this(callback, false, state)
			{ }

			public override void OnResponse(Mobile from, string text)
			{
				if (_Callback != null)
				{
					_Callback(from, text, _State);
				}
			}

			public override void OnCancel(Mobile from)
			{
				if (_CallbackHandlesCancel && _Callback != null)
				{
					_Callback(from, "", _State);
				}
				else if (_CancelCallback != null)
				{
					_CancelCallback(from, "", _State);
				}
			}
		}

		public Prompt BeginPrompt(PromptStateCallback callback, PromptStateCallback cancelCallback, object state)
		{
			Prompt p = new SimpleStatePrompt(callback, cancelCallback, state);

			Prompt = p;
			return p;
		}

		public Prompt BeginPrompt(PromptStateCallback callback, bool callbackHandlesCancel, object state)
		{
			Prompt p = new SimpleStatePrompt(callback, callbackHandlesCancel, state);

			Prompt = p;
			return p;
		}

		public Prompt BeginPrompt(PromptStateCallback callback, object state)
		{
			return BeginPrompt(callback, false, state);
		}

		private class SimpleStatePrompt<T> : Prompt
		{
			private readonly PromptStateCallback<T> _Callback;
			private readonly PromptStateCallback<T> _CancelCallback;

			private readonly bool _CallbackHandlesCancel;

			private readonly T _State;

			public SimpleStatePrompt(PromptStateCallback<T> callback, PromptStateCallback<T> cancelCallback, T state)
			{
				_Callback = callback;
				_CancelCallback = cancelCallback;
				_State = state;
			}

			public SimpleStatePrompt(PromptStateCallback<T> callback, bool callbackHandlesCancel, T state)
			{
				_Callback = callback;
				_State = state;
				_CallbackHandlesCancel = callbackHandlesCancel;
			}

			public SimpleStatePrompt(PromptStateCallback<T> callback, T state)
				: this(callback, false, state)
			{ }

			public override void OnResponse(Mobile from, string text)
			{
				if (_Callback != null)
				{
					_Callback(from, text, _State);
				}
			}

			public override void OnCancel(Mobile from)
			{
				if (_CallbackHandlesCancel && _Callback != null)
				{
					_Callback(from, "", _State);
				}
				else if (_CancelCallback != null)
				{
					_CancelCallback(from, "", _State);
				}
			}
		}

		public Prompt BeginPrompt<T>(PromptStateCallback<T> callback, PromptStateCallback<T> cancelCallback, T state)
		{
			Prompt p = new SimpleStatePrompt<T>(callback, cancelCallback, state);

			Prompt = p;
			return p;
		}

		public Prompt BeginPrompt<T>(PromptStateCallback<T> callback, bool callbackHandlesCancel, T state)
		{
			Prompt p = new SimpleStatePrompt<T>(callback, callbackHandlesCancel, state);

			Prompt = p;
			return p;
		}

		public Prompt BeginPrompt<T>(PromptStateCallback<T> callback, T state)
		{
			return BeginPrompt(callback, false, state);
		}

        public Prompt Prompt
        {
            get { return _Prompt; }
            set
            {
                Prompt oldPrompt = _Prompt;
                Prompt newPrompt = value;

                if (oldPrompt == newPrompt)
                {
                    return;
                }

                _Prompt = null;

                if (oldPrompt != null && newPrompt != null)
                {
                    oldPrompt.OnCancel(this);
                }

                _Prompt = newPrompt;

                if (newPrompt != null)
                {
                    newPrompt.SendTo(this);
                    //Send(new UnicodePrompt(newPrompt));
                }
            }
        }
		#endregion

		private bool InternalOnMove(Direction d)
		{
			if (!OnMove(d))
			{
				return false;
			}

			MovementEventArgs e = MovementEventArgs.Create(this, d);

			EventSink.InvokeMovement(e);

			bool ret = !e.Blocked;

			e.Free();

			return ret;
		}

		/// <summary>
		///     Overridable. Event invoked before the Mobile <see cref="Move">moves</see>.
		/// </summary>
		/// <returns>True if the move is allowed, false if not.</returns>
		protected virtual bool OnMove(Direction d)
		{
			if (_Hidden && _AccessLevel == AccessLevel.Player)
			{
				if (_AllowedStealthSteps-- <= 0 || (d & Direction.Running) != 0 || Mounted)
				{
					RevealingAction();
				}
			}

			return true;
		}

		private static readonly Packet[][] _MovingPacketCache = new Packet[][] {new Packet[8], new Packet[8]};

		private bool _Pushing;
		private bool _IgnoreMobiles;
		private bool _IsStealthing;

		public bool Pushing { get { return _Pushing; } set { _Pushing = value; } }

		private static int _WalkFoot = 400;
		private static int _RunFoot = 200;
		private static int _WalkMount = 200;
		private static int _RunMount = 100;

		public static int WalkFoot { get { return _WalkFoot; } set { _WalkFoot = value; } }
		public static int RunFoot { get { return _RunFoot; } set { _RunFoot = value; } }
		public static int WalkMount { get { return _WalkMount; } set { _WalkMount = value; } }
		public static int RunMount { get { return _RunMount; } set { _RunMount = value; } }

		private long _EndQueue;

		private static readonly List<IEntity> _MoveList = new List<IEntity>();
		private static readonly List<Mobile> _MoveClientList = new List<Mobile>();

		private static AccessLevel _FwdAccessOverride = AccessLevel.Counselor;
		private static bool _FwdEnabled = true;
		private static bool _FwdUotdOverride;
		private static int _FwdMaxSteps = 4;

		public static AccessLevel FwdAccessOverride { get { return _FwdAccessOverride; } set { _FwdAccessOverride = value; } }
		public static bool FwdEnabled { get { return _FwdEnabled; } set { _FwdEnabled = value; } }
		public static bool FwdUotdOverride { get { return _FwdUotdOverride; } set { _FwdUotdOverride = value; } }
		public static int FwdMaxSteps { get { return _FwdMaxSteps; } set { _FwdMaxSteps = value; } }

		public virtual void ClearFastwalkStack()
		{
			if (_MoveRecords != null && _MoveRecords.Count > 0)
			{
				_MoveRecords.Clear();
			}

			_EndQueue = Core.TickCount;
		}

		public virtual bool CheckMovement(Direction d, out int newZ)
		{
			return Movement.Movement.CheckMovement(this, d, out newZ);
		}

		public virtual bool Move(Direction d)
		{
			if (_Deleted)
			{
				return false;
			}

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
			{
				box.Close();
			}

			Point3D newLocation = _Location;
			Point3D oldLocation = newLocation;

			if ((_Direction & Direction.Mask) == (d & Direction.Mask))
			{
				// We are actually moving (not just a direction change)
				if (_Spell != null && !_Spell.OnCasterMoving(d))
				{
					return false;
				}

				if (_Paralyzed || _Frozen || _Sleep)
				{
					SendLocalizedMessage(500111); // You are frozen and can not move.

					return false;
				}

				int newZ;

				if (CheckMovement(d, out newZ))
				{
					int x = oldLocation.m_X, y = oldLocation.m_Y;
					int oldX = x, oldY = y;
					int oldZ = oldLocation.m_Z;

					switch (d & Direction.Mask)
					{
						case Direction.North:
							--y;
							break;
						case Direction.Right:
							++x;
							--y;
							break;
						case Direction.East:
							++x;
							break;
						case Direction.Down:
							++x;
							++y;
							break;
						case Direction.South:
							++y;
							break;
						case Direction.Left:
							--x;
							++y;
							break;
						case Direction.West:
							--x;
							break;
						case Direction.Up:
							--x;
							--y;
							break;
					}

					newLocation.m_X = x;
					newLocation.m_Y = y;
					newLocation.m_Z = newZ;

					_Pushing = false;

					Map map = _Map;

					if (map != null)
					{
						Sector oldSector = map.GetSector(oldX, oldY);
						Sector newSector = map.GetSector(x, y);

						if (oldSector != newSector)
						{
							for (int i = 0; i < oldSector.Mobiles.Count; ++i)
							{
								Mobile m = oldSector.Mobiles[i];

								if (m != this && m.X == oldX && m.Y == oldY && (m.Z + 15) > oldZ && (oldZ + 15) > m.Z && !m.OnMoveOff(this))
								{
									return false;
								}
							}

							for (int i = 0; i < oldSector.Items.Count; ++i)
							{
								Item item = oldSector.Items[i];

								if (item.AtWorldPoint(oldX, oldY) &&
									(item.Z == oldZ || ((item.Z + item.ItemData.Height) > oldZ && (oldZ + 15) > item.Z)) && !item.OnMoveOff(this))
								{
									return false;
								}
							}

							for (int i = 0; i < newSector.Mobiles.Count; ++i)
							{
								Mobile m = newSector.Mobiles[i];

								if (m.X == x && m.Y == y && (m.Z + 15) > newZ && (newZ + 15) > m.Z && !m.OnMoveOver(this))
								{
									return false;
								}
							}

							for (int i = 0; i < newSector.Items.Count; ++i)
							{
								Item item = newSector.Items[i];

								if (item.AtWorldPoint(x, y) &&
									(item.Z == newZ || ((item.Z + item.ItemData.Height) > newZ && (newZ + 15) > item.Z)) && !item.OnMoveOver(this))
								{
									return false;
								}
							}
						}
						else
						{
							for (int i = 0; i < oldSector.Mobiles.Count; ++i)
							{
								Mobile m = oldSector.Mobiles[i];

								if (m != this && m.X == oldX && m.Y == oldY && (m.Z + 15) > oldZ && (oldZ + 15) > m.Z && !m.OnMoveOff(this))
								{
									return false;
								}
							    if (m.X == x && m.Y == y && (m.Z + 15) > newZ && (newZ + 15) > m.Z && !m.OnMoveOver(this))
								{
									return false;
								}
							}

							for (int i = 0; i < oldSector.Items.Count; ++i)
							{
								Item item = oldSector.Items[i];

								if (item.AtWorldPoint(oldX, oldY) &&
									(item.Z == oldZ || ((item.Z + item.ItemData.Height) > oldZ && (oldZ + 15) > item.Z)) && !item.OnMoveOff(this))
								{
									return false;
								}
							    if (item.AtWorldPoint(x, y) &&
										 (item.Z == newZ || ((item.Z + item.ItemData.Height) > newZ && (newZ + 15) > item.Z)) && !item.OnMoveOver(this))
								{
									return false;
								}
							}
						}

						if (!Region.CanMove(this, d, newLocation, oldLocation, _Map))
						{
							return false;
						}
					}
					else
					{
						return false;
					}

					if (!InternalOnMove(d))
					{
						return false;
					}

					if (_FwdEnabled && _NetState != null && _AccessLevel < _FwdAccessOverride &&
						(!_FwdUotdOverride || !_NetState.IsUOTDClient))
					{
						if (_MoveRecords == null)
						{
							_MoveRecords = new Queue<MovementRecord>(6);
						}

						while (_MoveRecords.Count > 0)
						{
							MovementRecord r = _MoveRecords.Peek();

							if (r.Expired())
							{
								_MoveRecords.Dequeue();
							}
							else
							{
								break;
							}
						}

						if (_MoveRecords.Count >= _FwdMaxSteps)
						{
							var fw = new FastWalkEventArgs(_NetState);
							EventSink.InvokeFastWalk(fw);

							if (fw.Blocked)
							{
								return false;
							}
						}

						int delay = ComputeMovementSpeed(d);

						long end;

						if (_MoveRecords.Count > 0)
						{
							end = _EndQueue + delay;
						}
						else
						{
							end = Core.TickCount + delay;
						}

						_MoveRecords.Enqueue(MovementRecord.NewInstance(end));

						_EndQueue = end;
					}

					_LastMoveTime = Core.TickCount;
				}
				else
				{
					return false;
				}

				DisruptiveAction();
			}

			if (_NetState != null)
			{
				_NetState.Send(MovementAck.Instantiate(_NetState.Sequence, this));
			}

			SetDirection(d);
			SetLocation(newLocation, false);

			if (_Map != null)
			{
				IPooledEnumerable<IEntity> eable = _Map.GetObjectsInRange(_Location, Core.GlobalMaxUpdateRange);

				foreach (IEntity o in eable)
				{
					if (o == this)
					{
						continue;
					}

					if (o is Mobile)
					{
						var mob = o as Mobile;
						if (mob.NetState != null)
						{
							_MoveClientList.Add(mob);
						}
						_MoveList.Add(o);
					}
					else if (o is Item)
					{
						var item = (Item)o;

						if (item.HandlesOnMovement)
						{
							_MoveList.Add(item);
						}
					}
				}

				eable.Free();

				Packet[][] cache = _MovingPacketCache;

				foreach (Mobile m in _MoveClientList)
				{
					NetState ns = m.NetState;

					if (ns != null && Utility.InUpdateRange(_Location, m._Location) && m.CanSee(this))
					{
						if (ns.StygianAbyss)
						{
							Packet p;
							int noto = Notoriety.Compute(m, this);
							p = cache[0][noto];

							if (p == null)
							{
								cache[0][noto] = p = Packet.Acquire(new MobileMoving(this, noto));
							}

							ns.Send(p);
						}
						else
						{
							Packet p;
							int noto = Notoriety.Compute(m, this);
							p = cache[1][noto];

							if (p == null)
							{
								cache[1][noto] = p = Packet.Acquire(new MobileMovingOld(this, noto));
							}

							ns.Send(p);
						}
					}
				}

				for (int i = 0; i < cache.Length; ++i)
				{
					for (int j = 0; j < cache[i].Length; ++j)
					{
						Packet.Release(ref cache[i][j]);
					}
				}

				for (int i = 0; i < _MoveList.Count; ++i)
				{
					IEntity o = _MoveList[i];

					if (o is Mobile)
					{
						((Mobile)o).OnMovement(this, oldLocation);
					}
					else if (o is Item)
					{
						((Item)o).OnMovement(this, oldLocation);
					}
				}

				if (_MoveList.Count > 0)
				{
					_MoveList.Clear();
				}

				if (_MoveClientList.Count > 0)
				{
					_MoveClientList.Clear();
				}
			}

			OnAfterMove(oldLocation);
			return true;
		}

		public virtual void OnAfterMove(Point3D oldLocation)
		{ }

		public int ComputeMovementSpeed()
		{
			return ComputeMovementSpeed(Direction, false);
		}

		public int ComputeMovementSpeed(Direction dir)
		{
			return ComputeMovementSpeed(dir, true);
		}

		public virtual int ComputeMovementSpeed(Direction dir, bool checkTurning)
		{
			int delay;

			if (Mounted)
			{
				delay = (dir & Direction.Running) != 0 ? _RunMount : _WalkMount;
			}
			else
			{
				delay = (dir & Direction.Running) != 0 ? _RunFoot : _WalkFoot;
			}

			return delay;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when a Mobile <paramref name="m" /> moves off this Mobile.
		/// </summary>
		/// <returns>True if the move is allowed, false if not.</returns>
		public virtual bool OnMoveOff(Mobile m)
		{
			return true;
		}

		public virtual bool IsDeadBondedPet { get { return false; } }

		/// <summary>
		///     Overridable. Event invoked when a Mobile <paramref name="m" /> moves over this Mobile.
		/// </summary>
		/// <returns>True if the move is allowed, false if not.</returns>
		public virtual bool OnMoveOver(Mobile m)
		{
			if (_Map == null || _Deleted)
			{
				return true;
			}

			return m.CheckShove(this);
		}

		public virtual bool CheckShove(Mobile shoved)
		{
			if (!_IgnoreMobiles && (_Map.Rules & MapRules.FreeMovement) == 0)
			{
				if (!shoved.Alive || !Alive || shoved.IsDeadBondedPet || IsDeadBondedPet)
				{
					return true;
				}
			    if (shoved._Hidden && shoved.IsStaff())
				{
					return true;
				}

			    if (!_Pushing)
				{
					_Pushing = true;

					int number;

					if (IsStaff())
					{
						number = shoved._Hidden ? 1019041 : 1019040;
					}
					else
					{
						if (Stam == StamMax)
						{
							number = shoved._Hidden ? 1019043 : 1019042;
							Stam -= 10;

							RevealingAction();
						}
						else
						{
							return false;
						}
					}

					SendLocalizedMessage(number);
				}
			}

			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile sees another Mobile, <paramref name="m" />, move.
		/// </summary>
		public virtual void OnMovement(Mobile m, Point3D oldLocation)
		{ }

		public ISpell Spell
		{
			get { return _Spell; }
			set
			{
				//if( _Spell != null && value != null )
				//	Console.WriteLine("Warning: Spell has been overwritten");

				_Spell = value;
			}
		}

		[CommandProperty(AccessLevel.Administrator)]
		public bool AutoPageNotify { get { return _AutoPageNotify; } set { _AutoPageNotify = value; } }

		public virtual void CriminalAction(bool message)
		{
			if (_Deleted)
			{
				return;
			}

			Criminal = true;

			Region.OnCriminalAction(this, message);
		}

		public virtual bool CanUseStuckMenu()
		{
			if (_StuckMenuUses == null)
			{
				return true;
			}
		    for (int i = 0; i < _StuckMenuUses.Length; ++i)
			{
		        if ((DateTime.UtcNow - _StuckMenuUses[i]) > TimeSpan.FromDays(1.0))
				{
						return true;
					}
				}

				return false;
			}

		public virtual bool IsPlayer()
		{
			return Utilities.IsPlayer(this);
		}

		public virtual bool IsStaff()
		{
			return Utilities.IsStaff(this);
		}

		public virtual bool IsOwner()
		{
			return Utilities.IsOwner(this);
		}

		public virtual bool IsSnoop(Mobile from)
		{
			return (from != this);
		}

		/// <summary>
		///     Overridable. Any call to <see cref="Resurrect" /> will silently fail if this method returns false.
		///     <seealso cref="Resurrect" />
		/// </summary>
		public virtual bool CheckResurrect()
		{
			return true;
		}

		/// <summary>
		///     Overridable. Event invoked before the Mobile is <see cref="Resurrect">resurrected</see>.
		///     <seealso cref="Resurrect" />
		/// </summary>
		public virtual void OnBeforeResurrect()
		{ }

		/// <summary>
		///     Overridable. Event invoked after the Mobile is <see cref="Resurrect">resurrected</see>.
		///     <seealso cref="Resurrect" />
		/// </summary>
		public virtual void OnAfterResurrect()
		{ }

		public virtual void Resurrect()
		{
			if (!Alive)
			{
				if (!Region.OnResurrect(this))
				{
					return;
				}

				if (!CheckResurrect())
				{
					return;
				}

				OnBeforeResurrect();

				BankBox box = FindBankNoCreate();

				if (box != null && box.Opened)
				{
					box.Close();
				}

				Poison = null;

				Warmode = false;

				Hits = 10;
				Stam = StamMax;
				Mana = 0;

				BodyMod = 0;
				Body = Race.AliveBody(this);

				ProcessDeltaQueue();

				for (int i = _Items.Count - 1; i >= 0; --i)
				{
					if (i >= _Items.Count)
					{
						continue;
					}

					Item item = _Items[i];

					if (item.ItemID == 0x204E)
					{
						item.Delete();
					}
				}

				SendIncomingPacket();
				SendIncomingPacket();

				OnAfterResurrect();

				//Send( new DeathStatus( false ) );
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Owner)]
		public IAccount Account { get; set; }

		private bool _Deleted;

		public bool Deleted { get { return _Deleted; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int VirtualArmor
		{
			get { return _VirtualArmor; }
			set
			{
				if (_VirtualArmor != value)
				{
					_VirtualArmor = value;

					Delta(MobileDelta.Armor);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual double ArmorRating { get { return 0.0; } }

		public void DropHolding()
		{
			Item holding = _Holding;

			if (holding != null)
			{
				if (!holding.Deleted && holding.HeldBy == this && holding.Map == Map.Internal)
				{
					AddToBackpack(holding);
				}

				Holding = null;
				holding.ClearBounce();
			}
		}

		public virtual void Delete()
		{
			if (_Deleted)
			{
				return;
			}
		    if (!World.OnDelete(this))
			{
				return;
			}

		    if (_NetState != null)
			{
				_NetState.CancelAllTrades();
			}

			if (_NetState != null)
			{
				_NetState.Dispose();
			}

			DropHolding();

			Region.OnRegionChange(this, _Region, null);

			_Region = null;
			//Is the above line REALLY needed?  The old Region system did NOT have said line
			//and worked fine, because of this a LOT of extra checks have to be done everywhere...
			//I guess this should be there for Garbage collection purposes, but, still, is it /really/ needed?

			OnDelete();

			for (int i = _Items.Count - 1; i >= 0; --i)
			{
				if (i < _Items.Count)
				{
					_Items[i].OnParentDeleted(this);
				}
			}

			for (int i = 0; i < _Stabled.Count; i++)
			{
				_Stabled[i].Delete();
			}

			SendRemovePacket();

			if (_Guild != null)
			{
				_Guild.OnDelete(this);
			}

			_Deleted = true;

			if (_Map != null)
			{
				_Map.OnLeave(this);
				_Map = null;
			}

			_Hair = null;
			_FacialHair = null;
			_MountItem = null;

            #region Enhance Client
            _Face = null;
            #endregion

			World.RemoveMobile(this);

			OnAfterDelete();

			FreeCache();
		}

		/// <summary>
		///     Overridable. Virtual event invoked before the Mobile is deleted.
		/// </summary>
		public virtual void OnDelete()
		{
			if (_Spawner != null)
			{
				_Spawner.Remove(this);
				_Spawner = null;
			}
		}

		/// <summary>
		///     Overridable. Returns true if the player is alive, false if otherwise. By default, this is computed by: <c>!Deleted &amp;&amp; (!Player || !Body.IsGhost)</c>
		/// </summary>
		[CommandProperty(AccessLevel.Counselor)]
		public virtual bool Alive { get { return !_Deleted && (!_Player || !_Body.IsGhost); } }

		public virtual bool CheckSpellCast(ISpell spell)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile casts a <paramref name="spell" />.
		/// </summary>
		/// <param name="spell"></param>
		public virtual void OnSpellCast(ISpell spell)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked after <see cref="TotalWeight" /> changes.
		/// </summary>
		public virtual void OnWeightChange(int oldValue)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when the <see cref="Skill.Base" /> or <see cref="Skill.BaseFixedPoint" /> property of
		///     <paramref
		///         name="skill" />
		///     changes.
		/// </summary>
		public virtual void OnSkillChange(SkillName skill, double oldBase)
		{ }

		/// <summary>
		///     Overridable. Invoked after the mobile is deleted. When overridden, be sure to call the base method.
		/// </summary>
		public virtual void OnAfterDelete()
		{
			StopAggrExpire();

			CheckAggrExpire();

			if (_PoisonTimer != null)
			{
				_PoisonTimer.Stop();
			}

			if (_HitsTimer != null)
			{
				_HitsTimer.Stop();
			}

			if (_StamTimer != null)
			{
				_StamTimer.Stop();
			}

			if (_ManaTimer != null)
			{
				_ManaTimer.Stop();
			}

			if (_CombatTimer != null)
			{
				_CombatTimer.Stop();
			}

			if (_ExpireCombatant != null)
			{
				_ExpireCombatant.Stop();
			}

			if (_LogoutTimer != null)
			{
				_LogoutTimer.Stop();
			}

			if (_ExpireCriminal != null)
			{
				_ExpireCriminal.Stop();
			}

			if (_WarmodeTimer != null)
			{
				_WarmodeTimer.Stop();
			}

			if (_ParaTimer != null)
			{
				_ParaTimer.Stop();
			}

			if (_SleepTimer != null)
			{
				_SleepTimer.Stop();
			}

			if (_FrozenTimer != null)
			{
				_FrozenTimer.Stop();
			}

			if (_AutoManifestTimer != null)
			{
				_AutoManifestTimer.Stop();
			}

			foreach (BaseModule module in World.GetModules(this))
			{
				if (module != null)
				{
					module.Delete();
				}
			}
		}

		public virtual bool AllowSkillUse(SkillName name)
		{
			return true;
		}

		public virtual bool UseSkill(SkillName name)
		{
			return Skills.UseSkill(this, name);
		}

		public virtual bool UseSkill(int skillID)
		{
			return Skills.UseSkill(this, skillID);
		}

		private static CreateCorpseHandler _CreateCorpse;

		public static CreateCorpseHandler CreateCorpseHandler { get { return _CreateCorpse; } set { _CreateCorpse = value; } }

		public virtual DeathMoveResult GetParentMoveResultFor(Item item)
		{
			return item.OnParentDeath(this);
		}

		public virtual DeathMoveResult GetInventoryMoveResultFor(Item item)
		{
			return item.OnInventoryDeath(this);
		}

		public virtual bool RetainPackLocsOnDeath { get { return Core.AOS; } }

		public virtual void Kill()
		{
			if (!CanBeDamaged())
			{
				return;
			}
		    if (!Alive || IsDeadBondedPet)
			{
				return;
			}
		    if (_Deleted)
			{
				return;
			}
		    if (!Region.OnBeforeDeath(this))
			{
				return;
			}
		    if (!OnBeforeDeath())
			{
				return;
			}

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
			{
				box.Close();
			}

			if (_NetState != null)
			{
				_NetState.CancelAllTrades();
			}

			if (_Spell != null)
			{
				_Spell.OnCasterKilled();
			}
			//_Spell.Disturb( DisturbType.Kill );

			if (_Target != null)
			{
				_Target.Cancel(this, TargetCancelType.Canceled);
			}

			DisruptiveAction();

			Warmode = false;

			DropHolding();

			Hits = 0;
			Stam = 0;
			Mana = 0;

			Poison = null;
			Combatant = null;

			if (Paralyzed)
			{
				Paralyzed = false;

				if (_ParaTimer != null)
				{
					_ParaTimer.Stop();
				}
			}

			if (Frozen)
			{
				Frozen = false;

				if (_FrozenTimer != null)
				{
					_FrozenTimer.Stop();
				}
			}

			if (Asleep)
			{
				Asleep = false;
				Send(SpeedControl.Disable);

				if (_SleepTimer != null)
				{
					_SleepTimer.Stop();
				}
			}

			var content = new List<Item>();
			var equip = new List<Item>();
			var moveToPack = new List<Item>();

			var itemsCopy = new List<Item>(_Items);

			Container pack = Backpack;

			for (int i = 0; i < itemsCopy.Count; ++i)
			{
				Item item = itemsCopy[i];

				if (item == pack)
				{
					continue;
				}

				if ((item.Insured || item.LootType == LootType.Blessed) && item.Parent == this && item.Layer != Layer.Mount)
				{
					equip.Add(item);
				}

				DeathMoveResult res = GetParentMoveResultFor(item);

				switch (res)
				{
					case DeathMoveResult.MoveToCorpse:
						{
							content.Add(item);
							equip.Add(item);
							break;
						}
					case DeathMoveResult.MoveToBackpack:
						{
							moveToPack.Add(item);
							break;
						}
				}
			}

			if (pack != null)
			{
				var packCopy = new List<Item>(pack.Items);

				for (int i = 0; i < packCopy.Count; ++i)
				{
					Item item = packCopy[i];

					DeathMoveResult res = GetInventoryMoveResultFor(item);

					if (res == DeathMoveResult.MoveToCorpse)
					{
						content.Add(item);
					}
					else
					{
						moveToPack.Add(item);
					}
				}

				for (int i = 0; i < moveToPack.Count; ++i)
				{
					Item item = moveToPack[i];

					if (RetainPackLocsOnDeath && item.Parent == pack)
					{
						continue;
					}

					pack.DropItem(item);
				}
			}

			HairInfo hair = null;
			if (_Hair != null)
			{
				hair = new HairInfo(_Hair.ItemID, _Hair.Hue);
			}

			FacialHairInfo facialhair = null;
			if (_FacialHair != null)
			{
				facialhair = new FacialHairInfo(_FacialHair.ItemID, _FacialHair.Hue);
			}

			Container c = (_CreateCorpse == null ? null : _CreateCorpse(this, hair, facialhair, content, equip));

			/*_Corpse = c;

			for ( int i = 0; c != null && i < content.Count; ++i )
			c.DropItem( (Item)content[i] );

			if ( c != null )
			c.MoveToWorld( this.Location, this.Map );*/

			if (_Map != null)
			{
				Packet animPacket = null;

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state != _NetState)
					{
						if (animPacket == null)
						{
							animPacket = Packet.Acquire(new DeathAnimation(this, c));
						}
						;

						state.Send(animPacket);

						if (!state.Mobile.CanSee(this))
						{
							state.Send(RemovePacket);
						}
					}
				}

				Packet.Release(animPacket);

				eable.Free();
			}

			Region.OnDeath(this);
			OnDeath(c);
		}

		private Container _Corpse;

		[CommandProperty(AccessLevel.GameMaster)]
		public Container Corpse { get { return _Corpse; } set { _Corpse = value; } }

		/// <summary>
		///     Overridable. Event invoked before the Mobile is <see cref="Kill">killed</see>.
		///     <seealso cref="Kill" />
		///     <seealso cref="OnDeath" />
		/// </summary>
		/// <returns>True to continue with death, false to override it.</returns>
		public virtual bool OnBeforeDeath()
		{
			return true;
		}

		/// <summary>
		///     Overridable. Event invoked after the Mobile is <see cref="Kill">killed</see>. Primarily, this method is responsible for deleting an NPC or turning a PC into a ghost.
		///     <seealso cref="Kill" />
		///     <seealso cref="OnBeforeDeath" />
		/// </summary>
		public virtual void OnDeath(Container c)
		{
			int sound = GetDeathSound();

			if (sound >= 0)
			{
				Effects.PlaySound(this, Map, sound);
			}

			if (!_Player)
			{
				Delete();
			}
			else
			{
				Send(DeathStatus.Instantiate(true));

				Warmode = false;

				BodyMod = 0;
				//Body = this.Female ? 0x193 : 0x192;
				Body = Race.GhostBody(this);

				var deathShroud = new Item(0x204E);

				deathShroud.Movable = false;
				deathShroud.Layer = Layer.OuterTorso;

				AddItem(deathShroud);

				_Items.Remove(deathShroud);
				_Items.Insert(0, deathShroud);

				Poison = null;
				Combatant = null;

				Hits = 0;
				Stam = 0;
				Mana = 0;

				EventSink.InvokePlayerDeath(new PlayerDeathEventArgs(this));

				ProcessDeltaQueue();

				Send(DeathStatus.Instantiate(false));

				CheckStatTimers();
			}
		}

		#region Get*Sound
		public virtual int GetAngerSound()
		{
			if (_BaseSoundId != 0)
			{
				return _BaseSoundId;
			}

			return -1;
		}

		public virtual int GetIdleSound()
		{
			if (_BaseSoundId != 0)
			{
				return _BaseSoundId + 1;
			}

			return -1;
		}

		public virtual int GetAttackSound()
		{
			if (_BaseSoundId != 0)
			{
				return _BaseSoundId + 2;
			}

			return -1;
		}

		public virtual int GetHurtSound()
		{
			if (_BaseSoundId != 0)
			{
				return _BaseSoundId + 3;
			}

			return -1;
		}

		public virtual int GetDeathSound()
		{
		    if (_BaseSoundId != 0)
			{
				return _BaseSoundId + 4;
			}
		    if (_Body.IsHuman)
			{
		        return Utility.Random(_Female ? 0x314 : 0x423, _Female ? 4 : 5);
			}
				return -1;
			}
		#endregion

		private static char[] _GhostChars = {'o', 'O'};

		public static char[] GhostChars { get { return _GhostChars; } set { _GhostChars = value; } }

		private static bool _NoSpeechLOS;

		public static bool NoSpeechLOS { get { return _NoSpeechLOS; } set { _NoSpeechLOS = value; } }

		private static TimeSpan _AutoManifestTimeout = TimeSpan.FromSeconds(5.0);

		public static TimeSpan AutoManifestTimeout { get { return _AutoManifestTimeout; } set { _AutoManifestTimeout = value; } }

		private Timer _AutoManifestTimer;

		private class AutoManifestTimer : Timer
		{
			private readonly Mobile _Mobile;

			public AutoManifestTimer(Mobile m, TimeSpan delay)
				: base(delay)
			{
				_Mobile = m;
			}

			protected override void OnTick()
			{
				if (!_Mobile.Alive)
				{
					_Mobile.Warmode = false;
				}
			}
		}

		public virtual bool CheckTarget(Mobile from, Target targ, object targeted)
		{
			return true;
		}

		public static bool InsuranceEnabled { get; set; }

		public virtual void Use(Item item)
		{
			if (item == null || item.Deleted || item.QuestItem || Deleted)
			{
				return;
			}

			DisruptiveAction();

			if (_Spell != null && !_Spell.OnCasterUsingObject(item))
			{
				return;
			}

			object root = item.RootParent;
			bool okay = false;

			if (!Utility.InUpdateRange(this, item.GetWorldLocation()))
			{
				item.OnDoubleClickOutOfRange(this);
			}
			else if (!CanSee(item))
			{
				item.OnDoubleClickCantSee(this);
			}
			else if (!item.IsAccessibleTo(this))
			{
				Region reg = Region.Find(item.GetWorldLocation(), item.Map);

				if (reg == null || !reg.SendInaccessibleMessage(item, this))
				{
					item.OnDoubleClickNotAccessible(this);
				}
			}
			else if (!CheckAlive(false))
			{
				item.OnDoubleClickDead(this);
			}
			else if (item.InSecureTrade)
			{
				item.OnDoubleClickSecureTrade(this);
			}
			else if (!AllowItemUse(item))
			{
				okay = false;
			}
			else if (!item.CheckItemUse(this, item))
			{
				okay = false;
			}
			else if (root != null && root is Mobile && ((Mobile)root).IsSnoop(this))
			{
				item.OnSnoop(this);
			}
			else if (Region.OnDoubleClick(this, item))
			{
				okay = true;
			}

			if (okay)
			{
				if (!item.Deleted)
				{
					item.OnItemUsed(this, item);
				}

				if (!item.Deleted)
				{
					item.OnDoubleClick(this);
				}
			}
		}

		public virtual void Use(Mobile m)
		{
			if (m == null || m.Deleted || Deleted)
			{
				return;
			}

			DisruptiveAction();

			if (_Spell != null && !_Spell.OnCasterUsingObject(m))
			{
				return;
			}

			if (!Utility.InUpdateRange(this, m))
			{
				m.OnDoubleClickOutOfRange(this);
			}
			else if (!CanSee(m))
			{
				m.OnDoubleClickCantSee(this);
			}
			else if (!CheckAlive(false))
			{
				m.OnDoubleClickDead(this);
			}
			else if (Region.OnDoubleClick(this, m) && !m.Deleted)
			{
				m.OnDoubleClick(this);
			}
		}

		private static int _ActionDelay = 500;

		public static int ActionDelay { get { return _ActionDelay; } set { _ActionDelay = value; } }

		public virtual void Lift(Item item, int amount, out bool rejected, out LRReason reject)
		{
			rejected = true;
			reject = LRReason.Inspecific;

			if (item == null)
			{
				return;
			}

			Mobile from = this;
			NetState state = _NetState;

			if (from.IsStaff() || Core.TickCount - from.NextActionTime >= 0)
			{
				if (from.CheckAlive())
				{
					from.DisruptiveAction();

					if (from.Holding != null)
					{
						reject = LRReason.AreHolding;
					}
					else if (from.AccessLevel < AccessLevel.GameMaster && !from.InRange(item.GetWorldLocation(), 2))
					{
						reject = LRReason.OutOfRange;
					}
					else if (!from.CanSee(item) || !from.InLOS(item))
					{
						reject = LRReason.OutOfSight;
					}
					else if (!item.VerifyMove(from))
					{
						reject = LRReason.CannotLift;
					}
					#region Mondain's Legacy
					else if (item.QuestItem && amount != item.Amount && !from.IsStaff())
					{
						reject = LRReason.Inspecific;
						from.SendLocalizedMessage(1074868); // Stacks of quest items cannot be unstacked.
					}
					#endregion
					else if (!item.IsAccessibleTo(from))
					{
						reject = LRReason.CannotLift;
					}
					else if (item.Nontransferable && amount != item.Amount)
					{
						if (item.QuestItem)
						{
							from.SendLocalizedMessage(1074868); // Stacks of quest items cannot be unstacked.
						}

						reject = LRReason.CannotLift;
					}
					else if (!item.CheckLift(from, item, ref reject))
					{ }
					else
					{
						object root = item.RootParent;

						if (root != null && root is Mobile && !((Mobile)root).CheckNonlocalLift(from, item))
						{
							reject = LRReason.TryToSteal;
						}
						else if (!from.OnDragLift(item) || !item.OnDragLift(from))
						{
							reject = LRReason.Inspecific;
						}
						else if (!from.CheckAlive())
						{
							reject = LRReason.Inspecific;
						}
						else
                        {
                            #region Enhance Client
                            if (item.Parent != null && item.Parent is Container)
                                ((Container)item.Parent).FreePosition(item.GridLocation);
                            #endregion

                            item.SetLastMoved();

							if (item.Spawner != null)
							{
								item.Spawner.Remove(item);
								item.Spawner = null;
							}

							if (amount == 0)
							{
								amount = 1;
							}

							if (amount > item.Amount)
							{
								amount = item.Amount;
							}

							int oldAmount = item.Amount;
							//item.Amount = amount; //Set in LiftItemDupe

							if (amount < oldAmount)
							{
								LiftItemDupe(item, amount);
							}
							//item.Dupe( oldAmount - amount );

							Map map = from.Map;

							if (_DragEffects && map != null && (root == null || root is Item))
							{
								IPooledEnumerable<NetState> eable = map.GetClientsInRange(from.Location);
								Packet p = null;

								foreach (NetState ns in eable)
								{
									if (ns.Mobile != from && ns.Mobile.CanSee(from) && ns.Mobile.InLOS(from) && ns.Mobile.CanSee(root))
									{
										if (p == null)
										{
											IEntity src;

											if (root == null)
											{
												src = new Entity(Serial.Zero, item.Location, map);
											}
											else
											{
												src = new Entity(((Item)root).Serial, ((Item)root).Location, map);
											}

											p = Packet.Acquire(new DragEffect(src, from, item.ItemID, item.Hue, amount));
										}

										ns.Send(p);
									}
								}

								Packet.Release(p);

								eable.Free();
							}

							Point3D fixLoc = item.Location;
							Map fixMap = item.Map;
							bool shouldFix = (item.Parent == null);

							item.RecordBounce();
							item.OnItemLifted(from, item);
							item.Internalize();

							from.Holding = item;

							int liftSound = item.GetLiftSound(from);

							if (liftSound != -1)
							{
								from.Send(new PlaySound(liftSound, from));
							}

							from.NextActionTime = Core.TickCount + _ActionDelay;

							if (fixMap != null && shouldFix)
							{
								fixMap.FixColumn(fixLoc.m_X, fixLoc.m_Y);
							}

							reject = LRReason.Inspecific;
							rejected = false;
						}
					}
				}
				else
				{
					reject = LRReason.Inspecific;
				}
			}
			else
			{
				SendActionMessage();
				reject = LRReason.Inspecific;
			}

			if (rejected && state != null)
			{
				state.Send(new LiftRej(reject));

				if (item.Deleted)
				{
					return;
				}

				if (item.Parent is Item)
				{
					if (state.ContainerGridLines)
					{
						state.Send(new ContainerContentUpdate6017(item));
					}
					else
					{
						state.Send(new ContainerContentUpdate(item));
					}
				}
				else if (item.Parent is Mobile)
				{
					state.Send(new EquipUpdate(item));
				}
				else
				{
					item.SendInfoTo(state);
				}

				if (ObjectPropertyList.Enabled && item.Parent != null)
				{
					state.Send(item.OPLPacket);
				}
			}
		}

		public static Item LiftItemDupe(Item oldItem, int amount)
		{
			Item item;
			try
			{
				item = (Item)Activator.CreateInstance(oldItem.GetType());
			}
			catch
			{
				Console.WriteLine(
					"Warning: 0x{0:X}: Item must have a zero paramater constructor to be separated from a stack. '{1}'.",
					oldItem.Serial.Value,
					oldItem.GetType().Name);
				return null;
			}

			item.Visible = oldItem.Visible;
			item.Movable = oldItem.Movable;
			item.LootType = oldItem.LootType;
			item.Direction = oldItem.Direction;
			item.Hue = oldItem.Hue;
			item.ItemID = oldItem.ItemID;
			item.Location = oldItem.Location;
			item.Layer = oldItem.Layer;
			item.Name = oldItem.Name;
			item.Weight = oldItem.Weight;

			item.Amount = oldItem.Amount - amount;
			item.Map = oldItem.Map;

			oldItem.Amount = amount;
			oldItem.OnAfterDuped(item);

			if (oldItem.Parent is Mobile)
			{
				((Mobile)oldItem.Parent).AddItem(item);
			}
			else if (oldItem.Parent is Item)
			{
				((Item)oldItem.Parent).AddItem(item);
			}

			item.Delta(ItemDelta.Update);

			return item;
		}

		public virtual void SendDropEffect(Item item)
		{
			if (_DragEffects && !item.Deleted)
			{
				Map map = _Map;
				object root = item.RootParent;

				if (map != null && (root == null || root is Item))
				{
					IPooledEnumerable<NetState> eable = map.GetClientsInRange(_Location);
					Packet p = null;

					foreach (NetState ns in eable)
					{
						if (ns.StygianAbyss)
						{
							continue;
						}

						if (ns.Mobile != this && ns.Mobile.CanSee(this) && ns.Mobile.InLOS(this) && ns.Mobile.CanSee(root))
						{
							if (p == null)
							{
								IEntity trg;

								if (root == null)
								{
									trg = new Entity(Serial.Zero, item.Location, map);
								}
								else
								{
									trg = new Entity(((Item)root).Serial, ((Item)root).Location, map);
								}

								p = Packet.Acquire(new DragEffect(this, trg, item.ItemID, item.Hue, item.Amount));
							}

							ns.Send(p);
						}
					}

					Packet.Release(p);

					eable.Free();
				}
			}
		}
        #region Enhance Client
        public virtual bool Drop(Item to, Point3D loc, byte gridloc)
		{
			Mobile from = this;
			Item item = from.Holding;

			bool valid = (item != null && item.HeldBy == from && item.Map == Map.Internal);

			from.Holding = null;

			if (!valid)
			{
				return false;
			}

			bool bounced = true;

			item.SetLastMoved();

            if (to == null || !item.DropToItem(from, to, loc, gridloc))
			{
				item.Bounce(from);
			}
			else
			{
				bounced = false;
			}

			item.ClearBounce();

			if (!bounced)
			{
				SendDropEffect(item);
			}

			return !bounced;
		}
        #endregion

        public virtual bool Drop(Point3D loc)
		{
			Mobile from = this;
			Item item = from.Holding;

			bool valid = (item != null && item.HeldBy == from && item.Map == Map.Internal);

			from.Holding = null;

			if (!valid)
			{
				return false;
			}

			bool bounced = true;

			item.SetLastMoved();

			if (!item.DropToWorld(from, loc))
			{
				item.Bounce(from);
			}
			else
			{
				bounced = false;
			}

			item.ClearBounce();

			if (!bounced)
			{
				SendDropEffect(item);
			}

			return !bounced;
		}

		public virtual bool Drop(Mobile to, Point3D loc)
		{
			Mobile from = this;
			Item item = from.Holding;

			bool valid = (item != null && item.HeldBy == from && item.Map == Map.Internal);

			from.Holding = null;

			if (!valid)
			{
				return false;
			}

			bool bounced = true;

			item.SetLastMoved();

			if (to == null || !item.DropToMobile(from, to, loc))
			{
				item.Bounce(from);
			}
			else
			{
				bounced = false;
			}

			item.ClearBounce();

			if (!bounced)
			{
				SendDropEffect(item);
			}

			return !bounced;
		}

		private static readonly object _GhostMutateContext = new object();

		public virtual bool MutateSpeech(List<Mobile> hears, ref string text, ref object context)
		{
			if (Alive)
			{
				return false;
			}

			var sb = new StringBuilder(text.Length, text.Length);

			for (int i = 0; i < text.Length; ++i)
			{
				if (text[i] != ' ')
				{
					sb.Append(_GhostChars[Utility.Random(_GhostChars.Length)]);
				}
				else
				{
					sb.Append(' ');
				}
			}

			text = sb.ToString();
			context = _GhostMutateContext;
			return true;
		}

		public virtual void Manifest(TimeSpan delay)
		{
			Warmode = true;

			if (_AutoManifestTimer == null)
			{
				_AutoManifestTimer = new AutoManifestTimer(this, delay);
			}
			else
			{
				_AutoManifestTimer.Stop();
			}

			_AutoManifestTimer.Start();
		}

		public virtual bool CheckSpeechManifest()
		{
			if (Alive)
			{
				return false;
			}

			TimeSpan delay = _AutoManifestTimeout;

			if (delay > TimeSpan.Zero && (!Warmode || _AutoManifestTimer != null))
			{
				Manifest(delay);
				return true;
			}

			return false;
		}

		public virtual bool CheckHearsMutatedSpeech(Mobile m, object context)
		{
			if (context == _GhostMutateContext)
			{
				return (m.Alive && !m.CanHearGhosts);
			}

			return true;
		}

		private void AddSpeechItemsFrom(List<IEntity> list, Container cont)
		{
			for (int i = 0; i < cont.Items.Count; ++i)
			{
				Item item = cont.Items[i];

				if (item.HandlesOnSpeech)
				{
					list.Add(item);
				}

				if (item is Container)
				{
					AddSpeechItemsFrom(list, (Container)item);
				}
			}
		}

		private class LocationComparer : IComparer<IEntity>
		{
			private static LocationComparer _Instance;

			public static LocationComparer GetInstance(IEntity relativeTo)
			{
				if (_Instance == null)
				{
					_Instance = new LocationComparer(relativeTo);
				}
				else
				{
					_Instance._RelativeTo = relativeTo;
				}

				return _Instance;
			}

			private IEntity _RelativeTo;

			public IEntity RelativeTo { get { return _RelativeTo; } set { _RelativeTo = value; } }

			public LocationComparer(IEntity relativeTo)
			{
				_RelativeTo = relativeTo;
			}

			private int GetDistance(IEntity p)
			{
				int x = _RelativeTo.X - p.X;
				int y = _RelativeTo.Y - p.Y;
				int z = _RelativeTo.Z - p.Z;

				x *= 11;
				y *= 11;

				return (x * x) + (y * y) + (z * z);
			}

			public int Compare(IEntity x, IEntity y)
			{
				return GetDistance(x) - GetDistance(y);
			}
		}

		#region Get*InRange
		public IPooledEnumerable<Item> GetItemsInRange(int range)
		{
			Map map = _Map;

			if (map == null)
			{
				return Map.NullEnumerable<Item>.Instance;
			}

			return map.GetItemsInRange(_Location, range);
		}

		public IPooledEnumerable<IEntity> GetObjectsInRange(int range)
		{
			Map map = _Map;

			if (map == null)
			{
				return Map.NullEnumerable<IEntity>.Instance;
			}

			return map.GetObjectsInRange(_Location, range);
		}

		public IPooledEnumerable<Mobile> GetMobilesInRange(int range)
		{
			Map map = _Map;

			if (map == null)
			{
				return Map.NullEnumerable<Mobile>.Instance;
			}

			return map.GetMobilesInRange(_Location, range);
		}

		public IPooledEnumerable<NetState> GetClientsInRange(int range)
		{
			Map map = _Map;

			if (map == null)
			{
				return Map.NullEnumerable<NetState>.Instance;
			}

			return map.GetClientsInRange(_Location, range);
		}
		#endregion

		private static readonly List<Mobile> _Hears = new List<Mobile>();
		private static readonly List<IEntity> _OnSpeech = new List<IEntity>();

		public virtual void DoSpeech(string text, int[] keywords, MessageType type, int hue)
		{
			if (_Deleted || CommandSystem.Handle(this, text, type))
			{
				return;
			}

			int range = 15;

			switch (type)
			{
				case MessageType.Regular:
					_SpeechHue = hue;
					break;
				case MessageType.Emote:
					_EmoteHue = hue;
					break;
				case MessageType.Whisper:
					_WhisperHue = hue;
					range = 1;
					break;
				case MessageType.Yell:
					_YellHue = hue;
					range = Core.GlobalUpdateRange; //18
					break;
				default:
					type = MessageType.Regular;
					break;
			}

			var regArgs = new SpeechEventArgs(this, text, type, hue, keywords);

			EventSink.InvokeSpeech(regArgs);
			Region.OnSpeech(regArgs);
			OnSaid(regArgs);

			if (regArgs.Blocked)
			{
				return;
			}

			text = regArgs.Speech;

			if (string.IsNullOrEmpty(text))
			{
				return;
			}

			List<Mobile> hears = _Hears;
			List<IEntity> onSpeech = _OnSpeech;

			if (_Map != null)
			{
				IPooledEnumerable<IEntity> eable = _Map.GetObjectsInRange(_Location, range);

				foreach (IEntity o in eable)
				{
					if (o is Mobile)
					{
						var heard = (Mobile)o;

						if (heard.CanSee(this) && (_NoSpeechLOS || !heard.Player || heard.InLOS(this)))
						{
							if (heard._NetState != null)
							{
								hears.Add(heard);
							}

							if (heard.HandlesOnSpeech(this))
							{
								onSpeech.Add(heard);
							}

							for (int i = 0; i < heard.Items.Count; ++i)
							{
								Item item = heard.Items[i];

								if (item.HandlesOnSpeech)
								{
									onSpeech.Add(item);
								}

								if (item is Container)
								{
									AddSpeechItemsFrom(onSpeech, (Container)item);
								}
							}
						}
					}
					else if (o is Item)
					{
						if (((Item)o).HandlesOnSpeech)
						{
							onSpeech.Add(o);
						}

						if (o is Container)
						{
							AddSpeechItemsFrom(onSpeech, (Container)o);
						}
					}
				}

				eable.Free();

				object mutateContext = null;
				string mutatedText = text;
				SpeechEventArgs mutatedArgs = null;

				if (MutateSpeech(hears, ref mutatedText, ref mutateContext))
				{
					mutatedArgs = new SpeechEventArgs(this, mutatedText, type, hue, new int[0]);
				}

				CheckSpeechManifest();

				ProcessDelta();

				Packet regp = null;
				Packet mutp = null;

				for (int i = 0; i < hears.Count; ++i)
				{
					Mobile heard = hears[i];

					if (mutatedArgs == null || !CheckHearsMutatedSpeech(heard, mutateContext))
					{
						heard.OnSpeech(regArgs);

						NetState ns = heard.NetState;

						if (ns != null)
						{
							if (regp == null)
							{
								regp = Packet.Acquire(new UnicodeMessage(_Serial, Body, type, hue, 3, _Language, Name, text));
							}

							ns.Send(regp);
						}
					}
					else
					{
						heard.OnSpeech(mutatedArgs);

						NetState ns = heard.NetState;

						if (ns != null)
						{
							if (mutp == null)
							{
								mutp = Packet.Acquire(new UnicodeMessage(_Serial, Body, type, hue, 3, _Language, Name, mutatedText));
							}

							ns.Send(mutp);
						}
					}
				}

				Packet.Release(regp);
				Packet.Release(mutp);

				if (onSpeech.Count > 1)
				{
					onSpeech.Sort(LocationComparer.GetInstance(this));
				}

				for (int i = 0; i < onSpeech.Count; ++i)
				{
					IEntity obj = onSpeech[i];

					if (obj is Mobile)
					{
						var heard = (Mobile)obj;

						if (mutatedArgs == null || !CheckHearsMutatedSpeech(heard, mutateContext))
						{
							heard.OnSpeech(regArgs);
						}
						else
						{
							heard.OnSpeech(mutatedArgs);
						}
					}
					else
					{
						var item = (Item)obj;

						item.OnSpeech(regArgs);
					}
				}

				if (_Hears.Count > 0)
				{
					_Hears.Clear();
				}

				if (_OnSpeech.Count > 0)
				{
					_OnSpeech.Clear();
				}
			}
		}

		private static VisibleDamageType _VisibleDamageType;

		public static VisibleDamageType VisibleDamageType { get { return _VisibleDamageType; } set { _VisibleDamageType = value; } }

		private List<DamageEntry> _DamageEntries;

		public List<DamageEntry> DamageEntries { get { return _DamageEntries; } }

		public static Mobile GetDamagerFrom(DamageEntry de)
		{
			return (de == null ? null : de.Damager);
		}

		public Mobile FindMostRecentDamager(bool allowSelf)
		{
			return GetDamagerFrom(FindMostRecentDamageEntry(allowSelf));
		}

		public DamageEntry FindMostRecentDamageEntry(bool allowSelf)
		{
			for (int i = _DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= _DamageEntries.Count)
				{
					continue;
				}

				DamageEntry de = _DamageEntries[i];

				if (de.HasExpired)
				{
					_DamageEntries.RemoveAt(i);
				}
				else if (allowSelf || de.Damager != this)
				{
					return de;
				}
			}

			return null;
		}

		public Mobile FindLeastRecentDamager(bool allowSelf)
		{
			return GetDamagerFrom(FindLeastRecentDamageEntry(allowSelf));
		}

		public DamageEntry FindLeastRecentDamageEntry(bool allowSelf)
		{
			for (int i = 0; i < _DamageEntries.Count; ++i)
			{
				if (i < 0)
				{
					continue;
				}

				DamageEntry de = _DamageEntries[i];

				if (de.HasExpired)
				{
					_DamageEntries.RemoveAt(i);
					--i;
				}
				else if (allowSelf || de.Damager != this)
				{
					return de;
				}
			}

			return null;
		}

		public Mobile FindMostTotalDamager(bool allowSelf)
		{
			return GetDamagerFrom(FindMostTotalDamageEntry(allowSelf));
		}

		public DamageEntry FindMostTotalDamageEntry(bool allowSelf)
		{
			DamageEntry mostTotal = null;

			for (int i = _DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= _DamageEntries.Count)
				{
					continue;
				}

				DamageEntry de = _DamageEntries[i];

				if (de.HasExpired)
				{
					_DamageEntries.RemoveAt(i);
				}
				else if ((allowSelf || de.Damager != this) && (mostTotal == null || de.DamageGiven > mostTotal.DamageGiven))
				{
					mostTotal = de;
				}
			}

			return mostTotal;
		}

		public Mobile FindLeastTotalDamager(bool allowSelf)
		{
			return GetDamagerFrom(FindLeastTotalDamageEntry(allowSelf));
		}

		public DamageEntry FindLeastTotalDamageEntry(bool allowSelf)
		{
			DamageEntry mostTotal = null;

			for (int i = _DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= _DamageEntries.Count)
				{
					continue;
				}

				DamageEntry de = _DamageEntries[i];

				if (de.HasExpired)
				{
					_DamageEntries.RemoveAt(i);
				}
				else if ((allowSelf || de.Damager != this) && (mostTotal == null || de.DamageGiven < mostTotal.DamageGiven))
				{
					mostTotal = de;
				}
			}

			return mostTotal;
		}

		public DamageEntry FindDamageEntryFor(Mobile m)
		{
			for (int i = _DamageEntries.Count - 1; i >= 0; --i)
			{
				if (i >= _DamageEntries.Count)
				{
					continue;
				}

				DamageEntry de = _DamageEntries[i];

				if (de.HasExpired)
				{
					_DamageEntries.RemoveAt(i);
				}
				else if (de.Damager == m)
				{
					return de;
				}
			}

			return null;
		}

		public virtual Mobile GetDamageMaster(Mobile damagee)
		{
			return null;
		}

		public virtual DamageEntry RegisterDamage(int amount, Mobile from)
		{
			DamageEntry de = FindDamageEntryFor(from);

			if (de == null)
			{
				de = new DamageEntry(from);
			}

			de.DamageGiven += amount;
			de.LastDamage = DateTime.UtcNow;

			_DamageEntries.Remove(de);
			_DamageEntries.Add(de);

			Mobile master = from.GetDamageMaster(this);

			if (master != null)
			{
				List<DamageEntry> list = de.Responsible;

				if (list == null)
				{
					de.Responsible = list = new List<DamageEntry>();
				}

				DamageEntry resp = null;

				for (int i = 0; i < list.Count; ++i)
				{
					DamageEntry check = list[i];

					if (check.Damager == master)
					{
						resp = check;
						break;
					}
				}

				if (resp == null)
				{
					list.Add(resp = new DamageEntry(master));
				}

				resp.DamageGiven += amount;
				resp.LastDamage = DateTime.UtcNow;
			}

			return de;
		}

		private Mobile _LastKiller;

		[CommandProperty(AccessLevel.GameMaster)]
		public Mobile LastKiller { get { return _LastKiller; } set { _LastKiller = value; } }

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile is <see cref="Damage">damaged</see>. It is called before
		///     <see
		///         cref="Hits">
		///         hit points
		///     </see>
		///     are lowered or the Mobile is <see cref="Kill">killed</see>.
		///     <seealso cref="Damage" />
		///     <seealso cref="Hits" />
		///     <seealso cref="Kill" />
		/// </summary>
		public virtual void OnDamage(int amount, Mobile from, bool willKill)
		{ }

		public virtual void Damage(int amount)
		{
			Damage(amount, null);
		}

		public virtual bool CanBeDamaged()
		{
			return !_Blessed;
		}

		public virtual void Damage(int amount, Mobile from)
		{
			Damage(amount, from, true);
		}

		public virtual void Damage(int amount, Mobile from, bool informMount)
		{
			if (!CanBeDamaged() || _Deleted)
			{
				return;
			}

			if (!Region.OnDamage(this, ref amount))
			{
				return;
			}

			if (amount > 0)
			{
				int oldHits = Hits;
				int newHits = oldHits - amount;

				if (_Spell != null)
				{
					_Spell.OnCasterHurt();
				}

				//if ( _Spell != null && _Spell.State == SpellState.Casting )
				//	_Spell.Disturb( DisturbType.Hurt, false, true );

				if (from != null)
				{
					RegisterDamage(amount, from);
				}

				DisruptiveAction();

				Paralyzed = false;

				if (Asleep)
				{
					Asleep = false;

					if (from != null)
					{
						from.Send(SpeedControl.Disable);
					}
				}

				switch (_VisibleDamageType)
				{
					case VisibleDamageType.Related:
						{
							NetState ourState = _NetState, theirState = (from == null ? null : from._NetState);

							if (ourState == null)
							{
								Mobile master = GetDamageMaster(from);

								if (master != null)
								{
									ourState = master._NetState;
								}
							}

							if (theirState == null && from != null)
							{
								Mobile master = from.GetDamageMaster(this);

								if (master != null)
								{
									theirState = master._NetState;
								}
							}

							if (amount > 0 && (ourState != null || theirState != null))
							{
								Packet p = null; // = new DamagePacket( this, amount );

								if (ourState != null)
								{
									if (ourState.DamagePacket)
									{
										p = Packet.Acquire(new DamagePacket(this, amount));
									}
									else
									{
										p = Packet.Acquire(new DamagePacketOld(this, amount));
									}

									ourState.Send(p);
								}

								if (theirState != null && theirState != ourState)
								{
									bool newPacket = theirState.DamagePacket;

									if (newPacket && (p == null || !(p is DamagePacket)))
									{
										Packet.Release(p);
										p = Packet.Acquire(new DamagePacket(this, amount));
									}
									else if (!newPacket && (p == null || !(p is DamagePacketOld)))
									{
										Packet.Release(p);
										p = Packet.Acquire(new DamagePacketOld(this, amount));
									}

									theirState.Send(p);
								}

								Packet.Release(p);
							}

							break;
						}
					case VisibleDamageType.Everyone:
						{
							SendDamageToAll(amount);
							break;
						}
				}

				OnDamage(amount, from, newHits < 0);

				IMount m = Mount;
				if (m != null && informMount)
				{
					m.OnRiderDamaged(amount, from, newHits < 0);
				}

				if (newHits < 0)
				{
					_LastKiller = from;

					Hits = 0;

					if (oldHits >= 0)
					{
						Kill();
					}
				}
				else
				{
					Hits = newHits;
				}
			}
		}

		public virtual void SendDamageToAll(int amount)
		{
			if (amount < 0)
			{
				return;
			}

			Map map = _Map;

			if (map == null)
			{
				return;
			}

			IPooledEnumerable<NetState> eable = map.GetClientsInRange(_Location);

			Packet pNew = null;
			Packet pOld = null;

			foreach (NetState ns in eable)
			{
				if (ns.Mobile.CanSee(this))
				{
					if (ns.DamagePacket)
					{
						if (pNew == null)
						{
							pNew = Packet.Acquire(new DamagePacket(this, amount));
						}

						ns.Send(pNew);
					}
					else
					{
						if (pOld == null)
						{
							pOld = Packet.Acquire(new DamagePacketOld(this, amount));
						}

						ns.Send(pOld);
					}
				}
			}

			Packet.Release(pNew);
			Packet.Release(pOld);

			eable.Free();
		}

		public void Heal(int amount)
		{
			Heal(amount, this, true);
		}

		public void Heal(int amount, Mobile from)
		{
			Heal(amount, from, true);
		}

		public void Heal(int amount, Mobile from, bool message)
		{
			if (!Alive || IsDeadBondedPet)
			{
				return;
			}

			if (!Region.OnHeal(this, ref amount))
			{
				return;
			}

			OnHeal(ref amount, from);

			if ((Hits + amount) > HitsMax)
			{
				amount = HitsMax - Hits;
			}

			Hits += amount;

			if (message && amount > 0 && _NetState != null)
			{
				_NetState.Send(
					new MessageLocalizedAffix(
						Serial.MinusOne,
						-1,
						MessageType.Label,
						0x3B2,
						3,
						1008158,
						"",
						AffixType.Append | AffixType.System,
						amount.ToString(),
						""));
			}
		}

		public virtual void OnHeal(ref int amount, Mobile from)
		{ }

		public void UsedStuckMenu()
		{
			if (_StuckMenuUses == null)
			{
				_StuckMenuUses = new DateTime[2];
			}

			for (int i = 0; i < _StuckMenuUses.Length; ++i)
			{
				if ((DateTime.UtcNow - _StuckMenuUses[i]) > TimeSpan.FromDays(1.0))
				{
					_StuckMenuUses[i] = DateTime.UtcNow;
					return;
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Squelched { get { return _Squelched; } set { _Squelched = value; } }

		public virtual void Deserialize(GenericReader reader)
		{
			int version = reader.ReadInt();

			switch (version)
			{
				case 33:
					{
						_SpecialSlayerMechanics = reader.ReadBool();

						if (reader.ReadBool())
						{
							int length = reader.ReadInt();

							for (int i = 0; i < length; i++)
							{
								_SlayerVulnerabilities.Add(reader.ReadString());
							}
						}
						else
						{
							_SlayerVulnerabilities = new List<string>();
						}

						goto case 32;
					}
				case 32:
					{
						_IgnoreMobiles = reader.ReadBool();

						goto case 31;
					}
				case 31:
					{
						_LastStrGain = reader.ReadDeltaTime();
						_LastIntGain = reader.ReadDeltaTime();
						_LastDexGain = reader.ReadDeltaTime();

						goto case 30;
					}
				case 30:
					{
						byte hairflag = reader.ReadByte();

						if ((hairflag & 0x01) != 0)
						{
							_Hair = new HairInfo(reader);
						}

						if ((hairflag & 0x02) != 0)
						{
							_FacialHair = new FacialHairInfo(reader);
						}

                        #region Enhance Client
                        if ((hairflag & 0x04) != 0)
                        {
                            _Face = new FaceInfo(reader);
                        }
                        #endregion

						goto case 29;
					}
				case 29:
					{
						_Race = reader.ReadRace();
						goto case 28;
					}
				case 28:
					{
						if (version <= 30)
						{
							LastStatGain = reader.ReadDeltaTime();
						}

						goto case 27;
					}
				case 27:
					{
						_TithingPoints = reader.ReadInt();

						goto case 26;
					}
				case 26:
				case 25:
				case 24:
					{
						_Corpse = reader.ReadItem() as Container;

						goto case 23;
					}
				case 23:
					{
						_CreationTime = reader.ReadDateTime();

						goto case 22;
					}
				case 22: // Just removed followers
				case 21:
					{
						_Stabled = reader.ReadStrongMobileList();

						goto case 20;
					}
				case 20:
					{
						_CantWalk = reader.ReadBool();

						goto case 19;
					}
				case 19: // Just removed variables
				case 18:
					{
						_Virtues = new VirtueInfo(reader);

						goto case 17;
					}
				case 17:
					{
						_Thirst = reader.ReadInt();
						_BAC = reader.ReadInt();

						goto case 16;
					}
				case 16:
					{
						_ShortTermMurders = reader.ReadInt();

						if (version <= 24)
						{
							reader.ReadDateTime();
							reader.ReadDateTime();
						}

						goto case 15;
					}
				case 15:
					{
						if (version < 22)
						{
							reader.ReadInt(); // followers
						}

						_FollowersMax = reader.ReadInt();

						goto case 14;
					}
				case 14:
					{
						_MagicDamageAbsorb = reader.ReadInt();

						goto case 13;
					}
				case 13:
					{
						_GuildFealty = reader.ReadMobile();

						goto case 12;
					}
				case 12:
					{
						_Guild = reader.ReadGuild();

						goto case 11;
					}
				case 11:
					{
						_DisplayGuildTitle = reader.ReadBool();

						goto case 10;
					}
				case 10:
					{
						_CanSwim = reader.ReadBool();

						goto case 9;
					}
				case 9:
					{
						_Squelched = reader.ReadBool();

						goto case 8;
					}
				case 8:
					{
						_Holding = reader.ReadItem();

						goto case 7;
					}
				case 7:
					{
						_VirtualArmor = reader.ReadInt();

						goto case 6;
					}
				case 6:
					{
						_BaseSoundId = reader.ReadInt();

						goto case 5;
					}
				case 5:
					{
						_DisarmReady = reader.ReadBool();
						_StunReady = reader.ReadBool();

						goto case 4;
					}
				case 4:
					{
						if (version <= 25)
						{
							Poison.Deserialize(reader);
						}

						goto case 3;
					}
				case 3:
					{
						_StatCap = reader.ReadInt();

						goto case 2;
					}
				case 2:
					{
						_NameHue = reader.ReadInt();

						goto case 1;
					}
				case 1:
					{
						_Hunger = reader.ReadInt();

						goto case 0;
					}
				case 0:
					{
						if (version < 21)
						{
							_Stabled = new List<Mobile>();
						}

						if (version < 18)
						{
							_Virtues = new VirtueInfo();
						}

						if (version < 11)
						{
							_DisplayGuildTitle = true;
						}

						if (version < 3)
						{
							_StatCap = 225;
						}

						if (version < 15)
						{
							_Followers = 0;
							_FollowersMax = 5;
						}

						_Location = reader.ReadPoint3D();
						_Body = new Body(reader.ReadInt());
						_Name = reader.ReadString();
						_GuildTitle = reader.ReadString();
						_Criminal = reader.ReadBool();
						_Kills = reader.ReadInt();
						_SpeechHue = reader.ReadInt();
						_EmoteHue = reader.ReadInt();
						_WhisperHue = reader.ReadInt();
						_YellHue = reader.ReadInt();
						_Language = reader.ReadString();
						_Female = reader.ReadBool();
						_Warmode = reader.ReadBool();
						_Hidden = reader.ReadBool();
						_Direction = (Direction)reader.ReadByte();
						_Hue = reader.ReadInt();
						_Str = reader.ReadInt();
						_Dex = reader.ReadInt();
						_Int = reader.ReadInt();
						_Hits = reader.ReadInt();
						_Stam = reader.ReadInt();
						_Mana = reader.ReadInt();
						_Map = reader.ReadMap();
						_Blessed = reader.ReadBool();
						_Fame = reader.ReadInt();
						_Karma = reader.ReadInt();
						_AccessLevel = (AccessLevel)reader.ReadByte();

						_Skills = new Skills(this, reader);

						_Items = reader.ReadStrongItemList();

						_Player = reader.ReadBool();
						_Title = reader.ReadString();
						_Profile = reader.ReadString();
						_ProfileLocked = reader.ReadBool();

						if (version <= 18)
						{
							reader.ReadInt();
							reader.ReadInt();
							reader.ReadInt();
						}

						_AutoPageNotify = reader.ReadBool();

						_LogoutLocation = reader.ReadPoint3D();
						_LogoutMap = reader.ReadMap();

						_StrLock = (StatLockType)reader.ReadByte();
						_DexLock = (StatLockType)reader.ReadByte();
						_IntLock = (StatLockType)reader.ReadByte();

						_StatMods = new List<StatMod>();
						_SkillMods = new List<SkillMod>();

						if (reader.ReadBool())
						{
							_StuckMenuUses = new DateTime[reader.ReadInt()];

							for (int i = 0; i < _StuckMenuUses.Length; ++i)
							{
								_StuckMenuUses[i] = reader.ReadDateTime();
							}
						}
						else
						{
							_StuckMenuUses = null;
						}

						if (_Player && _Map != Map.Internal)
						{
							_LogoutLocation = _Location;
							_LogoutMap = _Map;

							_Map = Map.Internal;
						}

						if (_Map != null)
						{
							_Map.OnEnter(this);
						}

						if (_Criminal)
						{
							if (_ExpireCriminal == null)
							{
								_ExpireCriminal = new ExpireCriminalTimer(this);
							}

							_ExpireCriminal.Start();
						}

						if (ShouldCheckStatTimers)
						{
							CheckStatTimers();
						}

						if (!_Player && _Dex <= 100 && _CombatTimer != null)
						{
							_CombatTimer.Priority = TimerPriority.FiftyMS;
						}
						else if (_CombatTimer != null)
						{
							_CombatTimer.Priority = TimerPriority.EveryTick;
						}

						UpdateRegion();
						UpdateResistances();

						break;
					}
			}

			if (!_Player)
			{
				Utility.Intern(ref _Name);
			}

			Utility.Intern(ref _Title);
			Utility.Intern(ref _Language);

			/*	//Moved into cleanup in scripts.
			if( version < 30 )
			Timer.DelayCall( TimeSpan.Zero, new TimerCallback( ConvertHair ) );
			* */
		}

		public void ConvertHair()
		{
			Item hair;

			if ((hair = FindItemOnLayer(Layer.Hair)) != null)
			{
				HairItemID = hair.ItemID;
				HairHue = hair.Hue;
				hair.Delete();
			}

			if ((hair = FindItemOnLayer(Layer.FacialHair)) != null)
			{
				FacialHairItemID = hair.ItemID;
				FacialHairHue = hair.Hue;
				hair.Delete();
			}
		}

		public virtual bool ShouldCheckStatTimers { get { return true; } }

		public virtual void CheckStatTimers()
		{
			if (_Deleted)
			{
				return;
			}

			if (Hits < HitsMax)
			{
				if (CanRegenHits)
				{
					if (_HitsTimer == null)
					{
						_HitsTimer = new HitsTimer(this);
					}

					_HitsTimer.Start();
				}
				else if (_HitsTimer != null)
				{
					_HitsTimer.Stop();
				}
			}
			else
			{
				Hits = HitsMax;
			}

			if (Stam < StamMax)
			{
				if (CanRegenStam)
				{
					if (_StamTimer == null)
					{
						_StamTimer = new StamTimer(this);
					}

					_StamTimer.Start();
				}
				else if (_StamTimer != null)
				{
					_StamTimer.Stop();
				}
			}
			else
			{
				Stam = StamMax;
			}

			if (Mana < ManaMax)
			{
				if (CanRegenMana)
				{
					if (_ManaTimer == null)
					{
						_ManaTimer = new ManaTimer(this);
					}

					_ManaTimer.Start();
				}
				else if (_ManaTimer != null)
				{
					_ManaTimer.Stop();
				}
			}
			else
			{
				Mana = ManaMax;
			}
		}

		private DateTime _CreationTime;

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime CreationTime { get { return _CreationTime; } }

		int ISerializable.TypeReference { get { return _TypeRef; } }

		int ISerializable.SerialIdentity { get { return _Serial; } }

		public virtual void Serialize(GenericWriter writer)
		{
			writer.Write(33); // version

			writer.Write(_SpecialSlayerMechanics);

			if (_SlayerVulnerabilities != null && _SlayerVulnerabilities.Count > 0)
			{
				writer.Write(true);

				writer.Write(_SlayerVulnerabilities.Count);

				for (int i = 0; i < _SlayerVulnerabilities.Count; i++)
				{
					writer.Write(_SlayerVulnerabilities[i]);
				}
			}
			else
			{
				writer.Write(false);
			}

			writer.Write(_IgnoreMobiles);

			writer.WriteDeltaTime(_LastStrGain);
			writer.WriteDeltaTime(_LastIntGain);
			writer.WriteDeltaTime(_LastDexGain);

			byte hairflag = 0x00;

			if (_Hair != null)
			{
				hairflag |= 0x01;
			}

			if (_FacialHair != null)
			{
				hairflag |= 0x02;
			}

            #region Enhance Client
            if (_Face != null)
            {
                hairflag |= 0x04;
            }
            #endregion

			writer.Write(hairflag);

            if ((hairflag & 0x01) != 0)
            {
                if (_Hair != null)
                {
                    _Hair.Serialize(writer);
                }
            }

            if ((hairflag & 0x02) != 0)
            {
                if (_FacialHair != null)
                {
                    _FacialHair.Serialize(writer);
                }
            }

            #region Enhance Client
            if ((hairflag & 0x04) != 0)
            {
                if (_Face != null)
                {
                    _Face.Serialize(writer);
                }
            }
            #endregion

			writer.Write(Race);

			writer.Write(_TithingPoints);

			writer.Write(_Corpse);

			writer.Write(_CreationTime);

			writer.Write(_Stabled, true);

			writer.Write(_CantWalk);

			VirtueInfo.Serialize(writer, _Virtues);

			writer.Write(_Thirst);
			writer.Write(_BAC);

			writer.Write(_ShortTermMurders);
			//writer.Write( _ShortTermElapse );
			//writer.Write( _LongTermElapse );

			//writer.Write( _Followers );
			writer.Write(_FollowersMax);

			writer.Write(_MagicDamageAbsorb);

			writer.Write(_GuildFealty);

			writer.Write(_Guild);

			writer.Write(_DisplayGuildTitle);

			writer.Write(_CanSwim);

			writer.Write(_Squelched);

			writer.Write(_Holding);

			writer.Write(_VirtualArmor);

			writer.Write(_BaseSoundId);

			writer.Write(_DisarmReady);
			writer.Write(_StunReady);

			//Poison.Serialize( _Poison, writer );

			writer.Write(_StatCap);

			writer.Write(_NameHue);

			writer.Write(_Hunger);

			writer.Write(_Location);
			writer.Write(_Body);
			writer.Write(_Name);
			writer.Write(_GuildTitle);
			writer.Write(_Criminal);
			writer.Write(_Kills);
			writer.Write(_SpeechHue);
			writer.Write(_EmoteHue);
			writer.Write(_WhisperHue);
			writer.Write(_YellHue);
			writer.Write(_Language);
			writer.Write(_Female);
			writer.Write(_Warmode);
			writer.Write(_Hidden);
			writer.Write((byte)_Direction);
			writer.Write(_Hue);
			writer.Write(_Str);
			writer.Write(_Dex);
			writer.Write(_Int);
			writer.Write(_Hits);
			writer.Write(_Stam);
			writer.Write(_Mana);

			writer.Write(_Map);

			writer.Write(_Blessed);
			writer.Write(_Fame);
			writer.Write(_Karma);
			writer.Write((byte)_AccessLevel);
			_Skills.Serialize(writer);

			writer.Write(_Items);

			writer.Write(_Player);
			writer.Write(_Title);
			writer.Write(_Profile);
			writer.Write(_ProfileLocked);
			writer.Write(_AutoPageNotify);

			writer.Write(_LogoutLocation);
			writer.Write(_LogoutMap);

			writer.Write((byte)_StrLock);
			writer.Write((byte)_DexLock);
			writer.Write((byte)_IntLock);

			if (_StuckMenuUses != null)
			{
				writer.Write(true);

				writer.Write(_StuckMenuUses.Length);

				for (int i = 0; i < _StuckMenuUses.Length; ++i)
				{
					writer.Write(_StuckMenuUses[i]);
				}
			}
			else
			{
				writer.Write(false);
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public int LightLevel
		{
			get { return _LightLevel; }
			set
			{
				if (_LightLevel != value)
				{
					_LightLevel = value;

					CheckLightLevels(false);

					/*if ( _NetState != null )
					_NetState.Send( new PersonalLightLevel( this ) );*/
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public string Profile { get { return _Profile; } set { _Profile = value; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public bool ProfileLocked { get { return _ProfileLocked; } set { _ProfileLocked = value; } }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Administrator)]
		public bool Player
		{
			get { return _Player; }
			set
			{
				_Player = value;
				InvalidateProperties();

				if (!_Player && _Dex <= 100 && _CombatTimer != null)
				{
					_CombatTimer.Priority = TimerPriority.FiftyMS;
				}
				else if (_CombatTimer != null)
				{
					_CombatTimer.Priority = TimerPriority.EveryTick;
				}

				CheckStatTimers();
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public string Title
		{
			get { return _Title; }
			set
			{
				_Title = value;
				InvalidateProperties();
			}
		}

		private static readonly string[] _AccessLevelNames = {
			"Player", "VIP Player", "Counselor", "Decorator", "Spawner", "Game Master", "Seer", "Administrator", "Developer",
			"Co-Owner", "Owner"
		};

		public static string GetAccessLevelName(AccessLevel level)
		{
			return _AccessLevelNames[(int)level];
		}

		public virtual bool CanPaperdollBeOpenedBy(Mobile from)
		{
			return (Body.IsHuman || Body.IsGhost || IsBodyMod);
		}

		public virtual void GetChildContextMenuEntries(Mobile from, List<ContextMenuEntry> list, Item item)
		{ }

		public virtual void GetContextMenuEntries(Mobile from, List<ContextMenuEntry> list)
		{
			if (_Deleted)
			{
				return;
			}

			if (CanPaperdollBeOpenedBy(from))
			{
				list.Add(new PaperdollEntry(this));
			}

			if (from == this && Backpack != null && CanSee(Backpack) && CheckAlive(false))
			{
				list.Add(new OpenBackpackEntry(this));
			}
		}

		public void Internalize()
		{
			Map = Map.Internal;
		}

		public List<Item> Items { get { return _Items; } }

		/// <summary>
		///     Overridable. Virtual event invoked when <paramref name="item" /> is <see cref="AddItem">added</see> from the Mobile, such as when it is equipped.
		///     <seealso cref="Items" />
		///     <seealso cref="OnItemRemoved" />
		/// </summary>
		public virtual void OnItemAdded(Item item)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when <paramref name="item" /> is <see cref="RemoveItem">removed</see> from the Mobile.
		///     <seealso cref="Items" />
		///     <seealso cref="OnItemAdded" />
		/// </summary>
		public virtual void OnItemRemoved(Item item)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when <paramref name="item" /> is becomes a child of the Mobile; it's worn or contained at some level of the Mobile's
		///     <see
		///         cref="Mobile.Backpack">
		///         backpack
		///     </see>
		///     or <see cref="Mobile.BankBox">bank box</see>
		///     <seealso cref="OnSubItemRemoved" />
		///     <seealso cref="OnItemAdded" />
		/// </summary>
		public virtual void OnSubItemAdded(Item item)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when <paramref name="item" /> is removed from the Mobile, its
		///     <see
		///         cref="Mobile.Backpack">
		///         backpack
		///     </see>
		///     , or its <see cref="Mobile.BankBox">bank box</see>.
		///     <seealso cref="OnSubItemAdded" />
		///     <seealso cref="OnItemRemoved" />
		/// </summary>
		public virtual void OnSubItemRemoved(Item item)
		{ }

		public virtual void OnItemBounceCleared(Item item)
		{ }

		public virtual void OnSubItemBounceCleared(Item item)
		{ }

		public virtual int MaxWeight { get { return int.MaxValue; } }

		public void AddItem(Item item)
		{
			if (item == null || item.Deleted)
			{
				return;
			}

			if (item.Parent == this)
			{
				return;
			}
		    if (item.Parent is Mobile)
			{
				((Mobile)item.Parent).RemoveItem(item);
			}
			else if (item.Parent is Item)
			{
				((Item)item.Parent).RemoveItem(item);
			}
			else
			{
				item.SendRemovePacket();
			}

			item.Parent = this;
			item.Map = _Map;

			_Items.Add(item);

			if (!item.IsVirtualItem)
			{
				UpdateTotal(item, TotalType.Gold, item.TotalGold);
				UpdateTotal(item, TotalType.Items, item.TotalItems + 1);
				UpdateTotal(item, TotalType.Weight, item.TotalWeight + item.PileWeight);
			}

			item.Delta(ItemDelta.Update);

			item.OnAdded(this);
			OnItemAdded(item);

			if (item.PhysicalResistance != 0 || item.FireResistance != 0 || item.ColdResistance != 0 ||
				item.PoisonResistance != 0 || item.EnergyResistance != 0)
			{
				UpdateResistances();
			}
		}

		private static IWeapon _DefaultWeapon;

		public static IWeapon DefaultWeapon { get { return _DefaultWeapon; } set { _DefaultWeapon = value; } }

		public void RemoveItem(Item item)
		{
			if (item == null || _Items == null)
			{
				return;
			}

			if (_Items.Contains(item))
			{
				item.SendRemovePacket();

				//int oldCount = _Items.Count;

				_Items.Remove(item);

				if (!item.IsVirtualItem)
				{
					UpdateTotal(item, TotalType.Gold, -item.TotalGold);
					UpdateTotal(item, TotalType.Items, -(item.TotalItems + 1));
					UpdateTotal(item, TotalType.Weight, -(item.TotalWeight + item.PileWeight));
				}

				item.Parent = null;

				item.OnRemoved(this);
				OnItemRemoved(item);

				if (item.PhysicalResistance != 0 || item.FireResistance != 0 || item.ColdResistance != 0 ||
					item.PoisonResistance != 0 || item.EnergyResistance != 0)
				{
					UpdateResistances();
				}
			}
		}

		public virtual void Animate(int action, int frameCount, int repeatCount, bool forward, bool repeat, int delay)
		{
			Map map = _Map;

			if (map != null)
			{
				ProcessDelta();

				Packet p = null;
				//Packet pNew = null;

				IPooledEnumerable<NetState> eable = map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this))
					{
						state.Mobile.ProcessDelta();

						if (p == null)
						{
							#region SA
							if (Body.IsGargoyle)
							{
								frameCount = 10;

								if (Flying)
								{
									if (action >= 200 && action <= 270)
									{
										action = 75;
									}
									else
									{
										switch (action)
										{
											case 9:
											case 10:
											case 11:
												action = 71;
												break;
											case 12:
											case 13:
											case 14:
												action = 72;
												break;
											case 18:
											case 19:
												action = 71;
												break;
											case 20:
												action = 77;
												break;
											case 31:
												action = 71;
												break;
											case 34:
												action = 78;
												break;
										}
									}
								}
								else
								{
									if (action >= 260 && action <= 270)
									{
										action = 16;
									}
									else if (action >= 200 && action < 260)
									{
										action = 17;
									}
									else
									{
										switch (action)
										{
											case 9:
												action = 13;
												break;
											case 10:
												action = 14;
												break;
											case 11:
												action = 13;
												break;
											case 12:
											case 13:
											case 14:
												action = 12;
												break;
											case 18:
											case 19:
												action = 9;
												break;
										}
									}
								}
							}
							#endregion

							p = Packet.Acquire(new MobileAnimation(this, action, frameCount, repeatCount, forward, repeat, delay));
						}

						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void SendSound(int soundID)
		{
			if (soundID != -1 && _NetState != null)
			{
				Send(new PlaySound(soundID, this));
			}
		}

		public void SendSound(int soundID, IPoint3D p)
		{
			if (soundID != -1 && _NetState != null)
			{
				Send(new PlaySound(soundID, p));
			}
		}

		public void PlaySound(int soundID)
		{
			if (soundID == -1)
			{
				return;
			}

			if (_Map != null)
			{
				Packet p = Packet.Acquire(new PlaySound(soundID, this));

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this))
					{
						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		[CommandProperty(AccessLevel.Counselor)]
		public Skills Skills { get { return _Skills; } set { } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IgnoreMobiles
		{
			get { return _IgnoreMobiles; }
			set
			{
				if (_IgnoreMobiles != value)
				{
					_IgnoreMobiles = value;
					Delta(MobileDelta.Flags);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsStealthing { get { return _IsStealthing; } set { _IsStealthing = value; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Administrator)]
		public AccessLevel AccessLevel
		{
			get { return _AccessLevel; }
			set
			{
				AccessLevel oldValue = _AccessLevel;

				if (oldValue != value)
				{
					_AccessLevel = value;
					Delta(MobileDelta.Noto);
					InvalidateProperties();

					SendMessage("Your access level has been changed. You are now {0}.", GetAccessLevelName(value));

					ClearScreen();
					SendEverything();

					OnAccessLevelChanged(oldValue);
				}
			}
		}

		public virtual void OnAccessLevelChanged(AccessLevel oldLevel)
		{ }

		[CommandProperty(AccessLevel.Decorator)]
		public int Fame
		{
			get { return _Fame; }
			set
			{
				int oldValue = _Fame;

				if (oldValue != value)
				{
					_Fame = value;

					if (ShowFameTitle && (_Player || _Body.IsHuman) && (oldValue >= 10000) != (value >= 10000))
					{
						InvalidateProperties();
					}

					OnFameChange(oldValue);
				}
			}
		}

		public virtual void OnFameChange(int oldValue)
		{ }

		[CommandProperty(AccessLevel.Decorator)]
		public int Karma
		{
			get { return _Karma; }
			set
			{
				int old = _Karma;

				if (old != value)
				{
					_Karma = value;
					OnKarmaChange(old);
				}
			}
		}

		public virtual void OnKarmaChange(int oldValue)
		{ }

		public void RevealingAction()
		{
			RevealingAction(false);
		}

		// Mobile did something which should unhide him
		public virtual void RevealingAction(bool disruptive)
		{
			if (IsStaff())
			{
				return;
			}

			if (_Hidden)
			{
				Hidden = false;
			}

			if (disruptive)
			{
				DisruptiveAction(); // Anything that unhides you will also distrupt meditation
			}

			_IsStealthing = false;
		}

		#region Say/SayTo/Emote/Whisper/Yell
		public void SayTo(Mobile to, bool ascii, string text)
		{
			PrivateOverheadMessage(MessageType.Regular, _SpeechHue, ascii, text, to.NetState);
		}

		public void SayTo(Mobile to, string text)
		{
			SayTo(to, false, text);
		}

		public void SayTo(Mobile to, string format, params object[] args)
		{
			SayTo(to, false, String.Format(format, args));
		}

		public void SayTo(Mobile to, bool ascii, string format, params object[] args)
		{
			SayTo(to, ascii, String.Format(format, args));
		}

		public void SayTo(Mobile to, int number)
		{
			to.Send(new MessageLocalized(_Serial, Body, MessageType.Regular, _SpeechHue, 3, number, Name, ""));
		}

		public void SayTo(Mobile to, int number, string args)
		{
			to.Send(new MessageLocalized(_Serial, Body, MessageType.Regular, _SpeechHue, 3, number, Name, args));
		}

		public void Say(bool ascii, string text)
		{
			PublicOverheadMessage(MessageType.Regular, _SpeechHue, ascii, text);
		}

		public void Say(string text)
		{
			PublicOverheadMessage(MessageType.Regular, _SpeechHue, false, text);
		}

		public void Say(string format, params object[] args)
		{
			Say(String.Format(format, args));
		}

		public void Say(int number, AffixType type, string affix, string args)
		{
			PublicOverheadMessage(MessageType.Regular, _SpeechHue, number, type, affix, args);
		}

		public void Say(int number)
		{
			Say(number, "");
		}

		public void Say(int number, string args)
		{
			PublicOverheadMessage(MessageType.Regular, _SpeechHue, number, args);
		}

		public void Emote(string text)
		{
			PublicOverheadMessage(MessageType.Emote, _EmoteHue, false, text);
		}

		public void Emote(string format, params object[] args)
		{
			Emote(String.Format(format, args));
		}

		public void Emote(int number)
		{
			Emote(number, "");
		}

		public void Emote(int number, string args)
		{
			PublicOverheadMessage(MessageType.Emote, _EmoteHue, number, args);
		}

		public void Whisper(string text)
		{
			PublicOverheadMessage(MessageType.Whisper, _WhisperHue, false, text);
		}

		public void Whisper(string format, params object[] args)
		{
			Whisper(String.Format(format, args));
		}

		public void Whisper(int number)
		{
			Whisper(number, "");
		}

		public void Whisper(int number, string args)
		{
			PublicOverheadMessage(MessageType.Whisper, _WhisperHue, number, args);
		}

		public void Yell(string text)
		{
			PublicOverheadMessage(MessageType.Yell, _YellHue, false, text);
		}

		public void Yell(string format, params object[] args)
		{
			Yell(String.Format(format, args));
		}

		public void Yell(int number)
		{
			Yell(number, "");
		}

		public void Yell(int number, string args)
		{
			PublicOverheadMessage(MessageType.Yell, _YellHue, number, args);
		}
		#endregion

		[CommandProperty(AccessLevel.Decorator)]
		public bool Blessed
		{
			get { return _Blessed; }
			set
			{
				if (_Blessed != value)
				{
					_Blessed = value;
					Delta(MobileDelta.HealthbarYellow);
				}
			}
		}

		public void SendRemovePacket()
		{
			SendRemovePacket(true);
		}

		public void SendRemovePacket(bool everyone)
		{
			if (_Map != null)
			{
				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state != _NetState && (everyone || !state.Mobile.CanSee(this)))
					{
						state.Send(RemovePacket);
					}
				}

				eable.Free();
			}
		}

		public void ClearScreen()
		{
			NetState ns = _NetState;

			if (_Map != null && ns != null)
			{
				IPooledEnumerable<IEntity> eable = _Map.GetObjectsInRange(_Location, Core.GlobalMaxUpdateRange);

				foreach (IEntity o in eable)
				{
					if (o is Mobile)
					{
						var m = (Mobile)o;

						if (m != this && Utility.InUpdateRange(_Location, m._Location))
						{
							ns.Send(m.RemovePacket);
						}
					}
					else if (o is Item)
					{
						var item = (Item)o;

						if (InRange(item.Location, item.GetUpdateRange(this)))
						{
							ns.Send(item.RemovePacket);
						}
					}
				}

				eable.Free();
			}
		}

		public bool Send(Packet p)
		{
			return Send(p, false);
		}

		public bool Send(Packet p, bool throwOnOffline)
		{
		    if (_NetState != null)
			{
				_NetState.Send(p);
				return true;
			}
		    if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Packet could not be sent.");
			}
				return false;
			}

		#region Gumps/Menus
		public bool SendHuePicker(HuePicker p)
		{
			return SendHuePicker(p, false);
		}

		public bool SendHuePicker(HuePicker p, bool throwOnOffline)
		{
		    if (_NetState != null)
			{
				p.SendTo(_NetState);
				return true;
			}
		    if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Hue picker could not be sent.");
			}
				return false;
			}

		public Gump FindGump(Type type)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				foreach (Gump gump in ns.Gumps)
				{
					if (type.IsAssignableFrom(gump.GetType()))
					{
						return gump;
					}
				}
			}

			return null;
		}

		public bool CloseGump(Type type)
		{
		    if (_NetState != null)
			{
				Gump gump = FindGump(type);

				if (gump != null)
				{
					_NetState.Send(new CloseGump(gump.TypeID, 0));

					_NetState.RemoveGump(gump);

					gump.OnServerClose(_NetState);
				}

				return true;
			}
				return false;
			}

		[Obsolete("Use CloseGump( Type ) instead.")]
		public bool CloseGump(Type type, int buttonID)
		{
			return CloseGump(type);
		}

		[Obsolete("Use CloseGump( Type ) instead.")]
		public bool CloseGump(Type type, int buttonID, bool throwOnOffline)
		{
			return CloseGump(type);
		}

		public bool CloseAllGumps()
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				var gumps = new List<Gump>(ns.Gumps);

				ns.ClearGumps();

				foreach (Gump gump in gumps)
				{
					ns.Send(new CloseGump(gump.TypeID, 0));

					gump.OnServerClose(ns);
				}

				return true;
			}
				return false;
			}

		[Obsolete("Use CloseAllGumps() instead.", false)]
		public bool CloseAllGumps(bool throwOnOffline)
		{
			return CloseAllGumps();
		}

		public bool HasGump(Type type)
		{
			return (FindGump(type) != null);
		}

		[Obsolete("Use HasGump( Type ) instead.", false)]
		public bool HasGump(Type type, bool throwOnOffline)
		{
			return HasGump(type);
		}

		public bool SendGump(Gump g)
		{
			return SendGump(g, false);
		}

		public bool SendGump(Gump g, bool throwOnOffline)
		{
		    if (_NetState != null)
			{
				if (OnBeforeSendGump(g))
				{
					g.SendTo(_NetState);
					OnSendGump(g);
					return true;
				}
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Gump could not be sent.");
			}

			return false;
		}

		protected virtual bool OnBeforeSendGump(Gump g)
		{
			return g != null;
		}

		protected virtual void OnSendGump(Gump g)
		{ }

		public bool SendMenu(IMenu m)
		{
			return SendMenu(m, false);
		}

		public bool SendMenu(IMenu m, bool throwOnOffline)
		{
		    if (_NetState != null)
			{
				if (OnBeforeSendMenu(m))
				{
					m.SendTo(_NetState);
					OnSendMenu(m);
					return true;
				}
			}
			else if (throwOnOffline)
			{
				throw new MobileNotConnectedException(this, "Menu could not be sent.");
			}

			return false;
		}

		protected virtual bool OnBeforeSendMenu(IMenu m)
		{
			return m != null;
		}

		protected virtual void OnSendMenu(IMenu m)
		{ }
		#endregion

		/// <summary>
		///     Overridable. Event invoked before the Mobile says something.
		///     <seealso cref="DoSpeech" />
		/// </summary>
		public virtual void OnSaid(SpeechEventArgs e)
		{
			if (_Squelched)
			{
				if (Core.ML)
				{
					SendLocalizedMessage(500168); // You can not say anything, you have been muted.
				}
				else
				{
					SendMessage("You can not say anything, you have been squelched."); //Cliloc ITSELF changed during ML.
				}

				e.Blocked = true;
			}

			if (!e.Blocked)
			{
				RevealingAction();
			}
		}

		public virtual bool HandlesOnSpeech(Mobile from)
		{
			return false;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile hears speech. This event will only be invoked if
		///     <see
		///         cref="HandlesOnSpeech" />
		///     returns true.
		///     <seealso cref="DoSpeech" />
		/// </summary>
		public virtual void OnSpeech(SpeechEventArgs e)
		{ }

		public void SendEverything()
		{
			NetState ns = _NetState;

			if (_Map != null && ns != null)
			{
				IPooledEnumerable<IEntity> eable = _Map.GetObjectsInRange(_Location, Core.GlobalMaxUpdateRange);

				foreach (IEntity o in eable)
				{
					if (o is Item)
					{
						var item = (Item)o;

						if (CanSee(item) && InRange(item.Location, item.GetUpdateRange(this)))
						{
							item.SendInfoTo(ns);
						}
					}
					else if (o is Mobile)
					{
						var m = (Mobile)o;

						if (CanSee(m) && Utility.InUpdateRange(_Location, m._Location))
						{
							ns.Send(MobileIncoming.Create(ns, this, m));

							if (ns.StygianAbyss)
							{
									ns.Send(new HealthbarPoison(m));
									ns.Send(new HealthbarYellow(m));
								}

							if (m.IsDeadBondedPet)
							{
								ns.Send(new BondedStatus(0, m._Serial, 1));
							}

							if (ObjectPropertyList.Enabled)
							{
								ns.Send(m.OPLPacket);

								//foreach ( Item item in m._Items )
								//	ns.Send( item.OPLPacket );
							}
						}
					}
				}

				eable.Free();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public Map Map
		{
			get { return _Map; }
			set
			{
				if (_Deleted)
				{
					return;
				}

				if (_Map != value)
				{
					if (_NetState != null)
					{
						_NetState.ValidateAllTrades();
					}

					Map oldMap = _Map;

					if (_Map != null)
					{
						_Map.OnLeave(this);

						ClearScreen();
						SendRemovePacket();
					}

					for (int i = 0; i < _Items.Count; ++i)
					{
						_Items[i].Map = value;
					}

					_Map = value;

					UpdateRegion();

					if (_Map != null)
					{
						_Map.OnEnter(this);
					}

					NetState ns = _NetState;

					if (ns != null && _Map != null)
					{
						ns.Sequence = 0;
						ns.Send(new MapChange(this));
						ns.Send(new MapPatches());
						ns.Send(SeasonChange.Instantiate(GetSeason(), true));

						if (ns.StygianAbyss)
						{
							ns.Send(new MobileUpdate(this));
						}
						else
						{
							ns.Send(new MobileUpdateOld(this));
						}

						ClearFastwalkStack();
					}

					if (ns != null)
					{
						if (_Map != null)
						{
							ns.Send(new ServerChange(this, _Map));
						}

						ns.Sequence = 0;
						ClearFastwalkStack();

						ns.Send(MobileIncoming.Create(ns, this, this));

						if (ns.StygianAbyss)
						{
							ns.Send(new MobileUpdate(this));
							CheckLightLevels(true);
							ns.Send(new MobileUpdate(this));
						}
						else
						{
							ns.Send(new MobileUpdateOld(this));
							CheckLightLevels(true);
							ns.Send(new MobileUpdateOld(this));
						}
					}

					SendEverything();
					SendIncomingPacket();

					if (ns != null)
					{
						ns.Sequence = 0;
						ClearFastwalkStack();

						ns.Send(MobileIncoming.Create(ns, this, this));

						if (ns.StygianAbyss)
						{
							ns.Send(SupportedFeatures.Instantiate(ns));
							ns.Send(new MobileUpdate(this));
							ns.Send(new MobileAttributes(this));
						}
						else
						{
							ns.Send(SupportedFeatures.Instantiate(ns));
							ns.Send(new MobileUpdateOld(this));
							ns.Send(new MobileAttributes(this));
						}
					}

					OnMapChange(oldMap);
				}
			}
		}

		public void UpdateRegion()
		{
			if (_Deleted)
			{
				return;
			}

			Region newRegion = Region.Find(_Location, _Map);

			if (newRegion != _Region)
			{
				Region.OnRegionChange(this, _Region, newRegion);

				_Region = newRegion;
				OnRegionChange(_Region, newRegion);
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked when <see cref="Map" /> changes.
		/// </summary>
		protected virtual void OnMapChange(Map oldMap)
		{ }

		#region Beneficial Checks/Actions
		public virtual bool CanBeBeneficial(Mobile target)
		{
			return CanBeBeneficial(target, true, false);
		}

		public virtual bool CanBeBeneficial(Mobile target, bool message)
		{
			return CanBeBeneficial(target, message, false);
		}

		public virtual bool CanBeBeneficial(Mobile target, bool message, bool allowDead)
		{
			if (target == null)
			{
				return false;
			}

			if (_Deleted || target._Deleted || !Alive || IsDeadBondedPet ||
				(!allowDead && (!target.Alive || target.IsDeadBondedPet)))
			{
				if (message)
				{
					SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.
				}

				return false;
			}

			if (target == this)
			{
				return true;
			}

			if ( /*_Player &&*/ !Region.AllowBeneficial(this, target))
			{
				// TODO: Pets
				//if ( !(target._Player || target.Body.IsHuman || target.Body.IsAnimal) )
				//{
				if (message)
				{
					SendLocalizedMessage(1001017); // You can not perform beneficial acts on your target.
				}

				return false;
				//}
			}

			return true;
		}

		public virtual bool IsBeneficialCriminal(Mobile target)
		{
			if (this == target)
			{
				return false;
			}

			int n = Notoriety.Compute(this, target);

			return (n == Notoriety.Criminal || n == Notoriety.Murderer);
		}

		/// <summary>
		///     Overridable. Event invoked when the Mobile <see cref="DoBeneficial">does a beneficial action</see>.
		/// </summary>
		public virtual void OnBeneficialAction(Mobile target, bool isCriminal)
		{
			if (isCriminal)
			{
				CriminalAction(false);
			}
		}

		public virtual void DoBeneficial(Mobile target)
		{
			if (target == null)
			{
				return;
			}

			OnBeneficialAction(target, IsBeneficialCriminal(target));

			Region.OnBeneficialAction(this, target);
			target.Region.OnGotBeneficialAction(this, target);
		}

		public virtual bool BeneficialCheck(Mobile target)
		{
			if (CanBeBeneficial(target, true))
			{
				DoBeneficial(target);
				return true;
			}

			return false;
		}
		#endregion

		#region Harmful Checks/Actions
		public virtual bool CanBeHarmful(Mobile target)
		{
			return CanBeHarmful(target, true);
		}

		public virtual bool CanBeHarmful(Mobile target, bool message)
		{
			return CanBeHarmful(target, message, false);
		}

		public virtual bool CanBeHarmful(Mobile target, bool message, bool ignoreOurBlessedness)
		{
			if (target == null)
			{
				return false;
			}

			if (_Deleted || (!ignoreOurBlessedness && _Blessed) || target._Deleted || target._Blessed || !Alive ||
				IsDeadBondedPet || !target.Alive || target.IsDeadBondedPet)
			{
				if (message)
				{
					SendLocalizedMessage(1001018); // You can not perform negative acts on your target.
				}

				return false;
			}

			if (target == this)
			{
				return true;
			}

			// TODO: Pets
			if ( /*_Player &&*/ !Region.AllowHarmful(this, target))
				//(target._Player || target.Body.IsHuman) && !Region.AllowHarmful( this, target )  )
			{
				if (message)
				{
					SendLocalizedMessage(1001018); // You can not perform negative acts on your target.
				}

				return false;
			}

			return true;
		}

		public virtual bool IsHarmfulCriminal(Mobile target)
		{
			return this != target && (Notoriety.Compute(this, target) == Notoriety.Innocent);
		}

		/// <summary>
		///     Overridable. Event invoked when the Mobile <see cref="DoHarmful">does a harmful action</see>.
		/// </summary>
		public virtual void OnHarmfulAction(Mobile target, bool isCriminal)
		{
			if (isCriminal)
			{
				CriminalAction(false);
			}
		}

		public virtual void DoHarmful(Mobile target)
		{
			DoHarmful(target, false);
		}

		public virtual void DoHarmful(Mobile target, bool indirect)
		{
			if (target == null || _Deleted)
			{
				return;
			}

			bool isCriminal = IsHarmfulCriminal(target);

			OnHarmfulAction(target, isCriminal);
			target.AggressiveAction(this, isCriminal);

			Region.OnDidHarmful(this, target);
			target.Region.OnGotHarmful(this, target);

			if (!indirect)
			{
				Combatant = target;
			}

			if (_ExpireCombatant == null)
			{
				_ExpireCombatant = new ExpireCombatantTimer(this);
			}
			else
			{
				_ExpireCombatant.Stop();
			}

			_ExpireCombatant.Start();
		}

		public virtual bool HarmfulCheck(Mobile target)
		{
			if (CanBeHarmful(target))
			{
				DoHarmful(target);
				return true;
			}

			return false;
		}
		#endregion

		#region Stats
		/// <summary>
		///     Gets a list of all <see cref="StatMod">StatMod's</see> currently active for the Mobile.
		/// </summary>
		public List<StatMod> StatMods { get { return _StatMods; } }

		public bool RemoveStatMod(string name)
		{
			for (int i = 0; i < _StatMods.Count; ++i)
			{
				StatMod check = _StatMods[i];

				if (check.Name == name)
				{
					_StatMods.RemoveAt(i);
					CheckStatTimers();
					Delta(MobileDelta.Stat | GetStatDelta(check.Type));
					return true;
				}
			}

			return false;
		}

		public StatMod GetStatMod(string name)
		{
			for (int i = 0; i < _StatMods.Count; ++i)
			{
				StatMod check = _StatMods[i];

				if (check.Name == name)
				{
					return check;
				}
			}

			return null;
		}

		public void AddStatMod(StatMod mod)
		{
			for (int i = 0; i < _StatMods.Count; ++i)
			{
				StatMod check = _StatMods[i];

				if (check.Name == mod.Name)
				{
					Delta(MobileDelta.Stat | GetStatDelta(check.Type));
					_StatMods.RemoveAt(i);
					break;
				}
			}

			_StatMods.Add(mod);
			Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
			CheckStatTimers();
		}

		private MobileDelta GetStatDelta(StatType type)
		{
			MobileDelta delta = 0;

			if ((type & StatType.Str) != 0)
			{
				delta |= MobileDelta.Hits;
			}

			if ((type & StatType.Dex) != 0)
			{
				delta |= MobileDelta.Stam;
			}

			if ((type & StatType.Int) != 0)
			{
				delta |= MobileDelta.Mana;
			}

			return delta;
		}

		/// <summary>
		///     Computes the total modified offset for the specified stat type. Expired <see cref="StatMod" /> instances are removed.
		/// </summary>
		public int GetStatOffset(StatType type)
		{
			int offset = 0;

			for (int i = 0; i < _StatMods.Count; ++i)
			{
				StatMod mod = _StatMods[i];

				if (mod.HasElapsed())
				{
					_StatMods.RemoveAt(i);
					Delta(MobileDelta.Stat | GetStatDelta(mod.Type));
					CheckStatTimers();

					--i;
				}
				else if ((mod.Type & type) != 0)
				{
					offset += mod.Offset;
				}
			}

			return offset;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the <see cref="RawStr" /> changes.
		///     <seealso cref="RawStr" />
		///     <seealso cref="OnRawStatChange" />
		/// </summary>
		public virtual void OnRawStrChange(int oldValue)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when <see cref="RawDex" /> changes.
		///     <seealso cref="RawDex" />
		///     <seealso cref="OnRawStatChange" />
		/// </summary>
		public virtual void OnRawDexChange(int oldValue)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when the <see cref="RawInt" /> changes.
		///     <seealso cref="RawInt" />
		///     <seealso cref="OnRawStatChange" />
		/// </summary>
		public virtual void OnRawIntChange(int oldValue)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when the <see cref="RawStr" />, <see cref="RawDex" />, or <see cref="RawInt" /> changes.
		///     <seealso cref="OnRawStrChange" />
		///     <seealso cref="OnRawDexChange" />
		///     <seealso cref="OnRawIntChange" />
		/// </summary>
		public virtual void OnRawStatChange(StatType stat, int oldValue)
		{ }

		/// <summary>
		///     Gets or sets the base, unmodified, strength of the Mobile. Ranges from 1 to 65000, inclusive.
		///     <seealso cref="Str" />
		///     <seealso cref="StatMod" />
		///     <seealso cref="OnRawStrChange" />
		///     <seealso cref="OnRawStatChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int RawStr
		{
			get { return _Str; }
			set
			{
				if (value < 1)
				{
					value = 1;
				}
				else if (value > 65000)
				{
					value = 65000;
				}

				if (_Str != value)
				{
					int oldValue = _Str;

					_Str = value;
					Delta(MobileDelta.Stat | MobileDelta.Hits);

					if (Hits < HitsMax)
					{
						if (_HitsTimer == null)
						{
							_HitsTimer = new HitsTimer(this);
						}

						_HitsTimer.Start();
					}
					else if (Hits > HitsMax)
					{
						Hits = HitsMax;
					}

					OnRawStrChange(oldValue);
					OnRawStatChange(StatType.Str, oldValue);
				}
			}
		}

		/// <summary>
		///     Gets or sets the effective strength of the Mobile. This is the sum of the <see cref="RawStr" /> plus any additional modifiers. Any attempts to set this value when under the influence of a
		///     <see
		///         cref="StatMod" />
		///     will result in no change. It ranges from 1 to 65000, inclusive.
		///     <seealso cref="RawStr" />
		///     <seealso cref="StatMod" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Str
		{
			get
			{
				int value = _Str + GetStatOffset(StatType.Str);

				if (value < 1)
				{
					value = 1;
				}
				else if (value > 65000)
				{
					value = 65000;
				}

				return value;
			}
			set
			{
				if (_StatMods.Count == 0)
				{
					RawStr = value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the base, unmodified, dexterity of the Mobile. Ranges from 1 to 65000, inclusive.
		///     <seealso cref="Dex" />
		///     <seealso cref="StatMod" />
		///     <seealso cref="OnRawDexChange" />
		///     <seealso cref="OnRawStatChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int RawDex
		{
			get { return _Dex; }
			set
			{
				if (value < 1)
				{
					value = 1;
				}
				else if (value > 65000)
				{
					value = 65000;
				}

				if (_Dex != value)
				{
					int oldValue = _Dex;

					_Dex = value;
					Delta(MobileDelta.Stat | MobileDelta.Stam);

					if (Stam < StamMax)
					{
						if (_StamTimer == null)
						{
							_StamTimer = new StamTimer(this);
						}

						_StamTimer.Start();
					}
					else if (Stam > StamMax)
					{
						Stam = StamMax;
					}

					OnRawDexChange(oldValue);
					OnRawStatChange(StatType.Dex, oldValue);
				}
			}
		}

		/// <summary>
		///     Gets or sets the effective dexterity of the Mobile. This is the sum of the <see cref="RawDex" /> plus any additional modifiers. Any attempts to set this value when under the influence of a
		///     <see
		///         cref="StatMod" />
		///     will result in no change. It ranges from 1 to 65000, inclusive.
		///     <seealso cref="RawDex" />
		///     <seealso cref="StatMod" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Dex
		{
			get
			{
				int value = _Dex + GetStatOffset(StatType.Dex);

				if (value < 1)
				{
					value = 1;
				}
				else if (value > 65000)
				{
					value = 65000;
				}

				return value;
			}
			set
			{
				if (_StatMods.Count == 0)
				{
					RawDex = value;
				}
			}
		}

		/// <summary>
		///     Gets or sets the base, unmodified, intelligence of the Mobile. Ranges from 1 to 65000, inclusive.
		///     <seealso cref="Int" />
		///     <seealso cref="StatMod" />
		///     <seealso cref="OnRawIntChange" />
		///     <seealso cref="OnRawStatChange" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int RawInt
		{
			get { return _Int; }
			set
			{
				if (value < 1)
				{
					value = 1;
				}
				else if (value > 65000)
				{
					value = 65000;
				}

				if (_Int != value)
				{
					int oldValue = _Int;

					_Int = value;
					Delta(MobileDelta.Stat | MobileDelta.Mana);

					if (Mana < ManaMax)
					{
						if (_ManaTimer == null)
						{
							_ManaTimer = new ManaTimer(this);
						}

						_ManaTimer.Start();
					}
					else if (Mana > ManaMax)
					{
						Mana = ManaMax;
					}

					OnRawIntChange(oldValue);
					OnRawStatChange(StatType.Int, oldValue);
				}
			}
		}

		/// <summary>
		///     Gets or sets the effective intelligence of the Mobile. This is the sum of the <see cref="RawInt" /> plus any additional modifiers. Any attempts to set this value when under the influence of a
		///     <see
		///         cref="StatMod" />
		///     will result in no change. It ranges from 1 to 65000, inclusive.
		///     <seealso cref="RawInt" />
		///     <seealso cref="StatMod" />
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int Int
		{
			get
			{
				int value = _Int + GetStatOffset(StatType.Int);

				if (value < 1)
				{
					value = 1;
				}
				else if (value > 65000)
				{
					value = 65000;
				}

				return value;
			}
			set
			{
				if (_StatMods.Count == 0)
				{
					RawInt = value;
				}
			}
		}

		public virtual void OnHitsChange(int oldValue)
		{ }

		public virtual void OnStamChange(int oldValue)
		{ }

		public virtual void OnManaChange(int oldValue)
		{ }

		/// <summary>
		///     Gets or sets the current hit point of the Mobile. This value ranges from 0 to <see cref="HitsMax" />, inclusive. When set to the value of
		///     <see
		///         cref="HitsMax" />
		///     , the <see cref="AggressorInfo.CanReportMurder">CanReportMurder</see> flag of all aggressors is reset to false, and the list of damage entries is cleared.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Hits
		{
			get { return _Hits; }
			set
			{
				if (_Deleted)
				{
					if (_HitsTimer != null)
					{
						_HitsTimer.Stop();
					}

					return;
				}

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= HitsMax)
				{
					value = HitsMax;

					if (_HitsTimer != null)
					{
						_HitsTimer.Stop();
					}

					for (int i = 0; i < _Aggressors.Count; i++) //reset reports on full HP
					{
						_Aggressors[i].CanReportMurder = false;
					}

					if (_DamageEntries.Count > 0)
					{
						_DamageEntries.Clear(); // reset damage entries on full HP
					}
				}

				if (value < HitsMax)
				{
					if (CanRegenHits)
					{
						if (_HitsTimer == null)
						{
							_HitsTimer = new HitsTimer(this);
						}

						_HitsTimer.Start();
					}
					else if (_HitsTimer != null)
					{
						_HitsTimer.Stop();
					}
				}

				if (_Hits != value)
				{
					int oldValue = _Hits;
					_Hits = value;
					Delta(MobileDelta.Hits);
					OnHitsChange(oldValue);
				}
			}
		}

		/// <summary>
		///     Overridable. Gets the maximum hit point of the Mobile. By default, this returns:
		///     <c>
		///         50 + (<see cref="Str" /> / 2)
		///     </c>
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int HitsMax { get { return 50 + (Str / 2); } }

		/// <summary>
		///     Gets or sets the current stamina of the Mobile. This value ranges from 0 to <see cref="StamMax" />, inclusive.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Stam
		{
			get { return _Stam; }
			set
			{
				if (_Deleted)
				{
					if (_StamTimer != null)
					{
						_StamTimer.Stop();
					}

					return;
				}

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= StamMax)
				{
					value = StamMax;

					if (_StamTimer != null)
					{
						_StamTimer.Stop();
					}
				}

				if (value < StamMax)
				{
					if (CanRegenStam)
					{
						if (_StamTimer == null)
						{
							_StamTimer = new StamTimer(this);
						}

						_StamTimer.Start();
					}
					else if (_StamTimer != null)
					{
						_StamTimer.Stop();
					}
				}

				if (_Stam != value)
				{
					int oldValue = _Stam;
					_Stam = value;
					Delta(MobileDelta.Stam);
					OnStamChange(oldValue);
				}
			}
		}

		/// <summary>
		///     Overridable. Gets the maximum stamina of the Mobile. By default, this returns:
		///     <c>
		///         <see cref="Dex" />
		///     </c>
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int StamMax { get { return Dex; } }

		/// <summary>
		///     Gets or sets the current stamina of the Mobile. This value ranges from 0 to <see cref="ManaMax" />, inclusive.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int Mana
		{
			get { return _Mana; }
			set
			{
				if (_Deleted)
				{
					if (_ManaTimer != null)
					{
						_ManaTimer.Stop();
					}

					return;
				}

				if (value < 0)
				{
					value = 0;
				}
				else if (value >= ManaMax)
				{
					value = ManaMax;

					if (_ManaTimer != null)
					{
						_ManaTimer.Stop();
					}

					if (Meditating)
					{
						Meditating = false;
						SendLocalizedMessage(501846); // You are at peace.
					}
				}

				if (value < ManaMax)
				{
					if (CanRegenMana)
					{
						if (_ManaTimer == null)
						{
							_ManaTimer = new ManaTimer(this);
						}

						_ManaTimer.Start();
					}
					else if (_ManaTimer != null)
					{
						_ManaTimer.Stop();
					}
				}

				if (_Mana != value)
				{
					int oldValue = _Mana;
					_Mana = value;
					Delta(MobileDelta.Mana);
					OnManaChange(oldValue);
				}
			}
		}

		/// <summary>
		///     Overridable. Gets the maximum mana of the Mobile. By default, this returns:
		///     <c>
		///         <see cref="Int" />
		///     </c>
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int ManaMax { get { return Int; } }
		#endregion

		public virtual int Luck { get { return 0; } }

        #region Enhance Client
        public virtual int AttackChance
        {
            get { return 0; }
        }
        public virtual int WeaponSpeed
        {
            get { return 0; }
        }
        public virtual int WeaponDamage
        {
            get { return 0; }
        }
        public virtual int LowerRegCost
        {
            get { return 0; }
        }
        public virtual int RegenHits
        {
            get { return 0; }
        }
        public virtual int RegenStam
        {
            get { return 0; }
        }
        public virtual int RegenMana
        {
            get { return 0; }
        }
        public virtual int ReflectPhysical
        {
            get { return 0; }
        }
        public virtual int EnhancePotions
        {
            get { return 0; }
        }
        public virtual int DefendChance
        {
            get { return 0; }
        }
        public virtual int SpellDamage
        {
            get { return 0; }
        }
        public virtual int CastRecovery
        {
            get { return 0; }
        }
        public virtual int CastSpeed
        {
            get { return 0; }
        }
        public virtual int LowerManaCost
        {
            get { return 0; }
        }
        public virtual int BonusStr
        {
            get { return 0; }
        }
        public virtual int BonusDex
        {
            get { return 0; }
        }
        public virtual int BonusInt
        {
            get { return 0; }
        }
        public virtual int BonusHits
        {
            get { return 0; }
        }
        public virtual int BonusStam
        {
            get { return 0; }
        }
        public virtual int BonusMana
        {
            get { return 0; }
        }
        public virtual int MaxHitIncrease
        {
            get { return HitsMax; }
        }
        public virtual int MaxStamIncrease
        {
            get { return StamMax; }
        }
        public virtual int MaxManaIncrease
        {
            get { return ManaMax; }
        }
        #endregion

		public virtual int HuedItemID { get { return (_Female ? 0x2107 : 0x2106); } }

		private int _HueMod = -1;

		[Hue, CommandProperty(AccessLevel.Decorator)]
		public int HueMod
		{
			get { return _HueMod; }
			set
			{
				if (_HueMod != value)
				{
					_HueMod = value;

					Delta(MobileDelta.Hue);
				}
			}
		}

		[Hue, CommandProperty(AccessLevel.Decorator)]
		public virtual int Hue
		{
			get
			{
				if (_HueMod != -1)
				{
					return _HueMod;
				}

				return _Hue;
			}
			set
			{
				int oldHue = _Hue;

				if (oldHue != value)
				{
					_Hue = value;

					Delta(MobileDelta.Hue);
				}
			}
		}

		public void SetDirection(Direction dir)
		{
			_Direction = dir;
		}

		[CommandProperty(AccessLevel.Decorator)]
		public Direction Direction
		{
			get { return _Direction; }
			set
			{
				if (_Direction != value)
				{
					_Direction = value;

					Delta(MobileDelta.Direction);
					//ProcessDelta();
				}
			}
		}

		public virtual int GetSeason()
		{
			if (_Map != null)
			{
				return _Map.Season;
			}

			return 1;
		}

		public virtual int GetPacketFlags()
		{
			int flags = 0x0;

			if (_Paralyzed || _Frozen || _Sleep)
			{
				flags |= 0x01;
			}

			if (_Female)
			{
				flags |= 0x02;
			}

			if (_Flying)
			{
				flags |= 0x04;
			}

			if (_Blessed || _YellowHealthbar)
			{
				flags |= 0x08;
			}

			if (_IgnoreMobiles)
			{
				flags |= 0x10;
			}

			if (_Warmode)
			{
				flags |= 0x40;
			}

			if (_Hidden)
			{
				flags |= 0x80;
			}

			return flags;
		}

		// Pre-7.0.0.0 Packet Flags
		public virtual int GetOldPacketFlags()
		{
			int flags = 0x0;

			if (_Paralyzed || _Frozen || _Sleep)
			{
				flags |= 0x01;
			}

			if (_Female)
			{
				flags |= 0x02;
			}

			if (_Poison != null)
			{
				flags |= 0x04;
			}

			if (_Blessed || _YellowHealthbar)
			{
				flags |= 0x08;
			}

			if (_IgnoreMobiles)
			{
				flags |= 0x10;
			}

			if (_Warmode)
			{
				flags |= 0x40;
			}

			if (_Hidden)
			{
				flags |= 0x80;
			}

			return flags;
		}

		[CommandProperty(AccessLevel.Decorator)]
		public bool Female
		{
			get { return _Female; }
			set
			{
				if (_Female != value)
				{
					_Female = value;
					Delta(MobileDelta.Flags);
					OnGenderChanged(!_Female);
				}
			}
		}

		public virtual void OnGenderChanged(bool oldFemale)
		{ }

		[CommandProperty(AccessLevel.Decorator)]
		public bool Flying
		{
			get { return _Flying; }
			set
			{
				if (_Flying != value)
				{
					_Flying = value;
					Delta(MobileDelta.Flags);
				}
			}
		}

		#region Stygian Abyss
		public virtual void ToggleFlying()
		{ }
		#endregion

		[CommandProperty(AccessLevel.Decorator)]
		public bool Warmode
		{
			get { return _Warmode; }
			set
			{
				if (_Deleted)
				{
					return;
				}

				if (_Warmode != value)
				{
					if (_AutoManifestTimer != null)
					{
						_AutoManifestTimer.Stop();
						_AutoManifestTimer = null;
					}

					_Warmode = value;
					Delta(MobileDelta.Flags);

					if (_NetState != null)
					{
						Send(SetWarMode.Instantiate(value));
					}

					if (!_Warmode)
					{
						Combatant = null;
					}

					if (!Alive)
					{
						if (value)
						{
							Delta(MobileDelta.GhostUpdate);
						}
						else
						{
							SendRemovePacket(false);
						}
					}

					OnWarmodeChanged();
				}
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked after the Warmode property has changed.
		/// </summary>
		public virtual void OnWarmodeChanged()
		{ }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Hidden
		{
			get { return _Hidden; }
			set
			{
				if (_Hidden != value)
				{
					_Hidden = value;
					//Delta( MobileDelta.Flags );

					OnHiddenChanged();
				}
			}
		}

		public virtual void OnHiddenChanged()
		{
			_AllowedStealthSteps = 0;

			if (_Map != null)
			{
				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (!state.Mobile.CanSee(this))
					{
						state.Send(RemovePacket);
					}
					else
					{
						state.Send(MobileIncoming.Create(state, state.Mobile, this));

						if (IsDeadBondedPet)
						{
							state.Send(new BondedStatus(0, _Serial, 1));
						}

						if (ObjectPropertyList.Enabled)
						{
							state.Send(OPLPacket);

							//foreach ( Item item in _Items )
							//	state.Send( item.OPLPacket );
						}
					}
				}

				eable.Free();
			}
		}

		public virtual void OnConnected()
		{ }

		public virtual void OnDisconnected()
		{ }

		public virtual void OnNetStateChanged()
		{ }

		[CommandProperty(AccessLevel.GameMaster, AccessLevel.Owner)]
		public NetState NetState
		{
			get
			{
				if (_NetState != null && _NetState.Socket == null)
				{
					NetState = null;
				}

				return _NetState;
			}
			set
			{
				if (_NetState != value)
				{
					if (_Map != null)
					{
						_Map.OnClientChange(_NetState, value, this);
					}

					if (_Target != null)
					{
						_Target.Cancel(this, TargetCancelType.Disconnected);
					}

					if (_QuestArrow != null)
					{
						QuestArrow = null;
					}

					if (_Spell != null)
					{
						_Spell.OnConnectionChanged();
					}

					//if ( _Spell != null )
					//	_Spell.FinishSequence();

					if (_NetState != null)
					{
						_NetState.CancelAllTrades();
					}

					BankBox box = FindBankNoCreate();

					if (box != null && box.Opened)
					{
						box.Close();
					}

					// REMOVED:
					//_Actions.Clear();

					_NetState = value;

					if (_NetState == null)
					{
						OnDisconnected();
						EventSink.InvokeDisconnected(new DisconnectedEventArgs(this));

						// Disconnected, start the logout timer

						if (_LogoutTimer == null)
						{
							_LogoutTimer = new LogoutTimer(this);
						}
						else
						{
							_LogoutTimer.Stop();
						}

						_LogoutTimer.Delay = GetLogoutDelay();
						_LogoutTimer.Start();
					}
					else
					{
						OnConnected();
						EventSink.InvokeConnected(new ConnectedEventArgs(this));

						// Connected, stop the logout timer and if needed, move to the world

						if (_LogoutTimer != null)
						{
							_LogoutTimer.Stop();
						}

						_LogoutTimer = null;

						if (_Map == Map.Internal && _LogoutMap != null)
						{
							Map = _LogoutMap;
							Location = _LogoutLocation;
						}
					}

					for (int i = _Items.Count - 1; i >= 0; --i)
					{
						if (i >= _Items.Count)
						{
							continue;
						}

						Item item = _Items[i];

						if (item is SecureTradeContainer)
						{
							for (int j = item.Items.Count - 1; j >= 0; --j)
							{
								if (j < item.Items.Count)
								{
									item.Items[j].OnSecureTrade(this, this, this, false);
									AddToBackpack(item.Items[j]);
								}
							}

							Timer.DelayCall(TimeSpan.Zero, item.Delete);
						}
					}

					DropHolding();
					OnNetStateChanged();
				}
			}
		}

		public virtual bool CanSee(object o)
		{
			if (o is Item)
			{
				return CanSee((Item)o);
			}
		    if (o is Mobile)
			{
				return CanSee((Mobile)o);
			}
				return true;
			}

		public virtual bool CanSee(Item item)
		{
			if(item == null || item.Deleted)
			{
				return false;
			}

			if (_Map == null || _Map == Map.Internal)
			{
				return false;
			}

			if (item.Map == null || item.Map == Map.Internal)
			{
				return false;
			}

			if (!item.GhostVisible && !Alive && IsPlayer())
			{
				return false;
			}

			if (item.Parent != null)
			{
				if (item.Parent is Item)
				{
					var parent = item.Parent as Item;

					if (!(CanSee(parent) && parent.IsChildVisibleTo(this, item)))
					{
						return false;
					}
				}
				else if (item.Parent is Mobile)
				{
					if (!CanSee((Mobile)item.Parent))
					{
						return false;
					}
				}
			}

			if (item is BankBox)
			{
				var box = item as BankBox;

				if (IsPlayer() && (box.Owner != this || !box.Opened))
				{
					return false;
				}
			}
			else if (item is SecureTradeContainer)
			{
				SecureTrade trade = ((SecureTradeContainer)item).Trade;

				if (trade != null && trade.From.Mobile != this && trade.To.Mobile != this)
				{
					return false;
				}
			}

			return !item.Deleted && item.Map == _Map && (item.Visible || IsStaff());
		}

		private bool m_GhostVisible = true;

		[CommandProperty(AccessLevel.GameMaster)]
		public bool GhostVisible { get { return m_GhostVisible; } set { m_GhostVisible = value; } }

		public virtual bool CanSee(Mobile m)
		{
			if (_Deleted || m == null || m._Deleted || _Map == Map.Internal || m._Map == Map.Internal)
			{
				return false;
			}

			if (!m.GhostVisible && !Alive && m.IsPlayer())
			{
				return false;
			}

			return this == m ||
				   (m._Map == _Map && (!m.Hidden || (IsStaff() && _AccessLevel >= m.AccessLevel)) &&
					((m.Alive || (Core.SE && Skills.SpiritSpeak.Value >= 100.0)) || !Alive || IsStaff() || m.Warmode));
		}

		public virtual bool CanBeRenamedBy(Mobile from)
		{
			return (from.AccessLevel >= AccessLevel.Decorator && from._AccessLevel > _AccessLevel);
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public string Language
		{
			get { return _Language; }
			set
			{
				if (_Language != null && _Language != value)
				{
					_Language = value;
				}
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public int SpeechHue { get { return _SpeechHue; } set { _SpeechHue = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public int EmoteHue { get { return _EmoteHue; } set { _EmoteHue = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public int WhisperHue { get { return _WhisperHue; } set { _WhisperHue = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public int YellHue { get { return _YellHue; } set { _YellHue = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public string GuildTitle
		{
			get { return _GuildTitle; }
			set
			{
				string old = _GuildTitle;

				if (old != value)
				{
					_GuildTitle = value;

					if (_Guild != null && !_Guild.Disbanded && _GuildTitle != null)
					{
						SendLocalizedMessage(1018026, true, _GuildTitle); // Your guild title has changed :
					}

					InvalidateProperties();

					OnGuildTitleChange(old);
				}
			}
		}

		public virtual void OnGuildTitleChange(string oldTitle)
		{ }

		[CommandProperty(AccessLevel.Decorator)]
		public bool DisplayGuildTitle
		{
			get { return _DisplayGuildTitle; }
			set
			{
				_DisplayGuildTitle = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public Mobile GuildFealty { get { return _GuildFealty; } set { _GuildFealty = value; } }

		private string _NameMod;

		[CommandProperty(AccessLevel.Decorator)]
		public string NameMod
		{
			get { return _NameMod; }
			set
			{
				if (_NameMod != value)
				{
					_NameMod = value;
					Delta(MobileDelta.Name);
					InvalidateProperties();
				}
			}
		}

		private bool _YellowHealthbar;

		[CommandProperty(AccessLevel.Decorator)]
		public bool YellowHealthbar
		{
			get { return _YellowHealthbar; }
			set
			{
				_YellowHealthbar = value;
				Delta(MobileDelta.HealthbarYellow);
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public string RawName { get { return _Name; } set { Name = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public string Name
		{
			get
			{
				if (_NameMod != null)
				{
					return _NameMod;
				}

				return _Name;
			}
			set
			{
				if (_Name != value) // I'm leaving out the && _NameMod == null
				{
					string oldName = _Name;
					_Name = value;
					OnAfterNameChange(oldName, _Name);
					Delta(MobileDelta.Name);
					InvalidateProperties();
				}
			}
		}

		public virtual void OnAfterNameChange(string oldName, string newName)
		{ }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastStrGain { get { return _LastStrGain; } set { _LastStrGain = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastIntGain { get { return _LastIntGain; } set { _LastIntGain = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public DateTime LastDexGain { get { return _LastDexGain; } set { _LastDexGain = value; } }

		public DateTime LastStatGain
		{
			get
			{
				DateTime d = _LastStrGain;

				if (_LastIntGain > d)
				{
					d = _LastIntGain;
				}

				if (_LastDexGain > d)
				{
					d = _LastDexGain;
				}

				return d;
			}
			set
			{
				_LastStrGain = value;
				_LastIntGain = value;
				_LastDexGain = value;
			}
		}

		public BaseGuild Guild
		{
			get { return _Guild; }
			set
			{
				BaseGuild old = _Guild;

				if (old != value)
				{
					if (value == null)
					{
						GuildTitle = null;
					}

					_Guild = value;

					Delta(MobileDelta.Noto);
					InvalidateProperties();

					OnGuildChange(old);
				}
			}
		}

		public virtual void OnGuildChange(BaseGuild oldGuild)
		{ }

		#region Poison/Curing
		public Timer PoisonTimer { get { return _PoisonTimer; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public Poison Poison
		{
			get { return _Poison; }
			set
			{
				/*if ( _Poison != value && (_Poison == null || value == null || _Poison.Level < value.Level) )
				{*/
				_Poison = value;
				Delta(MobileDelta.HealthbarPoison);

				if (_PoisonTimer != null)
				{
					_PoisonTimer.Stop();
					_PoisonTimer = null;
				}

				if (_Poison != null)
				{
					_PoisonTimer = _Poison.ConstructTimer(this);

					if (_PoisonTimer != null)
					{
						_PoisonTimer.Start();
					}
				}

				CheckStatTimers();
				/*}*/
			}
		}

		/// <summary>
		///     Overridable. Event invoked when a call to <see cref="ApplyPoison" /> failed because <see cref="CheckPoisonImmunity" /> returned false: the Mobile was resistant to the poison. By default, this broadcasts an overhead message: * The poison seems to have no effect. *
		///     <seealso cref="CheckPoisonImmunity" />
		///     <seealso cref="ApplyPoison" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual void OnPoisonImmunity(Mobile from, Poison poison)
		{
			PublicOverheadMessage(MessageType.Emote, 0x3B2, 1005534); // * The poison seems to have no effect. *
		}

		/// <summary>
		///     Overridable. Virtual event invoked when a call to <see cref="ApplyPoison" /> failed because
		///     <see
		///         cref="CheckHigherPoison" />
		///     returned false: the Mobile was already poisoned by an equal or greater strength poison.
		///     <seealso cref="CheckHigherPoison" />
		///     <seealso cref="ApplyPoison" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual void OnHigherPoison(Mobile from, Poison poison)
		{ }

		/// <summary>
		///     Overridable. Event invoked when a call to <see cref="ApplyPoison" /> succeeded. By default, this broadcasts an overhead message varying by the level of the poison. Example: * Zippy begins to spasm uncontrollably. *
		///     <seealso cref="ApplyPoison" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual void OnPoisoned(Mobile from, Poison poison, Poison oldPoison)
		{
			if (poison != null)
			{
				#region Mondain's Legacy
				LocalOverheadMessage(MessageType.Regular, 0x21, 1042857 + (poison.RealLevel * 2));
				NonlocalOverheadMessage(MessageType.Regular, 0x21, 1042858 + (poison.RealLevel * 2), Name);
				#endregion
			}
		}

		/// <summary>
		///     Overridable. Called from <see cref="ApplyPoison" />, this method checks if the Mobile is immune to some
		///     <see
		///         cref="Poison" />
		///     . If true, <see cref="OnPoisonImmunity" /> will be invoked and
		///     <see
		///         cref="ApplyPoisonResult.Immune" />
		///     is returned.
		///     <seealso cref="OnPoisonImmunity" />
		///     <seealso cref="ApplyPoison" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual bool CheckPoisonImmunity(Mobile from, Poison poison)
		{
			return false;
		}

		/// <summary>
		///     Overridable. Called from <see cref="ApplyPoison" />, this method checks if the Mobile is already poisoned by some
		///     <see
		///         cref="Poison" />
		///     of equal or greater strength. If true, <see cref="OnHigherPoison" /> will be invoked and
		///     <see
		///         cref="ApplyPoisonResult.HigherPoisonActive" />
		///     is returned.
		///     <seealso cref="OnHigherPoison" />
		///     <seealso cref="ApplyPoison" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual bool CheckHigherPoison(Mobile from, Poison poison)
		{
			#region Mondain's Legacy
			return (_Poison != null && _Poison.RealLevel >= poison.RealLevel);
			#endregion
		}

		/// <summary>
		///     Overridable. Attempts to apply poison to the Mobile. Checks are made such that no <see cref="CheckHigherPoison">higher poison is active</see> and that the Mobile is not
		///     <see
		///         cref="CheckPoisonImmunity">
		///         immune to the poison
		///     </see>
		///     . Provided those assertions are true, the
		///     <paramref
		///         name="poison" />
		///     is applied and <see cref="OnPoisoned" /> is invoked.
		///     <seealso cref="Poison" />
		///     <seealso cref="CurePoison" />
		/// </summary>
		/// <returns>
		///     One of four possible values:
		///     <list type="table">
		///         <item>
		///             <term>
		///                 <see cref="ApplyPoisonResult.Cured">Cured</see>
		///             </term>
		///             <description>
		///                 The <paramref name="poison" /> parameter was null and so <see cref="CurePoison" /> was invoked.
		///             </description>
		///         </item>
		///         <item>
		///             <term>
		///                 <see cref="ApplyPoisonResult.HigherPoisonActive">HigherPoisonActive</see>
		///             </term>
		///             <description>
		///                 The call to <see cref="CheckHigherPoison" /> returned false.
		///             </description>
		///         </item>
		///         <item>
		///             <term>
		///                 <see cref="ApplyPoisonResult.Immune">Immune</see>
		///             </term>
		///             <description>
		///                 The call to <see cref="CheckPoisonImmunity" /> returned false.
		///             </description>
		///         </item>
		///         <item>
		///             <term>
		///                 <see cref="ApplyPoisonResult.Poisoned">Poisoned</see>
		///             </term>
		///             <description>
		///                 The <paramref name="poison" /> was successfully applied.
		///             </description>
		///         </item>
		///     </list>
		/// </returns>
		public virtual ApplyPoisonResult ApplyPoison(Mobile from, Poison poison)
		{
			if (poison == null)
			{
				CurePoison(from);
				return ApplyPoisonResult.Cured;
			}

			if (CheckHigherPoison(from, poison))
			{
				OnHigherPoison(from, poison);
				return ApplyPoisonResult.HigherPoisonActive;
			}

			if (CheckPoisonImmunity(from, poison))
			{
				OnPoisonImmunity(from, poison);
				return ApplyPoisonResult.Immune;
			}

			Poison oldPoison = _Poison;
			Poison = poison;

			OnPoisoned(from, poison, oldPoison);

			return ApplyPoisonResult.Poisoned;
		}

		/// <summary>
		///     Overridable. Called from <see cref="CurePoison" />, this method checks to see that the Mobile can be cured of
		///     <see
		///         cref="Poison" />
		///     <seealso cref="CurePoison" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual bool CheckCure(Mobile from)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when a call to <see cref="CurePoison" /> succeeded.
		///     <seealso cref="CurePoison" />
		///     <seealso cref="CheckCure" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual void OnCured(Mobile from, Poison oldPoison)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when a call to <see cref="CurePoison" /> failed.
		///     <seealso cref="CurePoison" />
		///     <seealso cref="CheckCure" />
		///     <seealso cref="Poison" />
		/// </summary>
		public virtual void OnFailedCure(Mobile from)
		{ }

		/// <summary>
		///     Overridable. Attempts to cure any poison that is currently active.
		/// </summary>
		/// <returns>True if poison was cured, false if otherwise.</returns>
		public virtual bool CurePoison(Mobile from)
		{
			if (CheckCure(from))
			{
				Poison oldPoison = _Poison;
				Poison = null;

				OnCured(from, oldPoison);

				return true;
			}

			OnFailedCure(from);

			return false;
		}
		#endregion

		private ISpawner _Spawner;

		public ISpawner Spawner { get { return _Spawner; } set { _Spawner = value; } }

		public Region WalkRegion { get; set; }

		public virtual void OnBeforeSpawn(Point3D location, Map m)
		{ }

		public virtual void OnAfterSpawn()
		{ }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Poisoned { get { return (_Poison != null); } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool IsBodyMod { get { return (_BodyMod.BodyID != 0); } }

		[CommandProperty(AccessLevel.Decorator)]
		public Body BodyMod
		{
			get { return _BodyMod; }
			set
			{
				if (_BodyMod != value)
				{
					_BodyMod = value;

					Delta(MobileDelta.Body);
					InvalidateProperties();

					CheckStatTimers();
				}
			}
		}

		private static readonly int[] _InvalidBodies = {
			//32,		// Dunno why is blocked
			//95,		// Used for Turkey
			//156,		// Dunno why is blocked
			//197,		// ML Dragon
			//198,		// ML Dragon
		};

		[Body, CommandProperty(AccessLevel.Decorator)]
		public Body Body
		{
			get
			{
				if (IsBodyMod)
				{
					return _BodyMod;
				}

				return _Body;
			}
			set
			{
				if (_Body != value && !IsBodyMod)
				{
					_Body = SafeBody(value);

					Delta(MobileDelta.Body);
					InvalidateProperties();

					CheckStatTimers();
				}
			}
		}

		public virtual int SafeBody(int body)
		{
			int delta = -1;

			for (int i = 0; delta < 0 && i < _InvalidBodies.Length; ++i)
			{
				delta = (_InvalidBodies[i] - body);
			}

			if (delta != 0)
			{
				return body;
			}

			return 0;
		}

		[Body, CommandProperty(AccessLevel.Decorator)]
		public int BodyValue { get { return Body.BodyID; } set { Body = value; } }

		[CommandProperty(AccessLevel.Counselor)]
		public Serial Serial { get { return _Serial; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public Point3D Location { get { return _Location; } set { SetLocation(value, true); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Point3D LogoutLocation { get { return _LogoutLocation; } set { _LogoutLocation = value; } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.GameMaster)]
		public Map LogoutMap { get { return _LogoutMap; } set { _LogoutMap = value; } }

		public Region Region
		{
			get
			{
			    if (_Region == null)
				{
					if (Map == null)
					{
						return Map.Internal.DefaultRegion;
					}
						return Map.DefaultRegion;
					}
			    return _Region;
				}
				}

		public void FreeCache()
		{
			Packet.Release(ref _RemovePacket);
			Packet.Release(ref _PropertyList);
			Packet.Release(ref _OPLPacket);
		}

		private Packet _RemovePacket;
		private readonly object rpLock = new object();

		public Packet RemovePacket
		{
			get
			{
				if (_RemovePacket == null)
				{
					lock (rpLock)
					{
						if (_RemovePacket == null)
						{
							_RemovePacket = new RemoveMobile(this);
							_RemovePacket.SetStatic();
						}
					}
				}

				return _RemovePacket;
			}
		}

		private Packet _OPLPacket;
		private readonly object oplLock = new object();

		public Packet OPLPacket
		{
			get
			{
				if (_OPLPacket == null)
				{
					lock (oplLock)
					{
						if (_OPLPacket == null)
						{
							_OPLPacket = new OPLInfo(PropertyList);
							_OPLPacket.SetStatic();
						}
					}
				}

				return _OPLPacket;
			}
		}

		private ObjectPropertyList _PropertyList;

		public ObjectPropertyList PropertyList
		{
			get
			{
				if (_PropertyList == null)
				{
					_PropertyList = new ObjectPropertyList(this);

					GetProperties(_PropertyList);

					_PropertyList.Terminate();
					_PropertyList.SetStatic();
				}

				return _PropertyList;
			}
		}

		public void ClearProperties()
		{
			Packet.Release(ref _PropertyList);
			Packet.Release(ref _OPLPacket);
		}

		public void InvalidateProperties()
		{
			if (!ObjectPropertyList.Enabled)
			{
				return;
			}

			if (_Map != null && _Map != Map.Internal && !World.Loading)
			{
				ObjectPropertyList oldList = _PropertyList;
				Packet.Release(ref _PropertyList);
				ObjectPropertyList newList = PropertyList;

				if (oldList == null || oldList.Hash != newList.Hash)
				{
					Packet.Release(ref _OPLPacket);
					Delta(MobileDelta.Properties);
				}
			}
			else
			{
				ClearProperties();
			}
		}

		private int _SolidHueOverride = -1;

		[CommandProperty(AccessLevel.Decorator)]
		public int SolidHueOverride
		{
			get { return _SolidHueOverride; }
			set
			{
				if (_SolidHueOverride == value)
				{
					return;
				}
				_SolidHueOverride = value;
				Delta(MobileDelta.Hue | MobileDelta.Body);
			}
		}

		public virtual void MoveToWorld(Point3D newLocation, Map map)
		{
			if (_Deleted)
			{
				return;
			}

			if (_Map == map)
			{
				SetLocation(newLocation, true);
				return;
			}

			BankBox box = FindBankNoCreate();

			if (box != null && box.Opened)
			{
				box.Close();
			}

			Point3D oldLocation = _Location;
			Map oldMap = _Map;

			Region oldRegion = _Region;

			if (oldMap != null)
			{
				oldMap.OnLeave(this);

				ClearScreen();
				SendRemovePacket();
			}

			for (int i = 0; i < _Items.Count; ++i)
			{
				_Items[i].Map = map;
			}

			_Map = map;

			_Location = newLocation;

			NetState ns = _NetState;

			if (_Map != null)
			{
				_Map.OnEnter(this);

				UpdateRegion();

				if (ns != null && _Map != null)
				{
					ns.Sequence = 0;
					ns.Send(new MapChange(this));
					ns.Send(new MapPatches());
					ns.Send(SeasonChange.Instantiate(GetSeason(), true));

					if (ns.StygianAbyss)
					{
						ns.Send(new MobileUpdate(this));
					}
					else
					{
						ns.Send(new MobileUpdateOld(this));
					}

					ClearFastwalkStack();
				}
			}
			else
			{
				UpdateRegion();
			}

			if (ns != null)
			{
				if (_Map != null)
				{
					Send(new ServerChange(this, _Map));
				}

				ns.Sequence = 0;
				ClearFastwalkStack();

				ns.Send(MobileIncoming.Create(ns, this, this));

				if (ns.StygianAbyss)
				{
					ns.Send(new MobileUpdate(this));
					CheckLightLevels(true);
					ns.Send(new MobileUpdate(this));
				}
				else
				{
					ns.Send(new MobileUpdateOld(this));
					CheckLightLevels(true);
					ns.Send(new MobileUpdateOld(this));
				}
			}

			SendEverything();
			SendIncomingPacket();

			if (ns != null)
			{
				ns.Sequence = 0;
				ClearFastwalkStack();

				ns.Send(MobileIncoming.Create(ns, this, this));

				if (ns.StygianAbyss)
				{
					ns.Send(SupportedFeatures.Instantiate(ns));
					ns.Send(new MobileUpdate(this));
					ns.Send(new MobileAttributes(this));
				}
				else
				{
					ns.Send(SupportedFeatures.Instantiate(ns));
					ns.Send(new MobileUpdateOld(this));
					ns.Send(new MobileAttributes(this));
				}
			}

			OnMapChange(oldMap);
			OnLocationChange(oldLocation);

			if (_Region != null)
			{
				_Region.OnLocationChanged(this, oldLocation);
			}
		}

		public virtual void SetLocation(Point3D newLocation, bool isTeleport)
		{
			if (_Deleted)
			{
				return;
			}

			Point3D oldLocation = _Location;

			if (oldLocation != newLocation)
			{
				_Location = newLocation;
				UpdateRegion();

				BankBox box = FindBankNoCreate();

				if (box != null && box.Opened)
				{
					box.Close();
				}

				if (_NetState != null)
				{
					_NetState.ValidateAllTrades();
				}

				if (_Map != null)
				{
					_Map.OnMove(oldLocation, this);
				}

				if (isTeleport && _NetState != null && (!_NetState.HighSeas || !_NoMoveHS))
				{
					_NetState.Sequence = 0;

					if (_NetState.StygianAbyss)
					{
						_NetState.Send(new MobileUpdate(this));
					}
					else
					{
						_NetState.Send(new MobileUpdateOld(this));
					}

					ClearFastwalkStack();
				}

				Map map = _Map;

				if (map != null)
				{
					// First, send a remove message to everyone who can no longer see us. (inOldRange && !inNewRange)

					IPooledEnumerable<NetState> eable = map.GetClientsInRange(oldLocation);

					foreach (NetState ns in eable)
					{
						if (ns != _NetState && !Utility.InUpdateRange(newLocation, ns.Mobile.Location))
						{
							ns.Send(RemovePacket);
						}
					}

					eable.Free();

				    Packet hbpPacket = Packet.Acquire(new HealthbarPoison(this)),
				        hbyPacket = Packet.Acquire(new HealthbarYellow(this));

					NetState ourState = _NetState;

					// Check to see if we are attached to a client
					if (ourState != null)
					{
						IPooledEnumerable<IEntity> eeable = map.GetObjectsInRange(newLocation, Core.GlobalMaxUpdateRange);

						// We are attached to a client, so it's a bit more complex. We need to send new items and people to ourself, and ourself to other clients

						foreach (IEntity o in eeable)
						{
							if (o is Item)
							{
								var item = (Item)o;

								int range = item.GetUpdateRange(this);
								Point3D loc = item.Location;

								if (!Utility.InRange(oldLocation, loc, range) && Utility.InRange(newLocation, loc, range) && CanSee(item))
								{
									item.SendInfoTo(ourState);
								}
							}
							else if (o != this && o is Mobile)
							{
								var m = (Mobile)o;

								if (!Utility.InUpdateRange(newLocation, m._Location))
								{
									continue;
								}

								bool inOldRange = Utility.InUpdateRange(oldLocation, m._Location);

								if (m._NetState != null && ((isTeleport && (!m._NetState.HighSeas || !_NoMoveHS)) || !inOldRange) &&
									m.CanSee(this))
								{
									m._NetState.Send(MobileIncoming.Create(m._NetState, m, this));

                                    if (ourState.StygianAbyss)
									{
                                        ourState.Send(new HealthbarPoison(m));
                                        ourState.Send(new HealthbarYellow(m));
										}

									if (IsDeadBondedPet)
									{
										m._NetState.Send(new BondedStatus(0, _Serial, 1));
									}

									if (ObjectPropertyList.Enabled)
									{
										m._NetState.Send(OPLPacket);

										//foreach ( Item item in _Items )
										//	m._NetState.Send( item.OPLPacket );
									}
								}

								if (!inOldRange && CanSee(m))
								{
									ourState.Send(MobileIncoming.Create(ourState, this, m));

									if (ourState.StygianAbyss)
									{
										if (m.Poisoned)
										{
											ourState.Send(new HealthbarPoison(m));
										}

										if (m.Blessed || m.YellowHealthbar)
										{
											ourState.Send(new HealthbarYellow(m));
										}
									}

									if (m.IsDeadBondedPet)
									{
										ourState.Send(new BondedStatus(0, m._Serial, 1));
									}

									if (ObjectPropertyList.Enabled)
									{
										ourState.Send(m.OPLPacket);

										//foreach ( Item item in m._Items )
										//	ourState.Send( item.OPLPacket );
									}
								}
							}
						}

						eeable.Free();
					}
					else
					{
						eable = map.GetClientsInRange(newLocation);

						// We're not attached to a client, so simply send an Incoming
						foreach (NetState ns in eable)
						{
							if (((isTeleport && (!ns.HighSeas || !_NoMoveHS)) || !Utility.InUpdateRange(oldLocation, ns.Mobile.Location)) &&
								ns.Mobile.CanSee(this))
							{
								ns.Send(MobileIncoming.Create(ns, ns.Mobile, this));

								if (ns.StygianAbyss)
								{
									if (_Poison != null)
									{
										ns.Send(new HealthbarPoison(this));
									}

									if (_Blessed || _YellowHealthbar)
									{
										ns.Send(new HealthbarYellow(this));
									}
								}

								if (IsDeadBondedPet)
								{
									ns.Send(new BondedStatus(0, _Serial, 1));
								}

								if (ObjectPropertyList.Enabled)
								{
									ns.Send(OPLPacket);

									//foreach ( Item item in _Items )
									//	ns.Send( item.OPLPacket );
								}
							}
						}

						eable.Free();
					}
				}

				OnLocationChange(oldLocation);

				Region.OnLocationChanged(this, oldLocation);
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked when <see cref="Location" /> changes.
		/// </summary>
		protected virtual void OnLocationChange(Point3D oldLocation)
		{ }

		#region Hair
		private HairInfo _Hair;
		private FacialHairInfo _FacialHair;

		[CommandProperty(AccessLevel.Decorator)]
		public int HairItemID
		{
			get
			{
				if (_Hair == null)
				{
					return 0;
				}

				return _Hair.ItemID;
			}
			set
			{
				if (_Hair == null && value > 0)
				{
					_Hair = new HairInfo(value);
				}
				else if (value <= 0)
				{
					_Hair = null;
				}
				else
				{
					_Hair.ItemID = value;
				}

				Delta(MobileDelta.Hair);
			}
		}

		//		[CommandProperty( AccessLevel.GameMaster )]
		//		public int HairSerial { get { return HairInfo.FakeSerial( this ); } }

		[CommandProperty(AccessLevel.Decorator)]
		public int FacialHairItemID
		{
			get
			{
				if (_FacialHair == null)
				{
					return 0;
				}

				return _FacialHair.ItemID;
			}
			set
			{
				if (_FacialHair == null && value > 0)
				{
					_FacialHair = new FacialHairInfo(value);
				}
				else if (value <= 0)
				{
					_FacialHair = null;
				}
				else
				{
					_FacialHair.ItemID = value;
				}

				Delta(MobileDelta.FacialHair);
			}
		}

		//		[CommandProperty( AccessLevel.GameMaster )]
		//		public int FacialHairSerial { get { return FacialHairInfo.FakeSerial( this ); } }

		[CommandProperty(AccessLevel.Decorator)]
		public int HairHue
		{
			get
			{
				if (_Hair == null)
				{
					return 0;
				}
				return _Hair.Hue;
			}
			set
			{
				if (_Hair != null)
				{
					_Hair.Hue = value;
					Delta(MobileDelta.Hair);
				}
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public int FacialHairHue
		{
			get
			{
				if (_FacialHair == null)
				{
					return 0;
				}

				return _FacialHair.Hue;
			}
			set
			{
				if (_FacialHair != null)
				{
					_FacialHair.Hue = value;
					Delta(MobileDelta.FacialHair);
				}
			}
		}
		#endregion

        #region Enhance Client
        private FaceInfo _Face;

        [CommandProperty(AccessLevel.GameMaster)]
        public int FaceItemID
        {
            get
            {
                if (_Face == null)
                {
                    return 0;
                }

                return _Face.ItemID;
            }
            set
            {
                if (_Face == null && value > 0)
                {
                    _Face = new FaceInfo(value);
                }
                else if (value <= 0)
                {
                    _Face = null;
                }
                else
                {
                    _Face.ItemID = value;
                    Delta(MobileDelta.Face);
                }
            }
        }

        [CommandProperty(AccessLevel.GameMaster)]
        public int FaceHue
        {
            get
            {
                if (_Face == null)
                {
                    return Hue;
                }

                return _Face.Hue;
            }
            set
            {
                if (_Face != null)
                {
                    _Face.Hue = value;
                    Delta(MobileDelta.Face);
                }
            }
        }
        #endregion

		public bool HasFreeHand()
		{
			return FindItemOnLayer(Layer.TwoHanded) == null;
		}

		private IWeapon _Weapon;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual IWeapon Weapon
		{
			get
			{
				var item = _Weapon as Item;

				if (item != null && !item.Deleted && item.Parent == this && CanSee(item))
				{
					return _Weapon;
				}

				_Weapon = null;

				item = FindItemOnLayer(Layer.OneHanded);

				if (item == null)
				{
					item = FindItemOnLayer(Layer.TwoHanded);
				}

				if (item is IWeapon)
				{
					return (_Weapon = (IWeapon)item);
				}
					return GetDefaultWeapon();
				}
			}

		public virtual IWeapon GetDefaultWeapon()
		{
			return _DefaultWeapon;
		}

		private BankBox _BankBox;

		[CommandProperty(AccessLevel.GameMaster)]
		public BankBox BankBox
		{
			get
			{
				if (_BankBox != null && !_BankBox.Deleted && _BankBox.Parent == this)
				{
					return _BankBox;
				}

				_BankBox = FindItemOnLayer(Layer.Bank) as BankBox;

				if (_BankBox == null)
				{
					AddItem(_BankBox = new BankBox(this));
				}

				return _BankBox;
			}
		}

		public BankBox FindBankNoCreate()
		{
			if (_BankBox != null && !_BankBox.Deleted && _BankBox.Parent == this)
			{
				return _BankBox;
			}

			_BankBox = FindItemOnLayer(Layer.Bank) as BankBox;

			return _BankBox;
		}

		private Container _Backpack;

		[CommandProperty(AccessLevel.GameMaster)]
		public Container Backpack
		{
			get
			{
				if (_Backpack != null && !_Backpack.Deleted && _Backpack.Parent == this)
				{
					return _Backpack;
				}

				return (_Backpack = (FindItemOnLayer(Layer.Backpack) as Container));
			}
		}

		public virtual bool KeepsItemsOnDeath { get { return IsStaff(); } }

		public Item FindItemOnLayer(Layer layer)
		{
			List<Item> eq = _Items;
			int count = eq.Count;

			for (int i = 0; i < count; ++i)
			{
				Item item = eq[i];

				if (!item.Deleted && item.Layer == layer)
				{
					return item;
				}
			}

			return null;
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public int X { get { return _Location.m_X; } set { Location = new Point3D(value, _Location.m_Y, _Location.m_Z); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public int Y { get { return _Location.m_Y; } set { Location = new Point3D(_Location.m_X, value, _Location.m_Z); } }

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public int Z { get { return _Location.m_Z; } set { Location = new Point3D(_Location.m_X, _Location.m_Y, value); } }

		#region Effects & Particles
		public void MovingEffect(
			IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes, int hue, int renderMode)
		{
			Effects.SendMovingEffect(this, to, itemID, speed, duration, fixedDirection, explodes, hue, renderMode);
		}

		public void MovingEffect(IEntity to, int itemID, int speed, int duration, bool fixedDirection, bool explodes)
		{
			Effects.SendMovingEffect(this, to, itemID, speed, duration, fixedDirection, explodes, 0, 0);
		}

		public void MovingParticles(
			IEntity to,
			int itemID,
			int speed,
			int duration,
			bool fixedDirection,
			bool explodes,
			int hue,
			int renderMode,
			int effect,
			int explodeEffect,
			int explodeSound,
			EffectLayer layer,
			int unknown)
		{
			Effects.SendMovingParticles(
				this,
				to,
				itemID,
				speed,
				duration,
				fixedDirection,
				explodes,
				hue,
				renderMode,
				effect,
				explodeEffect,
				explodeSound,
				layer,
				unknown);
		}

		public void MovingParticles(
			IEntity to,
			int itemID,
			int speed,
			int duration,
			bool fixedDirection,
			bool explodes,
			int hue,
			int renderMode,
			int effect,
			int explodeEffect,
			int explodeSound,
			int unknown)
		{
			Effects.SendMovingParticles(
				this,
				to,
				itemID,
				speed,
				duration,
				fixedDirection,
				explodes,
				hue,
				renderMode,
				effect,
				explodeEffect,
				explodeSound,
				(EffectLayer)255,
				unknown);
		}

		public void MovingParticles(
			IEntity to,
			int itemID,
			int speed,
			int duration,
			bool fixedDirection,
			bool explodes,
			int effect,
			int explodeEffect,
			int explodeSound,
			int unknown)
		{
			Effects.SendMovingParticles(
				this, to, itemID, speed, duration, fixedDirection, explodes, effect, explodeEffect, explodeSound, unknown);
		}

		public void MovingParticles(
			IEntity to,
			int itemID,
			int speed,
			int duration,
			bool fixedDirection,
			bool explodes,
			int effect,
			int explodeEffect,
			int explodeSound)
		{
			Effects.SendMovingParticles(
				this, to, itemID, speed, duration, fixedDirection, explodes, 0, 0, effect, explodeEffect, explodeSound, 0);
		}

		public void FixedEffect(int itemID, int speed, int duration, int hue, int renderMode)
		{
			Effects.SendTargetEffect(this, itemID, speed, duration, hue, renderMode);
		}

		public void FixedEffect(int itemID, int speed, int duration)
		{
			Effects.SendTargetEffect(this, itemID, speed, duration, 0, 0);
		}

		public void FixedParticles(
			int itemID, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer, int unknown)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, hue, renderMode, effect, layer, unknown);
		}

		public void FixedParticles(
			int itemID, int speed, int duration, int effect, int hue, int renderMode, EffectLayer layer)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, hue, renderMode, effect, layer, 0);
		}

		public void FixedParticles(int itemID, int speed, int duration, int effect, EffectLayer layer, int unknown)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, 0, 0, effect, layer, unknown);
		}

		public void FixedParticles(int itemID, int speed, int duration, int effect, EffectLayer layer)
		{
			Effects.SendTargetParticles(this, itemID, speed, duration, 0, 0, effect, layer, 0);
		}

		public void BoltEffect(int hue)
		{
			Effects.SendBoltEffect(this, true, hue);
		}
		#endregion

		public void SendIncomingPacket()
		{
			if (_Map != null)
			{
			    Packet hbpPacket = Packet.Acquire(new HealthbarPoison(this)),
			        hbyPacket = Packet.Acquire(new HealthbarYellow(this));

                IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this))
					{
						state.Send(MobileIncoming.Create(state, state.Mobile, this));

						if (state.StygianAbyss)
						{
							state.Send(hbpPacket);
							state.Send(hbyPacket);
							}

						if (IsDeadBondedPet)
						{
							state.Send(new BondedStatus(0, _Serial, 1));
						}

						if (ObjectPropertyList.Enabled)
						{
							state.Send(OPLPacket);

							//foreach ( Item item in _Items )
							//	state.Send( item.OPLPacket );
						}
					}
				}

				eable.Free();
			}
		}

		public bool PlaceInBackpack(Item item)
		{
			if (item.Deleted)
			{
				return false;
			}

			Container pack = Backpack;

			return pack != null && pack.TryDropItem(this, item, false);
		}

		/// <summary>
		///     Attempts to add the given item to this mobile's Backpack.
		/// </summary>
		/// <param name="item">The item to add to this mobile's Backpack.</param>
		/// <returns>True if the item was placed in this mobile's Backpack.</returns>
		public bool AddToBackpack(Item item)
		{
			return AddToBackpack(item, true);
		}

		/// <summary>
		///     Attempts to add the given item to this mobile's Backpack.
		/// </summary>
		/// <param name="item">The item to add to this mobile's Backpack.</param>
		/// <param name="moveToWorld">When true, will move the item to the world when the item can not be added to this mobile's Backpack.</param>
		/// <param name="callback">Callback action called when the method returns. Action(item, success)</param>
		/// <returns>True if the item was placed in this mobile's Backpack.</returns>
		public bool AddToBackpack(Item item, bool moveToWorld, Action<Item, bool> callback = null)
		{
			if (item == null || item.Deleted)
			{
				return false;
			}

			if (PlaceInBackpack(item))
			{
				if (callback != null)
				{
					callback(item, true);
				}

				return true;
			}

			Point3D loc = _Location;
			Map map = _Map;

				if ((map == null || map == Map.Internal) && _LogoutMap != null)
			{
					loc = _LogoutLocation;
					map = _LogoutMap;
			}

			if (map != null && map != Map.Internal && moveToWorld)
			{
				item.MoveToWorldAndStack(loc, map);
			}

			if (callback != null)
			{
				callback(item, false);
			}

			return false;
		}

		public virtual bool CheckLiftTrigger(Item item, ref LRReason reject)
		{
			return true;
		}

		public virtual bool CheckLift(Mobile from, Item item, ref LRReason reject)
		{
			return true;
		}

		public virtual bool CheckNonlocalLift(Mobile from, Item item)
		{
			if (from == this || (from.AccessLevel > AccessLevel && from.AccessLevel >= AccessLevel.GameMaster))
			{
				return true;
			}

			return false;
		}

		public bool HasTrade
		{
			get
			{
				if (_NetState != null)
				{
					return _NetState.Trades.Count > 0;
				}

				return false;
			}
		}

		public virtual bool CheckTrade(
			Mobile to, Item item, SecureTradeContainer cont, bool message, bool checkItems, int plusItems, int plusWeight)
		{
			return true;
		}

		public bool OpenTrade(Mobile from)
		{
			return OpenTrade(from, null);
		}

		/// The range check has not been moved from Mobile.OnDragDrop to Mobile.OpenTrade, meaning OpenTrade could be used to allow players to trade over long distances, should any custom system wish to do so..
		///
		public virtual bool OpenTrade(Mobile from, Item offer)
		{
			if (!from.Player || !Player || !from.Alive || !Alive)
			{
				return false;
			}

			NetState ourState = _NetState;
			NetState theirState = from._NetState;

			if (ourState == null || theirState == null)
			{
				return false;
			}

			SecureTradeContainer cont = theirState.FindTradeContainer(this);

			if (!from.CheckTrade(this, offer, cont, true, true, 0, 0))
			{
				return false;
			}

			if (cont == null)
			{
				cont = theirState.AddTrade(ourState);
			}

			if (offer != null)
			{
				cont.DropItem(offer);
			}

			return true;
		}

		/// <summary>
		///     Overridable. Event invoked when a Mobile (<paramref name="from" />) drops an
		///     <see cref="Item">
		///         <paramref name="dropped" />
		///     </see>
		///     onto the Mobile.
		/// </summary>
		public virtual bool OnDragDrop(Mobile from, Item dropped)
		{
			if (from == this)
			{
				Container pack = Backpack;

				if (pack != null)
                {
                    #region Enhance Client
                    return dropped.DropToItem(from, pack, new Point3D(-1, -1, 0), 0x0);
                    //return dropped.DropToItem(from, pack, new Point3D(-1, -1, 0), 0x0);
                    #endregion
                }

				return false;
			}
			
			if (from.InRange(Location, 2))
			{
				return OpenTrade(from, dropped);
			}

			return false;
		}

		public virtual bool CheckEquip(Item item)
		{
			for (int i = 0; i < _Items.Count; ++i)
			{
				if (_Items[i].CheckConflictingLayer(this, item, item.Layer) ||
					item.CheckConflictingLayer(this, _Items[i], _Items[i].Layer))
				{
					return false;
				}
			}

			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to wear <paramref name="item" />.
		/// </summary>
		/// <returns>True if the request is accepted, false if otherwise.</returns>
		public virtual bool OnEquip(Item item)
		{
			// For some reason OSI allows equipping quest items, but they are unmarked in the process
			if (item.QuestItem)
			{
				item.QuestItem = false;
				SendLocalizedMessage(1074769);
				// An item must be in your backpack (and not in a container within) to be toggled as a quest item.
			}

			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to lift <paramref name="item" />.
		/// </summary>
		/// <returns>True if the lift is allowed, false if otherwise.</returns>
		/// <example>
		///     The following example demonstrates usage. It will disallow any attempts to pick up a pick axe if the Mobile does not have enough strength.
		///     <code>
		///  public override bool OnDragLift( Item item )
		///  {
		/// 		if ( item is Pickaxe &amp;&amp; this.Str &lt; 60 )
		/// 		{
		/// 			SendMessage( "That is too heavy for you to lift." );
		/// 			return false;
		/// 		}
		/// 		
		/// 		return base.OnDragLift( item );
		///  }</code>
		/// </example>
		public virtual bool OnDragLift(Item item)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> into a
		///     <see cref="Container">
		///         <paramref name="container" />
		///     </see>
		///     .
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemInto(Item item, Container container, Point3D loc)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> directly onto another
		///     <see
		///         cref="Item" />
		///     , <paramref name="target" />. This is the case of stacking items.
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemOnto(Item item, Item target)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> into another
		///     <see
		///         cref="Item" />
		///     , <paramref name="target" />. The target item is most likely a <see cref="Container" />.
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemToItem(Item item, Item target, Point3D loc)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to give <paramref name="item" /> to a Mobile (
		///     <paramref
		///         name="target" />
		///     ).
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemToMobile(Item item, Mobile target)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile attempts to drop <paramref name="item" /> to the world at a
		///     <see cref="Point3D">
		///         <paramref name="location" />
		///     </see>
		///     .
		/// </summary>
		/// <returns>True if the drop is allowed, false if otherwise.</returns>
		public virtual bool OnDroppedItemToWorld(Item item, Point3D location)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event when <paramref name="from" /> successfully uses <paramref name="item" /> while it's on this Mobile.
		///     <seealso cref="Item.OnItemUsed" />
		/// </summary>
		public virtual void OnItemUsed(Mobile from, Item item)
		{
			EventSink.InvokeOnItemUse(new OnItemUseEventArgs(from, item));
		}

		public virtual bool CheckNonlocalDrop(Mobile from, Item item, Item target)
		{
			if (from == this || (from.AccessLevel > AccessLevel && from.AccessLevel >= AccessLevel.GameMaster))
			{
				return true;
			}

			return false;
		}

		public virtual bool CheckItemUse(Mobile from, Item item)
		{
			return true;
		}

		/// <summary>
		///     Overridable. Virtual event invoked when <paramref name="from" /> successfully lifts <paramref name="item" /> from this Mobile.
		///     <seealso cref="Item.OnItemLifted" />
		/// </summary>
		public virtual void OnItemLifted(Mobile from, Item item)
		{ }

		public virtual bool AllowItemUse(Item item)
		{
			return true;
		}

		public virtual bool AllowEquipFrom(Mobile mob)
		{
			return (mob == this || (mob.AccessLevel >= AccessLevel.Decorator && mob.AccessLevel > AccessLevel));
		}

		public virtual bool EquipItem(Item item)
		{
			if (item == null || item.Deleted || !item.CanEquip(this))
			{
				return false;
			}

			if (CheckEquip(item) && OnEquip(item) && item.OnEquip(this))
			{
				if (_Spell != null && !_Spell.OnCasterEquiping(item))
				{
					return false;
				}

				//if ( _Spell != null && _Spell.State == SpellState.Casting )
				//	_Spell.Disturb( DisturbType.EquipRequest );

				AddItem(item);
				return true;
			}

			return false;
		}

		internal int _TypeRef;

		public Mobile(Serial serial)
		{
			_Region = Map.Internal.DefaultRegion;
			_Serial = serial;
			_Aggressors = new List<AggressorInfo>();
			_Aggressed = new List<AggressorInfo>();
			_NextSkillTime = Core.TickCount;
			_DamageEntries = new List<DamageEntry>();

			Type ourType = GetType();
			_TypeRef = World.m_MobileTypes.IndexOf(ourType);

			if (_TypeRef == -1)
			{
				World.m_MobileTypes.Add(ourType);
				_TypeRef = World.m_MobileTypes.Count - 1;
			}
		}

		public Mobile()
		{
			_Region = Map.Internal.DefaultRegion;
			_Serial = Serial.NewMobile;

			DefaultMobileInit();

			World.AddMobile(this);

			Type ourType = GetType();
			_TypeRef = World.m_MobileTypes.IndexOf(ourType);

			if (_TypeRef == -1)
			{
				World.m_MobileTypes.Add(ourType);
				_TypeRef = World.m_MobileTypes.Count - 1;
			}
		}

		public void DefaultMobileInit()
		{
			_StatCap = 225;
			_FollowersMax = 5;
			_Skills = new Skills(this);
			_Items = new List<Item>();
			_StatMods = new List<StatMod>();
			_SkillMods = new List<SkillMod>();
			Map = Map.Internal;
			_AutoPageNotify = true;
			_Aggressors = new List<AggressorInfo>();
			_Aggressed = new List<AggressorInfo>();
			_Virtues = new VirtueInfo();
			_Stabled = new List<Mobile>();
			_DamageEntries = new List<DamageEntry>();

			_NextSkillTime = Core.TickCount;
			_CreationTime = DateTime.UtcNow;
		}

		private static readonly Queue<Mobile> _DeltaQueue = new Queue<Mobile>();
		private static readonly Queue<Mobile> _DeltaQueueR = new Queue<Mobile>();

		private bool _InDeltaQueue;
		private MobileDelta _DeltaFlags;

		public virtual void Delta(MobileDelta flag)
		{
			if (_Map == null || _Map == Map.Internal || _Deleted)
			{
				return;
			}

			_DeltaFlags |= flag;

			if (!_InDeltaQueue)
			{
				_InDeltaQueue = true;

				if (_Processing)
				{
					lock (_DeltaQueueR)
					{
						_DeltaQueueR.Enqueue(this);

						try
						{
							using (var op = new StreamWriter("delta-recursion.log", true))
							{
								op.WriteLine("# {0}", DateTime.UtcNow);
								op.WriteLine(new StackTrace());
								op.WriteLine();
							}
						}
						catch
						{ }
					}
				}
				else
				{
					_DeltaQueue.Enqueue(this);
				}
			}

			Core.Set();
		}

		private bool _NoMoveHS;

		public bool NoMoveHS { get { return _NoMoveHS; } set { _NoMoveHS = value; } }

		#region GetDirectionTo[..]
		public Direction GetDirectionTo(int x, int y)
		{
			int dx = _Location.m_X - x;
			int dy = _Location.m_Y - y;

			int rx = (dx - dy) * 44;
			int ry = (dx + dy) * 44;

			int ax = Math.Abs(rx);
			int ay = Math.Abs(ry);

			Direction ret;

			if (((ay >> 1) - ax) >= 0)
			{
				ret = (ry > 0) ? Direction.Up : Direction.Down;
			}
			else if (((ax >> 1) - ay) >= 0)
			{
				ret = (rx > 0) ? Direction.Left : Direction.Right;
			}
			else if (rx >= 0 && ry >= 0)
			{
				ret = Direction.West;
			}
			else if (rx >= 0 && ry < 0)
			{
				ret = Direction.South;
			}
			else if (rx < 0 && ry < 0)
			{
				ret = Direction.East;
			}
			else
			{
				ret = Direction.North;
			}

			return ret;
		}

		public Direction GetDirectionTo(Point2D p)
		{
			return GetDirectionTo(p.m_X, p.m_Y);
		}

		public Direction GetDirectionTo(Point3D p)
		{
			return GetDirectionTo(p.m_X, p.m_Y);
		}

		public Direction GetDirectionTo(IPoint2D p)
		{
			if (p == null)
			{
				return Direction.North;
			}

			return GetDirectionTo(p.X, p.Y);
		}
		#endregion

		public virtual void ProcessDelta()
		{
			Mobile m = this;
			MobileDelta delta;

			delta = m._DeltaFlags;

			if (delta == MobileDelta.None)
			{
				return;
			}

			MobileDelta attrs = delta & MobileDelta.Attributes;

			m._DeltaFlags = MobileDelta.None;
			m._InDeltaQueue = false;

			bool sendHits = false, sendStam = false, sendMana = false, sendAll = false, sendAny = false;
			bool sendIncoming = false, sendNonlocalIncoming = false;
			bool sendUpdate = false, sendRemove = false;
			bool sendPublicStats = false, sendPrivateStats = false;
			bool sendMoving = false, sendNonlocalMoving = false;
			bool sendOPLUpdate = ObjectPropertyList.Enabled && (delta & MobileDelta.Properties) != 0;

			bool sendHair = false, sendFacialHair = false, removeHair = false, removeFacialHair = false;

            #region Enhance Client
            bool sendFace = false, removeFace = false;
            #endregion

			bool sendHealthbarPoison = false, sendHealthbarYellow = false;

			if (attrs != MobileDelta.None)
			{
				sendAny = true;

				if (attrs == MobileDelta.Attributes)
				{
					sendAll = true;
				}
				else
				{
					sendHits = ((attrs & MobileDelta.Hits) != 0);
					sendStam = ((attrs & MobileDelta.Stam) != 0);
					sendMana = ((attrs & MobileDelta.Mana) != 0);
				}
			}

			if ((delta & MobileDelta.GhostUpdate) != 0)
			{
				sendNonlocalIncoming = true;
			}

			if ((delta & MobileDelta.Hue) != 0)
			{
				sendNonlocalIncoming = true;
				sendUpdate = true;
				sendRemove = true;
			}

			if ((delta & MobileDelta.Direction) != 0)
			{
				sendNonlocalMoving = true;
				sendUpdate = true;
			}

			if ((delta & MobileDelta.Body) != 0)
			{
				sendUpdate = true;
				sendIncoming = true;
			}

			/*if ( (delta & MobileDelta.Hue) != 0 )
			{
			sendNonlocalIncoming = true;
			sendUpdate = true;
			}
			else if ( (delta & (MobileDelta.Direction | MobileDelta.Body)) != 0 )
			{
			sendNonlocalMoving = true;
			sendUpdate = true;
			}
			else*/
			if ((delta & (MobileDelta.Flags | MobileDelta.Noto)) != 0)
			{
				sendMoving = true;
			}

			if ((delta & MobileDelta.HealthbarPoison) != 0)
			{
				sendHealthbarPoison = true;
			}

			if ((delta & MobileDelta.HealthbarYellow) != 0)
			{
				sendHealthbarYellow = true;
			}

			if ((delta & MobileDelta.Name) != 0)
			{
				sendAll = false;
				sendHits = false;
				sendAny = sendStam || sendMana;
				sendPublicStats = true;
			}

			if ((delta &
				 (MobileDelta.WeaponDamage | MobileDelta.Resistances | MobileDelta.Stat | MobileDelta.Weight | MobileDelta.Gold |
				  MobileDelta.Armor | MobileDelta.StatCap | MobileDelta.Followers | MobileDelta.TithingPoints | MobileDelta.Race)) !=
				0)
			{
				sendPrivateStats = true;
			}

			if ((delta & MobileDelta.Hair) != 0)
			{
				if (m.HairItemID <= 0)
				{
					removeHair = true;
				}

				sendHair = true;
			}

			if ((delta & MobileDelta.FacialHair) != 0)
			{
				if (m.FacialHairItemID <= 0)
				{
					removeFacialHair = true;
				}

				sendFacialHair = true;
			}

            #region Enhance Client
            if ((delta & MobileDelta.Face) != 0)
            {
                if (m.FaceItemID <= 0)
                {
                    removeFace = true;
                }

                sendFace = true;
            }
            #endregion

			var cache = new Packet[][] {new Packet[8], new Packet[8]};

			NetState ourState = m._NetState;

			if (ourState != null)
			{
				if (sendUpdate)
				{
					ourState.Sequence = 0;

					if (ourState.StygianAbyss)
					{
						ourState.Send(new MobileUpdate(m));
					}
					else
					{
						ourState.Send(new MobileUpdateOld(m));
					}

					ClearFastwalkStack();
				}

				if (sendIncoming)
				{
					ourState.Send(MobileIncoming.Create(ourState, m, m));
				}

				if (ourState.StygianAbyss)
				{
					if (sendMoving)
					{
						int noto = Notoriety.Compute(m, m);
						ourState.Send(cache[0][noto] = Packet.Acquire(new MobileMoving(m, noto)));
					}

					if (sendHealthbarPoison)
					{
						ourState.Send(new HealthbarPoison(m));
					}

					if (sendHealthbarYellow)
					{
						ourState.Send(new HealthbarYellow(m));
					}
				}
				else
				{
					if (sendMoving || sendHealthbarPoison || sendHealthbarYellow)
					{
						int noto = Notoriety.Compute(m, m);
						ourState.Send(cache[1][noto] = Packet.Acquire(new MobileMovingOld(m, noto)));
					}
				}

				if (sendPublicStats || sendPrivateStats)
				{
					ourState.Send(new MobileStatusExtended(m, _NetState));
				}
				else if (sendAll)
				{
					ourState.Send(new MobileAttributes(m));
				}
				else if (sendAny)
				{
					if (sendHits)
					{
						ourState.Send(new MobileHits(m));
					}

					if (sendStam)
					{
						ourState.Send(new MobileStam(m));
					}

					if (sendMana)
					{
						ourState.Send(new MobileMana(m));
					}
				}

				if (sendStam || sendMana)
				{
					var ip = _Party as IParty;

					if (ip != null && sendStam)
					{
						ip.OnStamChanged(this);
					}

					if (ip != null && sendMana)
					{
						ip.OnManaChanged(this);
					}
				}

				if (sendHair)
				{
					if (removeHair)
					{
						ourState.Send(new RemoveHair(m));
					}
					else
					{
						ourState.Send(new HairEquipUpdate(m));
					}
				}

				if (sendFacialHair)
				{
					if (removeFacialHair)
					{
						ourState.Send(new RemoveFacialHair(m));
					}
					else
					{
						ourState.Send(new FacialHairEquipUpdate(m));
					}
				}

                #region Enhance Client
                if (sendFace)
                {
                    if (removeFace)
                    {
                        ourState.Send(new RemoveFace(m));
                    }
                    else
                    {
                        ourState.Send(new FaceEquipUpdate(m));
                    }
                }
                #endregion

				if (sendOPLUpdate)
				{
					ourState.Send(OPLPacket);
				}
			}

			sendMoving = sendMoving || sendNonlocalMoving;
			sendIncoming = sendIncoming || sendNonlocalIncoming;
			sendHits = sendHits || sendAll;
            #region Enhance Client
            if (m._Map != null &&
                (sendRemove || sendIncoming || sendPublicStats || sendHits || sendMoving || sendOPLUpdate || sendHair ||
                 sendFacialHair || sendHealthbarPoison || sendHealthbarYellow || sendFace))
            #endregion
            {
				Mobile beholder;

				Packet hitsPacket = null;
				Packet statPacketTrue = null;
				Packet statPacketFalse = null;
				Packet deadPacket = null;
				Packet hairPacket = null;
				Packet facialhairPacket = null;
				Packet hbpPacket = null;
				Packet hbyPacket = null;
                #region Enhance Client
                Packet facePacket = null;
                #endregion

				IPooledEnumerable<NetState> eable = m.Map.GetClientsInRange(m._Location);

				foreach (NetState state in eable)
				{
					beholder = state.Mobile;

					if (beholder != m && beholder.CanSee(m))
					{
						if (sendRemove)
						{
							state.Send(RemovePacket);
						}

						if (sendIncoming)
						{
							state.Send(MobileIncoming.Create(state, beholder, m));

							if (m.IsDeadBondedPet)
							{
								if (deadPacket == null)
								{
									deadPacket = Packet.Acquire(new BondedStatus(0, m._Serial, 1));
								}

								state.Send(deadPacket);
							}
						}

						if (state.StygianAbyss)
						{
							if (sendMoving)
							{
								int noto = Notoriety.Compute(beholder, m);

								Packet p = cache[0][noto];

								if (p == null)
								{
									cache[0][noto] = p = Packet.Acquire(new MobileMoving(m, noto));
								}

								state.Send(p);
							}

							if (sendHealthbarPoison)
							{
								if (hbpPacket == null)
								{
									hbpPacket = Packet.Acquire(new HealthbarPoison(m));
								}

								state.Send(hbpPacket);
							}

							if (sendHealthbarYellow)
							{
								if (hbyPacket == null)
								{
									hbyPacket = Packet.Acquire(new HealthbarYellow(m));
								}

								state.Send(hbyPacket);
							}
						}
						else
						{
							if (sendMoving || sendHealthbarPoison || sendHealthbarYellow)
							{
								int noto = Notoriety.Compute(beholder, m);

								Packet p = cache[1][noto];

								if (p == null)
								{
									cache[1][noto] = p = Packet.Acquire(new MobileMovingOld(m, noto));
								}

								state.Send(p);
							}
						}

						if (sendPublicStats)
						{
							if (m.CanBeRenamedBy(beholder))
							{
								if (statPacketTrue == null)
								{
									statPacketTrue = Packet.Acquire(new MobileStatusCompact(true, m));
								}

								state.Send(statPacketTrue);
							}
							else
							{
								if (statPacketFalse == null)
								{
									statPacketFalse = Packet.Acquire(new MobileStatusCompact(false, m));
								}

								state.Send(statPacketFalse);
							}
						}
						else if (sendHits)
						{
							if (hitsPacket == null)
							{
								hitsPacket = Packet.Acquire(new MobileHitsN(m));
							}

							state.Send(hitsPacket);
						}

						if (sendHair)
						{
							if (hairPacket == null)
							{
								if (removeHair)
								{
									hairPacket = Packet.Acquire(new RemoveHair(m));
								}
								else
								{
									hairPacket = Packet.Acquire(new HairEquipUpdate(m));
								}
							}

							state.Send(hairPacket);
						}

						if (sendFacialHair)
						{
							if (facialhairPacket == null)
							{
								if (removeFacialHair)
								{
									facialhairPacket = Packet.Acquire(new RemoveFacialHair(m));
								}
								else
								{
									facialhairPacket = Packet.Acquire(new FacialHairEquipUpdate(m));
								}
							}

							state.Send(facialhairPacket);
						}

                        #region Enhance Client
                        if (sendFace)
                        {
                            if (facePacket == null)
                            {
                                if (removeFace)
                                {
                                    facePacket = Packet.Acquire(new RemoveFace(m));
                                }
                                else
                                {
                                    facePacket = Packet.Acquire(new FaceEquipUpdate(m));
                                }
                            }

                            state.Send(facePacket);
                        }
                        #endregion

						if (sendOPLUpdate)
						{
							state.Send(OPLPacket);
						}
					}
				}

				Packet.Release(hitsPacket);
				Packet.Release(statPacketTrue);
				Packet.Release(statPacketFalse);
				Packet.Release(deadPacket);
				Packet.Release(hairPacket);
				Packet.Release(facialhairPacket);
				Packet.Release(hbpPacket);
				Packet.Release(hbyPacket);
                #region Enhance Client
                Packet.Release(facePacket);
                #endregion

				eable.Free();
			}

			if (sendMoving || sendNonlocalMoving || sendHealthbarPoison || sendHealthbarYellow)
			{
				for (int i = 0; i < cache.Length; ++i)
				{
					for (int j = 0; j < cache[i].Length; ++j)
					{
						Packet.Release(ref cache[i][j]);
					}
				}
			}
		}

		private static bool _Processing;

		public static void ProcessDeltaQueue()
		{
			_Processing = true;

			if (_DeltaQueue.Count >= 512)
			{
				Parallel.ForEach(_DeltaQueue, m => m.ProcessDelta());
				_DeltaQueue.Clear();
			}
			else
			{
				while (_DeltaQueue.Count > 0)
				{
					_DeltaQueue.Dequeue().ProcessDelta();
				}
			}

			_Processing = false;

			while (_DeltaQueueR.Count > 0)
			{
				_DeltaQueueR.Dequeue().ProcessDelta();
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public int Kills
		{
			get { return _Kills; }
			set
			{
				int oldValue = _Kills;

				if (_Kills != value)
				{
					_Kills = value;

					if (_Kills < 0)
					{
						_Kills = 0;
					}

					if ((oldValue >= 5) != (_Kills >= 5))
					{
						Delta(MobileDelta.Noto);
						InvalidateProperties();
					}

					OnKillsChange(oldValue);
				}
			}
		}

		public virtual void OnKillsChange(int oldValue)
		{ }

		[CommandProperty(AccessLevel.GameMaster)]
		public int ShortTermMurders
		{
			get { return _ShortTermMurders; }
			set
			{
				if (_ShortTermMurders != value)
				{
					_ShortTermMurders = value;

					if (_ShortTermMurders < 0)
					{
						_ShortTermMurders = 0;
					}
				}
			}
		}

		[CommandProperty(AccessLevel.Counselor, AccessLevel.Decorator)]
		public bool Criminal
		{
			get { return _Criminal; }
			set
			{
				if (_Criminal != value)
				{
					_Criminal = value;
					Delta(MobileDelta.Noto);
					InvalidateProperties();
				}

				if (_Criminal)
				{
					if (_ExpireCriminal == null)
					{
						_ExpireCriminal = new ExpireCriminalTimer(this);
					}
					else
					{
						_ExpireCriminal.Stop();
					}

					_ExpireCriminal.Start();
				}
				else if (_ExpireCriminal != null)
				{
					_ExpireCriminal.Stop();
					_ExpireCriminal = null;
				}
			}
		}

		public bool CheckAlive()
		{
			return CheckAlive(true);
		}

		public bool CheckAlive(bool message)
		{
			if (!Alive)
			{
				if (message)
				{
					LocalOverheadMessage(MessageType.Regular, 0x3B2, 1019048); // I am dead and cannot do that.
				}

				return false;
			}
				return true;
			}

		#region Overhead messages
		public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			PublicOverheadMessage(type, hue, ascii, text, true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, bool ascii, string text, bool noLineOfSight)
		{
			if (_Map != null)
			{
				Packet p = null;

				if (ascii)
				{
					p = new AsciiMessage(_Serial, Body, type, hue, 3, Name, text);
				}
				else
				{
					p = new UnicodeMessage(_Serial, Body, type, hue, 3, _Language, Name, text);
				}

				p.Acquire();

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.InLOS(this)))
					{
						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number)
		{
			PublicOverheadMessage(type, hue, number, "", true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, string args)
		{
			PublicOverheadMessage(type, hue, number, args, true);
		}

		public void PublicOverheadMessage(MessageType type, int hue, int number, string args, bool noLineOfSight)
		{
			if (_Map != null)
			{
				Packet p = Packet.Acquire(new MessageLocalized(_Serial, Body, type, hue, 3, number, Name, args));

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.InLOS(this)))
					{
						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PublicOverheadMessage(
			MessageType type, int hue, int number, AffixType affixType, string affix, string args)
		{
			PublicOverheadMessage(type, hue, number, affixType, affix, args, true);
		}

		public void PublicOverheadMessage(
			MessageType type, int hue, int number, AffixType affixType, string affix, string args, bool noLineOfSight)
		{
			if (_Map != null)
			{
				Packet p =
					Packet.Acquire(new MessageLocalizedAffix(_Serial, Body, type, hue, 3, number, Name, affixType, affix, args));

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state.Mobile.CanSee(this) && (noLineOfSight || state.Mobile.InLOS(this)))
					{
						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void PrivateOverheadMessage(MessageType type, int hue, bool ascii, string text, NetState state)
		{
			if (state == null)
			{
				return;
			}

			if (ascii)
			{
				state.Send(new AsciiMessage(_Serial, Body, type, hue, 3, Name, text));
			}
			else
			{
				state.Send(new UnicodeMessage(_Serial, Body, type, hue, 3, _Language, Name, text));
			}
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, NetState state)
		{
			PrivateOverheadMessage(type, hue, number, "", state);
		}

		public void PrivateOverheadMessage(MessageType type, int hue, int number, string args, NetState state)
		{
			if (state == null)
			{
				return;
			}

			state.Send(new MessageLocalized(_Serial, Body, type, hue, 3, number, Name, args));
		}

		public void LocalOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				if (ascii)
				{
					ns.Send(new AsciiMessage(_Serial, Body, type, hue, 3, Name, text));
				}
				else
				{
					ns.Send(new UnicodeMessage(_Serial, Body, type, hue, 3, _Language, Name, text));
				}
			}
		}

		public void LocalOverheadMessage(MessageType type, int hue, int number)
		{
			LocalOverheadMessage(type, hue, number, "");
		}

		public void LocalOverheadMessage(MessageType type, int hue, int number, string args)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				ns.Send(new MessageLocalized(_Serial, Body, type, hue, 3, number, Name, args));
			}
		}

		public void NonlocalOverheadMessage(MessageType type, int hue, int number)
		{
			NonlocalOverheadMessage(type, hue, number, "");
		}

		public void NonlocalOverheadMessage(MessageType type, int hue, int number, string args)
		{
			if (_Map != null)
			{
				Packet p = Packet.Acquire(new MessageLocalized(_Serial, Body, type, hue, 3, number, Name, args));

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state != _NetState && state.Mobile.CanSee(this))
					{
						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}

		public void NonlocalOverheadMessage(MessageType type, int hue, bool ascii, string text)
		{
			if (_Map != null)
			{
				Packet p = null;

				if (ascii)
				{
					p = new AsciiMessage(_Serial, Body, type, hue, 3, Name, text);
				}
				else
				{
					p = new UnicodeMessage(_Serial, Body, type, hue, 3, Language, Name, text);
				}

				p.Acquire();

				IPooledEnumerable<NetState> eable = _Map.GetClientsInRange(_Location);

				foreach (NetState state in eable)
				{
					if (state != _NetState && state.Mobile.CanSee(this))
					{
						state.Send(p);
					}
				}

				Packet.Release(p);

				eable.Free();
			}
		}
		#endregion

		#region SendLocalizedMessage
		public void SendLocalizedMessage(int number)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				ns.Send(MessageLocalized.InstantiateGeneric(number));
			}
		}

		public void SendLocalizedMessage(int number, string args)
		{
			SendLocalizedMessage(number, args, 0x3B2);
		}

		public void SendLocalizedMessage(int number, string args, int hue)
		{
			if (hue == 0x3B2 && (args == null || args.Length == 0))
			{
				NetState ns = _NetState;

				if (ns != null)
				{
					ns.Send(MessageLocalized.InstantiateGeneric(number));
				}
			}
			else
			{
				NetState ns = _NetState;

				if (ns != null)
				{
					ns.Send(new MessageLocalized(Serial.MinusOne, -1, MessageType.Regular, hue, 3, number, "System", args));
				}
			}
		}

		public void SendLocalizedMessage(int number, bool append, string affix)
		{
			SendLocalizedMessage(number, append, affix, "", 0x3B2);
		}

		public void SendLocalizedMessage(int number, bool append, string affix, string args)
		{
			SendLocalizedMessage(number, append, affix, args, 0x3B2);
		}

		public void SendLocalizedMessage(int number, bool append, string affix, string args, int hue)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				ns.Send(
					new MessageLocalizedAffix(
						Serial.MinusOne,
						-1,
						MessageType.Regular,
						hue,
						3,
						number,
						"System",
						(append ? AffixType.Append : AffixType.Prepend) | AffixType.System,
						affix,
						args));
			}
		}
		#endregion

		public void LaunchBrowser(string url)
		{
			if (_NetState != null)
			{
				_NetState.LaunchBrowser(url);
			}
		}

		#region Send[ASCII]Message
		public void SendMessage(string text)
		{
			SendMessage(0x3B2, text);
		}

		public void SendMessage(string format, params object[] args)
		{
			SendMessage(0x3B2, String.Format(format, args));
		}

		public void SendMessage(int hue, string text)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				ns.Send(new UnicodeMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "ENU", "System", text));
			}
		}

		public void SendMessage(int hue, string format, params object[] args)
		{
			SendMessage(hue, String.Format(format, args));
		}

		public void SendAsciiMessage(string text)
		{
			SendAsciiMessage(0x3B2, text);
		}

		public void SendAsciiMessage(string format, params object[] args)
		{
			SendAsciiMessage(0x3B2, String.Format(format, args));
		}

		public void SendAsciiMessage(int hue, string text)
		{
			NetState ns = _NetState;

			if (ns != null)
			{
				ns.Send(new AsciiMessage(Serial.MinusOne, -1, MessageType.Regular, hue, 3, "System", text));
			}
		}

		public void SendAsciiMessage(int hue, string format, params object[] args)
		{
			SendAsciiMessage(hue, String.Format(format, args));
		}
		#endregion

		#region InRange
		public bool InRange(Point2D p, int range)
		{
			return (p.m_X >= (_Location.m_X - range)) && (p.m_X <= (_Location.m_X + range)) &&
				   (p.m_Y >= (_Location.m_Y - range)) && (p.m_Y <= (_Location.m_Y + range));
		}

		public bool InRange(Point3D p, int range)
		{
			return (p.m_X >= (_Location.m_X - range)) && (p.m_X <= (_Location.m_X + range)) &&
				   (p.m_Y >= (_Location.m_Y - range)) && (p.m_Y <= (_Location.m_Y + range));
		}

		public bool InRange(IPoint2D p, int range)
		{
			return (p.X >= (_Location.m_X - range)) && (p.X <= (_Location.m_X + range)) && (p.Y >= (_Location.m_Y - range)) &&
				   (p.Y <= (_Location.m_Y + range));
		}
		#endregion

		public void InitStats(int str, int dex, int intel)
		{
			_Str = str;
			_Dex = dex;
			_Int = intel;

			Hits = HitsMax;
			Stam = StamMax;
			Mana = ManaMax;

			Delta(MobileDelta.Stat | MobileDelta.Hits | MobileDelta.Stam | MobileDelta.Mana);
		}

		public virtual void DisplayPaperdollTo(Mobile to)
		{
			EventSink.InvokePaperdollRequest(new PaperdollRequestEventArgs(to, this));
		}

		private static bool _DisableDismountInWarmode;

		public static bool DisableDismountInWarmode { get { return _DisableDismountInWarmode; } set { _DisableDismountInWarmode = value; } }

		#region OnDoubleClick[..]
		/// <summary>
		///     Overridable. Event invoked when the Mobile is double clicked. By default, this method can either dismount or open the paperdoll.
		///     <seealso cref="CanPaperdollBeOpenedBy" />
		///     <seealso cref="DisplayPaperdollTo" />
		/// </summary>
		public virtual void OnDoubleClick(Mobile from)
		{
			if (this == from && (!_DisableDismountInWarmode || !_Warmode))
			{
				IMount mount = Mount;

				if (mount != null)
				{
					mount.Rider = null;
					return;
				}
			}

			if (CanPaperdollBeOpenedBy(from))
			{
				DisplayPaperdollTo(from);
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile is double clicked by someone who is over N tiles away, where N == Core.GlobalUpdateRange.
		///		<seealso cref="Core.GlobalUpdateRange" />
		///     <seealso cref="OnDoubleClick" />
		/// </summary>
		public virtual void OnDoubleClickOutOfRange(Mobile from)
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when the Mobile is double clicked by someone who can no longer see the Mobile. This may happen, for example, using 'Last Object' after the Mobile has hidden.
		///     <seealso cref="OnDoubleClick" />
		/// </summary>
		public virtual void OnDoubleClickCantSee(Mobile from)
		{ }

		/// <summary>
		///     Overridable. Event invoked when the Mobile is double clicked by someone who is not alive. Similar to
		///     <see
		///         cref="OnDoubleClick" />
		///     , this method will show the paperdoll. It does not, however, provide any dismount functionality.
		///     <seealso cref="OnDoubleClick" />
		/// </summary>
		public virtual void OnDoubleClickDead(Mobile from)
		{
			if (CanPaperdollBeOpenedBy(from))
			{
				DisplayPaperdollTo(from);
			}
		}
		#endregion

		/// <summary>
		///     Overridable. Event invoked when the Mobile requests to open his own paperdoll via the 'Open Paperdoll' macro.
		/// </summary>
		public virtual void OnPaperdollRequest()
		{
			if (CanPaperdollBeOpenedBy(this))
			{
				DisplayPaperdollTo(this);
			}
		}

		private static int _BodyWeight = 14;

		public static int BodyWeight { get { return _BodyWeight; } set { _BodyWeight = value; } }

		/// <summary>
		///     Overridable. Event invoked when <paramref name="from" /> wants to see this Mobile's stats.
		/// </summary>
		/// <param name="from"></param>
		public virtual void OnStatsQuery(Mobile from)
		{
			if (from.Map == Map && Utility.InUpdateRange(this, from) && from.CanSee(this))
			{
				from.Send(new MobileStatus(from, this, _NetState));
			}

			if (from == this)
			{
				Send(new StatLockInfo(this));
			}

			var ip = _Party as IParty;

			if (ip != null)
			{
				ip.OnStatsQuery(from, this);
			}
		}

		/// <summary>
		///     Overridable. Event invoked when <paramref name="from" /> wants to see this Mobile's skills.
		/// </summary>
		public virtual void OnSkillsQuery(Mobile from)
		{
			if (from == this)
			{
				Send(new SkillUpdate(_Skills));
			}
		}

		/// <summary>
		///     Overridable. Virtual event invoked when <see cref="Region" /> changes.
		/// </summary>
		public virtual void OnRegionChange(Region Old, Region New)
		{ }

		private Item _MountItem;

		[CommandProperty(AccessLevel.Decorator)]
		public IMount Mount
		{
			get
			{
				IMountItem mountItem = null;

				if (_MountItem != null && !_MountItem.Deleted && _MountItem.Parent == this)
				{
					mountItem = (IMountItem)_MountItem;
				}

				if (mountItem == null)
				{
					_MountItem = (mountItem = (FindItemOnLayer(Layer.Mount) as IMountItem)) as Item;
				}

				return mountItem == null ? null : mountItem.Mount;
			}
		}

		[CommandProperty(AccessLevel.Decorator)]
		public bool Mounted { get { return (Mount != null); } }

		private QuestArrow _QuestArrow;

		public QuestArrow QuestArrow
		{
			get { return _QuestArrow; }
			set
			{
				if (_QuestArrow != value)
				{
					if (_QuestArrow != null)
					{
						_QuestArrow.Stop();
					}

					_QuestArrow = value;
				}
			}
		}

		private static readonly string[] _GuildTypes = {"", " (Chaos)", " (Order)"};

		public virtual bool CanTarget { get { return true; } }
		public virtual bool ClickTitle { get { return true; } }

		public virtual bool PropertyTitle { get { return _OldPropertyTitles ? ClickTitle : true; } }

		private static bool _DisableHiddenSelfClick = true;
		private static bool _AsciiClickMessage = true;
		private static bool _GuildClickMessage = true;
		private static bool _OldPropertyTitles;

		public static bool DisableHiddenSelfClick { get { return _DisableHiddenSelfClick; } set { _DisableHiddenSelfClick = value; } }
		public static bool AsciiClickMessage { get { return _AsciiClickMessage; } set { _AsciiClickMessage = value; } }
		public static bool GuildClickMessage { get { return _GuildClickMessage; } set { _GuildClickMessage = value; } }
		public static bool OldPropertyTitles { get { return _OldPropertyTitles; } set { _OldPropertyTitles = value; } }

		public virtual bool ShowFameTitle { get { return true; } } //(_Player || _Body.IsHuman) && _Fame >= 10000; } 

		/// <summary>
		///     Overridable. Event invoked when the Mobile is single clicked.
		/// </summary>
		public virtual void OnSingleClick(Mobile from)
		{
			if (_Deleted)
			{
				return;
			}
		    if (IsPlayer() && DisableHiddenSelfClick && Hidden && @from == this)
			{
				return;
			}

		    if (_GuildClickMessage)
			{
				BaseGuild guild = _Guild;

				if (guild != null && (_DisplayGuildTitle || (_Player && guild.Type != GuildType.Regular)))
				{
					string title = GuildTitle;
					string type;

					if (title == null)
					{
						title = "";
					}
					else
					{
						title = title.Trim();
					}

					if (guild.Type >= 0 && (int)guild.Type < _GuildTypes.Length)
					{
						type = _GuildTypes[(int)guild.Type];
					}
					else
					{
						type = "";
					}

					string text = String.Format(title.Length <= 0 ? "[{1}]{2}" : "[{0}, {1}]{2}", title, guild.Abbreviation, type);

					PrivateOverheadMessage(MessageType.Regular, SpeechHue, true, text, from.NetState);
				}
			}

			int hue;

			if (_NameHue != -1)
			{
				hue = _NameHue;
			}
			else if (IsStaff())
			{
				hue = 11;
			}
			else
			{
				hue = Notoriety.GetHue(Notoriety.Compute(from, this));
			}

			string name = Name;

			if (name == null)
			{
				name = String.Empty;
			}

			string prefix = "";

			if (ShowFameTitle && (_Player || _Body.IsHuman) && _Fame >= 10000)
			{
				prefix = (_Female ? "Lady" : "Lord");
			}

			string suffix = "";

			if (ClickTitle && Title != null && Title.Length > 0)
			{
				suffix = Title;
			}

			suffix = ApplyNameSuffix(suffix);

			string val;

			if (prefix.Length > 0 && suffix.Length > 0)
			{
				val = String.Concat(prefix, " ", name, " ", suffix);
			}
			else if (prefix.Length > 0)
			{
				val = String.Concat(prefix, " ", name);
			}
			else if (suffix.Length > 0)
			{
				val = String.Concat(name, " ", suffix);
			}
			else
			{
				val = name;
			}

			PrivateOverheadMessage(MessageType.Label, hue, _AsciiClickMessage, val, from.NetState);
		}

		public bool CheckSkill(SkillName skill, double minSkill, double maxSkill)
		{
		    if (_SkillCheckLocationHandler == null)
			{
				return false;
			}
		    return _SkillCheckLocationHandler(this, skill, minSkill, maxSkill);
			}

		public bool CheckSkill(SkillName skill, double chance)
		{
	        if (_SkillCheckDirectLocationHandler == null)
			{
				return false;
			}
	        return _SkillCheckDirectLocationHandler(this, skill, chance);
			}

		public bool CheckTargetSkill(SkillName skill, object target, double minSkill, double maxSkill)
		{
	        if (_SkillCheckTargetHandler == null)
			{
				return false;
			}
	        return _SkillCheckTargetHandler(this, skill, target, minSkill, maxSkill);
			}

		public bool CheckTargetSkill(SkillName skill, object target, double chance)
		{
	        if (_SkillCheckDirectTargetHandler == null)
			{
				return false;
			}
	        return _SkillCheckDirectTargetHandler(this, skill, target, chance);
			}

		public virtual void DisruptiveAction()
		{
			if (Meditating)
			{
				Meditating = false;
				SendLocalizedMessage(500134); // You stop meditating.
			}
		}

		#region Armor
		public Item ShieldArmor { get { return FindItemOnLayer(Layer.TwoHanded); } }

		public Item NeckArmor { get { return FindItemOnLayer(Layer.Neck); } }

		public Item HandArmor { get { return FindItemOnLayer(Layer.Gloves); } }

		public Item HeadArmor { get { return FindItemOnLayer(Layer.Helm); } }

		public Item ArmsArmor { get { return FindItemOnLayer(Layer.Arms); } }

		public Item LegsArmor
		{
			get
			{
				Item ar = FindItemOnLayer(Layer.InnerLegs);

				if (ar == null)
				{
					ar = FindItemOnLayer(Layer.Pants);
				}

				return ar;
			}
		}

		public Item ChestArmor
		{
			get
			{
				Item ar = FindItemOnLayer(Layer.InnerTorso);

				if (ar == null)
				{
					ar = FindItemOnLayer(Layer.Shirt);
				}

				return ar;
			}
		}

		public Item Talisman { get { return FindItemOnLayer(Layer.Talisman); } }
		#endregion

		/// <summary>
		///     Gets or sets the maximum attainable value for <see cref="RawStr" />, <see cref="RawDex" />, and <see cref="RawInt" />.
		/// </summary>
		[CommandProperty(AccessLevel.GameMaster)]
		public int StatCap
		{
			get { return _StatCap; }
			set
			{
				if (_StatCap != value)
				{
					_StatCap = value;

					Delta(MobileDelta.StatCap);
				}
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public bool Meditating { get; set; }

		[CommandProperty(AccessLevel.Decorator)]
		public bool CanSwim { get { return _CanSwim; } set { _CanSwim = value; } }

		[CommandProperty(AccessLevel.Decorator)]
		public bool CantWalk { get { return _CantWalk; } set { _CantWalk = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public bool CanHearGhosts { get { return _CanHearGhosts || IsStaff(); } set { _CanHearGhosts = value; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public int RawStatTotal { get { return RawStr + RawDex + RawInt; } }

		public long NextSpellTime { get; set; }

		/// <summary>
		///     Overridable. Virtual event invoked when the sector this Mobile is in gets <see cref="Sector.Activate">activated</see>.
		/// </summary>
		public virtual void OnSectorActivate()
		{ }

		/// <summary>
		///     Overridable. Virtual event invoked when the sector this Mobile is in gets <see cref="Sector.Deactivate">deactivated</see>.
		/// </summary>
		public virtual void OnSectorDeactivate()
		{ }
	}
}
