#region Header
//   Vorspire    _,-'/-'/  SuperGump_Assets.cs
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
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

using Server;

using VitaNex.Crypto;
#endregion

namespace VitaNex.SuperGumps
{
	public sealed class SuperGumpAsset : Grid<Color>, IEquatable<SuperGumpAsset>
	{
		private static readonly object _CacheLock = new object();

		public static List<SuperGumpAsset> AssetCache { get; private set; }

		public static SuperGumpAsset Empty { get; private set; }

		static SuperGumpAsset()
		{
			AssetCache = new List<SuperGumpAsset>();
			Empty = new SuperGumpAsset();
		}

		public static bool IsNullOrEmpty(SuperGumpAsset asset)
		{
			return asset == null || asset == Empty || asset.Hash == Empty.Hash || asset.Capacity == 0 || asset.Count == 0;
		}

		public static SuperGumpAsset CreateInstance(FileInfo file)
		{
			return CreateInstance(file, true);
		}

		public static SuperGumpAsset CreateInstance(FileInfo file, bool cache)
		{
			return CreateInstance(file, cache, false);
		}

		public static SuperGumpAsset CreateInstance(FileInfo file, bool cache, bool reload)
		{
			if (file == null || !file.Exists)
			{
				return Empty;
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					var hash = CryptoGenerator.GenString(CryptoHashType.MD5, file.FullName);

					SuperGumpAsset a;

					lock (_CacheLock)
					{
						a = AssetCache.FirstOrDefault(ca => ca.Hash == hash);
					}

					if (a == null || reload)
					{
						using (var img = new Bitmap(file.FullName, true))
						{
							a = new SuperGumpAsset(file, img);

							if (cache)
							{
								lock (_CacheLock)
								{
									AssetCache.AddOrReplace(a);
								}
							}
						}
					}

					if (IsNullOrEmpty(a))
					{
						return Empty;
					}

					if (!cache || a.Capacity > 0x1000)
					{
						lock (_CacheLock)
						{
							AssetCache.Remove(a);
							AssetCache.Free(false);
						}
					}

					lock (_CacheLock)
					{
						if (AssetCache.Count > 100)
						{
							AssetCache.RemoveAt(0);
						}

						AssetCache.Free(false);
					}

					return a;
				},
				VitaNexCore.ToConsole);
		}

		public static bool IsValidAsset(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				return false;
			}

			path = path.ToLower();

			return path.EndsWith("bmp") || path.EndsWith("jpg") || path.EndsWith("jpeg") || path.EndsWith("png") ||
				   path.EndsWith("gif") || path.EndsWith("tiff") || path.EndsWith("exif");
		}

		public static SuperGumpAsset LoadAsset(string path)
		{
			if (!IsValidAsset(path))
			{
				return Empty;
			}

			if (!Insensitive.StartsWith(path, "http://") && !Insensitive.StartsWith(path, "https://"))
			{
				return LoadAsset(new FileInfo(path));
			}

			return LoadAsset(new Uri(path));
		}

		public static SuperGumpAsset LoadAsset(Uri url)
		{
			if (url == null)
			{
				return Empty;
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					if (!IsValidAsset(url.LocalPath))
					{
						return Empty;
					}

					var file = new FileInfo(VitaNexCore.DataDirectory + "/Assets/" + url.Host + "/" + url.LocalPath);

					if (!file.Exists)
					{
						file = file.EnsureFile();

						using (WebClient c = new WebClient())
						{
							c.DownloadFile(url, file.FullName);
						}
					}

					return LoadAsset(file);
				},
				VitaNexCore.ToConsole);
		}

		public static SuperGumpAsset LoadAsset(FileInfo file)
		{
			if (file == null || !IsValidAsset(file.FullName))
			{
				return Empty;
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					var asset = CreateInstance(file);

					if (IsNullOrEmpty(asset))
					{
						return Empty;
					}

					if (file.Exists)
					{
						return asset;
					}

					file.EnsureFile();

					using (var img = new Bitmap(asset.Width, asset.Height))
					{
						asset.ForEach(img.SetPixel);
						img.Save(file.FullName);
					}

					return asset;
				},
				VitaNexCore.ToConsole);
		}

		public string File { get; private set; }
		public string Name { get; private set; }
		public string Hash { get; private set; }

		private SuperGumpAsset()
			: base(0, 0)
		{
			File = String.Empty;
			Name = String.Empty;
			Hash = CryptoGenerator.GenString(CryptoHashType.MD5, File);

			DefaultValue = Color.Transparent;
		}

		private SuperGumpAsset(FileInfo file, Bitmap img)
			: base(Math.Min(128, img.Width), Math.Min(128, img.Height))
		{
			File = file.FullName;
			Name = Path.GetFileName(File);
			Hash = CryptoGenerator.GenString(CryptoHashType.MD5, File);

			DefaultValue = Color.Transparent;
			SetAllContent(img.GetPixel);
		}

		public override string ToString()
		{
			return File;
		}

		public override int GetHashCode()
		{
			return Hash.Aggregate(Hash.Length, (h, c) => unchecked((h * 397) ^ c));
		}

		public override bool Equals(object obj)
		{
			return obj is SuperGumpAsset && Equals((SuperGumpAsset)obj);
		}

		public bool Equals(SuperGumpAsset other)
		{
			return !ReferenceEquals(other, null) && Hash.Equals(other.Hash);
		}

		public static bool operator ==(SuperGumpAsset l, SuperGumpAsset r)
		{
			return ReferenceEquals(l, null) ? ReferenceEquals(r, null) : l.Equals(r);
		}

		public static bool operator !=(SuperGumpAsset l, SuperGumpAsset r)
		{
			return ReferenceEquals(l, null) ? !ReferenceEquals(r, null) : !l.Equals(r);
		}
	}

	public abstract partial class SuperGump
	{
		protected SuperGumpAsset LoadAsset(string path)
		{
			return SuperGumpAsset.LoadAsset(path);
		}

		protected SuperGumpAsset[] LoadAssets(string path, params string[] paths)
		{
			return (paths ?? new string[0]).With(path).Select(LoadAsset).Where(a => a != null).ToArray();
		}

		public void AddAsset(int x, int y, string path)
		{
			AddAsset(x, y, SuperGumpAsset.LoadAsset(path));
		}

		public void AddAsset(int x, int y, Uri url)
		{
			AddAsset(x, y, SuperGumpAsset.LoadAsset(url));
		}

		public void AddAsset(int x, int y, FileInfo file)
		{
			AddAsset(x, y, SuperGumpAsset.LoadAsset(file));
		}

		public void AddAsset(int x, int y, SuperGumpAsset asset)
		{
			if (SuperGumpAsset.IsNullOrEmpty(asset))
			{
				return;
			}

			int ax, ay;
			Color ac;

			for (ax = 0; ax < asset.Width; ax++)
			{
				for (ay = 0; ay < asset.Height; ay++)
				{
					if ((ac = asset[ax, ay]).A > 0)
					{
						AddHtml(x + ax, y + ay, 1, 1, " ".WrapUOHtmlBG(ac), false, false);
					}
				}
			}
		}
	}
}