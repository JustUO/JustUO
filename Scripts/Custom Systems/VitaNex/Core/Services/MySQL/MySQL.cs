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
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

using Server;
#endregion

namespace VitaNex.MySQL
{
	public static partial class MySQL
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static MySQLOptions CSOptions { get; private set; }

		public static List<MySQLConnection> Connections { get; private set; }

		public static PollTimer ConnectionPoller { get; private set; }

		public static event Action<MySQLConnection> OnConnected;
		public static event Action<MySQLConnection> OnDisconnected;

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

			Connections.AddOrReplace(c);

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

			Connections.Remove(c);
			Connections.Free(false);

			if (OnDisconnected != null)
			{
				OnDisconnected(c);
			}
		}

		public static string Escape(string value)
		{
			if (String.IsNullOrWhiteSpace(value))
			{
				return value ?? String.Empty;
			}

			return Regex.Replace(value, @"[\r\n\t\x00\x1A""`'/\\]", @"\$0");
		}

		public static string EncodeHtml(string value)
		{
			/*
			var chars = new[] {'£', '$', '€', '%', '^', '&', '<', '>'};

			var mutation = String.Empty;

			foreach (var c in value)
			{
				if (chars.Contains(c))
				{
					mutation += "&#" + (int)c + ";";
				}
				else
				{
					mutation += c;
				}
			}

			return mutation;
			*/

			return WebUtility.HtmlEncode(value);
		}

		public static string Encode(SimpleType value, bool html)
		{
			switch (value.Flag)
			{
				case DataType.Null:
					return String.Empty;
				case DataType.Char:
				case DataType.String:
					return html ? EncodeHtml((string)value.Value) : (string)value.Value;
				case DataType.Bool:
					return (bool)value.Value ? "1" : "0";
				case DataType.Byte:
				case DataType.UShort:
				case DataType.UInt:
				case DataType.ULong:
				case DataType.SByte:
				case DataType.Short:
				case DataType.Int:
				case DataType.Long:
					return String.Format("{0:0}", value.Value);
				case DataType.Decimal:
				case DataType.Double:
				case DataType.Float:
					return String.Format("{0:0.0#}", value.Value);
				case DataType.DateTime:
					{
						TimeStamp stamp = (DateTime)value.Value;

						return String.Format("{0:0}", (int)stamp.Stamp);
					}
				case DataType.TimeSpan:
					{
						TimeSpan span = (TimeSpan)value.Value;

						return String.Format("{0:0}", (int)span.TotalSeconds);
					}
				default:
					return html ? EncodeHtml(value.ToString()) : value.ToString();
			}
		}
	}
}