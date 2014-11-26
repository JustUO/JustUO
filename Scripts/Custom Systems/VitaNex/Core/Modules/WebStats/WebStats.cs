#region Header
//   Vorspire    _,-'/-'/  WebStats.cs
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
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Reflection;
using System.Text;

using Server;
using Server.Guilds;
using Server.Items;
using Server.Misc;
using Server.Network;

using VitaNex.IO;
using VitaNex.Text;
#endregion

namespace VitaNex.Modules.WebStats
{
	public static partial class WebStats
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static bool _Started;

		private static PollTimer _ActivityTimer;

		private static DateTime _LastUpdate = DateTime.MinValue;
		private static WebStatsRequestFlags _LastFlags = WebStatsRequestFlags.None;

		public static BinaryDataStore<string, WebStatsEntry> Stats { get; private set; }

		public static TcpListener Listener { get; private set; }

		public static List<WebStatsClient> Clients { get; private set; }

		public static Dictionary<IPAddress, List<NetState>> Snapshot { get; private set; }

		public static string JSONResponse { get; private set; }

		public static event Action<WebStatsClient> OnConnected;
		public static event Action<WebStatsClient> OnDisconnected;

		public static event Action<WebStatsClient, byte[]> OnSend;
		public static event Action<WebStatsClient, string, byte[]> OnReceive;

		private static readonly MethodInfo _IsPrivateNetwork = typeof(ServerList).GetMethod(
			"IsPrivateNetwork", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);

		public static void AcquireListener()
		{
			if (!CMOptions.ModuleEnabled)
			{
				ReleaseListener();
				return;
			}

			if (Listener != null && ((IPEndPoint)Listener.LocalEndpoint).Port != CMOptions.Port)
			{
				ReleaseListener();
			}

			if (Listener == null)
			{
				IPAddress address =
					NetworkInterface.GetAllNetworkInterfaces()
									.Select(adapter => adapter.GetIPProperties())
									.Select(
										properties =>
										properties.UnicastAddresses.Select(unicast => unicast.Address)
												  .FirstOrDefault(
													  ip =>
													  !IPAddress.IsLoopback(ip) && ip.AddressFamily != AddressFamily.InterNetworkV6 &&
													  (_IsPrivateNetwork == null || (bool)_IsPrivateNetwork.Invoke(null, new object[] {ip}))))
									.FirstOrDefault() ?? IPAddress.Any;

				Listener = new TcpListener(address, CMOptions.Port);
			}

			if (!Listener.Server.IsBound)
			{
				Listener.Start(CMOptions.MaxConnections);

				CMOptions.ToConsole("Listening: {0}", Listener.LocalEndpoint);
			}

			_Listening = true;
		}

		public static void ReleaseListener()
		{
			if (Listener == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					if (Listener.Server.IsBound)
					{
						Listener.Server.Disconnect(true);
					}
				});

			VitaNexCore.TryCatch(Listener.Stop);

			Listener = null;

