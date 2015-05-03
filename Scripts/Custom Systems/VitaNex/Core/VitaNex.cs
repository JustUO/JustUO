#region Header
//   Vorspire    _,-'/-'/  VitaNex.cs
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
using System.Threading;

using Server;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.Network;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex
{
	public static class TaskPriority
	{
		public const int Highest = 0, High = 250000, Medium = 500000, Low = 750000, Lowest = 1000000;
	}

	/// <summary>
	///     Exposes an interface for managing VitaNexCore and its' sub-systems.
	/// </summary>
	public static partial class VitaNexCore
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		private static VersionInfo _Version;

		public static VersionInfo Version
		{
			get { return _Version; }
			private set
			{
				_Version = value;

				if (_Version == null)
				{
					return;
				}

				_Version.Name = "VitaNexCore";
				_Version.Description = "Represents the local version value of Vita-Nex: Core";
			}
		}

		public static readonly object ConsoleLock = new object();
		public static readonly object IOLock = new object();

		private static readonly DateTime _Started = DateTime.UtcNow;

		/// <summary>
		///     Gets the amount of time that has passed since VitaNexCore was first initialized.
		/// </summary>
		public static TimeSpan UpTime { get { return DateTime.UtcNow - _Started; } }

		/// <summary>
		///     Gets the root directory for VitaNexCore.
		///     This is the directory where the core scripts reside.
		/// </summary>
		public static DirectoryInfo RootDirectory { get; private set; }

		/// <summary>
		///     Gets the working directory for VitaNexCore.
		/// </summary>
		public static DirectoryInfo BaseDirectory { get; private set; }

		/// <summary>
		///     Gets the build directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo BuildDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Build/"); } }

		/// <summary>
		///     Gets the data directory for VitaNexCore.
		/// </summary>
		public static DirectoryInfo DataDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Data/"); } }

		/// <summary>
		///     Gets the cache directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo CacheDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Cache/"); } }

		/// <summary>
		///     Gets the services directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo ServicesDirectory
		{
			get { return IOUtility.EnsureDirectory(BaseDirectory + "/Services/"); }
		}

		/// <summary>
		///     Gets the modules directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo ModulesDirectory
		{
			get { return IOUtility.EnsureDirectory(BaseDirectory + "/Modules/"); }
		}

		/// <summary>
		///     Gets the saves backup directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo BackupDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Backups/"); } }

		/// <summary>
		///     Gets the saves directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo SavesDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Saves/"); } }

		/// <summary>
		///     Gets the logs directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo LogsDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Logs/"); } }

		/// <summary>
		///     Gets a file used for unhandled and generec exception logging.
		/// </summary>
		public static FileInfo LogFile
		{
			get { return IOUtility.EnsureFile(LogsDirectory + "/Logs (" + DateTime.Now.ToSimpleString("D d M y") + ").log"); }
		}

		/// <summary>
		///     Gets a value representing whether VitaNexCore is busy performing a save or load action
		/// </summary>
		public static bool Busy { get; private set; }

		/// <summary>
		///     Gets a value representing the compile state of VitaNexCore
		/// </summary>
		public static bool Compiled { get; private set; }

		/// <summary>
		///     Gets a value representing the configure state of VitaNexCore
		/// </summary>
		public static bool Configured { get; private set; }

		/// <summary>
		///     Gets a value representing the initialize state of VitaNexCore
		/// </summary>
		public static bool Initialized { get; private set; }

		/// <summary>
		///     Gets a value representing whether this run is the first boot of VitaNexCore
		/// </summary>
		public static bool FirstBoot { get; private set; }

		public static bool Disposing { get; private set; }
		public static bool Disposed { get; private set; }

		public static event Action OnCompiled;
		public static event Action OnConfigured;
		public static event Action OnInitialized;
		public static event Action OnBackup;
		public static event Action OnSaved;
		public static event Action OnLoaded;
		public static event Action OnDispose;
		public static event Action OnDisposed;
		public static event Action<Exception> OnExceptionThrown;

		/// <summary>
		///     Configure method entry point, called by ScriptCompiler during compile of 'Scripts' directory.
		///     Performs a global invoke action, processing all Services and Modules that support CSConfig() and CMConfig()
		/// </summary>
		[CallPriority(Int32.MaxValue)]
		public static void Configure()
		{
			if (Configured)
			{
				return;
			}

			DisplayRetroBoot();

			CommandUtility.Register("VNC", AccessLevel.Administrator, OnCoreCommand);

			OutgoingPacketOverrides.Init();
			ExtendedOPL.Init();

			var now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Compile action started...");

			TryCatch(CompileServices, ToConsole);
			TryCatch(CompileModules, ToConsole);

			Compiled = true;

			if (OnCompiled != null)
			{
				TryCatch(OnCompiled, ToConsole);
			}

			var time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Compile action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);

			now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Configure action started...");

			TryCatch(ConfigureServices, ToConsole);
			TryCatch(ConfigureModules, ToConsole);

			Configured = true;

			if (OnConfigured != null)
			{
				TryCatch(OnConfigured, ToConsole);
			}

			time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Configure action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);

			ProcessINIT();

			EventSink.ServerStarted += () =>
			{
				EventSink.WorldSave += e =>
				{
					TryCatch(Backup, ToConsole);
					TryCatch(Save, ToConsole);
				};
				EventSink.Shutdown += e => TryCatch(Dispose, ToConsole);
				EventSink.Crashed += e => TryCatch(Dispose, ToConsole);
			};
		}

		/// <summary>
		///     Initialize method entry point, called by ScriptCompiler after compile of 'Scripts' directory.
		///     Performs a global invoke action, processing all Services and Modules that support CSInvoke() and CMInvoke()
		/// </summary>
		[CallPriority(Int32.MaxValue)]
		public static void Initialize()
		{
			if (Initialized)
			{
				return;
			}

			TryCatch(Load, ToConsole);

			var now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Invoke action started...");

			TryCatch(InvokeServices, ToConsole);
			TryCatch(InvokeModules, ToConsole);

			Initialized = true;

			if (OnInitialized != null)
			{
				TryCatch(OnInitialized, ToConsole);
			}

			var time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Invoke action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		/// <summary>
		///     Performs a global save action, processing all Services and Modules that support CSSave() and CMSave()
		/// </summary>
		public static void Save()
		{
			if (Busy)
			{
				ToConsole("Could not perform save action, the service is busy.");
				return;
			}

			Busy = true;

			var now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Save action started...");

			TryCatch(SaveServices, ToConsole);
			TryCatch(SaveModules, ToConsole);

			if (OnSaved != null)
			{
				TryCatch(OnSaved, ToConsole);
			}

			Busy = false;

			var time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Save action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		/// <summary>
		///     Performs a global load action, processing all Services and Modules that support CSLoad() and CMLoad()
		/// </summary>
		public static void Load()
		{
			if (Busy)
			{
				ToConsole("Could not perform load action, the service is busy.");
				return;
			}

			Busy = true;

			var now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Load action started...");

			TryCatch(LoadServices, ToConsole);
			TryCatch(LoadModules, ToConsole);

			if (OnLoaded != null)
			{
				TryCatch(OnLoaded, ToConsole);
			}

			Busy = false;

			var time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Load action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		/// <summary>
		///     Performs a global dispose action, processing all Services and Modules that support CSDispose() and CMDispose()
		/// </summary>
		public static void Dispose()
		{
			if (Busy || Disposing || Disposed)
			{
				ToConsole("Could not perform dispose action, the service is busy.");
				return;
			}

			Busy = Disposing = Disposed = true;

			var now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Dispose action started...");

			if (OnDispose != null)
			{
				TryCatch(OnDispose, ToConsole);
			}

			TryCatch(DisposeServices, ToConsole);
			TryCatch(DisposeModules, ToConsole);

			if (OnDisposed != null)
			{
				TryCatch(OnDisposed, ToConsole);
			}

			Busy = Disposing = false;

			var time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Dispose action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		/// <summary>
		///     Performs a global backup action, copying all files in the SavesDirectory to the BackupDirectory.
		/// </summary>
		public static void Backup()
		{
			if (Busy)
			{
				ToConsole("Could not perform backup action, the service is busy.");
				return;
			}

			Busy = true;

			var now = DateTime.UtcNow;

			ToConsole(String.Empty);
			ToConsole("Backup action started...");

			SavesDirectory.CopyDirectory(
				IOUtility.EnsureDirectory(BackupDirectory + "/" + DateTime.Now.ToSimpleString("D d M y"), true));

			if (_ExpireThread != null)
			{
				TryCatch(_ExpireThread.Abort);
				_ExpireThread = null;
			}

			_ExpireThread = new Thread(FlushExpired)
			{
				Name = "Backup Expire Flush",
				Priority = ThreadPriority.BelowNormal
			};
			_ExpireThread.Start();

			if (OnBackup != null)
			{
				TryCatch(OnBackup, ToConsole);
			}

			Busy = false;

			var time = (DateTime.UtcNow - now).TotalSeconds;

			ToConsole("Backup action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		private static Thread _ExpireThread;

		[STAThread]
		private static void FlushExpired()
		{
			BackupDirectory.EmptyDirectory(TimeSpan.FromDays(3.0));
		}

		private static void OnCoreCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null || e.Mobile.Deleted)
			{
				return;
			}

			if (e.Arguments == null || e.Arguments.Length == 0)
			{
				new MenuGump(
					e.Mobile as PlayerMobile,
					null,
					new MenuGumpOptions(
						new[]
						{
							new ListGumpEntry("Help", () => OnCoreCommand(new CommandEventArgs(e.Mobile, e.Command, "?", new[] {"?"}))),
							new ListGumpEntry("Services", () => new CoreServiceListGump(e.Mobile as PlayerMobile).Send()),
							new ListGumpEntry("Modules", () => new CoreModuleListGump(e.Mobile as PlayerMobile).Send())
						})).Send();
				return;
			}

			switch (e.Arguments[0].ToLower())
			{
				case "?":
				case "help":
				{
					e.Mobile.SendMessage(0x55, "Usage: {0}{1} <srv | mod | ? | help>", CommandSystem.Prefix, e.Command);
					e.Mobile.SendMessage(0x55, "Usage: srv <name> <ver | save>");
					e.Mobile.SendMessage(0x55, "Usage: mod <name> <ver | save | enable | disable>");
				}
					break;
				case "srv":
				{
					if (e.Arguments.Length < 2)
					{
						new CoreServiceListGump(e.Mobile as PlayerMobile).Send();
						return;
					}

					var search = e.Arguments[1];
					var info = _CoreServices.FirstOrDefault(csi => Insensitive.Contains(csi.Name, search));

					if (info == null)
					{
						new CoreServiceListGump(e.Mobile as PlayerMobile).Send();
						return;
					}

					if (e.Arguments.Length < 3)
					{
						e.Mobile.SendGump(new PropertiesGump(e.Mobile, info));
						return;
					}

					switch (e.Arguments[2].ToLower())
					{
						case "ver":
						case "version":
							e.Mobile.SendMessage(0x55, "{0} version: {1}", info.Name, info.Version);
							break;
						case "save":
						{
							var sh = info.GetSaveHandler();

							if (sh == null)
							{
								e.Mobile.SendMessage(0x22, "{0} does not implement the CSSave feature.", info.Name);
								return;
							}

							TryCatch(sh, ex => e.Mobile.SendMessage(0x22, "An error occured, check the logs for more information."));
						}
							break;
					}
				}
					break;
				case "mod":
				{
					if (e.Arguments.Length < 2)
					{
						new CoreModuleListGump(e.Mobile as PlayerMobile).Send();
						return;
					}

					var search = e.Arguments[1];
					var info = _CoreModules.FirstOrDefault(cmi => Insensitive.Contains(cmi.Name, search));

					if (info == null)
					{
						new CoreModuleListGump(e.Mobile as PlayerMobile).Send();
						return;
					}

					if (e.Arguments.Length < 3)
					{
						e.Mobile.SendGump(new PropertiesGump(e.Mobile, info));
						return;
					}

					switch (e.Arguments[2].ToLower())
					{
						case "ver":
						case "version":
							e.Mobile.SendMessage(0x55, "{0} version: {1}", info.Name, info.Version);
							break;
						case "enable":
						{
							if (info.Enabled)
							{
								e.Mobile.SendMessage(0x22, "{0} is already enabled.", info.Name);
							}
							else
							{
								info.Enabled = true;
								e.Mobile.SendMessage(0x55, "{0} has been enabled.", info.Name);
							}
						}
							break;
						case "disable":
						{
							if (!info.Enabled)
							{
								e.Mobile.SendMessage(0x22, "{0} is already disabled.", info.Name);
							}
							else
							{
								info.Enabled = false;
								e.Mobile.SendMessage(0x55, "{0} has been disabled.", info.Name);
							}
						}
							break;
						case "save":
						{
							var sh = info.GetSaveHandler();

							if (sh == null)
							{
								e.Mobile.SendMessage(0x22, "{0} does not implement the CMSave feature.", info.Name);
							}
							else
							{
								TryCatch(
									sh,
									ex =>
									{
										e.Mobile.SendMessage(0x22, "An error occured, check the logs for more information.");
										ToConsole(ex);
									});
							}
						}
							break;
					}
				}
					break;
			}
		}

		public static T TryCatchGet<T>(Func<T> func)
		{
			return TryCatchGet(func, ToConsole);
		}

		public static T TryCatchGet<T>(Func<T> func, Action<Exception> handler)
		{
			if (func != null)
			{
				try
				{
					return func();
				}
				catch (Exception e)
				{
					if (handler != null)
					{
						handler(e);
					}
				}
			}

			return default(T);
		}

		public static T TryCatchGet<T, TState>(Func<TState, T> func, TState state)
		{
			return TryCatchGet(func, state, null);
		}

		public static T TryCatchGet<T, TState>(Func<TState, T> func, TState state, Action<Exception> handler)
		{
			if (func != null)
			{
				try
				{
					return func(state);
				}
				catch (Exception e)
				{
					if (handler != null)
					{
						handler(e);
					}
				}
			}

			return default(T);
		}

		public static void TryCatch(Action action)
		{
			TryCatch(action, null);
		}

		public static void TryCatch(Action action, Action<Exception> handler)
		{
			if (action == null)
			{
				return;
			}

			try
			{
				action();
			}
			catch (Exception e)
			{
				if (handler != null)
				{
					handler(e);
				}
			}
		}

		public static void TryCatch<T>(Action<T> action, T state)
		{
			TryCatch(action, state, null);
		}

		public static void TryCatch<T>(Action<T> action, T state, Action<Exception> handler)
		{
			if (action == null)
			{
				return;
			}

			try
			{
				action(state);
			}
			catch (Exception e)
			{
				if (handler != null)
				{
					handler(e);
				}
			}
		}

		public static void WaitWhile(Func<bool> func)
		{
			WaitWhile(func, TimeSpan.MaxValue);
		}

		public static void WaitWhile(Func<bool> func, TimeSpan timeOut)
		{
			if (func == null)
			{
				return;
			}

			var now = DateTime.UtcNow;
			var expire = now.Add(timeOut);
			var test = true;

			while (test)
			{
				test = func();

				if (DateTime.UtcNow >= expire)
				{
					return;
				}

				Thread.Sleep(1);
			}
		}

		public static void ToConsole(string format, params object[] args)
		{
			lock (ConsoleLock)
			{
				Console.Write('[');
				Utility.PushColor(ConsoleColor.Yellow);
				Console.Write("VitaNexCore");
				Utility.PopColor();
				Console.Write("]: ");
				Utility.PushColor(ConsoleColor.DarkYellow);

				if (args.Length > 0)
				{
					Console.WriteLine(format, args);
				}
				else
				{
					Console.WriteLine(format);
				}

				Utility.PopColor();
			}
		}

		public static void ToConsole(Exception e)
		{
			lock (ConsoleLock)
			{
				Console.Write('[');
				Utility.PushColor(ConsoleColor.Yellow);
				Console.Write("VitaNexCore");
				Utility.PopColor();
				Console.Write("]: ");
				Utility.PushColor(ConsoleColor.DarkRed);
				Console.WriteLine(e);
				Utility.PopColor();
			}

			e.Log(LogFile);

			if (OnExceptionThrown != null)
			{
				OnExceptionThrown(e);
			}
		}

		private const ConsoleColor _BackgroundColor = ConsoleColor.Black;
		private const ConsoleColor _BorderColor = ConsoleColor.Green;
		private const ConsoleColor _TextColor = ConsoleColor.White;

		private static void DrawLine(string text = "", int align = 0)
		{
			text = text ?? "";
			align = Math.Max(0, Math.Min(2, align));

			var defBG = Console.BackgroundColor;
			const int borderWidth = 2;
			const int indentWidth = 1;
			var maxWidth = Console.WindowWidth - ((borderWidth + indentWidth) * 2);
			var lines = new List<string>();

			if (text.Length > maxWidth)
			{
				var words = text.Split(' ');

				if (words.Length == 0)
				{
					for (int i = 0, offset = 0, count; i < (text.Length / maxWidth); i++)
					{
						lines.Add(text.Substring(offset, (count = Math.Min(text.Length - offset, maxWidth))));
						offset += count;
					}
				}
				else
				{
					var rebuild = String.Empty;

					for (var wi = 0; wi < words.Length; wi++)
					{
						if (rebuild.Length + (words[wi].Length + 1) <= maxWidth)
						{
							rebuild += words[wi] + ' ';
						}
						else
						{
							lines.Add(rebuild);
							rebuild = words[wi] + ' ';
						}

						if (wi + 1 >= words.Length)
						{
							lines.Add(rebuild);
						}
					}
				}
			}
			else
			{
				lines.Add(text);
			}

			lock (ConsoleLock)
			{
				Utility.PushColor(_TextColor);

				foreach (var line in lines)
				{
					Console.BackgroundColor = _BorderColor;
					Console.Write(new String(' ', borderWidth));
					Console.BackgroundColor = _BackgroundColor;
					Console.Write(new String(' ', indentWidth));

					var len = maxWidth - line.Length;
					var str = line;

					switch (align)
					{
						//Center
						case 1:
							str = new String(' ', len / 2) + str + new String(' ', len / 2);
							break;
						//Right
						case 2:
							str = new String(' ', len) + str;
							break;
					}

					if (str.Length < maxWidth)
					{
						str += new String(' ', maxWidth - str.Length);
					}

					Console.Write(str);
					Console.Write(new String(' ', indentWidth));
					Console.BackgroundColor = _BorderColor;
					Console.Write(new String(' ', borderWidth));
				}

				lines.Clear();
				lines.TrimExcess();

				Console.BackgroundColor = defBG;
				Utility.PopColor();
			}
		}

		private static void DisplayRetroBoot()
		{
#if MONO
	    Console.WriteLine("VITA-NEX: CORE " + Version );
	    Console.WriteLine("Root Directory:     " + RootDirectory);
	    Console.WriteLine("Working Directory:  " + BaseDirectory);
	    Console.WriteLine("Build Directory:    " + BuildDirectory);
	    Console.WriteLine("Data Directory:     " + DataDirectory);
	    Console.WriteLine("Cache Directory:    " + CacheDirectory);
	    Console.WriteLine("Services Directory: " + ServicesDirectory);
	    Console.WriteLine("Modules Directory:  " + ModulesDirectory);
	    Console.WriteLine("Backup Directory:   " + BackupDirectory);
	    Console.WriteLine("Saves Directory:    " + SavesDirectory);
	    Console.WriteLine("Logs Directory:     " + LogsDirectory);
	    Console.WriteLine("http://core.vita-nex.com");
	    
	    if (FirstBoot)
	    {
		Console.WriteLine("Please see: " + RootDirectory + "/LICENSE");
	    }
	    
	    if (Core.Debug)
	    {
		Console.WriteLine("Server is running in DEBUG mode.");
	    }
	    #else
			ConsoleColor defBG;

			lock (ConsoleLock)
			{
				defBG = Console.BackgroundColor;
				Console.WriteLine();

				Console.BackgroundColor = _BorderColor;
				Console.Write(new String(' ', Console.WindowWidth));
			}

			DrawLine();
			DrawLine("**** VITA-NEX: CORE " + Version + " ****", 1);
			DrawLine();

			DrawLine("Root Directory:     " + RootDirectory);
			DrawLine("Working Directory:  " + BaseDirectory);
			DrawLine("Build Directory:    " + BuildDirectory);
			DrawLine("Data Directory:     " + DataDirectory);
			DrawLine("Cache Directory:    " + CacheDirectory);
			DrawLine("Services Directory: " + ServicesDirectory);
			DrawLine("Modules Directory:  " + ModulesDirectory);
			DrawLine("Backup Directory:   " + BackupDirectory);
			DrawLine("Saves Directory:    " + SavesDirectory);
			DrawLine("Logs Directory:     " + LogsDirectory);
			DrawLine();

			DrawLine("http://core.vita-nex.com", 1);
			DrawLine();

			if (FirstBoot)
			{
				File.ReadAllLines(IOUtility.GetSafeFilePath(RootDirectory + "/LICENSE", true)).ForEach(line => DrawLine(line));
				DrawLine();
			}

			if (Core.Debug)
			{
				DrawLine("Server is running in DEBUG mode.");
				DrawLine();
			}

			lock (ConsoleLock)
			{
				Console.BackgroundColor = _BorderColor;
				Console.Write(new String(' ', Console.WindowWidth));

				Console.BackgroundColor = defBG;
				Utility.PopColor();
				Console.WriteLine();
			}
#endif
		}
	}
}