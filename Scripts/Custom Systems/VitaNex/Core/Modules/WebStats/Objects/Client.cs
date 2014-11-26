#region Header
//   Vorspire    _,-'/-'/  Client.cs
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
using System.IO.Compression;
using System.Linq;
using System.Net.Sockets;
using System.Text;
#endregion

namespace VitaNex.Modules.WebStats
{
	public sealed class WebStatsClient
	{
		public TcpClient Client { get; private set; }

		public Encoding Encoding { get; set; }

		public bool Connected { get { return Client != null && Client.Connected; } }

		public WebStatsClient(TcpClient client, Encoding encoding)
		{
			Client = client;
			Encoding = encoding;
		}

		public void Encode(string data, out byte[] buffer, out int length)
		{
			length = Encoding.GetByteCount(data);
			buffer = new byte[length];

			Encoding.GetBytes(data, 0, data.Length, buffer, 0);
		}

		public void Decode(byte[] src, out string data)
		{
			data = Encoding.GetString(src);
		}

		public void Compress(ref byte[] buffer, ref int length)
		{
			using (MemoryStream inS = new MemoryStream(buffer.Take(length).ToArray()), outS = new MemoryStream())
			{
				using (DeflateStream ds = new DeflateStream(outS, CompressionMode.Compress))
				{
					inS.CopyTo(ds);

					outS.Position = 0;
				}

				buffer = outS.ToArray();
				length = buffer.Length;
			}
		}

		public void Decompress(ref byte[] buffer, ref int length)
		{
			using (MemoryStream inS = new MemoryStream(buffer.Take(length).ToArray()), outS = new MemoryStream())
			{
				using (DeflateStream ds = new DeflateStream(inS, CompressionMode.Decompress))
				{
					ds.CopyTo(outS);

					outS.Position = 0;
				}

				buffer = outS.ToArray();
				length = buffer.Length;
			}
		}

		public NetworkStream GetStream()
		{
			return Client != null ? Client.GetStream() : null;
		}

		public void Send(string data, bool encode, bool compress, Action<WebStatsClient, byte[]> callback)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					int len;
					byte[] buffer;

					if (encode)
					{
						Encode(data, out buffer, out len);
					}
					else
					{
						buffer = data.Select(c => (byte)c).ToArray();
						len = buffer.Length;
					}

					Send(buffer, len, compress, callback);
				},
				WebStats.CMOptions.ToConsole);
		}

		public void Send(byte[] buffer, int len, bool compress, Action<WebStatsClient, byte[]> callback)
		{
			NetworkStream stream = GetStream();

			if (compress)
			{
				Compress(ref buffer, ref len);
			}

			int count = 0;

			while (count < len)
			{
				var block = buffer.Skip(count).Take(Client.SendBufferSize).ToArray();

				stream.Write(block, 0, block.Length);

				count += block.Length;
			}

			if (callback != null)
			{
				callback(this, buffer);
			}
		}

		public void Receive(bool decompress, bool decode, Action<WebStatsClient, string, byte[]> callback)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					NetworkStream stream = GetStream();

					var buffer = new byte[Client.ReceiveBufferSize];
					int len = buffer.Length;

					stream.Read(buffer, 0, buffer.Length);

					if (decompress)
					{
						Decompress(ref buffer, ref len);
					}

					string data;

					if (decode)
					{
						Decode(buffer, out data);
					}
					else
					{
						data = new String(buffer.Select(b => (char)b).ToArray());
					}

					if (callback != null)
					{
						callback(this, data, buffer);
					}
				},
				WebStats.CMOptions.ToConsole);
		}

		public void Close()
		{
			VitaNexCore.TryCatch(
				() =>
				{
					if (!Connected)
					{
						return;
					}

					WebStats.Disconnected(this);

					Client.Close();
				},
				e =>
				{
					lock (WebStats.Clients)
					{
						WebStats.Clients.Remove(this);
					}

					WebStats.CMOptions.ToConsole(e);
				});
		}
	}
}