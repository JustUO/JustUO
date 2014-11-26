#region Header
//   Vorspire    _,-'/-'/  MobileExt.cs
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

using Server.Mobiles;
using Server.Targeting;

using VitaNex;
using VitaNex.Notify;
using VitaNex.Targets;
#endregion

namespace Server
{
	public static class MobileExtUtility
	{
		public static TimeSpan CombatHeatDelay = TimeSpan.FromSeconds(5.0);

		public static bool InCombat(this Mobile m)
		{
			return InCombat(m, CombatHeatDelay);
		}

		public static bool InCombat(this Mobile m, TimeSpan heat)
		{
			if (m == null || m.Deleted)
			{
				return false;
			}

			if (m.Combatant != null && !m.Combatant.Deleted)
			{
				return true;
			}

			if (heat <= TimeSpan.Zero)
			{
				return false;
			}

			var now = DateTime.Now;
			var utc = DateTime.UtcNow;

			if (
				m.Aggressors.Any(info => info.LastCombatTime + heat >= (info.LastCombatTime.Kind == DateTimeKind.Utc ? utc : now)) ||
				m.Aggressed.Any(info => info.LastCombatTime + heat >= (info.LastCombatTime.Kind == DateTimeKind.Utc ? utc : now)))
			{
				return true;
			}

			var d = m.FindMostRecentDamageEntry(false);

			return d != null && (d.LastDamage + heat >= (d.LastDamage.Kind == DateTimeKind.Utc ? utc : now));
		}

		public static bool IsControlled(this Mobile m)
		{
			Mobile master;
			return IsControlled(m, out master);
		}

		public static bool IsControlled(this Mobile m, out Mobile master)
		{
			if (m is BaseCreature)
			{
				BaseCreature c = (BaseCreature)m;
				master = c.GetMaster();
				return master != null && (c.Controlled || c.Summoned);
			}

			master = null;
			return false;
		}

		public static void TryParalyze(this Mobile m, TimeSpan duration, TimerStateCallback<Mobile> callback = null)
		{
			m.Paralyze(duration);

			if (callback != null)
			{
				Timer.DelayCall(duration, callback, m);
			}
		}

		public static void TryFreeze(this Mobile m, TimeSpan duration, TimerStateCallback<Mobile> callback = null)
		{
			m.Freeze(duration);

			if (callback != null)
			{
				Timer.DelayCall(duration, callback, m);
			}
		}

		private static readonly MethodInfo _SleepImpl = typeof(Mobile).GetMethod("Sleep") ??
														typeof(Mobile).GetMethod("DoSleep");

		public static void TrySleep(this Mobile m, TimeSpan duration, TimerStateCallback<Mobile> callback = null)
		{
			if (_SleepImpl != null)
			{
				VitaNexCore.TryCatch(
					() =>
					{
						_SleepImpl.Invoke(m, new object[] {duration});

						if (callback != null)
						{
							Timer.DelayCall(duration, callback, m);
						}
					});
			}
		}

		public static void SendNotification(
			this Mobile m,
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<NotifyGump> beforeSend = null,
			Action<NotifyGump> afterSend = null)
		{
			if (m is PlayerMobile)
			{
				Notify.Send((PlayerMobile)m, html, autoClose, delay, pause, color, beforeSend, afterSend);
			}
		}

		public static void SendNotification<TGump>(
			this Mobile m,
			string html,
			bool autoClose = true,
			double delay = 1.0,
			double pause = 3.0,
			Color? color = null,
			Action<TGump> beforeSend = null,
			Action<TGump> afterSend = null) where TGump : NotifyGump
		{
			if (m is PlayerMobile)
			{
				Notify.Send((PlayerMobile)m, html, autoClose, delay, pause, color, beforeSend, afterSend);
			}
		}

		public static int GetNotorietyHue(this Mobile source, Mobile target = null)
		{
			return ComputeNotoriety(source, target).GetHue();
		}

		public static Color GetNotorietyColor(this Mobile source, Mobile target = null)
		{
			return ComputeNotoriety(source, target).GetColor();
		}

		public static NotorietyType ComputeNotoriety(this Mobile source, Mobile target = null)
		{
			if (source == null && target != null)
			{
				source = target;
			}

			if (source != null)
			{
				return (NotorietyType)Notoriety.Compute(source, target ?? source);
			}

			return NotorietyType.None;
		}

