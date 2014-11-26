#region Header
//   Vorspire    _,-'/-'/  IOUtility.cs
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
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

using Server;
#endregion

namespace VitaNex.IO
{
	public static class IOUtility
	{
		public static char SEPARATOR { get { return Core.Unix ? '/' : '\\'; } }

		/// <summary>
		///     Parses a given file path and returns the same path with any syntax errors removed.
		///     Syntax errors can include double seperators such as 'path/to//file.txt'
		/// </summary>
		/// <param name="initialPath">The initial path string to parse.</param>
		/// <param name="incFileName">
		///     Determines whether to append a file name to the parsed path.
		///     This will only append if a file name is specified in the initial path.
		/// </param>
		/// <returns>The parsed path string with syntax errors removed.</returns>
		public static string GetSafeFilePath(string initialPath, bool incFileName)
		{
			var sb = new StringBuilder();

			var split = initialPath.Split(new[] {'\\', '/'}, StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < split.Length; i++)
			{
				if (i < split.Length - 1)
				{
					sb.AppendFormat("{0}{1}", split[i], SEPARATOR);
				}
				else if (incFileName)
				{
					sb.AppendFormat("{0}", split[i]);
				}
			}

			if (Core.Unix && sb[0] != SEPARATOR)
			{
				sb.Insert(0, SEPARATOR);
			}

			return sb.ToString();
		}

		public static string GetFileName(string path)
		{
			return Path.GetFileName(path);
		}

		/// <summary>
		///     Gets a safe file name string by replacing all invalid characters with a specified char filter
		/// </summary>
		/// <param name="name">String to parse</param>
		/// <param name="replace">Replacement char for invalid chars *Using an invalid char will cause a stack overflow, so don't be silly</param>
		/// <returns></returns>
		public static string GetSafeFileName(string name, char replace = '_')
		{
			return Path.GetInvalidFileNameChars().Aggregate(name, (current, c) => current.Replace(c, replace));
		}

		public static string GetUnusedFilePath(string path, string name)
		{
			string fullPath = GetSafeFilePath(path + SEPARATOR + name, true);

			for (int i = 2; File.Exists(fullPath) && i <= 1000; ++i)
			{
				var split = name.Split(new[] {'.'});

				fullPath = split.Length == 2
							   ? GetSafeFilePath(path + SEPARATOR + split[0] + i + "." + split[1], true)
							   : GetSafeFilePath(path + SEPARATOR + name + i, true);
			}

			return fullPath;
		}

		/// <summary>
		///     Invalidates the given file path and returns the cleaned path string with the appended file name.
		///     Unlike SafeFilePath, this method does not remove syntax errors.
		/// </summary>
		/// <param name="filePath">The file path to invalidate.</param>
		/// <param name="fileName">The file name to append.</param>
		/// <returns>The cleaned path string with the appended file name.</returns>
		public static string GetValidFilePath(string filePath, string fileName)
		{
			int lookup = filePath.Substring(filePath.Length - 1, 1).IndexOfAny(new[] {'\\', '/'});
			bool hasEndSep = lookup != -1;

			if (hasEndSep)
			{
				filePath = filePath.Substring(0, lookup);
			}

			var split = filePath.Split(new[] {'\\', '/'});

			string validPath = split.Aggregate("", (current, t) => current + (t + SEPARATOR));

			if (fileName.Length > 0)
			{
				if (fileName.StartsWith("\\") || fileName.StartsWith("/"))
				{
					fileName = fileName.TrimStart(new[] {'\\', '/'});
				}

				validPath += fileName;
			}
			else
			{
				if (validPath.EndsWith("\\") || validPath.EndsWith("/"))
				{
					validPath = validPath.TrimEnd(new[] {'\\', '/'});
				}
			}

			if (Core.Unix && !validPath.StartsWith(SEPARATOR.ToString(CultureInfo.InvariantCulture)))
			{
				validPath = SEPARATOR + validPath;
			}

			return validPath;
		}

		/// <summary>
		///     Ensures a files' existence
		/// </summary>
		/// <param name="name">File path</param>
		/// <param name="replace">True: replace the file if it exists</param>
		/// <returns>FileInfo representing the file ensured for 'name'</returns>
		public static FileInfo EnsureFile(string name, bool replace = false)
		{
			return new FileInfo(GetSafeFilePath(name, true)).EnsureFile(replace);
		}

