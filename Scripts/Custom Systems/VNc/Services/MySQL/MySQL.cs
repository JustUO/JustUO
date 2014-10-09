#region Header
//   Vorspire    _,-'/-'/  MySQL.cs
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
using System.Text;

using Server;
#endregion

namespace VitaNex.MySQL
{
	public static partial class MySQL
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static List<MySQLConnection> Connections { get; private set; }

		public static PollTimer ConnectionPoller { get; private set; }

		public static event Action<MySQLConnection> OnConnected, OnDisconnected;

		public static string GetConnectionString(this MySQLConnectionInfo info)
		{
			var cStr = new StringBuilder();

			if (Core.Is64Bit)
			{
				//cStr.Append(@"Provider=MSDASQL;");
			}

			cStr.AppendFormat("DRIVER={0};", info.Driver.AsConnectionString());

			if (info.DSN != null && !String.IsNullOrWhiteSpace(info.DSN.Name))
			{
				cStr.AppendFormat("DSN={0};", info.DSN.Name);
			}
			else
			{
				cStr.AppendFormat("SERVER={0};", info.IP);
				cStr.AppendFormat("PORT={0};", info.Port);

				cStr.AppendFormat("UID={0};", info.User);
				cStr.AppendFormat("PASSWORD={0};", info.Password);
			}

			cStr.Append(@"OPTION=3;");

			return cStr.ToString();
		}

		public static bool CanConnect(MySQLConnection c, bool message = true)
		{
			if (c == null)
			{
				if (message)
				{
					CSOptions.ToConsole("Connection invalid: The connection no longer exists.");
				}

				return false;
			}

			if (c.HasError)
			{
				if (message)
				{
					CSOptions.ToConsole("Connection invalid: The connection has errors.");
				}

				return false;
			}

			if (c.Credentials == null || !c.Credentials.IsValid())
			{
				if (message)
				{
					CSOptions.ToConsole("Connection invalid: The connection credentials are invalid.");
				}

				return false;
			}

			if (Connections.Count >= CSOptions.MaxConnections)
			{
				if (message)
				{
					CSOptions.ToConsole("Connection invalid: Max connection limit ({0:#,0}) reached.", CSOptions.MaxConnections);
				}

				return false;
			}

			if (message)
			{
				CSOptions.ToConsole("Connection validated.");
			}

			return true;
		}

		public static void Connected(MySQLConnection c)
		{
			if (c == null)
			{
				return;
			}

			if (!Connections.Contains(c))
			{
				Connections.Add(c);
			}

			if (OnConnected != null)
			{
				OnConnected(c);
			}
		}

		public static void Disconnected(MySQLConnection c)
		{
			if (c == null)
			{
				return;
			}

			if (Connections.Contains(c))
			{
				Connections.Remove(c);
			}

			if (OnDisconnected != null)
			{
				OnDisconnected(c);
			}
		}
	}
}