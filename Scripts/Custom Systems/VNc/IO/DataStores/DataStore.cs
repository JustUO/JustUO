#region Header
//   Vorspire    _,-'/-'/  DataStore.cs
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
#endregion

namespace VitaNex.IO
{
	public enum DataStoreStatus
	{
		Idle,
		Disposed,
		Initializing,
		Importing,
		Exporting,
		Copying
	}

	public enum DataStoreResult
	{
		Null,
		Busy,
		Error,
		OK
	}

	public interface IDataStore
	{
		DirectoryInfo Root { get; set; }
		string Name { get; set; }
		DataStoreStatus Status { get; }
		List<Exception> Errors { get; }
		bool HasErrors { get; }

		DataStoreResult Import();
		DataStoreResult Export();
	}

	public abstract class DataStore<TKey, TVal> : Dictionary<TKey, TVal>, IDataStore, IDisposable
	{
		private static int _InternalCounter;

		public Dictionary<TKey, TVal> Registry { get { return this; } }

		public new virtual TVal this[TKey key] { get { return base[key]; } set { base[key] = value; } }

		public virtual DirectoryInfo Root { get; set; }
		public virtual string Name { get; set; }

		public DataStoreStatus Status { get; private set; }
		public List<Exception> Errors { get; private set; }

		public bool HasErrors { get { return (Errors.Count > 0); } }

		public DataStore(string root, string name = null)
			: this(IOUtility.EnsureDirectory(root), name)
		{ }

		public DataStore(DirectoryInfo root, string name = null)
		{
			_InternalCounter++;

			Status = DataStoreStatus.Initializing;
			Errors = new List<Exception>();

			if (String.IsNullOrWhiteSpace(name))
			{
				name = "DataStore" + _InternalCounter;
			}

			Name = name;

			try
			{
				Root = root.EnsureDirectory(false);
			}
			catch
			{
				Root = IOUtility.EnsureDirectory(VitaNexCore.SavesDirectory + "/DataStores/" + Name);
			}

			Status = DataStoreStatus.Idle;
		}

		public virtual DataStoreResult Import()
		{
			try
			{
				if (Status != DataStoreStatus.Idle)
				{
					return DataStoreResult.Busy;
				}

				Errors.Clear();

				Status = DataStoreStatus.Importing;

				try
				{
					OnImport();
				}
				catch (Exception e1)
				{
					Errors.Add(e1);
					Status = DataStoreStatus.Idle;
					return DataStoreResult.Error;
				}

				Status = DataStoreStatus.Idle;
				return DataStoreResult.OK;
			}
			catch (Exception e2)
			{
				Errors.Add(e2);
				Status = DataStoreStatus.Idle;
				return DataStoreResult.Error;
			}
		}

		public virtual DataStoreResult Export()
		{
			try
			{
				if (Status != DataStoreStatus.Idle)
				{
					return DataStoreResult.Busy;
				}

				Errors.Clear();

				Status = DataStoreStatus.Exporting;

				try
				{
					OnExport();
				}
				catch (Exception e1)
				{
					Errors.Add(e1);
					Status = DataStoreStatus.Idle;
					return DataStoreResult.Error;
				}

				Status = DataStoreStatus.Idle;
				return DataStoreResult.OK;
			}
			catch (Exception e2)
			{
				Errors.Add(e2);
				Status = DataStoreStatus.Idle;
				return DataStoreResult.Error;
			}
		}

		public virtual DataStoreResult CopyTo(DataStore<TKey, TVal> dbTarget)
		{
			return CopyTo(dbTarget, true);
		}

		public virtual DataStoreResult CopyTo(DataStore<TKey, TVal> dbTarget, bool replace)
		{
			try
			{
				Errors.Clear();

				if (Status != DataStoreStatus.Idle)
				{
					return DataStoreResult.Busy;
				}

				if (this == dbTarget)
				{
					return DataStoreResult.OK;
				}

				Status = DataStoreStatus.Copying;

				foreach (var kvp in this)
				{
					if (!dbTarget.ContainsKey(kvp.Key))
					{
						dbTarget.Add(kvp.Key, kvp.Value);
					}
					else if (replace)
					{
						dbTarget[kvp.Key] = kvp.Value;
					}
				}

				try
				{
					OnCopiedTo(dbTarget);
				}
				catch (Exception e1)
				{
					Errors.Add(e1);
					Status = DataStoreStatus.Idle;
					return DataStoreResult.Error;
				}

				Status = DataStoreStatus.Idle;
				return DataStoreResult.OK;
			}
			catch (Exception e2)
			{
				Errors.Add(e2);
				Status = DataStoreStatus.Idle;
				return DataStoreResult.Error;
			}
		}

		protected virtual void OnCopiedTo(DataStore<TKey, TVal> dbTarget)
		{ }

		protected virtual void OnImport()
		{ }

		protected virtual void OnExport()
		{ }

		public void Dispose()
		{
			Status = DataStoreStatus.Disposed;
			Clear();
			Errors.Clear();
			_InternalCounter = 0;
			Name = null;
			Root = null;
		}

		public override string ToString()
		{
			return Name ?? base.ToString();
		}
	}

	public class DataStore<T> : DataStore<int, T>
	{
		public DataStore(string root, string name = null)
			: base(root, name)
		{ }

		public DataStore(DirectoryInfo root, string name = null)
			: base(root, name)
		{ }
	}
}