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
#endregion

namespace VitaNex
{
	public static class TaskPriority
	{
		public const int Highest = 10000, High = 75000, Medium = 50000, Low = 25000, Lowest = 0;
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

				_Version.Name = "VitaNexCore";
				_Version.Description = "Represents the local version value of Vita-Nex: Core";
			}
		}

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
		public static DirectoryInfo ServicesDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Services/"); } }

		/// <summary>
		///     Gets the modules directory for VitaNexCore
		/// </summary>
		public static DirectoryInfo ModulesDirectory { get { return IOUtility.EnsureDirectory(BaseDirectory + "/Modules/"); } }

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
		public static FileInfo LogFile { get { return IOUtility.EnsureFile(LogsDirectory + "/Logs (" + DateTime.Now.ToSimpleString("D d M y") + ").log"); } }

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
		///     Gets a value representing whether this is the first boot of VitaNexCore
		/// </summary>
		public static bool FirstBoot { get; private set; }

		public static event Action OnCompiled;
		public static event Action OnConfigured;
		public static event Action OnInitialized;
		public static event Action OnSaved;
		public static event Action OnLoaded;
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

			DateTime now = DateTime.UtcNow;
			Console.WriteLine();
			ToConsole("Compile action started...");

			CompileServices();
			CompileModules();

			Compiled = true;

			if (OnCompiled != null)
			{
				TryCatch(OnCompiled, ToConsole);
			}

			double time = (DateTime.UtcNow - now).TotalSeconds;
			ToConsole("Compile action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);

			now = DateTime.UtcNow;
			Console.WriteLine();
			ToConsole("Configure action started...");

			ConfigureServices();
			ConfigureModules();

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
				EventSink.WorldSave += e => TryCatch(Save, ToConsole);
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

			DateTime now = DateTime.UtcNow;
			Console.WriteLine();
			ToConsole("Invoke action started...");

			InvokeServices();
			InvokeModules();

			Initialized = true;

			if (OnInitialized != null)
			{
				TryCatch(OnInitialized, ToConsole);
			}

			double time = (DateTime.UtcNow - now).TotalSeconds;
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
			DateTime now = DateTime.UtcNow;
			Console.WriteLine();
			ToConsole("Save action started...");

			SaveServices();
			SaveModules();

			Busy = false;

			if (OnSaved != null)
			{
				TryCatch(OnSaved, ToConsole);
			}

			double time = (DateTime.UtcNow - now).TotalSeconds;
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
			DateTime now = DateTime.UtcNow;
			Console.WriteLine();
			ToConsole("Load action started...");

			LoadServices();
			LoadModules();

			Busy = false;

			if (OnLoaded != null)
			{
				TryCatch(OnLoaded, ToConsole);
			}

			double time = (DateTime.UtcNow - now).TotalSeconds;
			ToConsole("Load action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		/// <summary>
		///     Performs a global dispose action, processing all Services and Modules that support CSDispose() and CMDispose()
		/// </summary>
		public static void Dispose()
		{
			if (Busy)
			{
				ToConsole("Could not perform dispose action, the service is busy.");
				return;
			}

			Busy = true;
			DateTime now = DateTime.UtcNow;
			Console.WriteLine();
			ToConsole("Dispose action started...");

			DisposeServices();
			DisposeModules();

			Busy = false;

			if (OnDisposed != null)
			{
				TryCatch(OnDisposed, ToConsole);
			}

			double time = (DateTime.UtcNow - now).TotalSeconds;
			ToConsole("Dispose action completed in {0:F2} second{1}", time, (time != 1) ? "s" : String.Empty);
		}

		private static void OnCoreCommand(CommandEventArgs e)
		{
			if (e == null || e.Mobile == null || e.Mobile.Deleted)
			{
				return;
			}

			if (e.Arguments == null || e.Arguments.Length == 0)
			{
				OnCoreCommand(new CommandEventArgs(e.Mobile, e.Command, "?", new[] {"?"}));
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

						string search = e.Arguments[1];
						CoreServiceInfo info = _CoreServices.FirstOrDefault(csi => Insensitive.Contains(csi.Name, search));

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
									Action sh = info.GetSaveHandler();

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

						string search = e.Arguments[1];
						CoreModuleInfo info = _CoreModules.FirstOrDefault(cmi => Insensitive.Contains(cmi.Name, search));

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
									Action sh = info.GetSaveHandler();

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

		public static void TryCatch(Action action)
		{
			TryCatch(action, null);
		}

		public static void TryCatch(Action action, Action<Exception> handler)
		{
			if (action != null)
			{
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
		}

		public static void WaitWhile(Func<bool> func, TimeSpan? timeOut = null)
		{
			DateTime now = DateTime.UtcNow;
			int sleep = timeOut != null ? (int)Math.Max(1, Math.Min(1000, timeOut.Value.TotalMilliseconds / 10)) : 1;

			if (func != null)
			{
				if (timeOut != null)
				{
					DateTime expire = now + timeOut.Value;

					while (func())
					{
						Thread.Sleep(sleep);

						if (DateTime.UtcNow >= expire)
						{
							break;
						}
					}
				}
				else
				{
					while (func())
					{
						Thread.Sleep(sleep);
					}
				}
			}
			else if (timeOut != null)
			{
				DateTime expire = now + timeOut.Value;
				bool timedOut = false;

				while (!timedOut)
				{
					Thread.Sleep(sleep);

					if (DateTime.UtcNow >= expire)
					{
						timedOut = true;
					}
				}
			}
		}

		public static void ToConsole(string format, params object[] args)
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

		public static void ToConsole(Exception e)
		{
			Console.Write('[');
			Utility.PushColor(ConsoleColor.Yellow);
			Console.Write("VitaNexCore");
			Utility.PopColor();
			Console.Write("]: ");
			Utility.PushColor(ConsoleColor.DarkRed);
			Console.WriteLine(e);
			Utility.PopColor();

			e.Log(LogFile);

			if (OnExceptionThrown != null)
			{
				OnExceptionThrown(e);
			}
		}

		private const ConsoleColor _BackgroundColor = ConsoleColor.Blue;
		private const ConsoleColor _BorderColor = ConsoleColor.DarkCyan;
		private const ConsoleColor _TextColor = ConsoleColor.Gray;

		private static void DrawLine(string text = "", int align = 0)
		{
			text = text ?? "";
			align = Math.Max(0, Math.Min(2, align));

			ConsoleColor defBG = Console.BackgroundColor;
			const int borderWidth = 2;
			const int indentWidth = 1;
			int maxWidth = Console.WindowWidth - ((borderWidth + indentWidth) * 2);
			var lines = new List<string>();

			Utility.PushColor(_TextColor);

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
					string rebuild = String.Empty;

					for (int wi = 0; wi < words.Length; wi++)
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

			foreach (string line in lines)
			{
				Console.BackgroundColor = _BorderColor;
				Console.Write(new String(' ', borderWidth));
				Console.BackgroundColor = _BackgroundColor;
				Console.Write(new String(' ', indentWidth));

				int len = maxWidth - line.Length;
				string str = line;

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

			Console.BackgroundColor = defBG;
			Utility.PopColor();
		}

		private static void DisplayRetroBoot()
		{
			ConsoleColor defBG = Console.BackgroundColor;
			Console.WriteLine();

			Console.BackgroundColor = _BorderColor;
			Console.Write(new String(' ', Console.WindowWidth));

			DrawLine();
			DrawLine("**** VITA-NEX: CORE " + Version + " ****", 1);
			DrawLine();
			DrawLine("READY.");
			DrawLine(@"RUN UO\VORSPIRE\VITA-NEX\");
			DrawLine();

			DrawLine("Root Directory:     " + RootDirectory);
			DrawLine("Working Directory:  " + BaseDirectory);
			DrawLine("Build Directory:    " + BuildDirectory);
			DrawLine("Data Directory:     " + DataDirectory);
			DrawLine("Cache Directory:    " + CacheDirectory);
			DrawLine("Services Directory: " + ServicesDirectory);
			DrawLine("Modules Directory:  " + ModulesDirectory);
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

			Console.BackgroundColor = _BorderColor;
			Console.Write(new String(' ', Console.WindowWidth));

			Console.BackgroundColor = defBG;
			Utility.PopColor();
			Console.WriteLine();
		}
	}
}