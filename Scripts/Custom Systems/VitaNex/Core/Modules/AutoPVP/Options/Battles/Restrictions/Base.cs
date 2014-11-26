#region Header
//   Vorspire    _,-'/-'/  Base.cs
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
	public abstract class PvPBattleRestrictionsBase<TKey> : PropertyObject
	{
		private Dictionary<TKey, bool> _List = new Dictionary<TKey, bool>();

		public virtual Dictionary<TKey, bool> List { get { return _List; } set { _List = value ?? new Dictionary<TKey, bool>(); } }

		public PvPBattleRestrictionsBase()
		{
			Invalidate();
		}

		public PvPBattleRestrictionsBase(GenericReader reader)
			: base(reader)
		{ }

		public abstract void Invalidate();

		public virtual void Invert()
		{
			foreach (var t in _List.Keys)
			{
				_List[t] = !_List[t];
			}
		}

		public virtual void Reset(bool val)
		{
			foreach (var t in _List.Keys)
			{
				_List[t] = val;
			}
		}

		public virtual bool Remove(TKey key)
		{
			return _List.Remove(key);
		}

		public override void Reset()
		{
			Reset(false);
		}

		public override void Clear()
		{
			_List.Clear();
		}

		public virtual void SetRestricted(TKey key, bool val)
		{
			if (key != null)
			{
				_List.AddOrReplace(key, val);
			}
		}

		public virtual bool IsRestricted(TKey key)
		{
			return key != null && _List.ContainsKey(key) && _List[key];
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.WriteDictionary(_List, SerializeEntry);
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
					_List = reader.ReadDictionary(DeserializeEntry);
					break;
			}
		}

		public abstract void SerializeEntry(GenericWriter writer, TKey key, bool val);
		public abstract KeyValuePair<TKey, bool> DeserializeEntry(GenericReader reader);
	}
}