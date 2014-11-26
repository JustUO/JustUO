#region Header
//   Vorspire    _,-'/-'/  VitaNex_Services.cs
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
using System.Reflection;

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.IO;
using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex
{
	/// <summary>
	///     Exposes an interface for managing VitaNexCore and sub-systems.
	/// </summary>
	public static partial class VitaNexCore
	{
		private static List<CoreServiceInfo> _CoreServices;

		public static CoreServiceInfo[] CoreServices { get { return _CoreServices.ToArray(); } }

		public static Dictionary<Type, CoreServiceAttribute> CoreServiceTypeCache { get; private set; }
		public static Assembly[] ServiceAssemblies { get; private set; }

		public static event Action<CoreServiceInfo> OnServiceConfigured;
		public static event Action<CoreServiceInfo> OnServiceInvoked;
		public static event Action<CoreServiceInfo> OnServiceSaved;
		public static event Action<CoreServiceInfo> OnServiceLoaded;
		public static event Action<CoreServiceInfo> OnServiceDisposed;

		public static CoreServiceInfo GetService(Type t)
		{
			return CoreServices.FirstOrDefault(csi => csi.TypeOf.IsEqualOrChildOf(t));
		}

		public static CoreServiceInfo[] GetServices(string name, bool ignoreCase = true)
		{
			return FindServices(name, ignoreCase).ToArray();
		}

		public static IEnumerable<CoreServiceInfo> FindServices(string name, bool ignoreCase = true)
		{
			return
				CoreServices.Where(
					csi =>
					String.Equals(
						csi.Name, name, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
		}

		private static void CompileServices()
		{
			if (Compiled)
			{
				return;
			}

			ToConsole("Compiling Services...");

			TryCatch(
				() =>
				{
					var files = ServicesDirectory.GetFiles("*.vnc.srv.dll", SearchOption.AllDirectories);
					var asm = new List<Assembly>(files.Length);

					foreach (FileInfo file in files)
					{
						TryCatch(
							() =>
							{
								Assembly a = Assembly.LoadFrom(file.FullName);

								if (a != null && !asm.Contains(a))
								{
									asm.Add(a);
								}
							},
							ToConsole);
					}

					ServiceAssemblies = asm.ToArray();
					asm.AddRange(ScriptCompiler.Assemblies);
					ScriptCompiler.Assemblies = asm.ToArray();
				},
				ToConsole);
		}

		public static void ConfigureServices()
		{
			ToConsole("Configuring Services...");
			var types = GetCoreServiceTypes();

			_CoreServices = new List<CoreServiceInfo>(types.Count);

			foreach (CoreServiceInfo csi in types.Select(kvp => new CoreServiceInfo(kvp.Key, kvp.Value)))
			{
				_CoreServices.Add(csi);
				TryCatch(csi.OnRegistered, csi.ToConsole);
			}

			foreach (var csi in _CoreServices.OrderBy(csi => csi.Priority))
			{
				ConfigureService(csi);
			}
		}

		public static void ConfigureService(CoreServiceInfo csi)
		{
			if (csi == null)
			{
				return;
			}

			csi.ToConsole("Configuring...");

			if (!csi.Configured)
			{
				if (csi.ConfigSupported)
				{
					TryCatch(csi.GetConfigHandler(), csi.ToConsole);
				}

				TryCatch(csi.OnConfigured, csi.ToConsole);

				if (OnServiceConfigured != null)
				{
					TryCatch(() => OnServiceConfigured(csi), csi.ToConsole);
				}

				csi.ToConsole("Done.");
			}
			else
			{
				csi.ToConsole("Already configured, no action taken.");
			}
		}

		public static void InvokeServices()
		{
			ToConsole("Invoking Services...");

			foreach (var csi in _CoreServices.OrderBy(csi => csi.Priority))
			{
				InvokeService(csi);
			}
		}

		public static void InvokeService(CoreServiceInfo csi)
		{
			if (csi == null)
			{
				return;
			}

			csi.ToConsole("Invoking...");

			if (!csi.Invoked)
			{
				if (csi.InvokeSupported)
				{
					TryCatch(csi.GetInvokeHandler(), csi.ToConsole);
				}

				TryCatch(csi.OnInvoked, csi.ToConsole);

				if (OnServiceInvoked != null)
				{
					TryCatch(() => OnServiceInvoked(csi), csi.ToConsole);
				}

				csi.ToConsole("Done.");
			}
			else
			{
				csi.ToConsole("Already invoked, no action taken.");
			}
		}

		public static void SaveServices()
		{
			ToConsole("Saving Services...");

			foreach (var csi in _CoreServices.OrderBy(csi => csi.Priority))
			{
				SaveService(csi);
			}
		}

		public static void SaveService(CoreServiceInfo csi)
		{
			if (csi == null)
			{
				return;
			}

			csi.ToConsole("Saving...");

			TryCatch(csi.SaveOptions, csi.ToConsole);

			if (csi.SaveSupported)
			{
				TryCatch(csi.GetSaveHandler(), csi.ToConsole);
			}

			TryCatch(csi.OnSaved, csi.ToConsole);

			if (OnServiceSaved != null)
			{
				TryCatch(() => OnServiceSaved(csi), csi.ToConsole);
			}

			csi.ToConsole("Done.");
		}

		public static void LoadServices()
		{
			ToConsole("Loading Services...");

			foreach (var csi in _CoreServices.OrderBy(csi => csi.Priority))
			{
				LoadService(csi);
			}
		}

		public static void LoadService(CoreServiceInfo csi)
		{
			if (csi == null)
			{
				return;
			}

			csi.ToConsole("Loading...");

			TryCatch(csi.LoadOptions, csi.ToConsole);

			if (csi.LoadSupported)
			{
				TryCatch(csi.GetLoadHandler(), csi.ToConsole);
			}

			TryCatch(csi.OnLoaded, csi.ToConsole);

			if (OnServiceLoaded != null)
			{
				TryCatch(() => OnServiceLoaded(csi), csi.ToConsole);
			}

			csi.ToConsole("Done.");
		}

		public static void DisposeServices()
		{
			ToConsole("Disposing Services...");

			foreach (var csi in _CoreServices.OrderByDescending(csi => csi.Priority))
			{
				DisposeService(csi);
			}
		}

		public static void DisposeService(CoreServiceInfo csi)
		{
			if (csi == null)
			{
				return;
			}

			csi.ToConsole("Disposing...");

			if (!csi.Disposed)
			{
				if (csi.DisposeSupported)
				{
					TryCatch(csi.GetDisposeHandler(), csi.ToConsole);
				}

				TryCatch(csi.OnDisposed, csi.ToConsole);

				if (OnServiceDisposed != null)
				{
					TryCatch(() => OnServiceDisposed(csi), csi.ToConsole);
				}

				csi.ToConsole("Done.");
			}
			else
			{
				csi.ToConsole("Already disposed, no action taken.");
			}
		}

		/// <summary>
		///     Gets a collection of [cached] Types representing all CoreServices in this assembly
		/// </summary>
		public static Dictionary<Type, CoreServiceAttribute> GetCoreServiceTypes()
		{
			if (CoreServiceTypeCache != null && CoreServiceTypeCache.Count > 0)
			{
				return CoreServiceTypeCache;
			}

			CoreServiceTypeCache = new Dictionary<Type, CoreServiceAttribute>();

			foreach (var kvp in
				ScriptCompiler.Assemblies.SelectMany(
					asm => GetCoreServiceTypes(asm).Where(kvp => !CoreServiceTypeCache.ContainsKey(kvp.Key))))
			{
				CoreServiceTypeCache.Add(kvp.Key, kvp.Value);
			}

			return CoreServiceTypeCache;
		}

		private static IEnumerable<KeyValuePair<Type, CoreServiceAttribute>> GetCoreServiceTypes(Assembly asm)
		{
			CoreServiceAttribute[] attrs;

			foreach (Type typeOf in asm.GetTypes().Where(typeOf => typeOf != null && typeOf.IsClass))
			{
				attrs = typeOf.GetCustomAttributes(typeof(CoreServiceAttribute), false) as CoreServiceAttribute[];

				if (attrs != null && attrs.Length != 0)
				{
					yield return new KeyValuePair<Type, CoreServiceAttribute>(typeOf, attrs[0]);
				}
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class CoreServiceAttribute : Attribute
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public int Priority { get; set; }
		public bool Debug { get; set; }
		public bool QuietMode { get; set; }

		public CoreServiceAttribute(
			string name, string version, int priority = TaskPriority.Medium, bool debug = false, bool quietMode = true)
		{
			Name = name;
			Version = version;
			Priority = priority;
			Debug = debug;
			QuietMode = quietMode;
		}
	}

	public sealed class CoreServiceInfo : IComparable<CoreServiceInfo>
	{
		private const BindingFlags SearchFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		private readonly PropertyInfo _OptionsProperty;
		private readonly MethodInfo _ConfigMethod;
		private readonly MethodInfo _InvokeMethod;
		private readonly MethodInfo _LoadMethod;
		private readonly MethodInfo _SaveMethod;
		private readonly MethodInfo _DisposeMethod;

		private readonly Type _TypeOf;
		private readonly int _Priority;

		private bool _Debug;
		private bool _QuietMode;
		private string _Name;

		private Action _ConfigHandler;
		private Action _InvokeHandler;
		private Action _LoadHandler;
		private Action _SaveHandler;
		private Action _DisposeHandler;

		private CoreServiceOptions _Options;
		private VersionInfo _Version = new VersionInfo();

		public bool OptionsSupported { get { return _OptionsProperty != null; } }
		public bool ConfigSupported { get { return _ConfigMethod != null; } }
		public bool InvokeSupported { get { return _InvokeMethod != null; } }
		public bool LoadSupported { get { return _LoadMethod != null; } }
		public bool SaveSupported { get { return _SaveMethod != null; } }
		public bool DisposeSupported { get { return _DisposeMethod != null; } }

		public bool Configured { get; private set; }
		public bool Invoked { get; private set; }
		public bool Disposed { get; private set; }

		public Assembly DynamicAssembly { get; private set; }
		public FileInfo DynamicAssemblyFile { get; private set; }

		public bool Dynamic { get { return (DynamicAssembly != null && DynamicAssemblyFile != null); } }

		[CommandProperty(VitaNexCore.Access)]
		public int Priority { get { return _Priority; } }

		[CommandProperty(VitaNexCore.Access)]
		public Type TypeOf { get { return _TypeOf; } }

		[CommandProperty(VitaNexCore.Access)]
		public VersionInfo Version { get { return _Version ?? (_Version = new VersionInfo()); } }

		[CommandProperty(VitaNexCore.Access)]
		public string Name
		{
			get { return _Name ?? (_Name = _TypeOf.Name); }
			set
			{
				_Name = value ?? _TypeOf.Name;
				SaveState();
			}
		}

		[CommandProperty(VitaNexCore.Access)]
		public bool Debug
		{
			get { return _Debug; }
			set
			{
				_Debug = value;
				SaveState();
			}
		}

		[CommandProperty(VitaNexCore.Access)]
		public bool QuietMode
		{
			get { return _QuietMode; }
			set
			{
				_QuietMode = value;
				SaveState();
			}
		}

		[CommandProperty(VitaNexCore.Access)]
		public CoreServiceOptions Options
		{
			get
			{
				return OptionsSupported && _OptionsProperty.CanRead
						   ? (CoreServiceOptions)_OptionsProperty.GetValue(_TypeOf, null)
						   : (_Options ?? (_Options = new CoreServiceOptions(_TypeOf)));
			}
			set
			{
				if (OptionsSupported && _OptionsProperty.CanWrite)
				{
					_OptionsProperty.SetValue(_TypeOf, value, null);
				}
				else
				{
					_Options = (value ?? new CoreServiceOptions(_TypeOf));
				}
			}
		}

		public CoreServiceInfo(Type type, CoreServiceAttribute attr)
			: this(type, attr.Version, attr.Name, attr.Priority, attr.Debug, attr.QuietMode)
		{ }

		public CoreServiceInfo(
			Type type, string version, string name, int priority, bool debug, bool quietMode, Assembly dynAsm = null)
		{
			_TypeOf = type;
			_Name = name;
			_Version = version;
			_Priority = priority;
			_Debug = debug;
			_QuietMode = quietMode;

			if (dynAsm == null)
			{
				dynAsm =
					VitaNexCore.ServiceAssemblies.FirstOrDefault(
						a =>
						_TypeOf.Assembly != Core.Assembly && !ScriptCompiler.Assemblies.Contains(_TypeOf.Assembly) &&
						a == _TypeOf.Assembly);
			}

			if (dynAsm != null && dynAsm.Location.EndsWith(".vnc.srv.dll"))
			{
				DynamicAssembly = dynAsm;
				DynamicAssemblyFile = new FileInfo(DynamicAssembly.Location);
				_Version = DynamicAssembly.GetName().Version.ToString();
			}

			_OptionsProperty = _TypeOf.GetProperty("CSOptions", SearchFlags);
			_ConfigMethod = _TypeOf.GetMethod("CSConfig", SearchFlags);
			_InvokeMethod = _TypeOf.GetMethod("CSInvoke", SearchFlags);
			_DisposeMethod = _TypeOf.GetMethod("CSDispose", SearchFlags);
			_LoadMethod = _TypeOf.GetMethod("CSLoad", SearchFlags);
			_SaveMethod = _TypeOf.GetMethod("CSSave", SearchFlags);
		}

		public void OnRegistered()
		{
			LoadState();
		}

		public void OnConfigured()
		{
			Configured = true;
		}

		public void OnInvoked()
		{
			Invoked = true;
		}

		public void OnDisposed()
		{
			Disposed = true;
		}

		public void OnSaved()
		{ }

		public void OnLoaded()
		{ }

		public void SaveState()
		{
			IOUtility.EnsureFile(VitaNexCore.CacheDirectory + "/States/" + _TypeOf.FullName + ".state", true).Serialize(
				writer =>
				{
					int version = writer.SetVersion(0);

					switch (version)
					{
						case 0:
							{
								writer.Write(_Name);
								writer.Write(_Debug);
								writer.Write(_QuietMode);
							}
							break;
					}
				});
		}

		public void LoadState()
		{
			IOUtility.EnsureFile(VitaNexCore.CacheDirectory + "/States/" + _TypeOf.FullName + ".state").Deserialize(
				reader =>
				{
					if (reader.End())
					{
						return;
					}

					int version = reader.GetVersion();

					switch (version)
					{
						case 0:
							{
								_Name = reader.ReadString();
								_Debug = reader.ReadBool();
								_QuietMode = reader.ReadBool();
							}
							break;
					}
				});
		}

		public void SaveOptions()
		{
			IOUtility.EnsureFile(VitaNexCore.CacheDirectory + "/Options/" + _TypeOf.FullName + ".opt", true).Serialize(
				writer =>
				{
					int version = writer.SetVersion(0);

					switch (version)
					{
						case 0:
							{
								writer.WriteType(
									Options,
									t =>
									{
										if (t != null)
										{
											Options.Serialize(writer);
										}
									});
							}
							break;
					}
				});
		}

		public void LoadOptions()
		{
			IOUtility.EnsureFile(VitaNexCore.CacheDirectory + "/Options/" + _TypeOf.FullName + ".opt").Deserialize(
				reader =>
				{
					if (reader.End())
					{
						return;
					}

					int version = reader.GetVersion();

					switch (version)
					{
						case 0:
							{
								if (reader.ReadType() != null)
								{
									Options.Deserialize(reader);
								}
							}
							break;
					}
				});
		}

		public Action GetConfigHandler(bool throwException = false)
		{
			if (_ConfigHandler != null)
			{
				return _ConfigHandler;
			}

			if (ConfigSupported)
			{
				return (_ConfigHandler = () => _ConfigMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CSConfig' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public Action GetInvokeHandler(bool throwException = false)
		{
			if (_InvokeHandler != null)
			{
				return _InvokeHandler;
			}

			if (InvokeSupported)
			{
				return (_InvokeHandler = () => _InvokeMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CSInvoke' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public Action GetDisposeHandler(bool throwException = false)
		{
			if (_DisposeHandler != null)
			{
				return _DisposeHandler;
			}

			if (DisposeSupported)
			{
				return (_DisposeHandler = () => _DisposeMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CSDispose' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public Action GetLoadHandler(bool throwException = false)
		{
			if (_LoadHandler != null)
			{
				return _LoadHandler;
			}

			if (LoadSupported)
			{
				return (_LoadHandler = () => _LoadMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CSLoad' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public Action GetSaveHandler(bool throwException = false)
		{
			if (_SaveHandler != null)
			{
				return _SaveHandler;
			}

			if (SaveSupported)
			{
				return (_SaveHandler = () => _SaveMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CSSave' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public int CompareTo(CoreServiceInfo csi)
		{
			return csi == null ? -1 : _Priority.CompareTo(csi.Priority);
		}

		public void ToConsole(string[] lines)
		{
			if (QuietMode || lines == null || lines.Length == 0)
			{
				return;
			}

			foreach (string line in lines)
			{
				ToConsole(line);
			}
		}

		public void ToConsole(string format, params object[] args)
		{
			if (QuietMode)
			{
				return;
			}

			lock (VitaNexCore.ConsoleLock)
			{
				Console.Write('[');
				Utility.PushColor(ConsoleColor.Cyan);
				Console.Write(Name);
				Utility.PopColor();
				Console.Write("]: ");
				Utility.PushColor(ConsoleColor.DarkCyan);
				Console.WriteLine(format, args);
				Utility.PopColor();
			}
		}

		public void ToConsole(Exception[] errors)
		{
			if (errors == null || errors.Length == 0)
			{
				return;
			}

			foreach (Exception e in errors)
			{
				ToConsole(e);
			}
		}

		public void ToConsole(Exception e)
		{
			if (e == null)
			{
				return;
			}

			lock (VitaNexCore.ConsoleLock)
			{
				Console.Write('[');
				Utility.PushColor(ConsoleColor.Cyan);
				Console.Write(Name);
				Utility.PopColor();
				Console.Write("]: ");
				Utility.PushColor(ConsoleColor.DarkRed);
				Console.WriteLine((QuietMode && !Debug) ? e.Message : e.ToString());
				Utility.PopColor();
			}

			if (Debug)
			{
				e.Log(IOUtility.EnsureFile(VitaNexCore.LogsDirectory + "/Debug/" + TypeOf.FullName + ".log"));
			}
		}
	}

	public class CoreServiceOptions : PropertyObject
	{
		private Type _ServiceType;

		public CoreServiceInfo ServiceInstance { get { return VitaNexCore.GetService(_ServiceType); } }

		[CommandProperty(VitaNexCore.Access)]
		public Type ServiceType { get { return _ServiceType; } }

		[CommandProperty(VitaNexCore.Access)]
		public string ServiceVersion { get { return ServiceInstance.Version; } }

		[CommandProperty(VitaNexCore.Access)]
		public string ServiceName { get { return ServiceInstance.Name; } set { ServiceInstance.Name = value; } }

		[CommandProperty(VitaNexCore.Access)]
		public bool ServiceDebug { get { return ServiceInstance.Debug; } set { ServiceInstance.Debug = value; } }

		[CommandProperty(VitaNexCore.Access)]
		public bool ServiceQuietMode { get { return ServiceInstance.QuietMode; } set { ServiceInstance.QuietMode = value; } }

		public CoreServiceOptions(Type serviceType)
		{
			_ServiceType = serviceType;
		}

		public CoreServiceOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			ServiceDebug = false;
			ServiceQuietMode = false;
		}

		public override void Reset()
		{
			ServiceDebug = false;
			ServiceQuietMode = false;
		}

		public virtual void ToConsole(string[] lines)
		{
			ServiceInstance.ToConsole(lines);
		}

		public virtual void ToConsole(string format, params object[] args)
		{
			ServiceInstance.ToConsole(format, args);
		}

		public virtual void ToConsole(Exception[] errors)
		{
			ServiceInstance.ToConsole(errors);
		}

		public virtual void ToConsole(Exception e)
		{
			ServiceInstance.ToConsole(e);
		}

		public override string ToString()
		{
			return "Service Options";
		}

		public override void Serialize(GenericWriter writer)
		{
			writer.Write(VitaNexCore.Version);

			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.WriteType(_ServiceType);
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			/*string vncVersion = reader.ReadString();*/
			reader.ReadString();

			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					_ServiceType = reader.ReadType();
					break;
			}
		}
	}

	public sealed class CoreServiceListGump : ListGump<CoreServiceInfo>
	{
		public int VersionHue { get; set; }
		public int VersionOODHue { get; set; }

		public CoreServiceListGump(PlayerMobile user, Gump parent = null)
			: base(user, parent, emptyText: "There are no services to display.", title: "VitaNexCore Services")
		{
			VersionHue = 68;
			VersionOODHue = 43;

			CanMove = false;
			CanResize = false;

			Sorted = true;
			ForceRecompile = true;
		}

		public override int SortCompare(CoreServiceInfo a, CoreServiceInfo b)
		{
			if (a == null && b == null)
			{
				return 0;
			}

			if (b == null)
			{
				return -1;
			}

			if (a == null)
			{
				return 1;
			}

			return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
		}

		public override string GetSearchKeyFor(CoreServiceInfo key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override string GetLabelText(int index, int pageIndex, CoreServiceInfo entry)
		{
			return entry != null ? entry.Name : base.GetLabelText(index, pageIndex, null);
		}

		protected override int GetLabelHue(int index, int pageIndex, CoreServiceInfo entry)
		{
			return entry != null ? entry.Debug ? HighlightHue : TextHue : ErrorHue;
		}

		protected override void CompileList(List<CoreServiceInfo> list)
		{
			list.Clear();
			list.AddRange(VitaNexCore.CoreServices);

			base.CompileList(list);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.AppendEntry(new ListGumpEntry("Save All", VitaNexCore.SaveServices, HighlightHue));

			base.CompileMenuOptions(list);
		}

		protected override void SelectEntry(GumpButton button, CoreServiceInfo entry)
		{
			base.SelectEntry(button, entry);

			if (button == null || entry == null)
			{
				return;
			}

			var list = new MenuGumpOptions();

			list.AppendEntry(
				new ListGumpEntry(
					"Properties",
					b =>
					{
						Refresh(true);
						User.SendGump(new PropertiesGump(User, entry));
					},
					HighlightHue));

			if (entry.Debug)
			{
				list.Replace(
					"Enable Debug",
					new ListGumpEntry(
						"Debug Disable",
						b =>
						Send(
							new ConfirmDialogGump(
								User,
								this,
								title: "Disable Service Debugging?",
								html: "Disable Service Debugging: " + entry.Name + "\nDo you want to continue?",
								onAccept: a =>
								{
									entry.Debug = false;
									Refresh(true);
								},
								onCancel: Refresh)),
						HighlightHue));
			}
			else
			{
				list.Replace(
					"Disable Debug",
					new ListGumpEntry(
						"Enable Debug",
						b =>
						Send(
							new ConfirmDialogGump(
								User,
								this,
								title: "Enable Service Debugging?",
								html: "Enable Service Debugging: '" + entry.Name + "'\nDo you want to continue?",
								onAccept: a =>
								{
									entry.Debug = true;
									Refresh(true);
								},
								onCancel: Refresh)),
						HighlightHue));
			}

			if (entry.SaveSupported)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Save",
						b =>
						{
							VitaNexCore.SaveService(entry);
							Refresh(true);
						},
						HighlightHue));
			}
			else
			{
				list.RemoveEntry("Save");
			}

			if (entry.LoadSupported)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Load",
						b =>
						Send(
							new ConfirmDialogGump(
								User,
								this,
								title: "Load Service Data?",
								html:
									"Loading a service' saved data after it has been started may yield unexpected results.\nDo you want to continue?",
								onAccept: a =>
								{
									VitaNexCore.LoadService(entry);
									Refresh(true);
								},
								onCancel: Refresh)),
						HighlightHue));
			}
			else
			{
				list.RemoveEntry("Load");
			}

			Send(new MenuGump(User, Refresh(), list, button));
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, CoreServiceInfo entry)
		{
			base.CompileEntryLayout(layout, length, index, pIndex, yOffset, entry);

			layout.AddReplace(
				"label/list/entry/" + index,
				() =>
				{
					Version versionA = entry.Version, versionB = entry.Version;

					AddLabelCropped(65, 2 + yOffset, 150, 20, GetLabelHue(index, pIndex, entry), GetLabelText(index, pIndex, entry));
					AddLabelCropped(225, 2 + yOffset, 50, 20, (versionA < versionB) ? VersionOODHue : VersionHue, versionA.ToString(4));
					AddLabelCropped(285, 2 + yOffset, 50, 20, VersionHue, versionB.ToString(4));
				});
		}
	}
}