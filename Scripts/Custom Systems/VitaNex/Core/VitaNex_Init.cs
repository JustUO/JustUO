#region Header
//   Vorspire    _,-'/-'/  VitaNex_Init.cs
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
using System.Globalization;
using System.IO;
using System.Linq;

using Server;

using VitaNex.IO;
#endregion

namespace VitaNex
{
	public static partial class VitaNexCore
	{
		private static readonly VersionInfo _INITVersion;

		private static readonly Queue<Tuple<string, string>> _INITQueue;
		private static readonly Dictionary<string, Action<string>> _INITHandlers;

		static VitaNexCore()
		{
			_INITQueue = new Queue<Tuple<string, string>>();
			_INITHandlers = new Dictionary<string, Action<string>>();

			_INITVersion = "2.2.0.0";

			Version = null;

			if (!Directory.Exists(IOUtility.GetSafeDirectoryPath(Core.BaseDirectory + "/VitaNexCore")))
			{
				FirstBoot = true;
			}

			BaseDirectory = IOUtility.EnsureDirectory(Core.BaseDirectory + "/VitaNexCore");

			if (!File.Exists(BaseDirectory + "/FirstBoot.vnc"))
			{
				FirstBoot = true;

				IOUtility.EnsureFile(BaseDirectory + "/FirstBoot.vnc")
						 .AppendText(
							 true,
							 "This file serves no other purpose than to identify if",
							 "the software has been initialized for the first time. ",
							 "To re-initialize 'First-Boot' mode, simply delete this",
							 "file before starting the application.");
			}

			// HACK: See FindRootDirectory summary.
			var root = FindRootDirectory(Core.BaseDirectory + "/Scripts/VitaNex");

			if (root == null || !root.Exists)
			{
				return;
			}

			RootDirectory = root;

			ParseVersion();
			ParseINIT();

			RegisterINITHandler(
				"ROOT_DIR",
				path =>
				{
					root = FindRootDirectory(path);

					if (root == null || !root.Exists)
					{
						return;
					}

					RootDirectory = root;

					ParseVersion();
				});
		}

		/// <summary>
		/// Looks for the critical Vita-Nex: Core source files in the given path to determin the project's root directory.
		/// If the given path directory does not exist, the parent chain will be traversed until an existing directory is 
		/// found and a deep recursive search will be performed.
		/// If the given path contains the /Scripts/ directory, a deep search should find the required project files.
		/// </summary>
		/// <param name="path">A directory path to search for critical project files</param>
		/// <returns>An absolute root directory for the Vita-Nex: Core project source files</returns>
		private static DirectoryInfo FindRootDirectory(string path)
		{
			if (String.IsNullOrWhiteSpace(path))
			{
				return null;
			}

			path = IOUtility.GetSafeDirectoryPath(path);

			var root = TryCatchGet(
				() =>
				{
					var dir = new DirectoryInfo(path);

					while (!dir.Exists && dir.Parent != null)
					{
						dir = dir.Parent;
					}

					return dir;
				},
				ToConsole);

			if (root == null || !root.Exists)
			{
				return null;
			}

			var files = root.GetFiles("VitaNex*.cs", SearchOption.AllDirectories);

			if (files.Length < 4)
			{
				return null;
			}

			if (!files.All(f => f.Directory != null && f.Directory.FullName == root.FullName))
			{
				var file = files.FirstOrDefault(f => f.Directory != null && f.Directory.Exists);

				root = file != null &&
					   files.All(f => f.Directory != null && file.Directory != null && f.Directory.FullName == file.Directory.FullName)
						   ? file.Directory
						   : null;
			}

			if (root != null)
			{
				string corePath = IOUtility.GetSafeDirectoryPath(Core.BaseDirectory);
				string rootPath = IOUtility.GetSafeDirectoryPath(root.FullName.Replace(corePath, String.Empty));

				#if !MONO
				root = new DirectoryInfo(rootPath);
				#else
				root = new DirectoryInfo(IOUtility.GetSafeDirectoryPath(corePath + rootPath));
				#endif
			}

			return root;
		}

		private static void ParseVersion()
		{
			if ((Version == null || Version < _INITVersion) && RootDirectory != null && RootDirectory.Exists)
			{
				var files = RootDirectory.GetFiles("VERSION", SearchOption.TopDirectoryOnly);

				foreach (var file in files.Where(f => String.Equals("VERSION", f.Name) && String.IsNullOrWhiteSpace(f.Extension)))
				{
					using (var stream = file.OpenText())
					{
						string ver = stream.ReadToEnd().Trim();
						VersionInfo v;

						if (!VersionInfo.TryParse(ver, out v))
						{
							continue;
						}

						Version = v;
						break;
					}
				}
			}

			if (Version == null || Version < _INITVersion)
			{
				Version = _INITVersion;
			}
		}

		private static void ParseINIT()
		{
			var files = RootDirectory.GetFiles("VNC.cfg", SearchOption.AllDirectories);

			bool parse;
			bool die = false;

			foreach (var file in files.Select(f => new ConfigFileInfo(f)))
			{
				parse = false;

				var lines = file.ReadAllLines();

				foreach (string line in lines.Select(l => (l ?? String.Empty).Trim()))
				{
					if (!parse && line.StartsWith("[VNC_INIT]", true, CultureInfo.InvariantCulture))
					{
						parse = true;
						die = true;
					}

					if (parse && line.StartsWith("[VNC_EXIT]", true, CultureInfo.InvariantCulture))
					{
						parse = false;
					}

					if (!parse || String.IsNullOrWhiteSpace(line))
					{
						return;
					}

					var split = line.Split('=');

					string key = (split[0] ?? String.Empty).ToUpper();
					string value = String.Join(String.Empty, split, 1, split.Length - 1);

					if (!String.IsNullOrWhiteSpace(key))
					{
						_INITQueue.Enqueue(Tuple.Create(key, value));
					}
				}

				if (die)
				{
					break;
				}
			}
		}

		private static void ProcessINIT()
		{
			while (_INITQueue.Count > 0)
			{
				var instr = _INITQueue.Dequeue();

				if (_INITHandlers.ContainsKey(instr.Item1) && _INITHandlers[instr.Item1] != null)
				{
					_INITHandlers[instr.Item1](instr.Item2);
				}
			}
		}

		public static void RegisterINITHandler(string key, Action<string> callback)
		{
			if (String.IsNullOrWhiteSpace(key))
			{
				return;
			}

			key = key.ToUpper();

			if (_INITHandlers.ContainsKey(key))
			{
				if (callback != null)
				{
					_INITHandlers[key] = callback;
				}
				else
				{
					_INITHandlers.Remove(key);
				}
			}
			else if (callback != null)
			{
				_INITHandlers.Add(key, callback);
			}
		}
	}
}