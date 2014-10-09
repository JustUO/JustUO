#region Header
//   Vorspire    _,-'/-'/  Connection.cs
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
using System.Data;
using System.Data.Odbc;
using System.Text;

using Server;
#endregion

namespace VitaNex.MySQL
{
	/// <summary>
	///     Provides a basic MySQL Connection Class that wraps the OdbcConnection class.
	/// </summary>
	public class MySQLConnectionInfo : PropertyObject, IEquatable<MySQLConnectionInfo>
	{
		public static ODBCVersion DefaultODBCVersion = ODBCVersion.V_5_1;

		public static MySQLConnectionInfo Default { get { return new MySQLConnectionInfo("localhost", 3306, "root", String.Empty); } }

		/// <summary>
		///     MySQL Server IP Address
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public string IP { get; set; }

		/// <summary>
		///     MySQL Server Port
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public short Port { get; set; }

		/// <summary>
		///     MySQL Access User Name
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public string User { get; set; }

		/// <summary>
		///     MySQL Access Password
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public string Password { get; set; }

		/// <summary>
		///     MySQL Database Name
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public string Database { get; set; }

		/// <summary>
		///     OdbcConnection Driver Version
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public ODBCVersion Driver { get; set; }

		/// <summary>
		///     MySQL ODBC DSN
		/// </summary>
		[CommandProperty(MySQL.Access)]
		public MySQLDSN DSN { get; set; }

		public MySQLConnectionInfo(string ip, short port, string user, string pass)
			: this(ip, port, user, pass, DefaultODBCVersion)
		{ }

		public MySQLConnectionInfo(string ip, short port, string user, string pass, ODBCVersion driver)
			: this(ip, port, user, pass, driver, String.Empty)
		{ }

		public MySQLConnectionInfo(string ip, short port, string user, string pass, ODBCVersion driver, string dbName)
		{
			IP = ip;
			Port = port;
			User = user;
			Password = pass;
			Database = dbName;
			Driver = driver;
			DSN = new MySQLDSN();
		}

		public MySQLConnectionInfo(string ip, short port, string user, MySQLDSN dsn)
			: this(ip, port, user, dsn, DefaultODBCVersion)
		{ }

		public MySQLConnectionInfo(string ip, short port, string user, MySQLDSN dsn, ODBCVersion driver)
			: this(ip, port, user, dsn, driver, "")
		{ }

		public MySQLConnectionInfo(string ip, short port, string user, MySQLDSN dsn, ODBCVersion driver, string dbName)
		{
			IP = ip;
			Port = port;
			User = user;
			Password = String.Empty;
			Database = dbName;
			DSN = dsn;
			Driver = driver;
		}

		public MySQLConnectionInfo(GenericReader reader)
			: base(reader)
		{ }

		public bool Equals(MySQLConnectionInfo info)
		{
			if (info == null)
			{
				return false;
			}

			if (ReferenceEquals(this, info))
			{
				return true;
			}

			return (IP == info.IP && User == info.User && Password == info.Password && Database == info.Database &&
					DSN == info.DSN && Driver == info.Driver);
		}

		public override void Clear()
		{
			IP = String.Empty;
			Port = 3306;
			User = String.Empty;
			Password = String.Empty;
			Database = String.Empty;
			DSN = new MySQLDSN();
			Driver = DefaultODBCVersion;
		}

		public override void Reset()
		{
			IP = String.Empty;
			Port = 3306;
			User = String.Empty;
			Password = String.Empty;
			Database = String.Empty;
			DSN = new MySQLDSN();
			Driver = DefaultODBCVersion;
		}

		public virtual bool IsValid()
		{
			if (DSN != null && !String.IsNullOrWhiteSpace(DSN.Name))
			{
				return true;
			}

			return !String.IsNullOrWhiteSpace(IP) && Port > 0 && !String.IsNullOrWhiteSpace(User);
		}

