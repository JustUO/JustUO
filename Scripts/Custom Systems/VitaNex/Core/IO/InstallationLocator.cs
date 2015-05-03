#region Header
//   Vorspire    _,-'/-'/  InstallationLocator.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#if !MONO

#region References
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

using Microsoft.Win32;

using Server;
using Server.Misc;

using VitaNex.IO;
#endregion

namespace VitaNex
{
	public static class InstallationLocator
	{
		public static bool Is64Bit { get; private set; }

		public static string[] KnownInstallationRegistryKeys { get; private set; }
		public static string[] KnownRegistryKeyValueNames { get; private set; }
		public static string[] DetectedPaths { get; private set; }

		[CallPriority(-1)]
		public static void Configure()
		{
			Is64Bit = (IntPtr.Size == 8);

			KnownInstallationRegistryKeys = new[]
			{
				@"Electronic Arts\EA Games\Ultima Online Classic", @"Electronic Arts\EA Games\Ultima Online Stygian Abyss Classic",
				@"Origin Worlds Online\Ultima Online\KR Legacy Beta", @"EA Games\Ultima Online: Mondain's Legacy\1.00.0000",
				@"Origin Worlds Online\Ultima Online\1.0", @"Origin Worlds Online\Ultima Online Third Dawn\1.0",
				@"EA GAMES\Ultima Online Samurai Empire", @"EA Games\Ultima Online: Mondain's Legacy",
				@"EA GAMES\Ultima Online Samurai Empire\1.0", @"EA GAMES\Ultima Online Samurai Empire\1.00.0000",
				@"EA GAMES\Ultima Online: Samurai Empire\1.0", @"EA GAMES\Ultima Online: Samurai Empire\1.00.0000",
				@"EA Games\Ultima Online: Mondain's Legacy\1.0", @"EA Games\Ultima Online: Mondain's Legacy\1.00.0000",
				@"Origin Worlds Online\Ultima Online Samurai Empire BETA\2d\1.0",
				@"Origin Worlds Online\Ultima Online Samurai Empire BETA\3d\1.0",
				@"Origin Worlds Online\Ultima Online Samurai Empire\2d\1.0",
				@"Origin Worlds Online\Ultima Online Samurai Empire\3d\1.0"
			};

			KnownRegistryKeyValueNames = new[] {@"ExePath", @"InstallDir", @"Install Dir", @"GameExecutionPath"};

			Console.WriteLine("Searching for Ultima Online installations...");
			var paths = new List<string>();

			var dpCustom = typeof(DataPath).GetField("CustomPath", BindingFlags.NonPublic | BindingFlags.Static);

			if (dpCustom != null)
			{
				string custom = dpCustom.GetValue(null) as string;

				if (!String.IsNullOrWhiteSpace(custom) && !paths.Contains(custom))
				{
					paths.Add(custom);
				}
			}

			var cache = IOUtility.EnsureFile(VitaNexCore.BaseDirectory + "/DataPath.bin");

			cache.Deserialize(
				r =>
				{
					var l = r.ReadArray(r.ReadString);

					paths.AddRange(
						l.Not(paths.Contains).Where(path => File.Exists(IOUtility.GetSafeFilePath(path + "/client.exe", true))));
				});

			paths.AddRange(Locate().Not(paths.Contains));

			DetectedPaths = paths.ToArray();

			if (DetectedPaths.Length > 0)
			{
				Console.WriteLine("Found {0:#,0} Ultima Online installations:", DetectedPaths.Length);
				DetectedPaths.ForEach(Console.WriteLine);

				Core.DataDirectories.AddRange(DetectedPaths);
			}
			else
			{
				Console.WriteLine("Could not find any Ultima Online installations.");

				if (!Core.Service && Core.DataDirectories.Count == 0)
				{
					int attempt = 0;
					string path = IOUtility.GetSafeDirectoryPath(GetConsoleInput());

					while (String.IsNullOrWhiteSpace(path) && attempt < 3)
					{
						path = IOUtility.GetSafeDirectoryPath(GetConsoleInput());

						if (Directory.Exists(path))
						{
							break;
						}

						Console.WriteLine("The directory could not be found.");
						path = null;
						attempt++;
					}

					if (attempt < 3)
					{
						Core.DataDirectories.Add(path);
					}
				}
			}

			cache.Serialize(w => w.WriteArray(Core.DataDirectories.ToArray(), w.Write));
		}

		private static string GetConsoleInput()
		{
			Console.WriteLine("Enter the Ultima Online directory:");
			Console.Write("> ");

			return Console.ReadLine();
		}

		public static IEnumerable<string> Locate()
		{
			string prefix = Is64Bit ? @"SOFTWARE\Wow6432Node\" : @"SOFTWARE\";

			foreach (string knownKeyName in
				KnownInstallationRegistryKeys.Where(knownKeyName => !String.IsNullOrWhiteSpace(knownKeyName)))
			{
				string exePath;
				TryGetExePath(prefix + knownKeyName, out exePath);

				if (!String.IsNullOrWhiteSpace(exePath))
				{
					yield return exePath;
				}
			}
		}

		private static bool TryGetExePath(string regPath, out string exePath)
		{
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath) ?? Registry.CurrentUser.OpenSubKey(regPath))
				{
					if (key == null)
					{
						exePath = null;
						return false;
					}

					string dir = null, file = null;

					foreach (var pathStub in
						KnownRegistryKeyValueNames.Select(knownKeyValueName => key.GetValue(knownKeyValueName) as string)
												  .Where(pathStub => !String.IsNullOrWhiteSpace(pathStub)))
					{
						if (String.IsNullOrWhiteSpace(file) && Path.HasExtension(pathStub))
						{
							file = Path.GetFileName(pathStub);

							if (String.IsNullOrWhiteSpace(dir) && Path.IsPathRooted(pathStub))
							{
								dir = Path.GetDirectoryName(pathStub);
							}
						}

						if (String.IsNullOrWhiteSpace(dir) && Path.IsPathRooted(pathStub))
						{
							dir = pathStub;
						}

						if (String.IsNullOrWhiteSpace(dir) || String.IsNullOrWhiteSpace(file))
						{
							continue;
						}

						string fullPath = dir.Replace('/', '\\');

						if (fullPath[fullPath.Length - 1] != '\\')
						{
							fullPath += '\\';
						}

						fullPath += file;

						if (!File.Exists(fullPath))
						{
							continue;
						}

						exePath = dir;
						return true;
					}
				}
			}
			catch
			{ }

			exePath = null;
			return false;
		}
	}
}
#endif