#region Header
//   Vorspire    _,-'/-'/  ArtworkInfo.cs
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

using Server;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex
{
	public sealed class ArtworkInfo : PropertyObject
	{
		public ClientVersion Version { get; set; }
		public Pair<int, int> ItemID { get; set; }

		public ArtworkInfo(int oldItemID, int newItemID)
			: this(ArtworkSupport.DefaultVersion, oldItemID, newItemID)
		{ }

		public ArtworkInfo(ClientVersion version, int oldItemID, int newItemID)
		{
			Version = version ?? ArtworkSupport.DefaultVersion;
			ItemID = Pair.Create(oldItemID, newItemID);
		}

		public ArtworkInfo(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			Version = ArtworkSupport.DefaultVersion;
		}

		public override void Reset()
		{
			Version = ArtworkSupport.DefaultVersion;
		}

		public void RewriteID(NetState state, bool multi, Packet p, int offset)
		{
			if (state == null || p == null || offset >= p.UnderlyingStream.Length || state.Version >= Version)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				{
					int v = p is WorldItem ? 0 : p is WorldItemSA ? 1 : p is WorldItemHS ? 2 : -1;
					int id = ItemID.Right;

					switch (v)
					{
						case 0: //Old
							{
								id &= 0x3FFF;

								if (multi)
								{
									id |= 0x4000;
								}
							}
							break;
						case 1: //SA
							{
								id &= multi ? 0x3FFF : 0x7FFF;
							}
							break;
						case 2: //HS
							{
								id &= multi ? 0x3FFF : 0xFFFF;
							}
							break;
						default:
							return;
					}

					p.Rewrite(offset, (short)id);
				},
				ArtworkSupport.CSOptions.ToConsole);
		}

		public void RewriteID(NetState state, bool multi, ref byte[] buffer, int offset)
		{
			if (state == null || buffer == null || offset >= buffer.Length || state.Version >= Version)
			{
				return;
			}

			var b = buffer;

			buffer = VitaNexCore.TryCatchGet(
				() =>
				{
					int v = b.Length == 23 ? 0 : b.Length == 24 ? 1 : b.Length == 26 ? 2 : -1;
					int id = ItemID.Right;

					switch (v)
					{
						case 0: //Old
							{
								id &= 0x3FFF;

								if (multi)
								{
									id |= 0x4000;
								}
							}
							break;
						case 1: //SA
							{
								id &= multi ? 0x3FFF : 0x7FFF;
							}
							break;
						case 2: //HS
							{
								id &= multi ? 0x3FFF : 0xFFFF;
							}
							break;
						default:
							return b;
					}

					BitConverter.GetBytes((short)id).CopyTo(b, offset);

					return b;
				},
				ArtworkSupport.CSOptions.ToConsole);
		}

		public void SwitchID(NetState state, bool multi, WorldItem p)
		{
			RewriteID(state, multi, p, 7);
		}

		public void SwitchID(NetState state, bool multi, WorldItemSA p)
		{
			RewriteID(state, multi, p, 8);
		}

		public void SwitchID(NetState state, bool multi, WorldItemHS p)
		{
			RewriteID(state, multi, p, 8);
		}

		public void SwitchWorldItem(NetState state, bool multi, ref byte[] buffer)
		{
			RewriteID(state, multi, ref buffer, 7);
		}

		public void SwitchWorldItemSAHS(NetState state, bool multi, ref byte[] buffer)
		{
			RewriteID(state, multi, ref buffer, 8);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Version.SourceString);

						writer.Write(ItemID.Left);
						writer.Write(ItemID.Right);
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
						Version = new ClientVersion(reader.ReadString());

						int left = reader.ReadInt();
						int right = reader.ReadInt();

						ItemID = Pair.Create(left, right);
					}
					break;
			}
		}
	}
}