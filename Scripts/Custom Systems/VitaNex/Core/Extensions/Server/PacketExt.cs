#region Header
//   Vorspire    _,-'/-'/  PacketExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using Server.Network;
#endregion

namespace VitaNex.Network
{
	public static class PacketExtUtility
	{
		public static void Rewrite(this Packet p, int offset, bool value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}

		public static void Rewrite(this Packet p, int offset, byte value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}

		public static void Rewrite(this Packet p, int offset, sbyte value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}

		public static void Rewrite(this Packet p, int offset, short value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}

		public static void Rewrite(this Packet p, int offset, ushort value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}

		public static void Rewrite(this Packet p, int offset, int value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}

		public static void Rewrite(this Packet p, int offset, uint value, bool reset = true)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					long o = p.UnderlyingStream.Position;

					p.UnderlyingStream.Position = offset;
					p.UnderlyingStream.Write(value);

					if (reset)
					{
						p.UnderlyingStream.Position = o;
					}
				});
		}
	}
}