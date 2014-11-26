#region Header
//   Vorspire    _,-'/-'/  StatOffset.cs
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
using System.Globalization;

using Server;
#endregion

namespace VitaNex.Modules.EquipmentSets
{
	public class StatOffsetSetMod : EquipmentSetMod
	{
		private StatType _Stat = StatType.All;
		private int _Offset;
		private TimeSpan _Duration = TimeSpan.Zero;

		[CommandProperty(EquipmentSets.Access)]
		public StatType Stat
		{
			get { return _Stat; }
			set
			{
				_Stat = value;
				InvalidateDesc();
			}
		}

		[CommandProperty(EquipmentSets.Access)]
		public int Offset
		{
			get { return _Offset; }
			set
			{
				_Offset = value;
				InvalidateDesc();
			}
		}

		[CommandProperty(EquipmentSets.Access)]
		public TimeSpan Duration
		{
			get { return _Duration; }
			set
			{
				_Duration = value;
				InvalidateDesc();
			}
		}

		public string UID { get; set; }

		public StatOffsetSetMod(
			string uid = null,
			string name = "Stat Mod",
			int partsReq = 1,
			bool display = true,
			StatType stat = StatType.All,
			int offset = 1,
			TimeSpan? duration = null)
			: base(name, null, partsReq, display)
		{
			UID = uid ?? Name + DateTime.UtcNow.ToTimeStamp().Stamp.ToString(CultureInfo.InvariantCulture);

			_Stat = stat;
			_Offset = offset;
			_Duration = duration ?? TimeSpan.Zero;

			InvalidateDesc();
		}

		public StatOffsetSetMod(GenericReader reader)
			: base(reader)
		{ }

		public virtual void InvalidateDesc()
		{
			string statName = String.Empty;

			switch (_Stat)
			{
				case StatType.All:
					statName = "All Stats";
					break;
				case StatType.Dex:
					statName = "Dexterity";
					break;
				case StatType.Int:
					statName = "Intelligence";
					break;
				case StatType.Str:
					statName = "Strength";
					break;
			}

			Desc = _Duration > TimeSpan.Zero
					   ? String.Format("Increases {0} By {1:#,0} for {2}", statName, _Offset, _Duration.ToSimpleString("h:m:s"))
					   : String.Format("Increases {0} By {1}", statName, _Offset);
		}

		protected override bool OnActivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			if (EquipmentSets.CMOptions.ModuleDebug)
			{
				EquipmentSets.CMOptions.ToConsole(
					"OnActivate: '{0}', '{1}', '{2}', '{3}', '{4}'", m, UID, _Stat, _Offset, _Duration.ToSimpleString());
			}

			m.AddStatMod(new StatMod(_Stat, UID, _Offset, _Duration));
			return true;
		}

		protected override bool OnDeactivate(Mobile m, Tuple<EquipmentSetPart, Item>[] equipped)
		{
			if (m == null || m.Deleted || equipped == null)
			{
				return false;
			}

			if (EquipmentSets.CMOptions.ModuleDebug)
			{
				EquipmentSets.CMOptions.ToConsole(
					"OnDeactivate: '{0}', '{1}', '{2}', '{3}', '{4}'", m, UID, _Stat, _Offset, _Duration.ToSimpleString());
			}

			m.RemoveStatMod(UID);

			return true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
					writer.Write(UID);
					goto case 0;
				case 0:
					{
						writer.WriteFlag(Stat);
						writer.Write(Offset);
						writer.Write(Duration);
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
				case 1:
					UID = reader.ReadString();
					goto case 0;
				case 0:
					{
						Stat = reader.ReadFlag<StatType>();
						Offset = reader.ReadInt();
						Duration = reader.ReadTimeSpan();
					}
					break;
			}

			if (String.IsNullOrWhiteSpace(UID))
			{
				UID = Name + DateTime.UtcNow.ToTimeStamp().Stamp.ToString(CultureInfo.InvariantCulture);
			}
		}
	}
}