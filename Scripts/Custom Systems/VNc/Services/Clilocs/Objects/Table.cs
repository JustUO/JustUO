#region Header
//   Vorspire    _,-'/-'/  Table.cs
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
using System.Collections;
using System.Collections.Generic;
using System.IO;

using Server;
#endregion

namespace VitaNex
{
	public sealed class ClilocTable : IEnumerable<ClilocData>, IDisposable
	{
		private readonly Dictionary<int, ClilocData> _Table = new Dictionary<int, ClilocData>();

		public ClilocLNG Language { get; private set; }
		public FileInfo InputFile { get; private set; }

		public int Count { get { return _Table.Count; } }

		public bool Loaded { get; private set; }
		public ClilocInfo this[int index] { get { return Lookup(index); } }

		public void Dispose()
		{
			Unload();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return _Table.Values.GetEnumerator();
		}

		public IEnumerator<ClilocData> GetEnumerator()
		{
			return _Table.Values.GetEnumerator();
		}

		public void Clear()
		{
			foreach (ClilocData d in _Table.Values)
			{
				d.Clear();
			}
		}

		public void Unload()
		{
			if (!Loaded)
			{
				return;
			}

			Language = ClilocLNG.NULL;
			InputFile = null;
			_Table.Clear();

			Loaded = false;
		}

		public void Load(FileInfo file)
		{
			if (Loaded)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					ClilocLNG lng;

					if (!Enum.TryParse(file.Extension.TrimStart('.'), true, out lng))
					{
						throw new FileLoadException("Could not detect language for: " + file.FullName);
					}

					Language = lng;
					InputFile = file;

					InputFile.Deserialize(
						reader =>
						{
							long size = reader.Seek(0, SeekOrigin.End);
							reader.Seek(0, SeekOrigin.Begin);

							reader.ReadInt();
							reader.ReadShort();

							while (reader.Seek(0, SeekOrigin.Current) < size)
							{
								int index = reader.ReadInt();
								reader.ReadByte();
								int length = reader.ReadShort();
								long offset = reader.Seek(0, SeekOrigin.Current);
								reader.Seek(length, SeekOrigin.Current);

								if (_Table.ContainsKey(index))
								{
									_Table[index] = new ClilocData(Language, index, offset, length);
								}
								else
								{
									_Table.Add(index, new ClilocData(Language, index, offset, length));
								}
							}
						});

					Loaded = true;
				},
				Clilocs.CSOptions.ToConsole);
		}

		public bool Contains(int index)
		{
			return _Table.ContainsKey(index);
		}

		public bool IsNullOrWhiteSpace(int index)
		{
			if (!Contains(index) || _Table[index] == null)
			{
				return true;
			}

			ClilocInfo info = _Table[index].Lookup(InputFile);

			if (!String.IsNullOrWhiteSpace(info.Text))
			{
				return false;
			}

			return true;
		}

		public ClilocInfo Update(int index)
		{
			if (!Contains(index) || _Table[index] == null)
			{
				return null;
			}

			return _Table[index].Lookup(InputFile, true);
		}

		public ClilocInfo Lookup(int index)
		{
			if (!Contains(index) || _Table[index] == null)
			{
				return null;
			}

			return _Table[index].Lookup(InputFile);
		}

		public void LookupAll()
		{
			VitaNexCore.TryCatch(
				() => InputFile.Deserialize(
					reader =>
					{
						foreach (ClilocData d in _Table.Values)
						{
							d.Lookup(reader as BinaryFileReader);
						}
					}),
				Clilocs.CSOptions.ToConsole);
		}

		public override string ToString()
		{
			return ((Language == ClilocLNG.NULL) ? "Not Loaded" : "Cliloc." + Language);
		}
	}
}