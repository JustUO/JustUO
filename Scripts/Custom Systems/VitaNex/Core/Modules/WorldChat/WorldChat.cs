#region Header
//   Vorspire    _,-'/-'/  WorldChat.cs
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

using VitaNex.IO;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public static partial class WorldChat
	{
		public const AccessLevel Access = AccessLevel.Administrator;

		public static WorldChatOptions CMOptions { get; private set; }

		public static FileInfo ChannelSaves = IOUtility.EnsureFile(VitaNexCore.SavesDirectory + "/WorldChat/Channels.bin");

		private static readonly WorldChatChannel[] _PermaChannels;
		private static readonly List<WorldChatChannel> _Channels;

		public static List<WorldChatChannel> Channels { get { return _Channels; } }

		public static WorldChatChannel[] PermaChannels { get { return _PermaChannels; } }
		public static WorldChatChannel[] AllChannels { get { return PermaChannels.Merge(Channels.ToArray()); } }

		public static Type[] ChannelTypes { get; private set; }

		private static void OnLogin(LoginEventArgs e)
		{
			if (e == null || !(e.Mobile is PlayerMobile))
			{
				return;
			}

			var user = (PlayerMobile)e.Mobile;

			AllChannels.Where(c => user.AccessLevel <= c.Access && c.Available && c.AutoJoin).ForEach(c => c.Join(user));
		}

		private static void OnLogout(LogoutEventArgs e)
		{
			if (e == null || !(e.Mobile is PlayerMobile))
			{
				return;
			}

			var user = (PlayerMobile)e.Mobile;

			AllChannels.Where(c => c.IsUser(user)).ForEach(c => c.Leave(user));
		}

		private static void OnSpeech(SpeechEventArgs e)
		{
			if (e.Handled || e.Blocked || !(e.Mobile is PlayerMobile) || String.IsNullOrWhiteSpace(e.Speech) ||
				e.Speech.Length < 3 || e.Speech[0] != CMOptions.ChatPrefix)
			{
				return;
			}

			var pm = (PlayerMobile)e.Mobile;
			var args = e.Speech.Substring(1).Split(' ');

			if (args.Length < 2)
			{
				return;
			}

			string token = args[0];

			var channel = AllChannels.FirstOrDefault(c => c.Available && c.Token == token && c.Access <= e.Mobile.AccessLevel);

			if (channel == null)
			{
				return;
			}

			e.Handled = true;
			e.Blocked = true;

			if (Insensitive.Equals(args[1], "join"))
			{
				if (channel.Join(pm))
				{ }

				return;
			}

			if (Insensitive.Equals(args[1], "leave"))
			{
				if (channel.Leave(pm))
				{ }

				return;
			}

			if (pm.AccessLevel > AccessLevel.Counselor)
			{
				if (Insensitive.Equals(args[1], "kick"))
				{
					if (args.Length > 2)
					{
						PlayerMobile toKick = channel.Users.Keys.FirstOrDefault(u => Insensitive.Equals(u.RawName, args[2]));

						if (toKick != null && pm.AccessLevel > toKick.AccessLevel && channel.Kick(toKick))
						{ }
					}

					return;
				}

				if (Insensitive.Equals(args[1], "ban"))
				{
					if (args.Length > 2)
					{
						PlayerMobile toBan = channel.Users.Keys.FirstOrDefault(u => Insensitive.Equals(u.RawName, args[2]));

						TimeSpan duration = TimeSpan.Zero;

						if (args.Length > 3)
						{
							double mins;

							if (Double.TryParse(args[3], out mins))
							{
								duration = TimeSpan.FromMinutes(mins);
							}
						}

						if (toBan != null && pm.AccessLevel > toBan.AccessLevel && channel.Ban(toBan, duration))
						{ }
					}

					return;
				}
			}

			var tmp = args.ToList();
			tmp.RemoveAt(0);
			args = tmp.ToArray();
			tmp.Clear();

			if (channel.Message(pm, String.Join(" ", args)))
			{ }
		}
	}
}