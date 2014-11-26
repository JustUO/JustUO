#region Header
//   Vorspire    _,-'/-'/  Events.cs
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

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public static class DonationEvents
	{
		#region DatabaseImported
		public delegate void DatabaseImported(DatabaseImportedEventArgs e);

		public static event DatabaseImported OnDatabaseImported;

		public static void InvokeDatabaseImported(IDataStore db)
		{
			if (OnDatabaseImported != null)
			{
				OnDatabaseImported.Invoke(new DatabaseImportedEventArgs(db));
			}
		}

		public sealed class DatabaseImportedEventArgs : EventArgs
		{
			public DatabaseImportedEventArgs(IDataStore db)
			{
				Database = db;
			}

			public IDataStore Database { get; private set; }
		}
		#endregion DatabaseImported

		#region DatabaseExported
		public delegate void DatabaseExported(DatabaseExportedEventArgs e);

		public static event DatabaseExported OnDatabaseExported;

		public static void InvokeDatabaseExported(IDataStore db)
		{
			if (OnDatabaseExported != null)
			{
				OnDatabaseExported.Invoke(new DatabaseExportedEventArgs(db));
			}
		}

		public sealed class DatabaseExportedEventArgs : EventArgs
		{
			public DatabaseExportedEventArgs(IDataStore db)
			{
				Database = db;
			}

			public IDataStore Database { get; private set; }
		}
		#endregion DatabaseExported

		#region TransDelivered
		public delegate void TransDelivered(TransDeliveredEventArgs e);

		public static event TransDelivered OnTransDelivered;

		public static void InvokeTransDelivered(DonationTransaction trans)
		{
			if (OnTransDelivered != null)
			{
				OnTransDelivered.Invoke(new TransDeliveredEventArgs(trans));
			}
		}

		public sealed class TransDeliveredEventArgs : EventArgs
		{
			public TransDeliveredEventArgs(DonationTransaction trans)
			{
				Transaction = trans;
			}

			public DonationTransaction Transaction { get; private set; }
		}
		#endregion TransDelivered

		#region TransVoided
		public delegate void TransVoided(TransVoidedEventArgs e);

		public static event TransVoided OnTransVoided;

		public static void InvokeTransVoided(DonationTransaction trans)
		{
			if (OnTransVoided != null)
			{
				OnTransVoided.Invoke(new TransVoidedEventArgs(trans));
			}
		}

		public sealed class TransVoidedEventArgs : EventArgs
		{
			public TransVoidedEventArgs(DonationTransaction trans)
			{
				Transaction = trans;
			}

			public DonationTransaction Transaction { get; private set; }
		}
		#endregion TransVoided

		#region TransClaimed
		public delegate void TransClaimed(TransClaimedEventArgs e);

		public static event TransClaimed OnTransClaimed;

		public static void InvokeTransClaimed(DonationTransaction trans)
		{
			if (OnTransClaimed != null)
			{
				OnTransClaimed.Invoke(new TransClaimedEventArgs(trans));
			}
		}

		public sealed class TransClaimedEventArgs : EventArgs
		{
			public TransClaimedEventArgs(DonationTransaction trans)
			{
				Transaction = trans;
			}

			public DonationTransaction Transaction { get; private set; }
		}
		#endregion TransClaimed

		#region TransProcessed
		public delegate void TransProcessed(TransProcessedEventArgs e);

		public static event TransProcessed OnTransProcessed;

		public static void InvokeTransProcessed(DonationTransaction trans)
		{
			if (OnTransProcessed != null)
			{
				OnTransProcessed.Invoke(new TransProcessedEventArgs(trans));
			}
		}

		public sealed class TransProcessedEventArgs : EventArgs
		{
			public TransProcessedEventArgs(DonationTransaction trans)
			{
				Transaction = trans;
			}

			public DonationTransaction Transaction { get; private set; }
		}
		#endregion TransProcessed

		#region StateChanged
		public delegate void StateChanged(StateChangedEventArgs e);

		public static event StateChanged OnStateChanged;

		public static void InvokeStateChanged(DonationTransaction trans, DonationTransactionState oldState)
		{
			if (OnStateChanged != null)
			{
				OnStateChanged.Invoke(new StateChangedEventArgs(trans, oldState));
			}
		}

		public sealed class StateChangedEventArgs : EventArgs
		{
			public StateChangedEventArgs(DonationTransaction trans, DonationTransactionState oldState)
			{
				Transaction = trans;
				OldState = oldState;
			}

			public DonationTransaction Transaction { get; private set; }
			public DonationTransactionState OldState { get; private set; }
		}
		#endregion StateChanged

		#region ExceptionRaised
		public delegate void ExceptionRaised(ExceptionEventArgs e);

		public static event ExceptionRaised OnException;

		public static void InvokeException(Exception e, string format, params object[] args)
		{
			if (OnException != null)
			{
				OnException.Invoke(new ExceptionEventArgs(e, format ?? String.Empty, args));
			}
		}

		public sealed class ExceptionEventArgs : EventArgs
		{
			public ExceptionEventArgs(Exception ex, string format, params object[] args)
			{
				Exception = ex;
				Message = String.Format(format, args);
			}

			public Exception Exception { get; private set; }
			public string Message { get; private set; }
		}
		#endregion ExceptionRaised
	}
}