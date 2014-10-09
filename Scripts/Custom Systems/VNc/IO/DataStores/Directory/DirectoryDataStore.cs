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
using System.IO;
using System.Linq;
#endregion

namespace VitaNex.IO
{
	public abstract class DirectoryDataStore<TKey, TVal> : DataStore<TKey, TVal>
	{
		public virtual string FileExtension { get; set; }

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

			foreach (var entry in Registry)
			{
				string fileName = IOUtility.GetSafeFileName(
					String.Format("{0} ({1}).{2}", entry.Key, entry.Value, FileExtension), '%');

				try
				{
					OnExport(IOUtility.EnsureFile(Root.FullName + "/" + fileName, true), entry.Key, entry.Value);
				}
				catch (Exception e)
				{
					Errors.Add(e);
				}
			}
		}

		protected override void OnImport()
		{
			var files = Root.GetFiles();

			foreach (FileInfo file in files.Where(file => file.Name.EndsWith("." + FileExtension)))
			{
				try
				{
					TKey key;
					TVal val;

					OnImport(file, out key, out val);

					if (key == null || val == null)
					{
						continue;
					}

					if (Registry.ContainsKey(key))
					{
						Registry[key] = val;
					}
					else
					{
						Registry.Add(key, val);
					}
				}
				catch (Exception e)
				{
					Errors.Add(e);
				}
			}
		}

		protected abstract void OnImport(FileInfo file, out TKey key, out TVal val);
		protected abstract void OnExport(FileInfo file, TKey key, TVal val);
	}
}