		/// <summary>
		///     Parses a given directory path and returns the same path with any syntax errors removed.
		///     Syntax errors can include double seperators such as 'path/to//directory'
		/// </summary>
		/// <param name="initialPath">The initial path string to parse.</param>
		/// <returns>The parsed path string with syntax errors removed.</returns>
		public static string GetSafeDirectoryPath(string initialPath)
		{
			var sb = new StringBuilder();

			var split = initialPath.Split(new[] {'\\', '/'}, StringSplitOptions.RemoveEmptyEntries);

			foreach (string t in split)
			{
				sb.AppendFormat("{0}{1}", t, SEPARATOR);
			}

			if (Core.Unix && sb[0] != SEPARATOR)
			{
				sb.Insert(0, SEPARATOR);
			}

			return sb.ToString();
		}

		public static string GetUnusedDirectoryPath(string path, string name)
		{
			string fullPath = GetSafeDirectoryPath(path + SEPARATOR + name);

			for (int i = 2; Directory.Exists(fullPath) && i <= 1000; ++i)
			{
				fullPath = GetSafeDirectoryPath(path + SEPARATOR + name + i);
			}

			return fullPath;
		}

		/// <summary>
		///     Invalidates the given directory path and returns the cleaned path string.
		///     Unlike GetSafeDirectoryPath, this method does not remove syntax errors.
		/// </summary>
		/// <param name="path">The directory path to invalidate.</param>
		/// <returns>The cleaned directory path string.</returns>
		public static string GetValidDirectoryPath(string path)
		{
			int lookup = path.Substring(path.Length - 1, 1).IndexOfAny(new[] {'\\', '/'});
			bool hasEndSep = lookup != -1;

			if (hasEndSep)
			{
				path = path.Substring(0, lookup);
			}

			var split = path.Split(new[] {'\\', '/'});

			string validPath = split.Aggregate("", (current, t) => current + (t + SEPARATOR));

			if (!validPath.EndsWith("\\") && !validPath.EndsWith("/"))
			{
				validPath += SEPARATOR;
			}

			if (Core.Unix && !validPath.StartsWith(SEPARATOR.ToString(CultureInfo.InvariantCulture)))
			{
				validPath = SEPARATOR + validPath;
			}

			return validPath;
		}

		/// <summary>
		///     Ensures a directories' existence
		/// </summary>
		/// <param name="name">Directory path</param>
		/// <param name="replace">True: replace the directory if it exists</param>
		/// <returns>DirectoryInfo representing the directory ensured for 'name'</returns>
		public static DirectoryInfo EnsureDirectory(string name, bool replace = false)
		{
			return new DirectoryInfo(GetSafeDirectoryPath(name)).EnsureDirectory(replace);
		}

		/// <summary>
		///     Gets the base directory, relative the current Executing Assembly location.
		/// </summary>
		/// <returns>Fully qualified directory path for the the current Executing Assembly location.</returns>
		public static string GetBaseDirectory()
		{
			return GetSafeDirectoryPath(GetSafeFilePath(Assembly.GetExecutingAssembly().Location, false));
		}

		/// <summary>
		///     Gets the base executable path, relative the current Executing Assembly location.
		/// </summary>
		/// <returns>Fully qualified executable path for the the current Executing Assembly location.</returns>
		public static string GetExePath()
		{
			return GetSafeFilePath(Core.ExePath, true);
		}

		/// <summary>
		///     Reverses the directory seperator character in the given path string.
		///     \ becomes / and vice-versa.
		/// </summary>
		/// <param name="path">The path to reverse.</param>
		/// <returns>The given path with the directory seperator characters reversed.</returns>
		public static string ReversePath(string path)
		{
			var split = path.Split('\\');

			if (split.Length > 0)
			{
				return path.Replace("\\", "/");
			}

			split = path.Split('/');

			if (split.Length > 0)
			{
				return path.Replace("/", "\\");
			}

			return path;
		}

		public static FileStream OpenRead(string path, bool create = false, bool replace = false)
		{
			return new FileInfo(GetSafeFilePath(path, true)).OpenRead(create, replace);
		}

		public static FileStream OpenWrite(string path, bool create = false, bool replace = false)
		{
			return new FileInfo(GetSafeFilePath(path, true)).OpenWrite(create, replace);
		}
	}
}