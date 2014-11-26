#region Header
//   Vorspire    _,-'/-'/  AutoDonate.cs
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
using System.Data.Odbc;
using System.Linq;

using Server;
using Server.Accounting;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.MySQL;
using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public static partial class AutoDonate
	{
		public const AccessLevel Access = AccessLevel.Owner;

		private static MySQLConnection _Connection;

		private static AutoDonateOptions _CMOptions = new AutoDonateOptions();

		private static readonly BinaryDirectoryDataStore<IAccount, DonationProfile> _Profiles =
			new BinaryDirectoryDataStore<IAccount, DonationProfile>(
				VitaNexCore.SavesDirectory + "/AutoDonate", "Profiles", "pro");

		public static AutoDonateOptions CMOptions { get { return _CMOptions ?? (_CMOptions = new AutoDonateOptions()); } }

		public static BinaryDirectoryDataStore<IAccount, DonationProfile> Profiles { get { return _Profiles; } }

		public static MySQLConnection Connection
		{
			get
			{
				if (_Connection != null && !_Connection.Credentials.Equals(CMOptions.MySQL))
				{
					_Connection.Credentials = CMOptions.MySQL;
				}

				return _Connection ?? (_Connection = new MySQLConnection(CMOptions.MySQL));
			}
		}

		private static void OnTransDelivered(DonationEvents.TransDeliveredEventArgs e)
		{
			SpotCheck(e.Transaction.DeliverTo as PlayerMobile);
		}

		private static void OnLogin(LoginEventArgs e)
		{
			SpotCheck(e.Mobile as PlayerMobile);
		}

		private static void SpotCheck(PlayerMobile user)
		{
			if (user == null || user.Deleted || !user.IsOnline() || !user.Alive || CMOptions.Status != DataStoreStatus.Idle ||
				CMOptions.CurrencyType == null || CMOptions.ExchangeRate <= 0)
			{
				return;
			}

			DonationProfile profile = Find(user.Account);

			if (profile == null)
			{
				return;
			}

			if (profile.Any(trans => !trans.Hidden && trans.State == DonationTransactionState.Processed))
			{
				SuperGump.Send(
					new ConfirmDialogGump(
						user,
						null,
						title: "You Have Rewards!",
						html: "You have unclaimed donation rewards!\nWould you like to view them now?",
						onAccept: b => CheckDonate(user)));
			}
		}

		public static void Sync()
		{
			InvokeMySQL(
				() =>
				{
					ImportMySQL();
					Profiles.Export();
					ExportMySQL();

					if (_Connection == null)
					{
						return;
					}

					if (_Connection.Connected)
					{
						_Connection.Close();
					}

					_Connection = null;
				});
		}

		private static void InvokeMySQL(Action callback)
		{
			Action process = () =>
			{
				if (Connection.Connected)
				{
					Connection.UseDatabase(CMOptions.MySQL.Database);

					string query = @"CREATE TABLE IF NOT EXISTS `" + CMOptions.TableName + @"` (" +
								   @"`id` varchar(255) collate utf8_bin NOT NULL," + @"`state` varchar(255) collate utf8_bin NOT NULL," +
								   @"`account` varchar(255) collate utf8_bin NOT NULL," + @"`email` varchar(255) collate utf8_bin NOT NULL," +
								   @"`total` decimal(10,2) NOT NULL," + @"`credit` bigint(20) NOT NULL," + @"`time` int(11) NOT NULL," +
								   @"`version` int(11)," + @"`notes` text collate utf8_bin," + @"`extra` text collate utf8_bin," +
								   @"PRIMARY KEY  (`id`)," + @"KEY `state` (`state`)," + @"KEY `account` (`account`)," +
								   @"KEY `email` (`email`)" + @") ENGINE=MyISAM DEFAULT CHARSET=utf8 COLLATE=utf8_bin;";

					if (Connection.NonQuery(query) > 0)
					{
						CMOptions.ToConsole("Created MySQL Table '{0}'.", CMOptions.TableName);
					}
				}

				if (callback != null)
				{
					callback();
				}
			};

			TimerCallback action = () =>
			{
				if (!Core.Closing)
				{
					Connection.ConnectAsync(0, true, process);
					return;
				}

				Connection.Connect(0, true);
				process();
			};

			if (Core.Closing || Connection.Connected)
			{
				action();
			}
			else
			{
				Timer.DelayCall(TimeSpan.FromSeconds(3), action);
			}
		}

		private static void ImportMySQL()
		{
			ImportMySQL(DonationTransactionState.Pending);
			ImportMySQL(DonationTransactionState.Processed);
			ImportMySQL(DonationTransactionState.Claimed);
			ImportMySQL(DonationTransactionState.Void);
		}

		private static void ImportMySQL(DonationTransactionState state)
		{
			Connection.UseDatabase(CMOptions.MySQL.Database);

			int count = 0;

			var rows = Connection.Select(
				CMOptions.TableName,
				null,
				new[] {new MySQLCondition("state", state.ToString().ToUpper())},
				"time",
				MySQLSortOrder.ASC);

			if (Connection.HasError)
			{
				if (!CMOptions.ModuleQuietMode)
				{
					foreach (OdbcError e in Connection.Errors)
					{
						OnExceptionThrown(new Exception(e.Message), "OdbcError");
					}
				}
			}
			else if (rows.Length > 0)
			{
				var gTrans = new List<DonationTransaction>(rows.Length);

				foreach (MySQLRow row in rows)
				{
					VitaNexCore.TryCatch(
						() =>
						{
							var total = row["total"].GetValue<decimal>();
							var credit = row["credit"].GetValue<long>();
							int time = row["time"].GetValue<int>(), version = row["version"].GetValue<int>();
							string id = row["id"].GetValue<string>(),
								   email = row["email"].GetValue<string>(),
								   notes = row["notes"].GetValue<string>(),
								   extra = row["extra"].GetValue<string>(),
								   status = row["state"].GetValue<string>(),
								   account = row["account"].GetValue<string>();

							IAccount a = Accounts.GetAccount(account ?? String.Empty) ??
										 Accounts.GetAccounts().FirstOrDefault(ac => ac.AccessLevel == AccessLevel.Owner);

							DonationTransactionState s;

							if (a == null || !Enum.TryParse(status, true, out s))
							{
								s = DonationTransactionState.Void;
							}

							gTrans.Add(new DonationTransaction(id, s, a, email, total, credit, time, version, notes, extra));

							++count;
						},
						e => OnExceptionThrown(e, "Could not load MySQL data for transaction in row {0:#,0}", row.ID));
				}

				gTrans.TrimExcess();

				foreach (DonationTransaction trans in gTrans)
				{
					VitaNexCore.TryCatch(
						() =>
						{
							DonationProfile dp;

							if (!Profiles.TryGetValue(trans.Account, out dp))
							{
								Profiles.Add(trans.Account, dp = new DonationProfile(trans.Account));
							}
							else if (dp == null)
							{
								Profiles[trans.Account] = dp = new DonationProfile(trans.Account);
							}

							if (!dp.Contains(trans))
							{
								dp.Add(trans);
							}

							switch (trans.State)
							{
								case DonationTransactionState.Pending:
									{
										if (dp.Process(trans))
										{ }
									}
									break;
								case DonationTransactionState.Processed:
									break;
								case DonationTransactionState.Claimed:
									break;
								case DonationTransactionState.Void:
									{
										if (dp.Void(trans))
										{ }
									}
									break;
							}
						},
						e => OnExceptionThrown(e, "Could not load MySQL data for transaction ID {0}", trans.ID));
				}
			}

			CMOptions.ToConsole("Imported {0} {1} transactions.", count, state);
		}

		private static void ExportMySQL()
		{
			ExportMySQL(DonationTransactionState.Pending);
			ExportMySQL(DonationTransactionState.Processed);
			ExportMySQL(DonationTransactionState.Claimed);
			ExportMySQL(DonationTransactionState.Void);
		}

		private static void ExportMySQL(DonationTransactionState state)
		{
			Connection.UseDatabase(CMOptions.MySQL.Database);

			int count = 0;

			foreach (DonationTransaction trans in
				Profiles.Values.AsParallel().SelectMany(dp => dp.Where(t => t != null && t.State == state)))
			{
				VitaNexCore.TryCatch(
					() =>
					{
						bool success;

						if (Connection.Contains(CMOptions.TableName, "id", new[] {new MySQLCondition("id", trans.ID)}))
						{
							success = Connection.Update(
								CMOptions.TableName,
								new[]
								{
									new MySQLData("state", trans.State.ToString().ToUpper()), new MySQLData("version", trans.Version),
									new MySQLData("notes", trans.Notes), new MySQLData("extra", trans.Extra)
								},
								new[] {new MySQLCondition("id", trans.ID)});
						}
						else
						{
							success = Connection.Insert(
								CMOptions.TableName,
								new[]
								{
									new MySQLData("id", trans.ID), new MySQLData("state", trans.State.ToString().ToUpper()),
									new MySQLData("account", trans.Account.Username), new MySQLData("email", trans.Email),
									new MySQLData("total", trans.Total), new MySQLData("credit", trans.Credit.Value),
									new MySQLData("time", trans.Time.Stamp), new MySQLData("version", trans.Version),
									new MySQLData("notes", trans.Notes), new MySQLData("extra", trans.Extra)
								});
						}

						if (success)
						{
							++count;
						}
					},
					e => OnExceptionThrown(e, "Could not save MySQL data for transaction ID {0}", trans.ID));
			}

			CMOptions.ToConsole("Exported {0} {1} transactions.", count, state);
		}

		public static void Register(IAccount a, DonationProfile dp)
		{
			if (a == null || dp == null)
			{
				if (a == null && dp == null)
				{
					OnExceptionThrown(new ArgumentNullException("a"), "Failed to register unknown DonationProfile to unknown Account");
				}
				else if (a == null)
				{
					OnExceptionThrown(
						new ArgumentNullException("a"), "Failed to register DonationProfile `{0}` to unknown Account", dp);
				}
				else
				{
					OnExceptionThrown(new ArgumentNullException("a"), "Failed to register unknown DonationProfile to Account `{0}`", a);
				}

				return;
			}

			if (!Profiles.ContainsKey(a))
			{
				Profiles.Add(a, dp);
			}
			else
			{
				Profiles[a] = dp;
			}
		}

		public static void CheckDonate(PlayerMobile user, bool message = true)
		{
			if (user == null || user.Deleted)
			{
				return;
			}

			if (!CMOptions.ModuleEnabled)
			{
				if (message)
				{
					user.SendMessage("The donation system is currently unavailable, please try again later.");
				}

				return;
			}

			if (!user.Alive)
			{
				if (message)
				{
					user.SendMessage("You must be alive to use this command!");
				}

				return;
			}

			if (CMOptions.Status != DataStoreStatus.Idle)
			{
				if (message)
				{
					user.SendMessage("The donation system is busy, please try again in a few moments.");
				}

				return;
			}

			if (CMOptions.CurrencyType == null || CMOptions.ExchangeRate <= 0)
			{
				if (message)
				{
					user.SendMessage("Currency conversion is currently disabled - contact a member of staff to handle your donations.");
				}

				return;
			}

			DonationProfile profile = Find(user.Account);

			if (profile != null)
			{
				if (profile.Transactions.Count == 0 || profile.TrueForAll(trans => trans.Hidden))
				{
					if (message)
					{
						user.SendMessage("There are no current donation records for your account.");
					}
				}
				else
				{
					if (message)
					{
						user.SendMessage("Thank you for your donation{0}, {1}!", profile.Transactions.Count != 1 ? "s" : "", user.RawName);
					}

					new DonationProfileGump(user, profile).Send();
				}
			}
			else if (message)
			{
				user.SendMessage("There are no current donation records for your account.");
			}
		}

		public static void CheckConfig(PlayerMobile user)
		{
			if (user != null && !user.Deleted && user.AccessLevel >= Access)
			{
				user.SendGump(new PropertiesGump(user, CMOptions));
			}
		}

		public static DonationProfile Find(IAccount a)
		{
			return Validate(a) ? Profiles[a] : null;
		}

		public static IAccount Find(DonationProfile dp)
		{
			return dp != null && Profiles.ContainsValue(dp) ? Profiles.FirstOrDefault(kvp => kvp.Value == dp).Key : null;
		}

		public static bool Validate(IAccount a)
		{
			return a != null && Profiles.ContainsKey(a);
		}

		private static void OnExceptionThrown(Exception e, string message, params object[] args)
		{
			CMOptions.ToConsole(e);

			if (args != null)
			{
				if (!String.IsNullOrWhiteSpace(message))
				{
					CMOptions.ToConsole(message, args);
				}

				DonationEvents.InvokeException(e, message, args);
			}
			else
			{
				if (!String.IsNullOrWhiteSpace(message))
				{
					CMOptions.ToConsole(message);
				}

				DonationEvents.InvokeException(e, message);
			}
		}
	}
}