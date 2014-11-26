#region Header
//   Vorspire    _,-'/-'/  SuperGump_Init.cs
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
using Server.Mobiles;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		private static readonly object _GlobalLock = new object();
		private static readonly object _InstanceLock = new object();

		public static Dictionary<int, SuperGump> GlobalInstances { get; private set; }
		public static Dictionary<PlayerMobile, List<SuperGump>> Instances { get; private set; }

		static SuperGump()
		{
			GlobalInstances = new Dictionary<int, SuperGump>(5000);
			Instances = new Dictionary<PlayerMobile, List<SuperGump>>(50);
		}

		[CallPriority(Int32.MaxValue)]
		public static void Configure()
		{
			EventSink.Logout += OnLogoutImpl;
			EventSink.Disconnected += OnDisconnectedImpl;
			EventSink.Speech += OnSpeechImpl;
			EventSink.Movement += OnMovementImpl;

			NetState.GumpCap = 1024;

			VitaNexCore.OnInitialized += () =>
			{
				OutgoingPacketOverrides.Register(0xB0, true, OnEncode0xB0_0xDD);
				OutgoingPacketOverrides.Register(0xDD, true, OnEncode0xB0_0xDD);
			};
		}

		public static void OnLogoutImpl(LogoutEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null)
			{
				return;
			}

			lock (_InstanceLock)
			{
				if (!Instances.ContainsKey(user))
				{
					return;
				}
			}

			VitaNexCore.TryCatch(
				() =>
				{
					foreach (SuperGump g in GetInstances<SuperGump>(user, true))
					{
						g.Close(true);
					}
				},
				x => x.ToConsole(true));
		}

		public static void OnDisconnectedImpl(DisconnectedEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null)
			{
				return;
			}

			lock (_InstanceLock)
			{
				if (!Instances.ContainsKey(user))
				{
					return;
				}
			}

			VitaNexCore.TryCatch(
				() =>
				{
					foreach (SuperGump g in GetInstances<SuperGump>(user, true))
					{
						g.Close(true);
					}
				},
				x => x.ToConsole(true));
		}

		public static void OnSpeechImpl(SpeechEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null)
			{
				return;
			}

			lock (_InstanceLock)
			{
				if (!Instances.ContainsKey(user))
				{
					return;
				}
			}

			VitaNexCore.TryCatch(
				() =>
				{
					foreach (SuperGump g in GetInstances<SuperGump>(user, true))
					{
						g.OnSpeech(e);
					}
				},
				x => x.ToConsole(true));
		}

		public static void OnMovementImpl(MovementEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null)
			{
				return;
			}

			lock (_InstanceLock)
			{
				if (!Instances.ContainsKey(user))
				{
					return;
				}
			}

			VitaNexCore.TryCatch(
				() =>
				{
					foreach (SuperGump g in GetInstances<SuperGump>(user, true))
					{
						g.OnMovement(e);
					}
				},
				x => x.ToConsole(true));
		}

		public static TGump[] GetInstances<TGump>(PlayerMobile user, bool inherited = false) where TGump : SuperGump
		{
			return EnumerateInstances<TGump>(user, inherited).ToArray();
		}

		public static SuperGump[] GetInstances(PlayerMobile user, Type type, bool inherited = false)
		{
			return EnumerateInstances(user, type, inherited).ToArray();
		}

		public static IEnumerable<TGump> EnumerateInstances<TGump>(PlayerMobile user, bool inherited = false)
			where TGump : SuperGump
		{
			if (user == null)
			{
				yield break;
			}

			List<SuperGump> list;

			lock (_InstanceLock)
			{
				list = Instances.GetValue(user);

				if (list == null || list.Count == 0)
				{
					yield break;
				}
			}

			IEnumerable<SuperGump> e;

			lock (_InstanceLock)
			{
				e = list.AsParallel().Where(g => g != null && g.TypeEquals<TGump>(inherited)).OfType<TGump>();
			}

			foreach (TGump gump in e)
			{
				yield return gump;
			}
		}

		public static IEnumerable<SuperGump> EnumerateInstances(PlayerMobile user, Type type, bool inherited = false)
		{
			if (user == null)
			{
				yield break;
			}

			List<SuperGump> list;

			lock (_InstanceLock)
			{
				list = Instances.GetValue(user);

				if (list == null || list.Count == 0)
				{
					yield break;
				}
			}

			IEnumerable<SuperGump> e;

			lock (_InstanceLock)
			{
				e = list.AsParallel().Where(g => g != null && g.TypeEquals(type, inherited));
			}

			foreach (SuperGump gump in e)
			{
				yield return gump;
			}
		}

		private static void OnEncode0xB0_0xDD(NetState state, PacketReader reader, ref byte[] buffer, ref int length)
		{
			if (state == null || reader == null || buffer == null || length < 0)
			{
				return;
			}

			int pos = reader.Seek(0, SeekOrigin.Current);
			reader.Seek(3, SeekOrigin.Begin);
			int serial = reader.ReadInt32();
			reader.Seek(pos, SeekOrigin.Begin);

			if (serial <= 0)
			{
				return;
			}

			SuperGump g;

			lock (_GlobalLock)
			{
				g = GlobalInstances.GetValue(serial);
			}

			if (g != null && !g.Compiled)
			{
				g.Refresh(true);
			}
		}

		protected virtual void RegisterInstance()
		{
			if (User == null || User.Deleted || IsDisposed)
			{
				return;
			}

			lock (_GlobalLock)
			{
				GlobalInstances.AddOrReplace(Serial, this);
			}

			lock (_InstanceLock)
			{
				Instances.AddOrReplace(
					User,
					l =>
					{
						l = l ?? new List<SuperGump>(10);
						l.AddOrReplace(this);
						return l;
					});
			}

			OnInstanceRegistered();
		}

		protected virtual void UnregisterInstance()
		{
			lock (_GlobalLock)
			{
				GlobalInstances.Remove(Serial);
			}

			PlayerMobile user = User;

			if (user == null)
			{
				lock (_InstanceLock)
				{
					user = Instances.FirstOrDefault(kv => kv.Value.Contains(this)).Key;
				}
			}

			if (user == null)
			{
				return;
			}

			List<SuperGump> list;

			lock (_InstanceLock)
			{
				list = Instances.GetValue(User);

				if (list == null || list.Count == 0)
				{
					lock (_InstanceLock)
					{
						Instances.Remove(user);
					}

					return;
				}
			}

			bool removed = false;

			lock (_InstanceLock)
			{
				if (list.Remove(this))
				{
					removed = true;
				}

				list.TrimExcess();

				if (list.Count == 0)
				{
					Instances.Remove(user);
				}
			}

			if (removed)
			{
				OnInstanceUnregistered();
			}
		}

		protected virtual void OnInstanceRegistered()
		{
			//Console.WriteLine("{0} Registered to {1}", this, User);
		}

		protected virtual void OnInstanceUnregistered()
		{
			//Console.WriteLine("{0} Unregistered from {1}", this, User);
		}
	}
}