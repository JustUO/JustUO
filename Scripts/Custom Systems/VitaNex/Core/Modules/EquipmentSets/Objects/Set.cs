#region Header
//   Vorspire    _,-'/-'/  Set.cs
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
using System.Collections;
using System.Collections.Generic;
using System.Linq;

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public abstract class EquipmentSet : PropertyObject, IEnumerable<EquipmentSetPart>
	{
		public static Item[] GenerateParts<TSet>() where TSet : EquipmentSet
		{
			return GenerateParts(typeof(TSet));
		}

		public static Item[] GenerateParts(Type set)
		{
			var s = EquipmentSets.Sets.Values.FirstOrDefault(t => t.TypeEquals(set));

			if (s != null)
			{
				return s.GenerateParts();
			}

			return new Item[0];
		}

		private readonly List<Mobile> _ActiveOwners = new List<Mobile>();

		public List<Mobile> ActiveOwners { get { return _ActiveOwners; } }

		public List<EquipmentSetPart> Parts { get; protected set; }
		public List<EquipmentSetMod> Mods { get; protected set; }

		public EquipmentSetPart this[int index] { get { return Parts[index]; } set { Parts[index] = value; } }

		[CommandProperty(EquipmentSets.Access)]
		public int Count { get { return Parts.Count; } }

		[CommandProperty(EquipmentSets.Access)]
		public string Name { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool Display { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool DisplayParts { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool DisplayMods { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool Valid { get { return Validate(); } }

		public EquipmentSet(
			string name,
			bool display = true,
			bool displayParts = true,
			bool displayMods = true,
			IEnumerable<EquipmentSetPart> parts = null,
			IEnumerable<EquipmentSetMod> mods = null)
		{
			Name = name;
			Display = display;
			DisplayParts = displayParts;
			DisplayMods = displayMods;
			Parts = parts != null ? new List<EquipmentSetPart>(parts) : new List<EquipmentSetPart>();
			Mods = mods != null ? new List<EquipmentSetMod>(mods) : new List<EquipmentSetMod>();
		}

		public EquipmentSet(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

		public IEnumerator<EquipmentSetPart> GetEnumerator()
		{
			return Parts.GetEnumerator();
		}

		public bool Contains(EquipmentSetPart part)
		{
			return Parts.Contains(part);
		}

		public void Add(EquipmentSetPart part)
		{
			Parts.Add(part);
		}

		public void AddRange(IEnumerable<EquipmentSetPart> parts)
		{
			Parts.AddRange(parts);
		}

		public bool Remove(EquipmentSetPart part)
		{
			return Parts.Remove(part);
		}

		public bool Contains(EquipmentSetMod mod)
		{
			return Mods.Contains(mod);
		}

		public void Add(EquipmentSetMod mod)
		{
			Mods.Add(mod);
		}

		public void AddRange(IEnumerable<EquipmentSetMod> mods)
		{
			Mods.AddRange(mods);
		}

		public bool Remove(EquipmentSetMod mod)
		{
			return Mods.Remove(mod);
		}

		public bool HasPartTypeOf(Type type)
		{
			return Parts.Any(part => part.Valid && part.IsTypeOf(type));
		}

		public IEnumerable<Tuple<EquipmentSetPart, Item>> FindEquippedParts(Mobile m)
		{
			foreach (var part in Parts.Where(p => p.Valid))
			{
				Item item;

				if (part.IsEquipped(m, out item))
				{
					yield return Tuple.Create(part, item);
				}
			}
		}

		public Tuple<EquipmentSetPart, Item>[] GetEquippedParts(Mobile m)
		{
			return FindEquippedParts(m).ToArray();
		}

		public IEnumerable<EquipmentSetMod> FindAvailableMods(Mobile m, EquipmentSetPart[] equipped)
		{
			return Mods.Where(mod => mod.Valid && equipped.Length >= mod.PartsRequired);
		}

		public EquipmentSetMod[] GetAvailableMods(Mobile m, EquipmentSetPart[] equipped)
		{
			return FindAvailableMods(m, equipped).ToArray();
		}

		public Item[] GenerateParts()
		{
			return Parts.Select(part => part.CreateInstanceOfPart()).Not(item => item == null || item.Deleted).ToArray();
		}

		public void Invalidate(Mobile m, Item item)
		{
			int totalActive = 0;

			Type type = item.GetType();
			var changedPart = Tuple.Create(Parts.FirstOrDefault(p => p.IsTypeOf(type)), item);
			var equippedParts = GetEquippedParts(m);

			foreach (EquipmentSetMod mod in Mods.Where(sm => sm.Valid))
			{
				if (mod.IsActive(m))
				{
					++totalActive;

					if (equippedParts.Length < mod.PartsRequired && Deactivate(m, equippedParts, changedPart, mod))
					{
						--totalActive;
					}
				}
				else
				{
					if (equippedParts.Length >= mod.PartsRequired && Activate(m, equippedParts, changedPart, mod))
					{
						++totalActive;
					}
				}
			}

			if (EquipmentSets.CMOptions.ModuleDebug)
			{
				EquipmentSets.CMOptions.ToConsole("Mods: {0}, Parts: {1}", totalActive, equippedParts.Length);
			}

			SetActiveOwner(m, totalActive > 0);
			InvalidateAllProperties(m, equippedParts.Select(t => t.Item2).ToArray(), changedPart.Item2);
		}

		public void InvalidateAllProperties(Mobile m, Item[] equipped, Item changed)
		{
			if (m == null || m.Deleted || m.Map == null || m.Map == Map.Internal)
			{
				return;
			}

			m.InvalidateProperties();

			if (equipped != null && equipped.Length > 0)
			{
				foreach (Item item in equipped.Where(item => item != changed))
				{
					InvalidateItemProperties(item);
				}
			}

			if (changed != null)
			{
				InvalidateItemProperties(changed);
			}
		}

		public void InvalidateItemProperties(Item item)
		{
			if (item != null && !item.Deleted)
			{
				item.InvalidateProperties();
			}
		}

		private void SetActiveOwner(Mobile m, bool state)
		{
			if (state)
			{
				ActiveOwners.AddOrReplace(m);
			}
			else
			{
				ActiveOwners.Remove(m);
			}
		}

		public virtual bool Validate()
		{
			return Mods != null && !String.IsNullOrWhiteSpace(Name);
		}

		public bool Activate(
			Mobile m, Tuple<EquipmentSetPart, Item>[] equipped, Tuple<EquipmentSetPart, Item> added, EquipmentSetMod mod)
		{
			return OnActivate(m, equipped, added, mod) && mod.Activate(m, equipped);
		}

		public bool Deactivate(
			Mobile m, Tuple<EquipmentSetPart, Item>[] equipped, Tuple<EquipmentSetPart, Item> added, EquipmentSetMod mod)
		{
			return OnDeactivate(m, equipped, added, mod) && mod.Deactivate(m, equipped);
		}

		protected virtual bool OnActivate(
			Mobile m, Tuple<EquipmentSetPart, Item>[] equipped, Tuple<EquipmentSetPart, Item> added, EquipmentSetMod mod)
		{
			return m != null && !m.Deleted && equipped != null && mod != null && !mod.IsActive(m);
		}

		protected virtual bool OnDeactivate(
			Mobile m, Tuple<EquipmentSetPart, Item>[] equipped, Tuple<EquipmentSetPart, Item> removed, EquipmentSetMod mod)
		{
			return m != null && !m.Deleted && equipped != null && mod != null && mod.IsActive(m);
		}

		public virtual void GetProperties(Mobile viewer, ExtendedOPL list, bool equipped)
		{
			if (!equipped)
			{
				list.Add(
					"{0} [{1:#,0}]".WrapUOHtmlColor(EquipmentSets.CMOptions.SetNameColorRaw),
					Name.ToUpperWords(),
					Parts.Count(p => p.Valid));
			}
			else
			{
				list.Add(
					"{0} [{1:#,0} / {2:#,0}]".WrapUOHtmlColor(EquipmentSets.CMOptions.SetNameColorRaw),
					Name.ToUpperWords(),
					GetEquippedParts(viewer).Length,
					Parts.Count(p => p.Valid));
			}
		}

		public override string ToString()
		{
			return String.Format("{0}", Name);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Name);
						writer.Write(Display);
						writer.Write(DisplayParts);
						writer.Write(DisplayMods);

						writer.WriteList(
							Parts,
							p => writer.WriteType(
								p,
								t =>
								{
									if (t != null)
									{
										p.Serialize(writer);
									}
								}));

						writer.WriteList(
							Mods,
							m => writer.WriteType(
								m,
								t =>
								{
									if (t != null)
									{
										m.Serialize(writer);
									}
								}));
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 0:
					{
						Name = reader.ReadString();
						Display = reader.ReadBool();
						DisplayParts = reader.ReadBool();
						DisplayMods = reader.ReadBool();

						Parts = reader.ReadList(() => reader.ReadTypeCreate<EquipmentSetPart>(reader));
						Mods = reader.ReadList(() => reader.ReadTypeCreate<EquipmentSetMod>(reader));
					}
					break;
			}
		}
	}
}