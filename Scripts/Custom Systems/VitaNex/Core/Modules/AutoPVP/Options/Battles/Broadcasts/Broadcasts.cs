#region Header
//   Vorspire    _,-'/-'/  Broadcasts.cs
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
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPBattleBroadcasts : PropertyObject
	{
		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleLocalBroadcasts Local { get; protected set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual PvPBattleWorldBroadcasts World { get; protected set; }

		public PvPBattleBroadcasts()
		{
			Local = new PvPBattleLocalBroadcasts();
			World = new PvPBattleWorldBroadcasts();
		}

		public PvPBattleBroadcasts(GenericReader reader)
			: base(reader)
		{ }

		public override string ToString()
		{
			return "Battle Broadcasts";
		}

		public override void Clear()
		{
			Local.Clear();
			World.Clear();
		}

		public override void Reset()
		{
			Local.Reset();
			World.Reset();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlock(w => w.WriteType(Local, t => Local.Serialize(w)));
						writer.WriteBlock(w => w.WriteType(World, t => World.Serialize(w)));
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
						reader.ReadBlock(
							r => Local = reader.ReadTypeCreate<PvPBattleLocalBroadcasts>(r) ?? new PvPBattleLocalBroadcasts(r));
						reader.ReadBlock(
							r => World = reader.ReadTypeCreate<PvPBattleWorldBroadcasts>(r) ?? new PvPBattleWorldBroadcasts(r));
					}
					break;
			}
		}
	}
}