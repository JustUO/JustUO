#region Header
//   Vorspire    _,-'/-'/  ArtworkSupport_Init.cs
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

using Server;

using VitaNex.Network;
#endregion

namespace VitaNex
{
	[CoreService("Artwork Support", "1.0.0.0", TaskPriority.High)]
	public static partial class ArtworkSupport
	{
		static ArtworkSupport()
		{
			CSOptions = new CoreServiceOptions(typeof(ArtworkSupport));

			Info = new Dictionary<Type, List<ArtworkInfo>>();

			LandTextures = new short[TileData.MaxLandValue];
			LandTextures.SetAll((short)-1);

			StaticAnimations = new short[TileData.MaxItemValue];
			StaticAnimations.SetAll((short)-1);

			string filePath = Core.FindDataFile("tiledata.mul");

			if (String.IsNullOrWhiteSpace(filePath))
			{
				return;
			}

			var file = new FileInfo(filePath);

			if (!file.Exists)
			{
				return;
			}

			using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
			{
				var bin = new BinaryReader(fs);
				var buffer = new byte[25];

				if (fs.Length == 3188736)
				{
					// 7.0.9.0
					LandTextures = new short[0x4000];

					for (int i = 0; i < 0x4000; ++i)
					{
						if (i == 1 || (i > 0 && (i & 0x1F) == 0))
						{
							bin.Read(buffer, 0, 4);
						}

						bin.Read(buffer, 0, 8);

						short texture = bin.ReadInt16();

						bin.Read(buffer, 0, 20);

						LandTextures[i] = texture;
					}

					StaticAnimations = new short[0x10000];

					for (int i = 0; i < 0x10000; ++i)
					{
						if ((i & 0x1F) == 0)
						{
							bin.Read(buffer, 0, 4);
						}

						bin.Read(buffer, 0, 14);

						short anim = bin.ReadInt16();

						bin.Read(buffer, 0, 25);

						StaticAnimations[i] = anim;
					}
				}
				else
				{
					LandTextures = new short[0x4000];

					for (int i = 0; i < 0x4000; ++i)
					{
						if ((i & 0x1F) == 0)
						{
							bin.Read(buffer, 0, 4);
						}

						bin.Read(buffer, 0, 4);

						short texture = bin.ReadInt16();

						bin.Read(buffer, 0, 20);

						LandTextures[i] = texture;
					}

					if (fs.Length == 1644544)
					{
						// 7.0.0.0
						StaticAnimations = new short[0x8000];

						for (int i = 0; i < 0x8000; ++i)
						{
							if ((i & 0x1F) == 0)
							{
								bin.Read(buffer, 0, 4);
							}

							bin.Read(buffer, 0, 10);

							short anim = bin.ReadInt16();

							bin.Read(buffer, 0, 25);

							StaticAnimations[i] = anim;
						}
					}
					else
					{
						StaticAnimations = new short[0x4000];

						for (int i = 0; i < 0x4000; ++i)
						{
							if ((i & 0x1F) == 0)
							{
								bin.Read(buffer, 0, 4);
							}

							bin.Read(buffer, 0, 10);

							short anim = bin.ReadInt16();

							bin.Read(buffer, 0, 25);

							StaticAnimations[i] = anim;
						}
					}
				}
			}
		}

		private static void CSConfig()
		{
			_Parent0x1A = OutgoingPacketOverrides.GetHandler(0x1A);
			_Parent0xF3 = OutgoingPacketOverrides.GetHandler(0xF3);

			OutgoingPacketOverrides.Register(0x1A, true, HandleWorldItem);
			OutgoingPacketOverrides.Register(0xF3, true, HandleWorldItemSAHS);
		}

		private static void CSInvoke()
		{
			Type type = typeof(Item);

			foreach (var entry in
				type.GetChildren().AsParallel().Select(
					child => new
					{
						Type = child,
						Attrs = child.GetCustomAttributes<ArtworkSupportAttribute>(false)
					}))
			{
				foreach (var attr in entry.Attrs)
				{
					foreach (var pair in attr.ItemIDs)
					{
						Register(entry.Type, attr.Version, pair.Left, pair.Right);
					}
				}
			}
		}
	}
}