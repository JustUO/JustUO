#region Header
//   Vorspire    _,-'/-'/  PlayerNames_Init.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Threading.Tasks;

using Server;
using Server.Mobiles;

using VitaNex.IO;
#endregion

namespace VitaNex
{
	[CoreService("Player Name Register", "1.1.0", TaskPriority.High)]
	public static partial class PlayerNames
	{
		private static PlayerNamesOptions _CSOptions = new PlayerNamesOptions();

		public static PlayerNamesOptions CSOptions { get { return _CSOptions ?? (_CSOptions = new PlayerNamesOptions()); } }

		static PlayerNames()
		{
			Registry = new BinaryDataStore<string, List<PlayerMobile>>(VitaNexCore.SavesDirectory + "/PlayerNames", "Registry")
			{
				OnSerialize = Serialize,
				OnDeserialize = Deserialize
			};
		}

		private static void CSConfig()
		{
			EventSink.CharacterCreated += e => Register(e.Mobile as PlayerMobile);
			EventSink.Login += e => Register(e.Mobile as PlayerMobile);
			EventSink.Logout += e => Register(e.Mobile as PlayerMobile);
			EventSink.PlayerDeath += e => Register(e.Mobile as PlayerMobile);
		}

		private static void CSInvoke()
		{
			if (CSOptions.IndexOnStart)
			{
				Parallel.ForEach(World.Mobiles.Values, m => Register(m as PlayerMobile));
			}
		}

		private static void CSSave()
		{
			Registry.Export();
		}

		private static void CSLoad()
		{
			Registry.Import();
		}

		private static bool Serialize(GenericWriter writer)
		{
			writer.WriteBlockDictionary(
				Registry,
				(name, players) =>
				{
					writer.Write(name);
					writer.WriteMobileList(players, true);
				});

			return true;
		}

		private static bool Deserialize(GenericReader reader)
		{
			reader.ReadBlockDictionary(
				() =>
				{
					string name = reader.ReadString();
					var players = reader.ReadStrongMobileList<PlayerMobile>();
					return new KeyValuePair<string, List<PlayerMobile>>(name, players);
				});

			return true;
		}
	}
}