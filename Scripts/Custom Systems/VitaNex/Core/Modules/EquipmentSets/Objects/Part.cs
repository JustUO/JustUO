#region Header
//   Vorspire    _,-'/-'/  Part.cs
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
using System.Linq;

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public class EquipmentSetPart : PropertyObject
	{
		private bool _IncludeChildTypes;

		private readonly List<Mobile> _EquipOwners = new List<Mobile>();

		public List<Mobile> EquipOwners { get { return _EquipOwners; } }

		[CommandProperty(EquipmentSets.Access)]
		public string Name { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public ItemTypeSelectProperty TypeOf { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool IncludeChildTypes
		{
			get
			{
				if (_IncludeChildTypes && (TypeOf == null || !TypeOf.IsNotNull || TypeOf.InternalType.IsSealed))
				{
					_IncludeChildTypes = false;
				}

				return _IncludeChildTypes;
			}
			set
			{
				if (value && (TypeOf == null || !TypeOf.IsNotNull || TypeOf.InternalType.IsSealed))
				{
					value = false;
				}

				_IncludeChildTypes = value;
			}
		}

		[CommandProperty(EquipmentSets.Access)]
		public bool Display { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool DisplaySet { get; set; }

		[CommandProperty(EquipmentSets.Access)]
		public bool Valid { get { return Validate(); } }

		public EquipmentSetPart(
			string name, Type typeOf, bool childTypes = false, bool display = true, bool displaySet = true)
		{
			Name = name;
			TypeOf = typeOf;
			IncludeChildTypes = childTypes;
			Display = display;
			DisplaySet = displaySet;
		}

		public EquipmentSetPart(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		public virtual bool Validate()
		{
			if (TypeOf == null || String.IsNullOrWhiteSpace(Name))
			{
				return false;
			}

			if (_IncludeChildTypes && (TypeOf == null || !TypeOf.IsNotNull || TypeOf.InternalType.IsSealed))
			{
				_IncludeChildTypes = false;
			}

			return true;
		}

		public bool IsTypeOf(Type type)
		{
			return type != null && TypeOf != null && TypeOf.IsNotNull && _IncludeChildTypes
					   ? type.IsEqualOrChildOf(TypeOf)
					   : type.IsEqual(TypeOf);
		}

		public bool IsEquipped(Mobile m)
		{
			Item item;
			return IsEquipped(m, out item);
		}

		public bool IsEquipped(Mobile m, out Item item)
		{
			item = null;

			if (m == null)
			{
				return false;
			}

			item = m.Items.FirstOrDefault(i => IsTypeOf(i.GetType()) && m.FindItemOnLayer(i.Layer) == i);

			if (item != null)
			{
				if (!EquipOwners.Contains(m))
				{
					EquipOwners.Add(m);
				}

				return true;
			}

			EquipOwners.Remove(m);
			return false;
		}

		public Item CreateInstanceOfPart(params object[] args)
		{
			return args != null ? TypeOf.CreateInstance<Item>(args) : TypeOf.CreateInstance<Item>();
		}

		public virtual void GetProperties(Mobile viewer, ExtendedOPL list, bool equipped)
		{
			Item item;

			list.Add(
				equipped && IsEquipped(viewer, out item)
					? item.ResolveName(viewer).ToUpperWords().WrapUOHtmlColor(EquipmentSets.CMOptions.PartNameColorRaw)
					: Name.ToUpperWords().WrapUOHtmlColor(EquipmentSets.CMOptions.InactiveColorRaw));
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
						writer.Write(DisplaySet);
						writer.WriteType(TypeOf.InternalType);
						writer.Write(IncludeChildTypes);
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
						Display = reader.ReadBool();
						DisplaySet = reader.ReadBool();
						TypeOf = reader.ReadType();
						IncludeChildTypes = reader.ReadBool();
					}
					break;
			}
		}
	}
}