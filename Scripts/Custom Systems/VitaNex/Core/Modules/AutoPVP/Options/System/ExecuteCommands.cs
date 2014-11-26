#region Header
//   Vorspire    _,-'/-'/  ExecuteCommands.cs
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
	public class AutoPvPExecuteCommands : PropertyObject
	{
		public AutoPvPExecuteCommands()
		{
			SaveEnabled = LoadEnabled = SyncEnabled = true;
		}

		public AutoPvPExecuteCommands(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool SaveEnabled { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Save
		{
			get { return SaveEnabled; }
			set
			{
				if (value)
				{
					Execute("SAVE");
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual bool LoadEnabled { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Load
		{
			get { return LoadEnabled; }
			set
			{
				if (value)
				{
					Execute("LOAD");
				}
			}
		}

		[CommandProperty(AutoPvP.Access)]
		public virtual bool SyncEnabled { get; set; }

		[CommandProperty(AutoPvP.Access)]
		public virtual bool Sync
		{
			get { return SyncEnabled; }
			set
			{
				if (value)
				{
					Execute("SYNC");
				}
			}
		}

		public virtual void Execute(string cmd)
		{
			switch (cmd)
			{
				case "SAVE":
					{
						if (SaveEnabled)
						{
							AutoPvP.Save();
						}
					}
					break;
				case "LOAD":
					{
						if (LoadEnabled)
						{
							AutoPvP.Load();
						}
					}
					break;
				case "SYNC":
					{
						if (SyncEnabled)
						{
							AutoPvP.Sync();
						}
					}
					break;
			}
		}

		public override void Clear()
		{
			SaveEnabled = LoadEnabled = SyncEnabled = false;
		}

		public override void Reset()
		{
			SaveEnabled = LoadEnabled = SyncEnabled = true;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(SaveEnabled);
						writer.Write(LoadEnabled);
						writer.Write(SyncEnabled);
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
						SaveEnabled = reader.ReadBool();
						LoadEnabled = reader.ReadBool();
						SyncEnabled = reader.ReadBool();
					}
					break;
			}
		}

		public override string ToString()
		{
			return "Command Interface";
		}
	}
}