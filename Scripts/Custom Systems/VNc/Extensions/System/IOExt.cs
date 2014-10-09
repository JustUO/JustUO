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
using System.Text;
#endregion

namespace System.IO
{
	public static class IOExtUtility
	{
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
		/// <param name="fromDir">Directory to empty</param>
		/// <param name="incDirs">True: includes sub directories</param>
		public static void EmptyDirectory(this DirectoryInfo fromDir, bool incDirs)
		{
			try
			{
				var files = fromDir.GetFiles();

				foreach (FileInfo file in files)
				{
					try
					{
						file.Delete();
					}
					catch
					{ }
				}

				if (incDirs)
				{
					var dirs = fromDir.GetDirectories();

					foreach (DirectoryInfo dir in dirs)
					{
						if (fromDir == dir)
						{
							continue;
						}

						try
						{
							dir.Delete(true);
						}
						catch
						{ }
					}
				}
			}
			catch
			{ }
		}

		/// <summary>
		///     Empties the contents of the specified directory, only deleting files that meet the mask criteria
		/// </summary>
		/// <param name="fromDir">Directory to empty</param>
		/// <param name="mask">String mask to use to filter file names</param>
		/// <param name="option">Search options</param>
		public static void EmptyDirectory(this DirectoryInfo fromDir, string mask, SearchOption option)
		{
			try
			{
				var files = fromDir.GetFiles(mask, option);

				foreach (FileInfo file in files)
				{
					try
					{
						file.Delete();
					}
					catch
					{ }
				}

				var dirs = fromDir.GetDirectories(mask, option);

				foreach (DirectoryInfo dir in dirs)
				{
					try
					{
						dir.Delete(true);
					}
					catch
					{ }
				}
			}
			catch
			{ }
		}
	}
}