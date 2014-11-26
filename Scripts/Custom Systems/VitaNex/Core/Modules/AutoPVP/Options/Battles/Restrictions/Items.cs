#region Header
//   Vorspire    _,-'/-'/  Items.cs
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
#endregion

namespace VitaNex.Modules.AutoPvP
{
	[PropertyObject]
	public class PvPBattleItemRestrictions : PvPBattleRestrictionsBase<Type>
	{
		private static readonly Type _TypeOf = typeof(Item);

		public PvPBattleItemRestrictions()
		{ }

		public PvPBattleItemRestrictions(GenericReader reader)
			: base(reader)
		{ }

		private static Type FindType(string name, bool full = false, bool ignoreCase = true)
		{
			return Type.GetType(name, false, ignoreCase) ??
				   (full ? ScriptCompiler.FindTypeByFullName(name, ignoreCase) : ScriptCompiler.FindTypeByName(name, ignoreCase));
		}

		public override void Invalidate()
		{ }

		public virtual void SetRestricted(Item item, bool restrict)
		{
			if (item != null)
			{
				SetRestricted(item.GetType(), restrict);
			}
		}

		public virtual void SetRestricted(string item, bool restrict)
		{
			if (!String.IsNullOrWhiteSpace(item))
			{
				SetRestricted(FindType(item), restrict);
			}
		}

		public override void SetRestricted(Type key, bool val)
		{
			if (key == null)
			{
				return;
			}

			if (key.IsEqualOrChildOf(_TypeOf))
			{
				base.SetRestricted(key, val);
			}
		}

		public virtual bool IsRestricted(Item item)
		{
			return item != null && IsRestricted(item.GetType());
		}

		public override bool IsRestricted(Type key)
		{
			if (key == null)
			{
				return false;
			}

			if (key.IsEqualOrChildOf(_TypeOf))
			{
				return base.IsRestricted(_TypeOf) || base.IsRestricted(key);
			}

			return false;
		}

		public override string ToString()
		{
			return "Item Restrictions";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}

		public override void SerializeEntry(GenericWriter writer, Type key, bool val)
		{
			writer.WriteType(key);
			writer.Write(val);
		}

		public override KeyValuePair<Type, bool> DeserializeEntry(GenericReader reader)
		{
			Type k = reader.ReadType();
			bool v = reader.ReadBool();
			return new KeyValuePair<Type, bool>(k, v);
		}
	}
}