#region Header
//   Vorspire    _,-'/-'/  IOExt.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using VitaNex;
using VitaNex.IO;
#endregion

namespace System.IO
{
	public static class IOExtUtility
	{
		public static bool GetAttribute(this FileInfo file, FileAttributes attr)
		{
			if (file.Exists)
			{
				return file.Attributes.HasFlag(attr);
			}

			return false;
		}

		public static void SetAttribute(this FileInfo file, FileAttributes attr, bool value)
		{
			if (!file.Exists)
			{
				return;
			}

			if (value)
			{
				file.Attributes |= attr;
			}
			else
			{
				file.Attributes &= ~attr;
			}
		}

		public static void SetHidden(this FileInfo file, bool value)
		{
			SetAttribute(file, FileAttributes.Hidden, value);
		}

		public static FileStream OpenRead(this FileInfo file, bool create = false, bool replace = false)
		{
			if (file.Exists)
			{
				if (replace)
				{
					file = EnsureFile(file, true);
				}
			}
			else
			{
				if (create)
				{
					file = EnsureFile(file, replace);
				}
			}

			return file.OpenRead();
		}

		public static FileStream OpenWrite(this FileInfo file, bool create = false, bool replace = false)
		{
			if (file.Exists)
			{
				if (replace)
				{
					file = EnsureFile(file, true);
				}
			}
			else
			{
				if (create)
				{
					file = EnsureFile(file, replace);
				}
			}

			return file.OpenWrite();
		}

		public static void AppendText(this FileInfo file, bool truncate, params string[] lines)
		{
			if (lines == null || lines.Length == 0)
			{
				return;
			}

			if (!file.Exists)
			{
				file = EnsureFile(file);
			}
			else if (truncate)
			{
				file = EnsureFile(file, true);
			}

			using (TextWriter w = new StreamWriter(file.FullName, true, Encoding.UTF8))
			{
				foreach (string line in lines)
				{
					w.WriteLine(line);
				}

				w.Flush();
				w.Close();
			}
		}

		/// <summary>
		///     Ensures a files' existence
		/// </summary>
		/// <returns>FileInfo representing the file ensured for 'info'</returns>
		public static FileInfo EnsureFile(this FileInfo info)
		{
			return EnsureFile(info, false);
		}

		/// <summary>
		///     Ensures a files' existence
		/// </summary>
		/// <param name="info"></param>
		/// <param name="replace">True: replace the file if it exists</param>
		/// <returns>FileInfo representing the file ensured for 'info'</returns>
		public static FileInfo EnsureFile(this FileInfo info, bool replace)
		{
			EnsureDirectory(info.Directory, false);

			if (info.Exists && replace)
			{
				info.Delete();
				info.Create().Close();
			}
			else if (!info.Exists)
			{
				info.Create().Close();
			}

			return info;
		}

		/// <summary>
		///     Ensures a directories' existence
		/// </summary>
		/// <returns>DirectoryInfo representing the directory ensured for 'info'</returns>
		public static DirectoryInfo EnsureDirectory(this DirectoryInfo info)
		{
			return EnsureDirectory(info, false);
		}

		/// <summary>
		///     Ensures a directories' existence
		/// </summary>
		/// <param name="info"></param>
		/// <param name="replace">True: replace the directory if it exists</param>
		/// <returns>DirectoryInfo representing the directory ensured for 'info'</returns>
		public static DirectoryInfo EnsureDirectory(this DirectoryInfo info, bool replace)
		{
			if (info.Exists && replace)
			{
				EmptyDirectory(info, true);
				info.Delete(true);
				info.Create();
			}
			else if (!info.Exists)
			{
				info.Create();
			}

			return info;
		}

		/// <summary>
		///     Empties the contents of the specified directory with the option to include sub directories
		/// </summary>
		/// <param name="source">Directory to empty</param>
		/// <param name="incDirs">True: includes sub directories</param>
		public static void EmptyDirectory(this DirectoryInfo source, bool incDirs)
		{
			VitaNexCore.TryCatch(() => Parallel.ForEach(source.GetFiles(), file => VitaNexCore.TryCatch(file.Delete)));

			if (!incDirs)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() =>
				Parallel.ForEach(source.GetDirectories().Where(dir => dir != source), dir => VitaNexCore.TryCatch(dir.Delete, true)));
		}

