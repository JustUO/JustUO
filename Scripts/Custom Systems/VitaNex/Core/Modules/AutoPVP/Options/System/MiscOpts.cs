#region Header
//   Vorspire    _,-'/-'/  MiscOpts.cs
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
	public class AutoPvPMiscOptions : PropertyObject
	{
		[CommandProperty(AutoPvP.Access)]
		public virtual bool UseCategories { get; set; }

		public AutoPvPMiscOptions()
		{
			UseCategories = true;
		}

		public AutoPvPMiscOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			UseCategories = false;
		}

		public override void Reset()
		{
			UseCategories = true;
		}

		public override string ToString()
		{
			return "Misc Options";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(UseCategories);
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
						UseCategories = reader.ReadBool();
					}
					break;
			}
		}
	}
}