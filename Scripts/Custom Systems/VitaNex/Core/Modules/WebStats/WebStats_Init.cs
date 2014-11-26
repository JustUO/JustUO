#region Header
//   Vorspire    _,-'/-'/  WebStats_Init.cs
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
using System.Net;

using Server;
using Server.Items;
using Server.Network;

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.WebStats
{
	[CoreModule("Web Stats", "1.0.0.0")]
	public static partial class WebStats
	{
		public static WebStatsOptions CMOptions { get; private set; }

		static WebStats()
		{
			CMOptions = new WebStatsOptions();

			EventSink.ServerStarted += () => _Started = true;

			Clients = new List<WebStatsClient>();
			Snapshot = new Dictionary<IPAddress, List<NetState>>();

			Stats = new BinaryDataStore<string, WebStatsEntry>(VitaNexCore.SavesDirectory + "/WebStats", "Stats")
			{
				OnSerialize = SerializeStats,
				OnDeserialize = DeserializeStats
			};

			TimeSpan uptime = DateTime.UtcNow - Clock.ServerStart;

			Stats.Add("uptime", new WebStatsEntry(uptime, false));
			Stats.Add("uptime_peak", new WebStatsEntry(uptime, true));

			Stats.Add("online", new WebStatsEntry(0, false));
			Stats.Add("online_max", new WebStatsEntry(0, false));
			Stats.Add("online_peak", new WebStatsEntry(0, true));

			Stats.Add("unique", new WebStatsEntry(0, false));
			Stats.Add("unique_max", new WebStatsEntry(0, false));
			Stats.Add("unique_peak", new WebStatsEntry(0, true));

			Stats.Add("items", new WebStatsEntry(0, false));
			Stats.Add("items_max", new WebStatsEntry(0, false));
			Stats.Add("items_peak", new WebStatsEntry(0, true));

			Stats.Add("mobiles", new WebStatsEntry(0, false));
			Stats.Add("mobiles_max", new WebStatsEntry(0, false));
			Stats.Add("mobiles_peak", new WebStatsEntry(0, true));

			Stats.Add("guilds", new WebStatsEntry(0, false));
			Stats.Add("guilds_max", new WebStatsEntry(0, false));
			Stats.Add("guilds_peak", new WebStatsEntry(0, true));

			OnConnected += HandleConnection;

			_ActivityTimer = PollTimer.FromSeconds(
				60.0,
				() =>
				{
					if (!_Listening || Listener == null || Listener.Server == null || !Listener.Server.IsBound)
					{
						_Listening = false;
						ListenAsync();
					}

					Clients.RemoveRange(c => !c.Connected);
				},
				() => CMOptions.ModuleEnabled && Clients.Count > 0);
		}

		private static void CMInvoke()
		{
			ListenAsync();
		}

		private static void CMEnabled()
		{
			ListenAsync();
		}

		private static void CMDisabled()
		{
			ReleaseListener();
		}

		private static void CMSave()
		{
			Stats.Export();
		}

		private static void CMLoad()
		{
			Stats.Import();
		}

		private static void CMDisposed()
		{
			if (Listener == null)
			{
				return;
			}

			Listener.Stop();
			Listener = null;
		}

		private static bool SerializeStats(GenericWriter writer)
		{
			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteBlockDictionary(
							Stats,
							(w, k, v) =>
							{
								w.Write(k);
								v.Serialize(w);
							});
					}
					break;
			}

			return true;
		}

		private static bool DeserializeStats(GenericReader reader)
		{
			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						reader.ReadBlockDictionary(
							r =>
							{
								string k = r.ReadString();
								WebStatsEntry v = new WebStatsEntry(r);

								return new KeyValuePair<string, WebStatsEntry>(k, v);
							},
							Stats);
					}
					break;
			}

			return true;
		}
	}
}