		/// <summary>
		///     Empties the contents of the specified directory, including all sub-directories and files that are older than the given age.
		/// </summary>
		/// <param name="source">Directory to empty</param>
		/// <param name="age">Age at which a directory or file is considered old enough to be deleted</param>
		public static void EmptyDirectory(this DirectoryInfo source, TimeSpan age)
		{
			EmptyDirectory(source, age, "*", SearchOption.AllDirectories);
		}

		/// <summary>
		///     Empties the contents of the specified directory, only deleting files that meet the mask criteria and are older than the given age.
		/// </summary>
		/// <param name="source">Directory to empty</param>
		/// <param name="age">Age at which a directory or file is considered old enough to be deleted</param>
		/// <param name="mask">String mask to use to filter file names</param>
		/// <param name="option">Search options</param>
		public static void EmptyDirectory(this DirectoryInfo source, TimeSpan age, string mask, SearchOption option)
		{
			DateTime expire = DateTime.UtcNow.Subtract(age);

			VitaNexCore.TryCatch(
				() =>
				Parallel.ForEach(
					AllDirectories(source, mask, option).Where(dir => dir.CreationTimeUtc < expire),
					dir => VitaNexCore.TryCatch(dir.Delete, true)));

			VitaNexCore.TryCatch(
				() =>
				Parallel.ForEach(
					AllFiles(source, mask, option).Where(file => file.CreationTimeUtc < expire),
					file => VitaNexCore.TryCatch(file.Delete)));
		}

		/// <summary>
		///     Empties the contents of the specified directory, only deleting files that meet the mask criteria
		/// </summary>
		/// <param name="source">Directory to empty</param>
		/// <param name="mask">String mask to use to filter file names</param>
		/// <param name="option">Search options</param>
		public static void EmptyDirectory(this DirectoryInfo source, string mask, SearchOption option)
		{
			VitaNexCore.TryCatch(
				() => Parallel.ForEach(AllDirectories(source, mask, option), dir => VitaNexCore.TryCatch(dir.Delete, true)));

			VitaNexCore.TryCatch(
				() => Parallel.ForEach(AllFiles(source, mask, option), file => VitaNexCore.TryCatch(file.Delete)));
		}

		/// <summary>
		///     Copies the contents of the specified directory to the specified target directory, only including files that meet the mask criteria
		/// </summary>
		/// <param name="source">Directory to copy</param>
		/// <param name="dest">Directory to copy to</param>
		public static void CopyDirectory(this DirectoryInfo source, DirectoryInfo dest)
		{
			CopyDirectory(source, dest, "*", SearchOption.AllDirectories);
		}

		/// <summary>
		///     Copies the contents of the specified directory to the specified target directory, only including files that meet the mask criteria
		/// </summary>
		/// <param name="source">Directory to copy</param>
		/// <param name="dest">Directory to copy to</param>
		/// <param name="mask">String mask to use to filter file names</param>
		/// <param name="option">Search options</param>
		public static void CopyDirectory(this DirectoryInfo source, DirectoryInfo dest, string mask, SearchOption option)
		{
			VitaNexCore.TryCatch(
				() =>
				Parallel.ForEach(
					AllFiles(source, mask, option),
					file =>
					VitaNexCore.TryCatch(
						() => file.CopyTo(IOUtility.EnsureFile(file.FullName.Replace(source.FullName, dest.FullName)).FullName, true))));
		}

		public static IEnumerable<DirectoryInfo> AllDirectories(this DirectoryInfo source, string mask, SearchOption option)
		{
			foreach (DirectoryInfo dir in source.EnumerateDirectories(mask).Where(dir => dir != source))
			{
				if (option == SearchOption.AllDirectories)
				{
					foreach (DirectoryInfo sub in AllDirectories(dir, mask, SearchOption.AllDirectories).Where(sub => sub != source))
					{
						yield return sub;
					}
				}

				yield return dir;
			}
		}

		public static IEnumerable<FileInfo> AllFiles(this DirectoryInfo source, string mask, SearchOption option)
		{
			if (option == SearchOption.AllDirectories)
			{
				foreach (FileInfo file in
					source.EnumerateDirectories()
						  .Where(dir => dir != source)
						  .SelectMany(dir => AllFiles(dir, mask, SearchOption.AllDirectories)))
				{
					yield return file;
				}
			}

			foreach (FileInfo file in source.EnumerateFiles(mask))
			{
				yield return file;
			}
		}
	}
}