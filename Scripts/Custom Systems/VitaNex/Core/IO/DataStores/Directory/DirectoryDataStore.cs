#region Header
//   Vorspire    _,-'/-'/  DirectoryDataStore.cs
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
using System.Threading.Tasks;
#endregion

namespace VitaNex.IO
{
	public abstract class DirectoryDataStore<TKey, TVal> : DataStore<TKey, TVal>
	{
		public virtual string FileExtension { get; set; }

		public bool Async { get; set; }

		public DirectoryDataStore(DirectoryInfo root)
			: base(root)
		{
			FileExtension = "vnc";
		}

		public DirectoryDataStore(DirectoryInfo root, string name)
			: base(root, name)
		{
			FileExtension = "vnc";
		}

		public DirectoryDataStore(DirectoryInfo root, string name, string fileExt)
			: base(root, name)
		{
			FileExtension = String.IsNullOrWhiteSpace(fileExt) ? "vnc" : fileExt;
		}

		protected override void OnExport()
		{
			Root.EmptyDirectory(false);

			if (Async)
			{
				Parallel.ForEach(this, OnExport);
				return;
			}

			foreach (var kv in this)
			{
				OnExport(kv);
			}
		}

		private void OnExport(KeyValuePair<TKey, TVal> kv)
		{
			TKey key = kv.Key;
			TVal value = kv.Value;

			string fileName = IOUtility.GetSafeFileName(String.Format("{0} ({1}).{2}", key, value, FileExtension), '%');

			try
			{
				OnExport(IOUtility.EnsureFile(Root.FullName + "/" + fileName, true), key, value);
			}
			catch (Exception e)
			{
				lock (SyncRoot)
				{
					Errors.Add(e);
				}
			}
		}

		protected override void OnImport()
		{
			foreach (var file in Root.GetFiles().Where(file => file.Name.EndsWith("." + FileExtension)))
			{
				OnImport(file);
			}
		}

		private void OnImport(FileInfo file)
		{
			try
			{
				TKey key;
				TVal val;

				OnImport(file, out key, out val);

				if (key == null || val == null)
				{
					return;
				}

				if (ContainsKey(key))
				{
					this[key] = val;
				}
				else
				{
					Add(key, val);
				}
			}
			catch (Exception e)
			{
				lock (SyncRoot)
				{
					Errors.Add(e);
				}
			}
		}

		protected abstract void OnExport(FileInfo file, TKey key, TVal val);
		protected abstract void OnImport(FileInfo file, out TKey key, out TVal val);
	}
}