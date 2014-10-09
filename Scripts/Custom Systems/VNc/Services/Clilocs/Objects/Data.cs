#region Header
//   Vorspire    _,-'/-'/  Data.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.IO;
using System.Text;

using Server;
#endregion

namespace VitaNex
{
	public sealed class ClilocData
	{
		public ClilocData(ClilocLNG lng, int index, long offset, long length)
		{
			Language = lng;
			Index = index;
			Offset = offset;
			Length = length;
		}

		public ClilocLNG Language { get; private set; }
		public int Index { get; private set; }
		public long Offset { get; private set; }
		public long Length { get; private set; }

		private ClilocInfo Info { get; set; }

		public void Clear()
		{
			Info = null;
		}

		public ClilocInfo Lookup(GenericReader bin)
		{
			bin.Seek(Offset, SeekOrigin.Begin);
			var data = new byte[Length];

			for (long i = 0; i < data.Length; i++)
			{
				data[i] = bin.ReadByte();
			}

			return Info = new ClilocInfo(Language, Index, Encoding.UTF8.GetString(data));
		}

		public ClilocInfo Lookup(FileInfo file, bool forceUpdate = false)
		{
			if (Info != null && !forceUpdate)
			{
				return Info;
			}

			VitaNexCore.TryCatch(() => file.Deserialize(reader => Lookup(reader)), Clilocs.CSOptions.ToConsole);
			return Info;
		}
	}
}