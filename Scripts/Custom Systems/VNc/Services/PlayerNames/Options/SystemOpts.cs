#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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

namespace VitaNex
{
	public class PlayerNamesOptions : CoreServiceOptions
	{
		public PlayerNamesOptions()
			: base(typeof(PlayerNames))
		{ }

		public PlayerNamesOptions(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(PlayerNames.Access)]
		public virtual bool IndexOnStart { get; set; }

		public override void Clear()
		{
			base.Clear();

			IndexOnStart = false;
		}

		public override void Reset()
		{
			base.Reset();

			IndexOnStart = false;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(IndexOnStart);
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
						IndexOnStart = reader.ReadBool();
					}
					break;
			}
		}
	}
}