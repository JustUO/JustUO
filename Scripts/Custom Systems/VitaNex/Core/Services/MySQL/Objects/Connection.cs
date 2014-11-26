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
using System.Linq;
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

	public class MySQLConnection : IDisposable
	{
		public bool IsDisposed { get; private set; }

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

		private volatile OdbcTransaction _Transaction;

		public OdbcTransaction Transaction { get { return _Transaction; } }

		public MySQLConnection(MySQLConnectionInfo credentials)
		{
			Credentials = credentials;
		}

		/// <summary>
		///     Called when the Connection instance receives any messages.
		///     Resets the MySQLConnection object Message and Errors members.
		/// </summary>
		/// <param name="sender">Sender object</param>
		/// <param name="e">Odbc Message Event Arguments</param>
		private void OnMessage(object sender, OdbcInfoMessageEventArgs e)
		{
			if (IsDisposed)
			{
				return;
			}

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
			if (IsDisposed)
			{
				return false;
			}

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

						VitaNexCore.WaitWhile(() => Instance.State == ConnectionState.Connecting, TimeSpan.FromSeconds(3));

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

					if (Instance.State != ConnectionState.Open)
					{
						Instance.Close();

						for (int i = 1; i <= retries; i++)
						{
							MySQL.CSOptions.ToConsole("Connection Attempt {0}.", i);

							Instance.Open();

							VitaNexCore.WaitWhile(() => Instance.State == ConnectionState.Connecting, TimeSpan.FromSeconds(3));

							if (Instance.State != ConnectionState.Open)
							{
								Instance.Close();
								continue;
							}

							MySQL.CSOptions.ToConsole("Connection Successful.");

							OnConnected();

							if (!String.IsNullOrWhiteSpace(Credentials.Database))
							{
								if (createDB)
								{
									NonQuery(
										@"CREATE DATABASE IF NOT EXISTS `{0}` DEFAULT CHARSET `utf8` DEFAULT COLLATE `utf8_bin` DEFAULT ENGINE `INNODB`",
										Credentials.Database);
								}

								Instance.ChangeDatabase(Credentials.Database);
							}

							return true;
						}
					}

					if (Instance.State != ConnectionState.Open)
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
			ConnectAsync(
				retries,
				createDB,
				b =>
				{
					if (callback != null)
					{
						callback();
					}
				});
		}

		public void ConnectAsync(int retries = 0, bool createDB = false, Action<bool> callback = null)
		{
			if (IsDisposed)
			{
				return;
			}

			if (Connected)
			{
				if (callback != null)
				{
					callback(true);
				}

				return;
			}

			if (Connecting)
			{
				if (callback != null)
				{
					callback(false);
				}

				return;
			}

			var f = new Func<int, bool, bool>(Connect);

			f.BeginInvoke(
				retries,
				createDB,
				o =>
				{
					bool connected = f.EndInvoke(o);

					if (callback != null)
					{
						callback(connected);
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
			return !IsDisposed && MySQL.CanConnect(this);
		}

		/// <summary>
		///     Switches to a different Database Name on the connected MySQL Server
		/// </summary>
		/// <param name="dbName">Database Name to switch to</param>
		public void UseDatabase(string dbName)
		{
			if (IsDisposed)
			{
				return;
			}

			if (Connected && !String.IsNullOrWhiteSpace(dbName))
			{
				Instance.ChangeDatabase(dbName);
			}
		}

		public void BeginTransaction(Action callback)
		{
			BeginTransaction<object>(o => callback(), null);
		}

		public void BeginTransaction<T>(Action<T> callback, T state)
		{
			if (_Transaction != null)
			{
				EndTransaction();
			}

			_Transaction = Instance.BeginTransaction();

			if (callback != null)
			{
				callback(state);
			}
		}

		public void EndTransaction()
		{
			if (_Transaction == null)
			{
				return;
			}

			using (_Transaction)
			{
				if (_Transaction.Connection == null)
				{
					_Transaction = null;
					return;
				}

				VitaNexCore.TryCatch(
					_Transaction.Commit,
					x =>
					{
						MySQL.CSOptions.ToConsole(x);

						VitaNexCore.TryCatch(_Transaction.Rollback, MySQL.CSOptions.ToConsole);
					});

				_Transaction = null;
			}
		}

		private static void AppendWhere(StringBuilder query, params MySQLCondition[] conditions)
		{
			if (query == null || query.Length == 0 || conditions == null || conditions.Length == 0)
			{
				return;
			}

			query.Append(" WHERE ");

			for (int x = 0; x < conditions.Length; x++)
			{
				if (x > 0)
				{
					query.AppendFormat(" {0} ", conditions[x].QueryJoin);
				}

				query.AppendFormat(
					"`{0}` {1} '{2}'", conditions[x].Key, conditions[x].GetOperation(), MySQL.Escape(conditions[x].ValueString));
			}
		}

		public MySQLRow[] Query(string query, params object[] args)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(query))
			{
				return new MySQLRow[0];
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					var rows = new List<MySQLRow>();

					OdbcCommand cmd = Instance.CreateCommand();

					if (_Transaction != null)
					{
						cmd.Transaction = _Transaction;
					}

					if (args == null || args.Length == 0)
					{
						cmd.CommandText = query;
					}
					else
					{
						cmd.CommandText = String.Format(query, args);
					}

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
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(query))
			{
				return 0;
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					OdbcCommand cmd = Instance.CreateCommand();

					if (_Transaction != null)
					{
						cmd.Transaction = _Transaction;
					}

					if (args == null || args.Length == 0)
					{
						cmd.CommandText = query;
					}
					else
					{
						cmd.CommandText = String.Format(query, args);
					}

					return cmd.ExecuteNonQuery();
				},
				MySQL.CSOptions.ToConsole);
		}

		[Obsolete(
			"Use `Select` where the `conditions` argument is of type `MySQLCondition[]`. " +
			"This overload may be removed in a future version.", false)]
		public MySQLRow[] Select(
			string table,
			string[] cols = null,
			MySQLData[] conditions = null,
			string sortBy = null,
			MySQLSortOrder order = MySQLSortOrder.None,
			int offset = 0,
			int count = 0)
		{
			return Select(
				table,
				cols,
				conditions != null ? conditions.Select(d => new MySQLCondition(d.Key, d.Value)).ToArray() : null,
				sortBy,
				order,
				offset,
				count);
		}

		public MySQLRow[] Select(
			string table,
			string[] cols = null,
			MySQLCondition[] conditions = null,
			string sortBy = null,
			MySQLSortOrder order = MySQLSortOrder.None,
			int offset = 0,
			int count = 0)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table))
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
				query.AppendFormat("SELECT {0} FROM `{1}`", (cols[0] == "*" ? "*" : "`" + cols[0] + "`"), table);
			}

			AppendWhere(query, conditions);

			if (!String.IsNullOrWhiteSpace(sortBy) && order != MySQLSortOrder.None)
			{
				query.AppendFormat(" ORDER BY `{0}` {1}", sortBy, order);
			}

			if (count > 0 && offset >= 0)
			{
				query.AppendFormat(" LIMIT {0}, {1}", offset, count);
			}

			return Query(query.ToString()) ?? new MySQLRow[0];
		}

		[Obsolete(
			"Use `SelectRow` where the `conditions` argument is of type `MySQLCondition[]`. " +
			"This overload may be removed in a future version.", false)]
		public MySQLRow SelectRow(string table, string[] cols = null, MySQLData[] conditions = null)
		{
			return SelectRow(
				table, cols, conditions != null ? conditions.Select(d => new MySQLCondition(d.Key, d.Value)).ToArray() : null);
		}

		public MySQLRow SelectRow(string table, string[] cols = null, MySQLCondition[] conditions = null)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table))
			{
				return null;
			}

			var rows = Select(table, cols, conditions, null, MySQLSortOrder.None, 0, 1);

			return rows.Length > 0 ? rows[0] : null;
		}

		public int ReplaceMany(string table, MySQLData[][] batch)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table) || batch == null || batch.Length == 0)
			{
				return -1;
			}

			return InsertMany(table, batch, true);
		}

		public bool Replace(string table, MySQLData[] data)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table) || data == null || data.Length == 0)
			{
				return false;
			}

			return Insert(table, data, true);
		}

		public int InsertMany(string table, MySQLData[][] batch, bool replace = false)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table) || batch == null || batch.Length == 0)
			{
				return -1;
			}

			var query = new StringBuilder();

			query.AppendFormat("{0} INTO `{1}` ", replace ? "REPLACE" : "INSERT", table);

			bool defined = false;
			bool empty = true;

			foreach (var data in batch.Where(data => data != null && data.Length > 0))
			{
				string[] keys = new string[data.Length], vals = new string[data.Length];

				for (int x = 0; x < data.Length; x++)
				{
					keys[x] = data[x].Key;
					vals[x] = data[x].Value != null ? data[x].Value.ToString() : String.Empty;
					vals[x] = MySQL.Escape(vals[x]);
				}

				if (!defined)
				{
					query.AppendFormat("(`{0}`)", ((keys.Length > 1) ? String.Join("`,`", keys) : keys[0]));
					query.Append(" VALUES ");

					defined = true;
				}

				query.AppendFormat(
					"{0}('{1}')", !empty ? ", " : String.Empty, ((vals.Length > 1) ? String.Join("','", vals) : vals[0]));

				empty = false;
			}

			return NonQuery(query.ToString());
		}

		public bool Insert(string table, MySQLData[] data, bool replace = false)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table) || data == null || data.Length == 0)
			{
				return false;
			}

			var query = new StringBuilder();

			query.AppendFormat("{0} INTO `{1}` ", replace ? "REPLACE" : "INSERT", table);

			string[] keys = new string[data.Length], vals = new string[data.Length];

			for (int x = 0; x < data.Length; x++)
			{
				keys[x] = data[x].Key;
				vals[x] = data[x].Value != null ? data[x].ValueString : String.Empty;
				vals[x] = MySQL.Escape(vals[x]);
			}

			query.AppendFormat("(`{0}`)", ((keys.Length > 1) ? String.Join("`,`", keys) : keys[0]));
			query.Append(" VALUES ");
			query.AppendFormat("('{0}')", ((vals.Length > 1) ? String.Join("','", vals) : vals[0]));

			return NonQuery(query.ToString()) > 0;
		}

		[Obsolete(
			"Use `Update` where the `conditions` argument is of type `MySQLCondition[]`. " +
			"This overload may be removed in a future version.", false)]
		public bool Update(string table, MySQLData[] data, MySQLData[] conditions = null, bool ensure = false)
		{
			return Update(
				table,
				data,
				conditions != null ? conditions.Select(d => new MySQLCondition(d.Key, d.Value)).ToArray() : null,
				ensure);
		}

		public bool Update(string table, MySQLData[] data, MySQLCondition[] conditions = null, bool ensure = false)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table) || data == null || data.Length == 0)
			{
				return false;
			}

			var query = new StringBuilder();

			query.AppendFormat(ensure ? "REPLACE INTO `{0}` SET" : "UPDATE `{0}` SET", table);

			for (int x = 0; x < data.Length; x++)
			{
				query.AppendFormat("`{0}` = '{1}'", data[x].Key, MySQL.Escape(data[x].ValueString));

				if (x + 1 < data.Length)
				{
					query.Append(", ");
				}
			}

			if (!ensure)
			{
				AppendWhere(query, conditions);
			}

			return NonQuery(query.ToString()) > 0;
		}

		[Obsolete(
			"Use `Delete` where the `conditions` argument is of type `MySQLCondition[]`. " +
			"This overload may be removed in a future version.", false)]
		public bool Delete(string table, MySQLData[] conditions = null)
		{
			return Delete(
				table, conditions != null ? conditions.Select(d => new MySQLCondition(d.Key, d.Value)).ToArray() : null);
		}

		public bool Delete(string table, MySQLCondition[] conditions = null)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table))
			{
				return false;
			}

			var query = new StringBuilder();

			query.AppendFormat("DELETE FROM `{0}`", table);

			AppendWhere(query, conditions);

			return NonQuery(query.ToString()) > 0;
		}

		[Obsolete(
			"Use `Contains` where the `conditions` argument is of type `MySQLCondition[]`. " +
			"This overload may be removed in a future version.", false)]
		public bool Contains(string table, string col, MySQLData[] conditions = null)
		{
			return Contains(
				table, col, conditions != null ? conditions.Select(d => new MySQLCondition(d.Key, d.Value)).ToArray() : null);
		}

		public bool Contains(string table, string col, MySQLCondition[] conditions = null)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table) || String.IsNullOrWhiteSpace(col))
			{
				return false;
			}

			MySQLRow row = SelectRow(table, new[] {col}, conditions);

			if (row != null && row[col] != MySQLData.Empty)
			{
				return true;
			}

			return false;
		}

		public bool Truncate(string table)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table))
			{
				return false;
			}

			return NonQuery("TRUNCATE TABLE `{0}`", table) > 0;
		}

		public bool TableExists(string table)
		{
			if (IsDisposed || !Connected || String.IsNullOrWhiteSpace(table))
			{
				return false;
			}

			var rows = Query("SHOW TABLES LIKE '{0}'", table);

			return rows != null && rows.Length > 0;
		}

		public string[] GetTables()
		{
			if (IsDisposed || !Connected)
			{
				return new string[0];
			}

			var rows = Query("SHOW TABLES");

			return rows != null ? rows.SelectMany(r => r.Results.Select(r2 => r2.Value.ValueString)).ToArray() : new string[0];
		}

		private static string GetDataTypeString(SimpleType o, bool primary)
		{
			string result = String.Empty;

			switch (o.Flag)
			{
				case DataType.String:
					result = "TEXT";
					break;

				case DataType.DateTime:
				case DataType.TimeSpan:
					result = "INT";
					break;

				case DataType.Char:
					result = "CHAR";
					break;

				case DataType.Bool:
					result = "TINYINT UNSIGNED";
					break;

				case DataType.Byte:
					result = "TINYINT UNSIGNED";
					break;
				case DataType.SByte:
					result = "TINYINT";
					break;

				case DataType.UShort:
					result = "SMALLINT UNSIGNED";
					break;
				case DataType.Short:
					result = "SMALLINT";
					break;

				case DataType.UInt:
					result = "INT UNSIGNED";
					break;
				case DataType.Int:
					result = "INT";
					break;

				case DataType.ULong:
					result = "BIGINT UNSIGNED";
					break;
				case DataType.Long:
					result = "BIGINT";
					break;

				case DataType.Decimal:
					result = "DECIMAL";
					break;
				case DataType.Double:
					result = "DOUBLE";
					break;
				case DataType.Float:
					result = "FLOAT";
					break;
			}

			if (result == "TEXT" && primary)
			{
				result = "VARCHAR (255)";
			}

			return result;
		}

		public string GetCreateTableQuery(string table, IDictionary<string, SimpleType> data, string engine = "INNODB")
		{
			if (String.IsNullOrWhiteSpace(table) || data == null || data.Count == 0)
			{
				return String.Empty;
			}

			StringBuilder query = new StringBuilder("CREATE TABLE IF NOT EXISTS `" + table + "` ( ");

			int key = 0;
			string primaryKey = "";

			foreach (var kv in data)
			{
				if (key == 0)
				{
					query.AppendFormat("`{0}` {1} NOT NULL, ", kv.Key, GetDataTypeString(kv.Value, true));
					primaryKey = kv.Key;
				}
				else
				{
					query.AppendFormat("`{0}` {1} NULL, ", kv.Key, GetDataTypeString(kv.Value, false));
				}

				++key;
			}

			if (!String.IsNullOrWhiteSpace(primaryKey))
			{
				query.AppendFormat("PRIMARY KEY (`{0}`) )", primaryKey);
			}
			else
			{
				query.Append(" )");
			}

			if (!String.IsNullOrWhiteSpace(engine))
			{
				query.AppendFormat(" ENGINE = {0}", engine);
			}

			return query.ToString();
		}

		public bool CreateTable(string table, IDictionary<string, SimpleType> data)
		{
			if (String.IsNullOrWhiteSpace(table) || data == null || data.Count == 0)
			{
				return false;
			}

			NonQuery(GetCreateTableQuery(table, data));

			return TableExists(table);
		}

		/// <summary>
		///     Closes and Disposes the instance of Connection
		/// </summary>
		public void Close()
		{
			if (IsDisposed)
			{
				return;
			}

			bool wasConnected = Connected;

			EndTransaction();

			if (Instance != null)
			{
				Instance.Close();
				Instance.Dispose();
				Instance = null;
			}

			if (wasConnected)
			{
				MySQL.Disconnected(this);
			}
		}

		public override string ToString()
		{
			return String.Format("[{0}] => '{1}'", Credentials.IP, Connected ? Instance.State.ToString() : "Disconnected");
		}

		~MySQLConnection()
		{
			Dispose();
		}

		public void Dispose()
		{
			if (IsDisposed)
			{
				return;
			}

			Close();

			IsDisposed = true;

			GC.SuppressFinalize(this);
		}
	}
}