		public override string ToString()
		{
			return String.IsNullOrWhiteSpace(IP) ? "..." : IP;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(IP);
						writer.Write(Port);
						writer.Write(User);
						writer.Write(Password);
						writer.Write(Database);
						writer.WriteFlag(Driver);
						DSN.Serialize(writer);
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
						IP = reader.ReadString();
						Port = reader.ReadShort();
						User = reader.ReadString();
						Password = reader.ReadString();
						Database = reader.ReadString();
						Driver = reader.ReadFlag<ODBCVersion>();
						DSN = new MySQLDSN(reader);
					}
					break;
			}
		}
	}

	public class MySQLConnection
	{
		public MySQLConnection(MySQLConnectionInfo credentials)
		{
			Credentials = credentials;
		}

		/// <summary>
		///     Stores the connection information used by this instance.
		/// </summary>
		public MySQLConnectionInfo Credentials { get; set; }

		/// <summary>
		///     OdbcConnection extension: Message
		/// </summary>
		public string Message { get; private set; }

		/// <summary>
		///     OdbcConnection extension: Errors
		/// </summary>
		public OdbcError[] Errors { get; private set; }

		/// <summary>
		///     OdbcConnection Instance
		/// </summary>
		public OdbcConnection Instance { get; private set; }

		/// <summary>
		///     Returns true if the Connection is Open
		/// </summary>
		public bool Connected { get { return (Instance != null && Instance.State == ConnectionState.Open); } }

		/// <summary>
		///     Returns true if the Connection is Connecting
		/// </summary>
		public bool Connecting { get { return (Instance != null && Instance.State == ConnectionState.Connecting); } }

		/// <summary>
		///     Returns true if the Connection contains Errors
		/// </summary>
		public bool HasError { get { return (Errors != null && Errors.Length > 0); } }

		/// <summary>
		///     Called when the Connection instance receives any messages.
		///     Resets the MySQLConnection object Message and Errors members.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">Odbc Message Event Arguments</param>
		private void OnMessage(object sender, OdbcInfoMessageEventArgs e)
		{
			if (e.Errors != null && e.Errors.Count > 0)
			{
				OdbcErrorCollection errList = e.Errors;

				Errors = new OdbcError[errList.Count];

				for (int i = 0; i < errList.Count; i++)
				{
					Errors[i] = errList[i];
				}
			}
			else
			{
				Errors = null;
			}

			Message = e.Message.Length > 0 ? e.Message : null;
		}

		/// <summary>
		///     Attempts to connect to the specified MySQL Server with the given settings.
		/// </summary>
		/// <param name="retries">Retry connection attempts this many times if the initial connection fails.</param>
		/// <param name="createDB">If a database name is specified, should it try to create it if it doesn't exist?</param>
		/// <returns>True if connection successful</returns>
		public bool Connect(int retries = 0, bool createDB = false)
		{
			if (Connected)
			{
				return true;
			}

			if (Connecting)
			{
				return false;
			}

			if (!CanConnect())
			{
				return false;
			}

			string conStr = Credentials.GetConnectionString();

			//MySQL.CSOptions.ToConsole("{0}", conStr);

			bool connected = VitaNexCore.TryCatchGet(
				() =>
				{
					if (Instance == null)
					{
						MySQL.CSOptions.ToConsole("Connection Attempt.");

						Instance = new OdbcConnection(conStr);
						Instance.InfoMessage += OnMessage;
						Instance.Open();

						VitaNexCore.WaitWhile(() => (Instance.State == ConnectionState.Connecting), TimeSpan.FromSeconds(3));

						if (Instance.State == ConnectionState.Broken)
						{
							Instance.Close();
						}

						if (Instance.State == ConnectionState.Open)
						{
							MySQL.CSOptions.ToConsole("Connection Successful.");
							MySQL.Connected(this);

							if (!String.IsNullOrWhiteSpace(Credentials.Database))
							{
								if (createDB)
								{
									NonQuery(
										@"CREATE DATABASE IF NOT EXISTS `{0}` DEFAULT CHARSET `utf8` DEFAULT COLLATE `utf8_bin`", Credentials.Database);
								}

								Instance.ChangeDatabase(Credentials.Database);
							}

							return true;
						}
					}

					if (Instance == null)
					{
						return false;
					}

					if (Instance.State == ConnectionState.Broken)
					{
						Instance.Close();
					}

					if (Instance.State == ConnectionState.Closed)
					{
						for (int i = 1; i <= retries; i++)
						{
							MySQL.CSOptions.ToConsole("Connection Attempt {0}.", i);

							Instance.Open();

							VitaNexCore.WaitWhile(() => (Instance.State == ConnectionState.Connecting), TimeSpan.FromSeconds(3));

							if (Instance.State == ConnectionState.Broken)
							{
								Instance.Close();
							}

							if (Instance.State != ConnectionState.Open)
							{
								continue;
							}

							MySQL.CSOptions.ToConsole("Connection Successful.");

							OnConnected();

							if (!String.IsNullOrWhiteSpace(Credentials.Database))
							{
								if (createDB)
								{
									NonQuery(
										@"CREATE DATABASE IF NOT EXISTS `{0}` DEFAULT CHARSET `utf8` DEFAULT COLLATE `utf8_bin`", Credentials.Database);
								}

								Instance.ChangeDatabase(Credentials.Database);
							}

							return true;
						}
					}

					if (Instance.State == ConnectionState.Broken)
					{
						Instance.Close();
					}

					return false;
				},
				MySQL.CSOptions.ToConsole);

			if (!connected)
			{
				MySQL.CSOptions.ToConsole("Connection Failed.");
				Close();
			}

			return connected;
		}

		public void ConnectAsync(int retries = 0, bool createDB = false, Action callback = null)
		{
			if (Connected || Connecting)
			{
				if (callback != null)
				{
					callback();
				}

				return;
			}

			new Func<int, bool, bool>(Connect).BeginInvoke(
				retries,
				createDB,
				o =>
				{
					if (callback != null)
					{
						callback();
					}
				},
				null);
		}

		protected virtual void OnConnected()
		{
			MySQL.Connected(this);
		}

		public virtual bool CanConnect()
		{
			return MySQL.CanConnect(this);
		}

		/// <summary>
		///     Switches to a different Database Name on the connected MySQL Server
		/// </summary>
		/// <param name="dbName">Database Name to switch to</param>
		public void UseDatabase(string dbName)
		{
			if (!Connected)
			{
				Credentials.Database = dbName;
			}
			else if (Credentials.Database != dbName)
			{
				Credentials.Database = dbName;

				if (!String.IsNullOrWhiteSpace(Credentials.Database))
				{
					Instance.ChangeDatabase(Credentials.Database);
				}
			}
		}

		public MySQLRow[] Query(string query)
		{
			if (!Connected || String.IsNullOrWhiteSpace(query))
			{
				return new MySQLRow[0];
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					var rows = new List<MySQLRow>();

					OdbcCommand cmd = Instance.CreateCommand();
					cmd.CommandText = query;

					OdbcDataReader reader = cmd.ExecuteReader();

					if (reader.HasRows)
					{
						int rowID = 0;

						while (reader.Read())
						{
							int count = reader.FieldCount;
							var results = new MySQLData[count];

							for (int x = 0; x < count; x++)
							{
								results[x] = new MySQLData(reader.GetName(x), reader.GetValue(x));
							}

							rows.Add(new MySQLRow(rowID, results));
							rowID++;
						}
					}

					return rows.ToArray();
				},
				MySQL.CSOptions.ToConsole) ?? new MySQLRow[0];
		}

		public int NonQuery(string query, params object[] args)
		{
			if (!Connected || String.IsNullOrWhiteSpace(query))
			{
				return 0;
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					OdbcCommand cmd = Instance.CreateCommand();
					cmd.CommandText = String.Format(query, args);

					return cmd.ExecuteNonQuery();
				},
				MySQL.CSOptions.ToConsole);
		}

		public MySQLRow[] Select(
			string table,
			string[] cols = null,
			MySQLData[] conditions = null,
			string sortBy = null,
			MySQLSortOrder order = MySQLSortOrder.None)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table))
			{
				return new MySQLRow[0];
			}

			var query = new StringBuilder();

			if (cols == null || cols.Length == 0)
			{
				cols = new[] {"*"};
			}

			if (cols.Length > 1)
			{
				query.AppendFormat("SELECT `{0}` FROM `{1}`", String.Join("`,`", cols), table);
			}
			else
			{
				query.AppendFormat("SELECT {0} FROM `{1}`", ((cols[0] == "*") ? "*" : "`" + cols[0] + "`"), table);
			}

			if (conditions != null && conditions.Length > 0)
			{
				query.Append(" WHERE ");

				for (int x = 0; x < conditions.Length; x++)
				{
					query.AppendFormat("`{0}`='{1}'", conditions[x].Key, conditions[x].Value);

					if (x + 1 < conditions.Length)
					{
						query.Append(" AND ");
					}
				}
			}

			if (!String.IsNullOrWhiteSpace(sortBy) && order != MySQLSortOrder.None)
			{
				query.AppendFormat(" ORDER BY `{0}` {1}", sortBy, order.ToString());
			}

			return Query(query.ToString()) ?? new MySQLRow[0];
		}

		public MySQLRow SelectRow(string table, string[] cols = null, MySQLData[] conditions = null)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table))
			{
				return null;
			}

			var rows = Select(table, cols, conditions);

			return rows.Length > 0 ? rows[0] : null;
		}

		public bool Replace(string table, MySQLData[] data)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table) || data == null || data.Length == 0)
			{
				return false;
			}

			return Insert(table, data, true);
		}

		public bool Insert(string table, MySQLData[] data, bool replace = false)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table) || data == null || data.Length == 0)
			{
				return false;
			}

			var query = new StringBuilder();

			query.AppendFormat("{0} INTO `{1}` ", replace ? "REPLACE" : "INSERT", table);

			string[] keys = new string[data.Length], vals = new string[data.Length];

			for (int x = 0; x < data.Length; x++)
			{
				keys[x] = data[x].Key;
				vals[x] = data[x].Value.ToString();
			}

			query.AppendFormat("(`{0}`)", ((keys.Length > 1) ? String.Join("`,`", keys) : keys[0]));
			query.Append(" VALUES ");
			query.AppendFormat("('{0}')", ((vals.Length > 1) ? String.Join("','", vals) : vals[0]));

			return NonQuery(query.ToString()) > 0;
		}

		public bool Update(string table, MySQLData[] data, MySQLData[] conditions = null, bool ensure = false)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table) || data == null || data.Length == 0)
			{
				return false;
			}

			var query = new StringBuilder();

			query.AppendFormat(ensure ? "REPLACE INTO `{0}` SET" : "UPDATE `{0}` SET", table);

			for (int x = 0; x < data.Length; x++)
			{
				query.AppendFormat("`{0}`='{1}'", data[x].Key, data[x].Value);

				if (x + 1 < data.Length)
				{
					query.Append(", ");
				}
			}

			if (!ensure && conditions != null && conditions.Length > 0)
			{
				query.Append(" WHERE ");

				for (int x = 0; x < conditions.Length; x++)
				{
					query.AppendFormat("`{0}`='{1}'", conditions[x].Key, conditions[x].Value);

					if (x + 1 < conditions.Length)
					{
						query.Append(" AND ");
					}
				}
			}

			return NonQuery(query.ToString()) > 0;
		}

		public bool Delete(string table, MySQLData[] conditions = null)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table))
			{
				return false;
			}

			var query = new StringBuilder();

			query.AppendFormat("DELETE FROM `{0}`", table);

			if (conditions != null && conditions.Length > 0)
			{
				query.Append(" WHERE ");

				for (int x = 0; x < conditions.Length; x++)
				{
					query.AppendFormat("`{0}`='{1}'", conditions[x].Key, conditions[x].Value);

					if (x + 1 < conditions.Length)
					{
						query.Append(" AND ");
					}
				}
			}

			return NonQuery(query.ToString()) > 0;
		}

		public bool Contains(string table, string col, MySQLData[] conditions = null)
		{
			if (!Connected || String.IsNullOrWhiteSpace(table) || String.IsNullOrWhiteSpace(col))
			{
				return false;
			}

			MySQLRow row = SelectRow(table, new[] {col}, conditions);

			if (row != null && row[col] != null)
			{
				return true;
			}

			return false;
		}

		/// <summary>
		///     Closes and Disposes the instance of Connection
		/// </summary>
		public void Close()
		{
			if (!Connected)
			{
				Instance = null;
				return;
			}

			if (Instance != null)
			{
				Instance.Close();
				Instance.Dispose();
				Instance = null;
			}

			MySQL.Disconnected(this);
		}

		public override string ToString()
		{
			return String.Format("[{0}] => '{1}'", Credentials.IP, Connected ? Instance.State.ToString() : "Disconnected");
		}
	}
}