		public static void Control(this BaseCreature pet, Mobile master)
		{
			if (pet == null || pet.Deleted || pet.IsStabled || master == null || master.Deleted)
			{
				return;
			}

			pet.CurrentWayPoint = null;

			pet.ControlMaster = master;
			pet.Controlled = true;
			pet.ControlTarget = null;
			pet.ControlOrder = OrderType.Come;
			pet.Guild = null;

			pet.Delta(MobileDelta.Noto);
		}

		public static bool Stable(this BaseCreature pet, bool maxLoyalty = true, bool autoStable = true)
		{
			if (pet == null || pet.Deleted || pet.IsStabled || !(pet.ControlMaster is PlayerMobile))
			{
				return false;
			}

			var master = (PlayerMobile)pet.ControlMaster;

			pet.ControlTarget = null;
			pet.ControlOrder = OrderType.None;
			pet.Internalize();

			pet.SetControlMaster(null);
			pet.SummonMaster = null;

			pet.IsStabled = true;

			if (maxLoyalty)
			{
				pet.Loyalty = BaseCreature.MaxLoyalty; // Wonderfully happy
			}

			master.Stabled.Add(pet);

			if (autoStable)
			{
				master.AutoStabled.Add(pet);
			}

			return true;
		}

		/// <summary>
		///     Begin targeting for the specified Mobile with definded handlers
		/// </summary>
		/// <param name="m">Mobile owner of the new GenericSelectTarget instance</param>
		/// <param name="success">Success callback</param>
		/// <param name="fail">Failure callback</param>
		/// <param name="range">Maximum distance allowed</param>
		/// <param name="allowGround">Allow ground as valid target</param>
		/// <param name="flags">Target flags determine the target action</param>
		public static GenericSelectTarget<TObj> BeginTarget<TObj>(
			this Mobile m,
			Action<Mobile, TObj> success,
			Action<Mobile> fail,
			int range = -1,
			bool allowGround = false,
			TargetFlags flags = TargetFlags.None)
		{
			if (m == null || m.Deleted)
			{
				return null;
			}

			var t = new GenericSelectTarget<TObj>(success, fail, range, allowGround, flags);

			m.Target = t;

			return t;
		}

		public static TMobile GetLastKiller<TMobile>(this Mobile m, bool petMaster = false) where TMobile : Mobile
		{
			if (m == null || m.LastKiller == null)
			{
				return null;
			}

			var killer = m.LastKiller as TMobile;

			if (killer == null && petMaster && m.LastKiller is BaseCreature)
			{
				killer = ((BaseCreature)m.LastKiller).GetMaster<TMobile>();
			}

			return killer;
		}

		public static void GetEquipment(this Mobile m, out Item[] equip, out int slots)
		{
			equip = GetEquipment(m);
			slots = GetEquipmentSlots(equip);
		}

		public static Item[] GetEquipment(this Mobile m)
		{
			return m == null ? new Item[0] : m.Items.Where(i => i.Layer.IsEquip()).ToArray();
		}

		public static int GetEquipmentSlotsMax(this Mobile m)
		{
			var max = LayerExtUtility.EquipLayers.Length - 2;
				// -2 for InnerLegs and OuterLegs, because nothing uses the layers yet...

			if (m == null)
			{
				return max;
			}

			var equip = GetEquipment(m);

			foreach (var i in equip)
			{
				if (i.Layer == Layer.InnerLegs || i.Layer == Layer.OuterLegs)
				{
					// If they have an item with InnerLegs or OuterLegs, increase max slots by 1.
					++max;
				}
				else if (i.Layer == Layer.TwoHanded && i is IWeapon)
				{
					// Offset max by -1 if they have a TwoHanded weapon (which takes up the shield slot)
					--max;
				}
			}

			return max;
		}

		public static int GetEquipmentSlots(this Mobile m)
		{
			return m == null ? 0 : GetEquipmentSlots(GetEquipment(m));
		}

		private static int GetEquipmentSlots(params Item[] items)
		{
			return items == null || items.Length == 0 ? 0 : items.Sum(i => i.Layer == Layer.TwoHanded && i is IWeapon ? 2 : 1);
		}

