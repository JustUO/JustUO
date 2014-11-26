#region Header
//   Vorspire    _,-'/-'/  Channel.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2013  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;

using Server;
using Server.Misc;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public struct WorldChatMessage
	{
		public PlayerMobile User { get; private set; }
		public string Text { get; private set; }
		public MapPoint Place { get; private set; }
		public DateTime Time { get; private set; }

		public WorldChatMessage(PlayerMobile user, string text, MapPoint place, DateTime time)
			: this()
		{
			User = user;
			Text = text;
			Place = place;
			Time = time;
		}

		public override string ToString()
		{
			return Text;
		}
	}

	public abstract class WorldChatChannel : PropertyObject
	{
		private static int _NextUID;

		public static event Action<WorldChatChannel, PlayerMobile> OnUserJoin;
		public static event Action<WorldChatChannel, PlayerMobile> OnUserLeave;
		public static event Action<WorldChatChannel, PlayerMobile> OnUserKicked;
		public static event Action<WorldChatChannel, PlayerMobile> OnUserBanned;
		public static event Action<WorldChatChannel, PlayerMobile> OnUserUnbanned;
		public static event Action<WorldChatChannel, PlayerMobile, WorldChatMessage> OnUserMessage;

		private static void InvokeUserJoin(WorldChatChannel channel, PlayerMobile user)
		{
			if (OnUserJoin != null)
			{
				OnUserJoin(channel, user);
			}
		}

		private static void InvokeUserLeave(WorldChatChannel channel, PlayerMobile user)
		{
			if (OnUserLeave != null)
			{
				OnUserLeave(channel, user);
			}
		}

		private static void InvokeUserKicked(WorldChatChannel channel, PlayerMobile user)
		{
			if (OnUserKicked != null)
			{
				OnUserKicked(channel, user);
			}
		}

		private static void InvokeUserBanned(WorldChatChannel channel, PlayerMobile user)
		{
			if (OnUserBanned != null)
			{
				OnUserBanned(channel, user);
			}
		}

		private static void InvokeUserUnbanned(WorldChatChannel channel, PlayerMobile user)
		{
			if (OnUserUnbanned != null)
			{
				OnUserUnbanned(channel, user);
			}
		}

		private static void InvokeUserMessage(WorldChatChannel channel, PlayerMobile user, WorldChatMessage message)
		{
			if (OnUserMessage != null)
			{
				OnUserMessage(channel, user, message);
			}
		}

		public Dictionary<PlayerMobile, DateTime> Users { get; private set; }
		public Dictionary<PlayerMobile, DateTime> Bans { get; private set; }
		public Dictionary<PlayerMobile, WorldChatMessage> History { get; private set; }

		private readonly int _UID;

		[CommandProperty(WorldChat.Access)]
		public int UID { get { return _UID; } }

		[CommandProperty(WorldChat.Access)]
		public bool Permanent { get { return WorldChat.PermaChannels.Contains(this); } }

		[CommandProperty(WorldChat.Access)]
		public string Name { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Summary { get; set; }

		[CommandProperty(WorldChat.Access)]
		public string Token { get; set; }

		[CommandProperty(WorldChat.Access)]
		public bool AutoJoin { get; set; }

		[CommandProperty(WorldChat.Access)]
		public bool Available { get; set; }

		[CommandProperty(WorldChat.Access)]
		public AccessLevel Access { get; set; }

		[CommandProperty(WorldChat.Access)]
		public ProfanityAction ProfanityAction { get; set; }

		[CommandProperty(WorldChat.Access)]
		public TimeSpan SpamDelay { get; set; }

		[CommandProperty(WorldChat.Access)]
		public KnownColor TextColor { get; set; }

		[CommandProperty(WorldChat.Access)]
		public int TextHue { get; set; }

		[CommandProperty(WorldChat.Access)]
		public int UserLimit { get; set; }

		[CommandProperty(WorldChat.Access)]
		public int UserCount { get { return Users.Keys.Count(u => u.AccessLevel <= Access); } }

		[CommandProperty(WorldChat.Access)]
		public int BanCount { get { return Bans.Count; } }

		[CommandProperty(WorldChat.Access)]
		public int HistoryCount { get { return History.Count; } }

		public WorldChatChannel()
		{
			_UID = _NextUID++;

			Name = "Chat";
			Summary = "";

			ProfanityAction = ProfanityAction.None;
			TextColor = KnownColor.LawnGreen;
			TextHue = 85;
			UserLimit = 500;
			SpamDelay = TimeSpan.FromSeconds(5.0);

			History = new Dictionary<PlayerMobile, WorldChatMessage>();
			Users = new Dictionary<PlayerMobile, DateTime>();
			Bans = new Dictionary<PlayerMobile, DateTime>();

			Token = _UID.ToString(CultureInfo.InvariantCulture);
		}

		public WorldChatChannel(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			Name = "Chat";
			Summary = "";
			Token = UID.ToString(CultureInfo.InvariantCulture);

			Available = false;

			ProfanityAction = ProfanityAction.None;
			TextColor = KnownColor.LawnGreen;
			TextHue = 85;
			UserLimit = 0;
			SpamDelay = TimeSpan.Zero;

			History.Clear();
			Bans.Clear();
		}

		public override void Reset()
		{
			Name = "Chat";
			Summary = "";
			Token = UID.ToString(CultureInfo.InvariantCulture);

			Available = true;

			ProfanityAction = ProfanityAction.None;
			TextColor = KnownColor.LawnGreen;
			TextHue = 85;
			UserLimit = 500;
			SpamDelay = TimeSpan.FromSeconds(5.0);

			History.Clear();
			Bans.Clear();
		}

		public virtual Dictionary<PlayerMobile, WorldChatMessage> GetHistoryView(PlayerMobile user)
		{
			var list = new Dictionary<PlayerMobile, WorldChatMessage>();
			History.Where(kv => CanSee(user, kv.Value)).ForEach(kv => list.Add(kv.Key, kv.Value));
			return list;
		}

		public virtual bool CanSee(PlayerMobile user, WorldChatMessage message)
		{
			return user != null && (user == message.User || (IsUser(user) && !IsBanned(user)));
		}

		public virtual bool IsUser(PlayerMobile user)
		{
			return user != null && Users.ContainsKey(user);
		}

		public virtual bool IsBanned(PlayerMobile user)
		{
			return user != null && Bans.ContainsKey(user) && Bans[user] > DateTime.Now;
		}

		protected virtual bool OnProfanityDetected(PlayerMobile user, string text, bool message = true)
		{
			return false;
		}

		public virtual string FormatMessage(PlayerMobile user, string text)
		{
			return String.Format(
				"[{0}] [{1}{2}]: {3}", Name, WorldChat.CMOptions.AccessPrefixes[user.AccessLevel], user.RawName, text);
		}

		public virtual bool CanMessage(PlayerMobile user, string text, bool message = true)
		{
			if (!Available)
			{
				if (message)
				{
					InternalMessage(user, "The channel '{0}' is currently unavailable.", Name);
				}

				return false;
			}

			if (!IsUser(user) && user.AccessLevel < AccessLevel.Counselor)
			{
				if (message)
				{
					InternalMessage(user, "You are not in the channel '{0}'", Name);
				}

				return false;
			}

			if (user.AccessLevel < Access)
			{
				if (message)
				{
					InternalMessage(user, "You do not have sufficient access to speak in the channel '{0}'", Name);
				}

				return false;
			}

			if (IsUser(user) && user.AccessLevel < AccessLevel.Counselor && Users[user] > DateTime.Now)
			{
				if (message)
				{
					InternalMessage(user, "Spam detected, message blocked.");
				}

				return false;
			}

			if (
				!NameVerification.Validate(
					text,
					0,
					Int32.MaxValue,
					true,
					true,
					false,
					Int32.MaxValue,
					ProfanityProtection.Exceptions,
					ProfanityProtection.Disallowed,
					ProfanityProtection.StartDisallowed))
			{
				switch (ProfanityAction)
				{
					case ProfanityAction.None:
						return true;
					case ProfanityAction.Criminal:
						user.Criminal = true;
						return true;
					case ProfanityAction.CriminalAction:
						user.CriminalAction(true);
						return true;
					case ProfanityAction.Disallow:
						return false;
					case ProfanityAction.Disconnect:
						Kick(user, false, message);
						return false;
					case ProfanityAction.Other:
						return OnProfanityDetected(user, text, message);
				}
			}

			return true;
		}

		public virtual void InternalMessage(PlayerMobile user, string text, params object[] args)
		{
			user.SendMessage(TextHue, String.Format(text, args));
		}

		public virtual void MessageTo(PlayerMobile user, PlayerMobile to, string text)
		{
			InternalMessage(to, text);
		}

		public virtual bool Message(PlayerMobile user, string text, bool message = true)
		{
			if (!CanMessage(user, text, message))
			{
				return false;
			}

			if (!IsUser(user))
			{
				Join(user, message);
			}

			var msg = new WorldChatMessage(user, text, user.ToMapPoint(), DateTime.Now);

			string formatted = FormatMessage(user, text);

			Users.Keys.Where(u => CanSee(u, msg)).ForEach(u => MessageTo(user, u, formatted));

			if (WorldChat.CMOptions.HistoryBuffer > 0)
			{
				while (HistoryCount >= WorldChat.CMOptions.HistoryBuffer)
				{
					History.Pop();
				}

				History.Add(user, msg);
			}
			else
			{
				History.Clear();
			}

			Users[user] = DateTime.Now + SpamDelay;

			OnMessage(user, msg, message);
			return true;
		}

		protected virtual void OnMessage(PlayerMobile user, WorldChatMessage text, bool message = true)
		{
			InvokeUserMessage(this, user, text);
		}

		public virtual bool Kick(PlayerMobile user, bool ban = false, bool message = true)
		{
			if (!Users.Remove(user))
			{
				return false;
			}

			if (message)
			{
				InternalMessage(user, "You have been kicked from the channel '{0}'", Name);
			}

			if (ban)
			{
				Ban(user, TimeSpan.Zero, false, message);
			}

			OnKicked(user, message);
			return true;
		}

		protected virtual void OnKicked(PlayerMobile user, bool message = true)
		{
			InvokeUserKicked(this, user);
		}

		public virtual bool Ban(PlayerMobile user, TimeSpan duration, bool kick = true, bool message = true)
		{
			if (kick)
			{
				Kick(user, false, message);
			}

			if (Bans.ContainsKey(user))
			{
				Bans[user] = duration <= TimeSpan.Zero ? DateTime.MaxValue : DateTime.Now + duration;
				return false;
			}

			Bans.Add(user, duration <= TimeSpan.Zero ? DateTime.MaxValue : DateTime.Now + duration);

			if (message)
			{
				InternalMessage(user, "You have been banned from the channel '{0}' until {1}", Name, Bans[user].ToSimpleString());
			}

			OnBanned(user, message);
			return true;
		}

		protected virtual void OnBanned(PlayerMobile user, bool message = true)
		{
			InvokeUserBanned(this, user);
		}

		public virtual bool Unban(PlayerMobile user, bool message = true)
		{
			if (!Bans.Remove(user))
			{
				return false;
			}

			if (message)
			{
				InternalMessage(user, "You are no longer banned from the channel '{0}'", Name);
			}

			OnUnbanned(user, message);
			return true;
		}

		protected virtual void OnUnbanned(PlayerMobile user, bool message = true)
		{
			InvokeUserUnbanned(this, user);
		}

		public virtual bool CanJoin(PlayerMobile user, bool message = true)
		{
			if (!Available)
			{
				if (message)
				{
					InternalMessage(user, "The channel '{0}' is currently unavailable.", Name);
				}

				return false;
			}

			if (IsBanned(user))
			{
				if (message)
				{
					InternalMessage(user, "You are banned from the channel '{0}'", Name);
				}

				return false;
			}

			if (IsUser(user))
			{
				if (message)
				{
					InternalMessage(user, "You are already in the channel '{0}'", Name);
				}

				return false;
			}

			if (user.AccessLevel < AccessLevel.Counselor && UserCount >= UserLimit)
			{
				if (message)
				{
					InternalMessage(user, "The user limit has been reached for the channel '{0}", Name);
				}

				return false;
			}

			return true;
		}

		public virtual bool Join(PlayerMobile user, bool message = true)
		{
			if (!CanJoin(user, message))
			{
				return false;
			}

			Users.Add(user, DateTime.Now);

			if (message)
			{
				InternalMessage(user, "You have joined the channel '{0}'", Name);
			}

			OnJoin(user, message);
			return true;
		}

		protected virtual void OnJoin(PlayerMobile user, bool message = true)
		{
			InvokeUserJoin(this, user);
		}

		public virtual bool CanLeave(PlayerMobile user, bool message = true)
		{
			if (!IsUser(user))
			{
				if (message)
				{
					InternalMessage(user, "You are not in the channel '{0}'", Name);
				}

				return false;
			}

			return true;
		}

		public virtual bool Leave(PlayerMobile user, bool message = true)
		{
			if (!CanLeave(user, message))
			{
				return false;
			}

			Users.Remove(user);

			if (message)
			{
				InternalMessage(user, "You have left the channel '{0}'", Name);
			}

			OnLeave(user, message);
			return true;
		}

		protected virtual void OnLeave(PlayerMobile user, bool message = true)
		{
			InvokeUserLeave(this, user);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			writer.Write(Name);
			writer.Write(Summary);
			writer.Write(Token);
			writer.Write(Available);
			writer.Write(AutoJoin);
			writer.WriteFlag(Access);
			writer.WriteFlag(ProfanityAction);
			writer.WriteFlag(TextColor);
			writer.Write(TextHue);
			writer.Write(UserLimit);
			writer.Write(SpamDelay);

			writer.WriteBlockDictionary(
				Bans,
				(w, k, v) =>
				{
					w.Write(k);
					w.Write(v);
				});
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			Name = reader.ReadString();
			Summary = reader.ReadString();
			Token = reader.ReadString();
			Available = reader.ReadBool();
			AutoJoin = reader.ReadBool();
			Access = reader.ReadFlag<AccessLevel>();
			ProfanityAction = reader.ReadFlag<ProfanityAction>();
			TextColor = reader.ReadFlag<KnownColor>();
			TextHue = reader.ReadInt();
			UserLimit = reader.ReadInt();
			SpamDelay = reader.ReadTimeSpan();

			Bans = reader.ReadBlockDictionary(
				r =>
				{
					var k = r.ReadMobile<PlayerMobile>();
					var v = r.ReadDateTime();
					return new KeyValuePair<PlayerMobile, DateTime>(k, v);
				});

			History = new Dictionary<PlayerMobile, WorldChatMessage>();
			Users = new Dictionary<PlayerMobile, DateTime>();
		}
	}
}