			_Listening = false;
		}

		private static bool _Listening;

		private static void ListenAsync()
		{
			AcquireListener();

			if (Listener == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() => Listener.BeginAcceptTcpClient(
					r =>
					{
						var client = VitaNexCore.TryCatchGet(() => Listener.EndAcceptTcpClient(r), CMOptions.ToConsole);

						ListenAsync();

						if (client != null && client.Connected)
						{
							VitaNexCore.TryCatch(() => Connected(client), CMOptions.ToConsole);
						}
					},
					null),
				e =>
				{
					_Listening = false;
					CMOptions.ToConsole(e);
				});
		}

		private static void Connected(TcpClient tcp)
		{
			if (tcp == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					if (Listener != null && _Started)
					{
						Connected(new WebStatsClient(tcp, Encoding.UTF8));
					}
					else
					{
						tcp.Close();
					}
				},
				CMOptions.ToConsole);
		}

		public static void Connected(WebStatsClient client)
		{
			lock (Clients)
			{
				if (!Clients.Contains(client))
				{
					Clients.Add(client);
				}
			}

			CMOptions.ToConsole("[{0}] Client connected: {1}", Clients.Count, client.Client.Client.RemoteEndPoint);

			if (OnConnected != null)
			{
				VitaNexCore.TryCatch(
					() => OnConnected(client),
					e =>
					{
						lock (Clients)
						{
							Clients.Remove(client);
						}

						CMOptions.ToConsole(e);
					});
			}
		}

		public static void Disconnected(WebStatsClient client)
		{
			lock (Clients)
			{
				Clients.Remove(client);
			}

			CMOptions.ToConsole("[{0}] Client disconnected: {1}", Clients.Count, client.Client.Client.RemoteEndPoint);

			if (OnDisconnected != null)
			{
				VitaNexCore.TryCatch(() => OnDisconnected(client), CMOptions.ToConsole);
			}
		}

		public static void EndReceive(WebStatsClient client, string data, byte[] buffer)
		{
			if (OnReceive != null)
			{
				VitaNexCore.TryCatch(() => OnReceive(client, data, buffer));
			}
		}

		public static void EndSend(WebStatsClient client, byte[] buffer)
		{
			if (OnSend != null)
			{
				VitaNexCore.TryCatch(() => OnSend(client, buffer));
			}
		}

		public static void HandleConnection(WebStatsClient client)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					var headers = new Dictionary<string, string>();

					client.Receive(
						false,
						true,
						(c, d, b) =>
						{
							EndReceive(c, d, b);

							if (d.Length == 0)
							{
								return;
							}

							var lines = d.Split(new[] {'\r', '\n'}, StringSplitOptions.RemoveEmptyEntries);

							lines = lines.Take(lines.Length - 1).ToArray();

							if (CMOptions.ModuleDebug)
							{
								CMOptions.ToConsole(lines.Not(String.IsNullOrWhiteSpace).ToArray());
							}

							lines.ForEach(
								line =>
								{
									line = line.Trim();

									var header = line.Split(new[] {' '}, StringSplitOptions.RemoveEmptyEntries);

									if (header.Length == 0)
									{
										return;
									}

									string hk = header[0].Replace(":", String.Empty);

									if (String.IsNullOrWhiteSpace(hk))
									{
										return;
									}

									string hv = header.Length > 1 ? String.Join(" ", header.Skip(1)) : String.Empty;

									if (!headers.ContainsKey(hk))
									{
										headers.Add(hk, hv);
									}
									else
									{
										headers[hk] = hv;
									}
								});
						});

					if (headers.Count > 0)
					{
						HandleHttpRequest(client, headers);
						return;
					}

					client.Send(GetJSON(false), true, false, EndSend);
				},
				CMOptions.ToConsole);

			if (client != null)
			{
				client.Close();
			}
		}

		private static void HandleHttpRequest(WebStatsClient client, Dictionary<string, string> headers)
		{
			var queries = new Dictionary<string, string>();

			bool preFlight = false;
			string[] args;

			if (headers.ContainsKey("GET"))
			{
				args = headers["GET"].Split(' ');
			}
			else if (headers.ContainsKey("OPTIONS"))
			{
				preFlight = true;
				args = headers["OPTIONS"].Split(' ');
			}
			else
			{
				args = new string[0];
			}

			if (args.Length > 0)
			{
				string rawQuery = !String.IsNullOrWhiteSpace(args[0]) ? args[0] : String.Empty;

				if (rawQuery.StartsWith("/?"))
				{
					foreach (string q in rawQuery.Substring(2).Split(new[] {'&'}, StringSplitOptions.RemoveEmptyEntries))
					{
						var query = q.Split(new[] {'='}, StringSplitOptions.RemoveEmptyEntries);

						string qk = query.Length > 0 ? query[0] : String.Empty;

						if (String.IsNullOrWhiteSpace(qk))
						{
							return;
						}

						string qv = query.Length > 1 ? query[1] : String.Empty;

						if (!queries.ContainsKey(qk))
						{
							queries.Add(qk, qv);
						}
						else
						{
							queries[qk] = qv;
						}
					}
				}
			}

			bool origin = headers.ContainsKey("Origin");
			bool jsonp = false;
			string response = preFlight ? String.Empty : GetJSON(false);

			if (queries.ContainsKey("callback"))
			{
				string func = queries["callback"];

				if (String.IsNullOrWhiteSpace(func))
				{
					func = "webstatsCallback";
				}

				response = String.Format("{0}({1});", func, response);
				jsonp = true;
			}

			var sendHeaders = new List<string>
			{
				"HTTP/1.1 200 OK", //
				"Date: " + DateTime.UtcNow.ToSimpleString("D, d M y t@h:m:s@") + " GMT", //
				"Last-Modified: " + _LastUpdate.ToSimpleString("D, d M y t@h:m:s@"), //
				"Server: " + ServerList.ServerName
			};

			if (preFlight)
			{
				if (origin)
				{
					sendHeaders.AddRange(
						new[]
						{
							"Access-Control-Allow-Methods: POST, GET, OPTIONS", //
							"Access-Control-Allow-Headers: Origin, X-Requested-With, Content-Type, Accept", //
							"Access-Control-Allow-Origin: " + headers["Origin"]
						});
				}

				sendHeaders.AddRange(
					new[]
					{
						"Vary: Accept-Encoding", //
						"Content-Encoding: deflate", //
						"Content-Length: 0", //
						"Content-Type: text/plain; charset=utf-8", //
						"Keep-Alive: timeout=2, max=120", //
						"Connection: keep-alive"
					});

				client.Send(String.Join("\r\n", sendHeaders) + "\r\n\r\n", false, false, EndSend);
			}
			else
			{
				byte[] buffer;
				int length;

				client.Encode(response, out buffer, out length);
				client.Compress(ref buffer, ref length);

				if (origin)
				{
					sendHeaders.Add("Access-Control-Allow-Origin: " + headers["Origin"]);
				}

				sendHeaders.AddRange(
					new[]
					{
						"Vary: Accept-Encoding", //
						"Content-Encoding: deflate", //
						"Content-Length: " + length, //
						(jsonp ? "Content-Type: text/javascript;" : "Content-Type: application/json;") + " charset=utf-8", //
						"Connection: close"
					});

				client.Send(String.Join("\r\n", sendHeaders) + "\r\n\r\n", false, false, EndSend);
				client.Send(buffer, length, false, EndSend);
			}

			if (!CMOptions.ModuleDebug)
			{
				return;
			}

			CMOptions.ToConsole("HEADERS>>>\n");
			CMOptions.ToConsole(sendHeaders.ToArray());
		}

		public static bool UpdateStats(bool forceUpdate)
		{
			if (!forceUpdate && _LastFlags == CMOptions.RequestFlags && DateTime.UtcNow - _LastUpdate < CMOptions.UpdateInterval)
			{
				return false;
			}

			_LastUpdate = DateTime.UtcNow;
			_LastFlags = CMOptions.RequestFlags;

			var states = NetState.Instances.Where(ns => ns != null && ns.Socket != null && ns.Mobile != null).ToArray();

			Snapshot.Clear();

			foreach (var ns in states)
			{
				IPEndPoint ep = (IPEndPoint)ns.Socket.RemoteEndPoint;

				if (!Snapshot.ContainsKey(ep.Address))
				{
					Snapshot.Add(ep.Address, new List<NetState>());
				}

				Snapshot[ep.Address].Add(ns);
			}

			#region Uptime
			TimeSpan uptime = DateTime.UtcNow - Clock.ServerStart;

			Stats["uptime"].Value = uptime;

			if (Stats["uptime_peak"].Cast<TimeSpan>() < uptime)
			{
				Stats["uptime_peak"].Value = uptime;
			}
			#endregion

			#region Online
			int connected = states.Length;

			Stats["online"].Value = connected;

			if (Stats["online_max"].Cast<int>() < connected)
			{
				Stats["online_max"].Value = connected;
			}

			if (Stats["online_peak"].Cast<int>() < connected)
			{
				Stats["online_peak"].Value = connected;
			}
			#endregion

			#region Unique
			int unique = Snapshot.Count;

			Stats["unique"].Value = unique;

			if (Stats["unique_max"].Cast<int>() < unique)
			{
				Stats["unique_max"].Value = unique;
			}

			if (Stats["unique_peak"].Cast<int>() < unique)
			{
				Stats["unique_peak"].Value = unique;
			}
			#endregion

			#region Items
			int items = World.Items.Count;

			Stats["items"].Value = items;

			if (Stats["items_max"].Cast<int>() < items)
			{
				Stats["items_max"].Value = items;
			}

			if (Stats["items_peak"].Cast<int>() < items)
			{
				Stats["items_peak"].Value = items;
			}
			#endregion

			#region Mobiles
			int mobiles = World.Mobiles.Count;

			Stats["mobiles"].Value = mobiles;

			if (Stats["mobiles_max"].Cast<int>() < mobiles)
			{
				Stats["mobiles_max"].Value = mobiles;
			}

			if (Stats["mobiles_peak"].Cast<int>() < mobiles)
			{
				Stats["mobiles_peak"].Value = mobiles;
			}
			#endregion

			#region Guilds
			int guilds = BaseGuild.List.Count;

			Stats["guilds"].Value = guilds;

			if (Stats["guilds_max"].Cast<int>() < guilds)
			{
				Stats["guilds_max"].Value = guilds;
			}

			if (Stats["guilds_peak"].Cast<int>() < guilds)
			{
				Stats["guilds_peak"].Value = guilds;
			}
			#endregion

			return true;
		}

		private static string GetJSON(bool forceUpdate)
		{
			if (!UpdateStats(forceUpdate))
			{
				return JSONResponse;
			}

			var values = new Dictionary<string, SimpleType>();
			var subValues = new List<string>();

			string serverObject = "\"server\": { }";
			string statsObject = "\"stats\": { }";
			string playersObject = "\"players\": [ ]";
			string guildsObject = "\"guilds\": [ ]";

			if (CMOptions.DisplayServer)
			{
				values.Clear();

				values.Add("name", ServerList.ServerName);

				IPEndPoint ipep =
					Server.Network.Listener.EndPoints.FirstOrDefault(
						ip =>
						ip.Address.ToString() != "0.0.0.0" && ip.Address.ToString() != "127.0.0.1" &&
						!ip.Address.ToString().StartsWith("192.168"));

				if (ipep != null)
				{
					values.Add("host", ipep.Address.ToString());
					values.Add("port", ipep.Port);
				}

				values.Add("framework", Environment.OSVersion.VersionString);

				serverObject = JSON.EncodeObject("server", values);
			}

			if (CMOptions.DisplayStats)
			{
				values.Clear();

				Stats.ForEach((k, v) => values.Add(k, v.Data));

				statsObject = JSON.EncodeObject("stats", values);
			}

			if (CMOptions.DisplayPlayers)
			{
				var playerObjects = new List<string>(Snapshot.Sum(kv => kv.Value.Count));

				Snapshot.ForEach(
					(ip, states) => states.ForEach(
						ns =>
						{
							values.Clear();

							Mobile m = ns.Mobile;

							values.Add("id", m.Serial.Value);
							values.Add("name", m.RawName ?? String.Empty);
							values.Add("title", m.Title ?? String.Empty);
							values.Add("profile", m.Profile ?? String.Empty);
							values.Add("guild_id", m.Guild != null ? m.Guild.Id : -1);
							values.Add("guild_abbr", m.Guild != null ? m.Guild.Abbreviation ?? String.Empty : String.Empty);

							string jsonInfo = JSON.EncodeObject("info", values);

							string jsonStats = "\"stats\": [ ]";

							if (CMOptions.DisplayPlayerStats)
							{
								values.Clear();

								values.Add("str", m.Str);
								values.Add("str_raw", m.RawStr);

								values.Add("dex", m.Dex);
								values.Add("dex_raw", m.RawDex);

								values.Add("int", m.Int);
								values.Add("int_raw", m.RawInt);

								values.Add("hits", m.Hits);
								values.Add("hits_max", m.HitsMax);

								values.Add("stam", m.Stam);
								values.Add("stam_max", m.StamMax);

								values.Add("mana", m.Mana);
								values.Add("mana_max", m.ManaMax);

								jsonStats = JSON.EncodeObject("stats", values);
							}

							string jsonSkills = "\"skills\": [ ]";

							if (CMOptions.DisplayPlayerSkills)
							{
								subValues.Clear();

								foreach (Skill skill in m.Skills)
								{
									values.Clear();

									values.Add("id", skill.SkillID);
									values.Add("name", skill.Name);
									values.Add("base", skill.Base);
									values.Add("value", skill.Value);
									values.Add("cap", skill.Cap);

									subValues.Add(JSON.EncodeObject(values));
								}

								jsonSkills = JSON.FormatArray("skills", subValues);
							}

							string jsonEquip = "\"equip\": [ ]";

							if (CMOptions.DisplayPlayerEquip)
							{
								subValues.Clear();

								foreach (Item item in m.Items.Where(i => i.Parent == m))
								{
									values.Clear();

									values.Add("id", item.Serial.Value);
									values.Add("type", item.GetType().Name);
									values.Add("layer", (int)item.Layer);
									values.Add("art", item.ItemID);
									values.Add("hue", item.Hue);
									values.Add("name", item.ResolveName());

									subValues.Add(JSON.EncodeObject(values));
								}

								jsonEquip = JSON.FormatArray("equip", subValues);
							}

							playerObjects.Add(String.Format("{{ {0} }}", String.Join(", ", jsonInfo, jsonStats, jsonSkills, jsonEquip)));
						}));

				playersObject = JSON.FormatArray("players", playerObjects);
			}

			if (CMOptions.DisplayGuilds)
			{
				var guildObjects = new List<string>(BaseGuild.List.Count);

				BaseGuild.List.ForEach(
					(id, bg) =>
					{
						values.Clear();

						values.Add("id", bg.Id);
						values.Add("name", bg.Name);
						values.Add("abbr", bg.Abbreviation);
						values.Add("type", bg.Type.ToString());
						values.Add("disbanded", bg.Disbanded);

						string jsonInfo = "\"info\": { }";
						string jsonMembers = "\"members\": [ ]";
						string jsonAllies = "\"allies\": [ ]";
						string jsonEnemies = "\"enemies\": [ ]";

						if (bg is Guild)
						{
							var g = (Guild)bg;

							values.Add("leader_id", g.Leader != null ? g.Leader.Serial.Value : -1);
							values.Add("leader_name", g.Leader != null ? g.Leader.RawName : String.Empty);
							values.Add("charter", g.Charter ?? String.Empty);
							values.Add("website", g.Website ?? String.Empty);

							jsonInfo = JSON.EncodeObject("info", values);

							subValues.Clear();

							foreach (Mobile m in g.Members)
							{
								values.Clear();

								values.Add("id", m.Serial.Value);
								values.Add("name", m.RawName ?? String.Empty);
								values.Add("title", m.Title ?? String.Empty);

								subValues.Add(JSON.EncodeObject(values));
							}

							jsonMembers = JSON.FormatArray("members", subValues);

							subValues.Clear();

							foreach (Guild ally in g.Allies)
							{
								values.Clear();

								values.Add("id", ally.Id);
								values.Add("abbr", ally.Abbreviation ?? String.Empty);
								values.Add("name", ally.Name ?? String.Empty);

								subValues.Add(JSON.EncodeObject(values));
							}

							jsonAllies = JSON.FormatArray("allies", subValues);

							subValues.Clear();

							foreach (Guild enemy in g.Enemies)
							{
								values.Clear();

								values.Add("id", enemy.Id);
								values.Add("abbr", enemy.Abbreviation ?? String.Empty);
								values.Add("name", enemy.Name ?? String.Empty);

								subValues.Add(JSON.EncodeObject(values));
							}

							jsonEnemies = JSON.FormatArray("enemies", subValues);
						}

						guildObjects.Add(String.Format("{{ {0} }}", String.Join(", ", jsonInfo, jsonMembers, jsonAllies, jsonEnemies)));
					});

				guildsObject = JSON.FormatArray("guilds", guildObjects);
			}

			string vncVersion = String.Format("\"vnc_version\": {0}", JSON.Encode(VitaNexCore.Version.Value));
			string modVersion = String.Format("\"mod_version\": {0}", JSON.Encode(CMOptions.ModuleVersion));

			JSONResponse = String.Format(
				"{{ {0} }}", String.Join(", ", vncVersion, modVersion, serverObject, statsObject, playersObject, guildsObject));

			File.WriteAllText(
				IOUtility.GetSafeFilePath(VitaNexCore.DataDirectory + "/WebStats.json", true), JSONResponse, Encoding.UTF8);

			return JSONResponse;
		}
	}
}