		public static TItem FindItemOnLayer<TItem>(this Mobile m, Layer layer) where TItem : Item
		{
			return m == null ? null : m.FindItemOnLayer(layer) as TItem;
		}

		public static bool HasItem<TItem>(
			this Mobile m, int amount = 1, bool children = true, Func<TItem, bool> predicate = null) where TItem : Item
		{
			return predicate == null
					   ? HasItem(m, typeof(TItem), amount, children)
					   : HasItem(m, typeof(TItem), amount, children, i => predicate(i as TItem));
		}

		public static bool HasItem(
			this Mobile m, Type type, int amount = 1, bool children = true, Func<Item, bool> predicate = null)
		{
			if (m == null || type == null || amount < 1)
			{
				return false;
			}

			long total = 0;

			total =
				m.Items.Where(i => i != null && !i.Deleted && i.TypeEquals(type, children) && (predicate == null || predicate(i)))
				 .Aggregate(total, (c, i) => c + (long)i.Amount);

			if (m.Backpack != null)
			{
				total =
					m.Backpack.FindItemsByType(type, true)
					 .Where(i => i != null && !i.Deleted && i.TypeEquals(type, children) && (predicate == null || predicate(i)))
					 .Aggregate(total, (c, i) => c + (long)i.Amount);
			}

			return total >= amount;
		}

		public static bool HasItems(
			this Mobile m, Type[] types, int[] amounts = null, bool children = true, Func<Item, bool> predicate = null)
		{
			if (m == null || types == null || types.Length == 0)
			{
				return false;
			}

			if (amounts == null)
			{
				amounts = new int[0];
			}

			int count = 0;

			for (int i = 0; i < types.Length; i++)
			{
				var t = types[i];
				int amount = amounts.InBounds(i) ? amounts[i] : 1;

				if (HasItem(m, t, amount, children, predicate))
				{
					++count;
				}
			}

			return count >= types.Length;
		}

		public static TItem FindItemByType<TItem>(this Mobile m) where TItem : Item
		{
			if (m == null)
			{
				return null;
			}

			if (m.Holding is TItem)
			{
				return (TItem)m.Holding;
			}

			var item = m.Items.OfType<TItem>().FirstOrDefault();

			if (item == null)
			{
				var p = m.Backpack;
				var b = m.FindBankNoCreate();

				item = (p != null ? p.FindItemByType<TItem>() : null) ?? (b != null ? b.FindItemByType<TItem>() : null);
			}

			return item;
		}

		public static TItem FindItemByType<TItem>(this Mobile m, bool recurse) where TItem : Item
		{
			if (m == null)
			{
				return null;
			}

			if (m.Holding is TItem)
			{
				return (TItem)m.Holding;
			}

			var item = m.Items.OfType<TItem>().FirstOrDefault();

			if (item == null)
			{
				var p = m.Backpack;
				var b = m.FindBankNoCreate();

				item = (p != null ? p.FindItemByType<TItem>(recurse) : null) ??
					   (b != null ? b.FindItemByType<TItem>(recurse) : null);
			}

			return item;
		}

		public static TItem FindItemByType<TItem>(this Mobile m, bool recurse, Predicate<TItem> predicate) where TItem : Item
		{
			if (m == null)
			{
				return null;
			}

			if (m.Holding is TItem)
			{
				return (TItem)m.Holding;
			}

			var item = m.Items.OfType<TItem>().FirstOrDefault(i => predicate(i));

			if (item == null)
			{
				var p = m.Backpack;
				var b = m.FindBankNoCreate();

				item = (p != null ? p.FindItemByType(recurse, predicate) : null) ??
					   (b != null ? b.FindItemByType(recurse, predicate) : null);
			}

			return item;
		}

		public static List<TItem> FindItemsByType<TItem>(this Mobile m) where TItem : Item
		{
			var items = new List<TItem>();

			if (m != null)
			{
				if (m.Holding is TItem)
				{
					items.Add((TItem)m.Holding);
				}

				items.AddRange(m.Items.OfType<TItem>());

				var p = m.Backpack;
				var b = m.FindBankNoCreate();

				if (p != null)
				{
					items.AddRange(p.FindItemsByType<TItem>());
				}

				if (b != null)
				{
					items.AddRange(b.FindItemsByType<TItem>());
				}
			}

			return items;
		}

