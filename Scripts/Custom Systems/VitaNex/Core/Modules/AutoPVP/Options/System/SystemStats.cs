#region Header
//   Vorspire    _,-'/-'/  SystemStats.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class AutoPvPStatistics : PropertyObject
	{
		[CommandProperty(AutoPvP.Access)]
		public virtual DataStoreStatus BattlesStatus { get { return AutoPvP.Profiles.Status; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual DataStoreStatus ProfilesStatus { get { return AutoPvP.Battles.Status; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Battles { get { return AutoPvP.Battles.Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int BattlesInternal { get { return AutoPvP.GetBattles(PvPBattleState.Internal).Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int BattlesEnded { get { return AutoPvP.GetBattles(PvPBattleState.Ended).Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int BattlesPreparing { get { return AutoPvP.GetBattles(PvPBattleState.Preparing).Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int BattlesQueueing { get { return AutoPvP.GetBattles(PvPBattleState.Queueing).Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int BattlesRunning { get { return AutoPvP.GetBattles(PvPBattleState.Running).Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Participants { get { return AutoPvP.GetParticipants().Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Profiles { get { return AutoPvP.Profiles.Count; } }

		[CommandProperty(AutoPvP.Access)]
		public virtual int Scenarios { get { return AutoPvP.Scenarios.Length; } }

		public AutoPvPStatistics()
		{ }

		public AutoPvPStatistics(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{ }

		public override void Reset()
		{ }

		public override string ToString()
		{
			return "View Statistics";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
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
					break;
			}
		}
	}
}