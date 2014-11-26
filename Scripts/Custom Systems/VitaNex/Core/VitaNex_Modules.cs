#region Header
//   Vorspire    _,-'/-'/  VitaNex_Modules.cs
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
	///     Exposes an interface for managing VitaNexCore and its' sub-systems.
	/// </summary>
	public static partial class VitaNexCore
	{
		private static List<CoreModuleInfo> _CoreModules;

		public static CoreModuleInfo[] CoreModules { get { return _CoreModules.ToArray(); } }

		public static Dictionary<Type, CoreModuleAttribute> CoreModuleTypeCache { get; private set; }
		public static Assembly[] ModuleAssemblies { get; private set; }

		public static event Action<CoreModuleInfo> OnModuleEnabled;
		public static event Action<CoreModuleInfo> OnModuleDisabled;
		public static event Action<CoreModuleInfo> OnModuleConfigured;
		public static event Action<CoreModuleInfo> OnModuleInvoked;
		public static event Action<CoreModuleInfo> OnModuleSaved;
		public static event Action<CoreModuleInfo> OnModuleLoaded;
		public static event Action<CoreModuleInfo> OnModuleDisposed;

		public static CoreModuleInfo GetModule(Type t)
		{
			return CoreModules.FirstOrDefault(cmi => cmi.TypeOf.IsEqualOrChildOf(t));
		}

		public static CoreModuleInfo[] GetModules(string name, bool ignoreCase = true)
		{
			return FindModules(name, ignoreCase).ToArray();
		}

		public static IEnumerable<CoreModuleInfo> FindModules(string name, bool ignoreCase = true)
		{
			return
				CoreModules.Where(
					cmi =>
					String.Equals(
						cmi.Name, name, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture));
		}

		private static void CompileModules()
		{
			if (Compiled)
			{
				return;
			}

			ToConsole("Compiling Modules...");

			TryCatch(
				() =>
				{
					var files = ModulesDirectory.GetFiles("*.vnc.mod.dll", SearchOption.AllDirectories);
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

					ModuleAssemblies = asm.ToArray();
					asm.AddRange(ScriptCompiler.Assemblies);
					ScriptCompiler.Assemblies = asm.ToArray();
				},
				ToConsole);
		}

		public static void ConfigureModules()
		{
			ToConsole("Configuring Modules...");

			var types = GetCoreModuleTypes();

			_CoreModules = new List<CoreModuleInfo>(types.Count);

			foreach (
				CoreModuleInfo cmi in types.Select(kvp => new CoreModuleInfo(kvp.Key, kvp.Value)).OrderBy(cmi => cmi.Priority))
			{
				_CoreModules.Add(cmi);
				TryCatch(cmi.OnRegistered, cmi.ToConsole);
			}

			foreach (var cmi in _CoreModules.OrderBy(cmi => cmi.Priority))
			{
				ConfigureModule(cmi);
			}
		}

		public static void ConfigureModule(CoreModuleInfo cmi)
		{
			if (cmi == null || !cmi.Enabled)
			{
				return;
			}

			cmi.ToConsole("Configuring...");

			if (!cmi.Configured)
			{
				if (cmi.ConfigSupported)
				{
					TryCatch(cmi.GetConfigHandler(), cmi.ToConsole);
				}

				TryCatch(cmi.OnConfigured, cmi.ToConsole);

				if (OnModuleConfigured != null)
				{
					TryCatch(() => OnModuleConfigured(cmi), cmi.ToConsole);
				}

				cmi.ToConsole("Done.");
			}
			else
			{
				cmi.ToConsole("Already configured, no action taken.");
			}
		}

		public static void InvokeModules()
		{
			ToConsole("Invoking Modules...");

			foreach (var cmi in _CoreModules.OrderBy(cmi => cmi.Priority))
			{
				InvokeModule(cmi);
			}
		}

		public static void InvokeModule(CoreModuleInfo cmi)
		{
			if (cmi == null || !cmi.Enabled)
			{
				return;
			}

			cmi.ToConsole("Invoking...");

			if (!cmi.Invoked)
			{
				if (cmi.InvokeSupported)
				{
					TryCatch(cmi.GetInvokeHandler(), cmi.ToConsole);
				}

				TryCatch(cmi.OnInvoked, cmi.ToConsole);

				if (OnModuleInvoked != null)
				{
					TryCatch(() => OnModuleInvoked(cmi), cmi.ToConsole);
				}

				cmi.ToConsole("Done.");
			}
			else
			{
				cmi.ToConsole("Already invoked, no action taken.");
			}
		}

		public static void SaveModules()
		{
			ToConsole("Saving Modules...");

			foreach (var cmi in _CoreModules.OrderBy(cmi => cmi.Priority))
			{
				SaveModule(cmi);
			}
		}

		public static void SaveModule(CoreModuleInfo cmi)
		{
			if (cmi == null || !cmi.Enabled)
			{
				return;
			}

			cmi.ToConsole("Saving...");

			TryCatch(cmi.SaveOptions, cmi.ToConsole);

			if (cmi.SaveSupported)
			{
				TryCatch(cmi.GetSaveHandler(), cmi.ToConsole);
			}

			TryCatch(cmi.OnSaved, cmi.ToConsole);

			if (OnModuleSaved != null)
			{
				TryCatch(() => OnModuleSaved(cmi), cmi.ToConsole);
			}

			cmi.ToConsole("Done.");
		}

		public static void LoadModules()
		{
			ToConsole("Loading Modules...");

			foreach (var cmi in _CoreModules.OrderBy(cmi => cmi.Priority))
			{
				LoadModule(cmi);
			}
		}

		public static void LoadModule(CoreModuleInfo cmi)
		{
			if (cmi == null || !cmi.Enabled)
			{
				return;
			}

			cmi.ToConsole("Loading...");

			TryCatch(cmi.LoadOptions, cmi.ToConsole);

			if (cmi.LoadSupported)
			{
				TryCatch(cmi.GetLoadHandler(), cmi.ToConsole);
			}

			TryCatch(cmi.OnLoaded, cmi.ToConsole);

			if (OnModuleLoaded != null)
			{
				TryCatch(() => OnModuleLoaded(cmi), cmi.ToConsole);
			}

			cmi.ToConsole("Done.");
		}

		public static void DisposeModules()
		{
			ToConsole("Disposing Modules...");

			foreach (var cmi in _CoreModules.OrderByDescending(cmi => cmi.Priority))
			{
				DisposeModule(cmi);
			}
		}

		public static void DisposeModule(CoreModuleInfo cmi)
		{
			if (cmi == null)
			{
				return;
			}

			cmi.ToConsole("Disposing...");

			if (!cmi.Disposed)
			{
				if (cmi.DisposeSupported)
				{
					TryCatch(cmi.GetDisposeHandler(), cmi.ToConsole);
				}

				TryCatch(cmi.OnDisposed, cmi.ToConsole);

				if (OnModuleDisposed != null)
				{
					TryCatch(() => OnModuleDisposed(cmi), cmi.ToConsole);
				}

				cmi.ToConsole("Done.");
			}
			else
			{
				cmi.ToConsole("Already disposed, no action taken.");
			}
		}

		public static void InvokeModuleEnabled(CoreModuleInfo cmi)
		{
			if (cmi == null)
			{
				return;
			}

			if (OnModuleEnabled != null)
			{
				TryCatch(() => OnModuleEnabled(cmi), ToConsole);
			}
		}

		public static void InvokeModuleDisabled(CoreModuleInfo cmi)
		{
			if (cmi == null)
			{
				return;
			}

			if (OnModuleDisabled != null)
			{
				OnModuleDisabled(cmi);
			}
		}

		/// <summary>
		///     Gets a collection of [cached] Types representing all CoreModules this assembly
		/// </summary>
		/// <returns></returns>
		public static Dictionary<Type, CoreModuleAttribute> GetCoreModuleTypes()
		{
			if (CoreModuleTypeCache != null && CoreModuleTypeCache.Count > 0)
			{
				return CoreModuleTypeCache;
			}

			CoreModuleTypeCache = new Dictionary<Type, CoreModuleAttribute>();

			foreach (var kvp in
				ScriptCompiler.Assemblies.SelectMany(
					asm => GetCoreModuleTypes(asm).Where(kvp => !CoreModuleTypeCache.ContainsKey(kvp.Key))))
			{
				CoreModuleTypeCache.Add(kvp.Key, kvp.Value);
			}

			return CoreModuleTypeCache;
		}

		private static IEnumerable<KeyValuePair<Type, CoreModuleAttribute>> GetCoreModuleTypes(Assembly asm)
		{
			CoreModuleAttribute[] attrs;

			foreach (Type typeOf in asm.GetTypes().Where(typeOf => typeOf != null && typeOf.IsClass))
			{
				attrs = typeOf.GetCustomAttributes(typeof(CoreModuleAttribute), false) as CoreModuleAttribute[];

				if (attrs != null && attrs.Length != 0)
				{
					yield return new KeyValuePair<Type, CoreModuleAttribute>(typeOf, attrs[0]);
				}
			}
		}
	}

	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
	public sealed class CoreModuleAttribute : Attribute
	{
		public string Name { get; set; }
		public string Version { get; set; }
		public bool Enabled { get; set; }
		public int Priority { get; set; }
		public bool Debug { get; set; }
		public bool QuietMode { get; set; }

		public CoreModuleAttribute(
			string name,
			string version,
			bool enabled = false,
			int priority = TaskPriority.Medium,
			bool debug = false,
			bool quietMode = true)
		{
			Name = name;
			Version = version;
			Enabled = enabled;
			Priority = priority;
			Debug = debug;
			QuietMode = quietMode;
		}
	}

	public sealed class CoreModuleInfo : IComparable<CoreModuleInfo>
	{
		private const BindingFlags SearchFlags =
			BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance;

		private readonly PropertyInfo _OptionsProperty;
		private readonly MethodInfo _ConfigMethod;
		private readonly MethodInfo _DisabledMethod;
		private readonly MethodInfo _DisposeMethod;
		private readonly MethodInfo _EnabledMethod;
		private readonly MethodInfo _InvokeMethod;
		private readonly MethodInfo _LoadMethod;
		private readonly MethodInfo _SaveMethod;

		private readonly Type _TypeOf;

		private int _Priority;
		private bool _Enabled;
		private bool _Debug;
		private bool _QuietMode;
		private string _Name;

		private Action _EnabledHandler;
		private Action _DisabledHandler;
		private Action _ConfigHandler;
		private Action _InvokeHandler;
		private Action _LoadHandler;
		private Action _SaveHandler;
		private Action _DisposeHandler;

		private CoreModuleOptions _Options;
		private VersionInfo _Version = new VersionInfo();

		public bool OptionsSupported { get { return _OptionsProperty != null; } }
		public bool EnabledSupported { get { return _EnabledMethod != null; } }
		public bool DisabledSupported { get { return _DisabledMethod != null; } }
		public bool ConfigSupported { get { return _ConfigMethod != null; } }
		public bool InvokeSupported { get { return _InvokeMethod != null; } }
		public bool LoadSupported { get { return _LoadMethod != null; } }
		public bool SaveSupported { get { return _SaveMethod != null; } }
		public bool DisposeSupported { get { return _DisposeMethod != null; } }

		public bool Configured { get; private set; }
		public bool Invoked { get; private set; }
		public bool Disposed { get; private set; }
		public bool Deferred { get; private set; }

		public Assembly DynamicAssembly { get; private set; }
		public FileInfo DynamicAssemblyFile { get; private set; }

		public bool Dynamic { get { return (DynamicAssembly != null && DynamicAssemblyFile != null); } }

		[CommandProperty(VitaNexCore.Access)]
		public int Priority
		{
			get { return _Priority; }
			set
			{
				_Priority = value;
				SaveState();
			}
		}

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
		public bool Enabled
		{
			get { return _Enabled; }
			set
			{
				if (!_Enabled && value)
				{
					_Enabled = true;

					if (!Configured)
					{
						VitaNexCore.ConfigureModule(this);
					}

					if (Deferred)
					{
						VitaNexCore.LoadModule(this);
					}

					if (!Invoked)
					{
						VitaNexCore.InvokeModule(this);
					}

					if (EnabledSupported && !Deferred)
					{
						VitaNexCore.TryCatch(GetEnabledHandler(), Options.ToConsole);
					}

					VitaNexCore.TryCatch(OnEnabled, Options.ToConsole);
					VitaNexCore.InvokeModuleEnabled(this);

					Deferred = false;
					SaveState();
				}
				else if (_Enabled && !value)
				{
					if (!Deferred)
					{
						VitaNexCore.SaveModule(this);

						if (DisabledSupported)
						{
							VitaNexCore.TryCatch(GetDisabledHandler(), Options.ToConsole);
						}

						VitaNexCore.TryCatch(OnDisabled, Options.ToConsole);
						VitaNexCore.InvokeModuleDisabled(this);
					}

					_Enabled = false;
					SaveState();
				}
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
		public CoreModuleOptions Options
		{
			get
			{
				return OptionsSupported && _OptionsProperty.CanRead
						   ? (CoreModuleOptions)_OptionsProperty.GetValue(_TypeOf, null)
						   : (_Options ?? (_Options = new CoreModuleOptions(_TypeOf)));
			}
			set
			{
				if (OptionsSupported && _OptionsProperty.CanWrite)
				{
					_OptionsProperty.SetValue(_TypeOf, value, null);
				}
				else
				{
					_Options = (value ?? new CoreModuleOptions(_TypeOf));
				}
			}
		}

		public CoreModuleInfo(Type t, CoreModuleAttribute attr)
			: this(t, attr.Version, attr.Name, attr.Enabled, attr.Priority, attr.Debug, attr.QuietMode)
		{ }

		public CoreModuleInfo(
			Type typeOf,
			string version,
			string name,
			bool enabled,
			int priority,
			bool debug,
			bool quietMode,
			Assembly dynAsm = null)
		{
			_TypeOf = typeOf;
			_Name = name;
			_Version = version;
			_Enabled = enabled;
			_Priority = priority;
			_Debug = debug;
			_QuietMode = quietMode;
			Deferred = !_Enabled;

			if (dynAsm == null)
			{
				dynAsm =
					VitaNexCore.ModuleAssemblies.FirstOrDefault(
						a =>
						_TypeOf.Assembly != Core.Assembly && !ScriptCompiler.Assemblies.Contains(_TypeOf.Assembly) &&
						a == _TypeOf.Assembly);
			}

			if (dynAsm != null && dynAsm.Location.EndsWith(".vnc.mod.dll"))
			{
				DynamicAssembly = dynAsm;
				DynamicAssemblyFile = new FileInfo(DynamicAssembly.Location);
				_Version = DynamicAssembly.GetName().Version.ToString();
			}

			_OptionsProperty = _TypeOf.GetProperty("CMOptions", SearchFlags);
			_ConfigMethod = _TypeOf.GetMethod("CMConfig", SearchFlags);
			_InvokeMethod = _TypeOf.GetMethod("CMInvoke", SearchFlags);
			_DisposeMethod = _TypeOf.GetMethod("CMDispose", SearchFlags);
			_LoadMethod = _TypeOf.GetMethod("CMLoad", SearchFlags);
			_SaveMethod = _TypeOf.GetMethod("CMSave", SearchFlags);
			_EnabledMethod = _TypeOf.GetMethod("CMEnabled", SearchFlags);
			_DisabledMethod = _TypeOf.GetMethod("CMDisabled", SearchFlags);
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

		public void OnEnabled()
		{ }

		public void OnDisabled()
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
								writer.Write(_Enabled);
								writer.Write(_Name);
								writer.Write(_Priority);
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
								_Enabled = reader.ReadBool();
								_Name = reader.ReadString();
								_Priority = reader.ReadInt();
								_Debug = reader.ReadBool();
								_QuietMode = reader.ReadBool();

								Deferred = !_Enabled;
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
							writer.WriteType(
								Options,
								t =>
								{
									if (t != null)
									{
										Options.Serialize(writer);
									}
								});
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
					"The 'CMConfig' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
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
					"The 'CMInvoke' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
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
					"The 'CMDispose' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
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
					"The 'CMLoad' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
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
					"The 'CMSave' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public Action GetEnabledHandler(bool throwException = false)
		{
			if (_EnabledHandler != null)
			{
				return _EnabledHandler;
			}

			if (EnabledSupported)
			{
				return (_EnabledHandler = () => _EnabledMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CMEnabled' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public Action GetDisabledHandler(bool throwException = false)
		{
			if (_DisabledHandler != null)
			{
				return _DisabledHandler;
			}

			if (EnabledSupported)
			{
				return (_DisabledHandler = () => _DisabledMethod.Invoke(_TypeOf, null));
			}

			if (throwException)
			{
				throw new NotSupportedException(
					"The 'CMEnabled' method of '" + _TypeOf.FullName + "' does not exist or can not be accessed");
			}

			return null;
		}

		public int CompareTo(CoreModuleInfo cmi)
		{
			return cmi == null ? -1 : _Priority.CompareTo(cmi.Priority);
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
				Utility.PushColor(ConsoleColor.Green);
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
				Utility.PushColor(ConsoleColor.Green);
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

	public class CoreModuleOptions : PropertyObject
	{
		private Type _ModuleType;

		public CoreModuleInfo ModuleInstance { get { return VitaNexCore.GetModule(_ModuleType); } }

		[CommandProperty(VitaNexCore.Access)]
		public Type ModuleType { get { return _ModuleType; } }

		[CommandProperty(VitaNexCore.Access)]
		public string ModuleVersion { get { return ModuleInstance.Version; } }

		[CommandProperty(VitaNexCore.Access)]
		public bool ModuleEnabled { get { return ModuleInstance.Enabled; } set { ModuleInstance.Enabled = value; } }

		[CommandProperty(VitaNexCore.Access)]
		public string ModuleName { get { return ModuleInstance.Name; } set { ModuleInstance.Name = value; } }

		[CommandProperty(VitaNexCore.Access)]
		public int ModulePriority { get { return ModuleInstance.Priority; } set { ModuleInstance.Priority = value; } }

		[CommandProperty(VitaNexCore.Access)]
		public bool ModuleDebug { get { return ModuleInstance.Debug; } set { ModuleInstance.Debug = value; } }

		[CommandProperty(VitaNexCore.Access)]
		public bool ModuleQuietMode { get { return ModuleInstance.QuietMode; } set { ModuleInstance.QuietMode = value; } }

		public CoreModuleOptions(Type moduleType)
		{
			_ModuleType = moduleType;
		}

		public CoreModuleOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			ModuleDebug = false;
			ModuleQuietMode = false;
		}

		public override void Reset()
		{
			ModuleDebug = false;
			ModuleQuietMode = false;
		}

		public virtual void ToConsole(string[] lines)
		{
			ModuleInstance.ToConsole(lines);
		}

		public virtual void ToConsole(string format, params object[] args)
		{
			ModuleInstance.ToConsole(format, args);
		}

		public virtual void ToConsole(Exception[] errors)
		{
			ModuleInstance.ToConsole(errors);
		}

		public virtual void ToConsole(Exception e)
		{
			ModuleInstance.ToConsole(e);
		}

		public override string ToString()
		{
			return "Module Options";
		}

		public override void Serialize(GenericWriter writer)
		{
			writer.Write(VitaNexCore.Version);

			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					writer.WriteType(_ModuleType);
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
					_ModuleType = reader.ReadType();
					break;
			}
		}
	}

	public sealed class CoreModuleListGump : ListGump<CoreModuleInfo>
	{
		public int VersionHue { get; set; }
		public int VersionOODHue { get; set; }

		public CoreModuleListGump(PlayerMobile user, Gump parent = null)
			: base(user, parent, emptyText: "There are no modules to display.", title: "VitaNexCore Modules")
		{
			VersionHue = 68;
			VersionOODHue = 43;

			CanMove = false;
			CanResize = false;

			Sorted = true;
			ForceRecompile = true;
		}

		public override int SortCompare(CoreModuleInfo a, CoreModuleInfo b)
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

			if (!a.Enabled && !b.Enabled)
			{
				return 0;
			}

			if (!b.Enabled)
			{
				return -1;
			}

			if (!a.Enabled)
			{
				return 1;
			}

			return String.Compare(a.Name, b.Name, StringComparison.Ordinal);
		}

		public override string GetSearchKeyFor(CoreModuleInfo key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override string GetLabelText(int index, int pageIndex, CoreModuleInfo entry)
		{
			return entry != null ? entry.Name : base.GetLabelText(index, pageIndex, null);
		}

		protected override int GetLabelHue(int index, int pageIndex, CoreModuleInfo entry)
		{
			return entry != null && entry.Enabled ? entry.Debug ? HighlightHue : TextHue : ErrorHue;
		}

		protected override void CompileList(List<CoreModuleInfo> list)
		{
			list.Clear();
			list.AddRange(VitaNexCore.CoreModules);

			base.CompileList(list);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.AppendEntry(new ListGumpEntry("Save All", VitaNexCore.SaveModules, HighlightHue));

			base.CompileMenuOptions(list);
		}

		protected override void SelectEntry(GumpButton button, CoreModuleInfo entry)
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

			if (entry.Enabled)
			{
				list.Replace(
					"Enable",
					new ListGumpEntry(
						"Disable",
						b =>
						Send(
							new ConfirmDialogGump(
								User,
								this,
								title: "Disable Module?",
								html: "Disable Module: " + entry.Name + "\nDo you want to continue?",
								onAccept: a =>
								{
									entry.Enabled = false;
									Refresh(true);
								},
								onCancel: Refresh)),
						HighlightHue));
			}
			else
			{
				list.Replace(
					"Disable",
					new ListGumpEntry(
						"Enable",
						b =>
						Send(
							new ConfirmDialogGump(
								User,
								this,
								title: "Enable Module?",
								html: "Enable Module: '" + entry.Name + "'\nDo you want to continue?",
								onAccept: a =>
								{
									entry.Enabled = true;
									Refresh(true);
								},
								onCancel: Refresh)),
						HighlightHue));
			}

			if (entry.Enabled)
			{
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
									title: "Disable Module Debugging?",
									html: "Disable Module Debugging: " + entry.Name + "\nDo you want to continue?",
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
									title: "Enable Module Debugging?",
									html: "Enable Module Debugging: '" + entry.Name + "'\nDo you want to continue?",
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
								VitaNexCore.SaveModule(entry);
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
									title: "Load Module Data?",
									html:
										"Loading a modules' saved data after it has been started may yield unexpected results.\nDo you want to continue?",
									onAccept: a =>
									{
										VitaNexCore.LoadModule(entry);
										Refresh(true);
									},
									onCancel: Refresh)),
							HighlightHue));
				}
				else
				{
					list.RemoveEntry("Load");
				}
			}
			else
			{
				list.RemoveEntry("Save");
				list.RemoveEntry("Load");
				list.RemoveEntry("Enable Debug");
				list.RemoveEntry("Disable Debug");
			}

			Send(new MenuGump(User, Refresh(), list, button));
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, CoreModuleInfo entry)
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
					AddLabelCropped(
						345, 2 + yOffset, 60, 20, entry.Enabled ? HighlightHue : ErrorHue, entry.Enabled ? "Enabled" : "Disabled");
				});
		}
	}
}