		public static List<TItem> FindItemsByType<TItem>(this Mobile m, bool recurse) where TItem : Item
		{
			var items = new List<TItem>();

			if (m != null)
			{
				if (m.Holding is TItem)
				{
					items.Add((TItem)m.Holding);
				}

				items.AddRange(m.Items.OfType<TItem>());

				var p = m.Backpack;
				var b = m.FindBankNoCreate();

				if (p != null)
				{
					items.AddRange(p.FindItemsByType<TItem>(recurse));
				}

				if (b != null)
				{
					items.AddRange(b.FindItemsByType<TItem>(recurse));
				}
			}

			return items;
		}

		public static List<TItem> FindItemsByType<TItem>(this Mobile m, bool recurse, Predicate<TItem> predicate)
			where TItem : Item
		{
			var items = new List<TItem>();

			if (m != null)
			{
				if (m.Holding is TItem)
				{
					items.Add((TItem)m.Holding);
				}

				items.AddRange(m.Items.OfType<TItem>().Where(i => predicate(i)));

				var p = m.Backpack;
				var b = m.FindBankNoCreate();

				if (p != null)
				{
					items.AddRange(p.FindItemsByType(recurse, predicate));
				}

				if (b != null)
				{
					items.AddRange(b.FindItemsByType(recurse, predicate));
				}
			}

			return items;
		}

		public static List<TItem> GetEquippedItems<TItem>(this Mobile m) where TItem : Item
		{
			return FindEquippedItems<TItem>(m).ToList();
		}

		public static IEnumerable<TItem> FindEquippedItems<TItem>(this Mobile m) where TItem : Item
		{
			if (m == null)
			{
				yield break;
			}

			foreach (var item in m.Items.OfType<TItem>().Where(i => i.Parent == m))
			{
				yield return item;
			}
		}

		public static void Dismount(this Mobile m, Mobile f = null)
		{
			Dismount(m, BlockMountType.None, f);
		}

		public static void Dismount(this Mobile m, BlockMountType type, Mobile f = null)
		{
			Dismount(m, type, TimeSpan.Zero, f);
		}

		public static void Dismount(this Mobile m, BlockMountType type, TimeSpan duration, Mobile f = null)
		{
			SetMountBlock(m, type, duration, true, f);
		}

		private static readonly MethodInfo _BaseMountBlock = typeof(BaseMount).GetMethod(
			"SetMountPrevention", BindingFlags.Static | BindingFlags.Public);
		private static readonly MethodInfo _PlayerMountBlock = typeof(PlayerMobile).GetMethod(
			"SetMountPrevention", BindingFlags.Instance | BindingFlags.Public);

		public static void SetMountBlock(
			this Mobile m, BlockMountType type, TimeSpan duration, bool dismount, Mobile f = null)
		{
			if (m == null)
			{
				return;
			}

			if (dismount && m.Mounted)
			{
				m.Mount.Rider = null;
			}

			if (m is PlayerMobile && _PlayerMountBlock != null)
			{
				_PlayerMountBlock.Invoke(m, new object[] {type, duration, dismount});
				return;
			}

			if (_BaseMountBlock != null)
			{
				_BaseMountBlock.Invoke(null, new object[] {m, type, duration});
			}
		}

		public static void SetAllSkills(this Mobile m, double val)
		{
			if (m == null || m.Skills == null)
			{
				return;
			}

			val = Math.Max(0.0, val);

			foreach (Skill skill in m.Skills)
			{
				if (skill.Cap < val)
				{
					skill.SetCap(val);
				}

				skill.SetBase(val, true, false);
			}
		}

		public static void SetAllSkills(this Mobile m, double val, double cap)
		{
			if (m == null || m.Skills == null)
			{
				return;
			}

			val = Math.Max(0.0, val);
			cap = Math.Max(val, cap);

			foreach (Skill skill in m.Skills)
			{
				skill.SetCap(cap);
				skill.SetBase(val, true, false);
			}
		}

		public static int GetGumpID(this Mobile m)
		{
			int val = -1;

			if (m.Body.IsHuman)
			{
				if (m.Race == Race.Gargoyle)
				{
					val = m.Female ? 665 : 666;
				}
				else if (m.Race == Race.Elf)
				{
					val = m.Female ? 15 : 14;
				}
				else
				{
					val = m.Female ? 13 : 12;
				}
			}

			return val;
		}
	}
}