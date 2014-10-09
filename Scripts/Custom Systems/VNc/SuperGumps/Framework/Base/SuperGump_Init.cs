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
using Server.Gumps;
using Server.Mobiles;
using Server.Network;

using VitaNex.Network;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		public static Dictionary<int, SuperGump> GlobalInstances { get; private set; }
		public static Dictionary<PlayerMobile, List<SuperGump>> Instances { get; private set; }

		static SuperGump()
		{
			GlobalInstances = new Dictionary<int, SuperGump>();
			Instances = new Dictionary<PlayerMobile, List<SuperGump>>();			
		}

		public static void Configure()
		{
			EventSink.Logout += OnLogoutImpl;
			EventSink.Disconnected += OnDisconnectedImpl;
			EventSink.Speech += OnSpeechImpl;
			EventSink.Movement += OnMovementImpl;

			NetState.GumpCap = 1024;
			
			VitaNexCore.OnConfigured += () =>
			{
				OutgoingPacketOverrides.Register(0xB0, true, OnEncode0xB0_0xDD);
				OutgoingPacketOverrides.Register(0xDD, true, OnEncode0xB0_0xDD);
			};
		}

		public static void OnLogoutImpl(LogoutEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null || !Instances.ContainsKey(user))
			{
				return;
			}

			foreach (var g in EnumerateInstances<SuperGump>(user, true))
			{
				g.Close(true);
			}
		}

		public static void OnDisconnectedImpl(DisconnectedEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null || !Instances.ContainsKey(user))
			{
				return;
			}

			foreach (var g in EnumerateInstances<SuperGump>(user, true))
			{
				g.Close(true);
			}
		}

		public static void OnSpeechImpl(SpeechEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null || !Instances.ContainsKey(user))
			{
				return;
			}

			foreach (var g in GetInstances<SuperGump>(user, true))
			{
				g.OnSpeech(e);
			}
		}

		public static void OnMovementImpl(MovementEventArgs e)
		{
			var user = e.Mobile as PlayerMobile;

			if (user == null || !Instances.ContainsKey(user))
			{
				return;
			}

			foreach (var g in GetInstances<SuperGump>(user, true))
			{
				g.OnMovement(e);
			}
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
			return EnumerateInstances(user, typeof(TGump), inherited).OfType<TGump>();
		}

		public static IEnumerable<SuperGump> EnumerateInstances(PlayerMobile user, Type type, bool inherited = false)
		{
			if (user == null || !Instances.ContainsKey(user) || Instances[user] == null)
			{
				yield break;
			}

			foreach (var gump in
				Instances[user].AsParallel()
							   .Where(g => inherited ? g.GetType().IsEqualOrChildOf(type) : g.GetType().IsEqual(type)))
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

			if (serial < 0 || !GlobalInstances.ContainsKey(serial))
			{
				return;
			}

			SuperGump gump = GlobalInstances[serial];

			if (!gump.Compiled)
			{
				gump.Refresh(true);
			}
		}

		protected virtual void RegisterInstance()
		{
			if (User == null || User.Deleted)
			{
				return;
			}

			lock (GlobalInstances)
			{
				SuperGump gump;

				if (!GlobalInstances.TryGetValue(Serial, out gump))
				{
					GlobalInstances.Add(Serial, this);
				}
				else if (gump != this)
				{
					GlobalInstances[Serial] = this;
				}
			}

			lock (Instances)
			{
				List<SuperGump> list;

				if (!Instances.TryGetValue(User, out list))
				{
					Instances.Add(
						User,
						new List<SuperGump>
						{
							this
						});
				}
				else if (list == null)
				{
					Instances[User] = new List<SuperGump>
					{
						this
					};
				}
				else if (!list.Contains(this))
				{
					list.Add(this);
				}
			}

			OnInstanceRegistered();
		}

		protected virtual void UnregisterInstance()
		{
			lock (GlobalInstances)
			{
				GlobalInstances.Remove(Serial);
			}
			
			bool removed = false;

			lock (Instances)
			{
				var user = User ?? Instances.FirstOrDefault(kv => kv.Value.Contains(this)).Key;

				if (user == null)
				{
					return;
				}

				List<SuperGump> list;

				if (!Instances.TryGetValue(user, out list))
				{
					return;
				}

				if (list == null)
				{
					Instances.Remove(user);
					return;
				}

				if (list.Remove(this))
				{
					removed = true;
				}

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