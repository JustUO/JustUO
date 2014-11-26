#region Header
//   Vorspire    _,-'/-'/  Mod.cs
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

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public abstract class EquipmentSetMod : PropertyObject
	{
		private readonly List<Mobile> _ActiveOwners = new List<Mobile>();

		public List<Mobile> ActiveOwners { get { return _ActiveOwners; } }

		[CommandProperty(EquipmentSets.Access)]
		public string Name { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public string Desc { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public int PartsRequired { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool Display { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool Valid { get { return Validate(); } }

		public EquipmentSetMod(string name = "Set Mod", string desc = null, int partsReq = 1, bool display = true)
		{
			Name = name;
			Desc = desc ?? String.Empty;
			Display = display;
			PartsRequired = partsReq;
		}

		public EquipmentSetMod(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		public bool IsActive(Mobile m)
		{
			return m != null && ActiveOwners.Contains(m);
		}

		public bool Activate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			if (OnActivate(m, equipped))
			{
				if (!ActiveOwners.Contains(m))
				{
					ActiveOwners.Add(m);
				}

				return true;
			}

			return false;
		}

		public bool Deactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			return OnDeactivate(m, equipped) && ActiveOwners.Remove(m);
		}

		protected abstract bool OnActivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped);
		protected abstract bool OnDeactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped);

		public virtual bool Validate()
		{
			return !String.IsNullOrWhiteSpace(Name);
		}

		public virtual void GetProperties(Mobile viewer, ExtendedOPL list, bool equipped)
		{
			if (!String.IsNullOrEmpty(Desc))
			{
				list.Add(
					"[{0:#,0}] {1}: {2}".WrapUOHtmlColor(
						equipped && IsActive(viewer) ? EquipmentSets.CMOptions.ModNameColorRaw : EquipmentSets.CMOptions.InactiveColorRaw),
					PartsRequired,
					Name.ToUpperWords(),
					Desc);
			}
			else
			{
				list.Add(
					"[{0:#,0}] {1}".WrapUOHtmlColor(
						equipped && IsActive(viewer) ? EquipmentSets.CMOptions.ModNameColorRaw : EquipmentSets.CMOptions.InactiveColorRaw),
					PartsRequired,
					Name.ToUpperWords());
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
						writer.Write(Desc);
						writer.Write(Display);
						writer.Write(PartsRequired);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						Name = reader.ReadString();
						Desc = reader.ReadString();
						Display = reader.ReadBool();
						PartsRequired = reader.ReadInt();
					}
					break;
			}
		}
	}
}