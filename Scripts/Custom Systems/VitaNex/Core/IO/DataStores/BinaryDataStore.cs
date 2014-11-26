#region Header
//   Vorspire    _,-'/-'/  BinaryDataStore.cs
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

using Server;
#endregion

namespace VitaNex.IO
{
	public class BinaryDataStore<T1, T2> : DataStore<T1, T2>
	{
		public FileInfo Document { get; set; }

		public Func<GenericWriter, bool> OnSerialize { get; set; }
		public Func<GenericReader, bool> OnDeserialize { get; set; }

		public bool Async { get; set; }

		public BinaryDataStore(string root, string name)
			: this(IOUtility.EnsureDirectory(root), name)
		{ }

		public BinaryDataStore(DirectoryInfo root, string name)
			: base(root, name)
		{
			Document = IOUtility.EnsureFile(root.FullName + "/" + name + ((name.IndexOf('.') != -1) ? String.Empty : ".bin"));
		}

		protected override void OnExport()
		{
			if (Async)
			{
				Document.SerializeAsync(OnExport);
			}
			else
			{
				Document.Serialize(OnExport);
			}
		}

		private void OnExport(GenericWriter writer)
		{
			bool handled = false;

			if (OnSerialize != null)
			{
				handled = OnSerialize(writer);
			}

			if (!handled)
			{
				Serialize(writer);
			}
		}

		protected override void OnImport()
		{
			if (Document.Exists && Document.Length != 0)
			{
				Document.Deserialize(OnImport);
			}
		}

		private void OnImport(GenericReader reader)
		{
			bool handled = false;

			if (OnDeserialize != null)
			{
				handled = OnDeserialize(reader);
			}

			if (!handled)
			{
				Deserialize(reader);
			}
		}

		protected virtual void Serialize(GenericWriter writer)
		{ }

		protected virtual void Deserialize(GenericReader reader)
		{ }
	}
}