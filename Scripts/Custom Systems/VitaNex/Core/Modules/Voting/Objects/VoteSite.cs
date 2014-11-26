#region Header
//   Vorspire    _,-'/-'/  VoteSite.cs
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
using System.Linq;

using Server;
using Server.Mobiles;
#endregion

namespace VitaNex.Modules.Voting
{
	public interface IVoteSite
	{
		int UID { get; }
		bool Valid { get; }
		bool Deleted { get; }

		bool Enabled { get; set; }
		string Name { get; set; }
		string Link { get; set; }
		TimeSpan Interval { get; set; }

		int Tokens { get; set; }
		int BonusTokens { get; set; }
		int BonusTokensChance { get; set; }

		bool CanVote(PlayerMobile voter, bool message = true);
		void Serialize(GenericWriter writer);
		void Deserialize(GenericReader reader);

		bool ValidateLink();
		bool Vote(PlayerMobile voter, bool message = true);

		void Delete();
	}

	public class VoteSite : PropertyObject, IVoteSite, IEquatable<IVoteSite>
	{
		public static VoteSite Empty = new VoteSite(String.Empty, String.Empty, TimeSpan.Zero, 0, 0, 0);

		private int _BonusTokens;
		private int _BonusTokensChance;
		private string _Link;

		public int UID { get; private set; }
		public bool Deleted { get; private set; }

		[CommandProperty(Voting.Access)]
		public bool Enabled { get; set; }

		[CommandProperty(Voting.Access)]
		public string Name { get; set; }

		[CommandProperty(Voting.Access)]
		public string Link
		{
			get { return _Link; }
			set
			{
				_Link = value;
				ValidateLink();
			}
		}

		[CommandProperty(Voting.Access)]
		public TimeSpan Interval { get; set; }

		[CommandProperty(Voting.Access)]
		public int Tokens { get; set; }

		[CommandProperty(Voting.Access)]
		public int BonusTokens { get { return _BonusTokens; } set { _BonusTokens = Math.Max(0, value); } }

		[CommandProperty(Voting.Access)]
		public int BonusTokensChance { get { return _BonusTokensChance; } set { _BonusTokensChance = Math.Max(0, Math.Min(100, value)); } }

		[CommandProperty(Voting.Access)]
		public bool Valid { get { return (!Deleted && !String.IsNullOrWhiteSpace(Name) && ValidateLink()); } }

		public VoteSite()
			: this("Vita-Nex", "http://core.vita-nex.com", TimeSpan.Zero, 0, 0, 0)
		{ }

		public VoteSite(string name, string link, TimeSpan interval, int tokens, int bonusTokens, int bonusTokensChance)
		{
			UID = (int)TimeStamp.UtcNow.Stamp;
			Name = name;
			Link = link;
			Interval = interval;
			Tokens = Math.Max(0, tokens);
			BonusTokens = Math.Max(0, bonusTokens);
			BonusTokensChance = Math.Max(0, bonusTokensChance);
		}

		public VoteSite(GenericReader reader)
		{
			Deserialize(reader);
		}

		public bool Equals(IVoteSite other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return UID == other.UID;
		}

		public virtual bool CanVote(PlayerMobile voter, bool message = true)
		{
			if (voter == null || voter.Deleted)
			{
				return false;
			}

			if (!Valid || !Enabled || Deleted)
			{
				if (message)
				{
					voter.SendMessage("This vote site is currently disabled.");
				}

				return false;
			}

			VoteProfile p = Voting.EnsureProfile(voter);

			if (p == null || p.Deleted)
			{
				if (message)
				{
					voter.SendMessage("Your vote profile can't be accessed right now.");
				}

				return false;
			}

			DateTime now = DateTime.UtcNow;
			var entry = p.GetHistory(now - Interval).LastOrDefault(e => e.VoteSite == this);

			if (entry != null && now <= (entry.VoteTime + Interval))
			{
				if (message)
				{
					voter.SendMessage(
						"You can't vote at this site yet. Try again in {0}", (now - (entry.VoteTime + Interval)).ToSimpleString("h:m:s"));
				}

				return false;
			}

			if (Voting.CMOptions.RestrictByIP)
			{
				var p2 = Voting.Profiles.Values.FirstOrDefault(o => o != p && o.LoginIPs.Any(oip => p.LoginIPs.Contains(oip)));

				if (p2 != null)
				{
					var entry2 = p2.GetHistory(now - Interval).LastOrDefault(e => e.VoteSite == this);

					if (entry2 != null && now <= (entry2.VoteTime + Interval))
					{
						if (message)
						{
							voter.SendMessage(
								"You have already cast a vote from this IP recently. Try again in {0}",
								(now - (entry2.VoteTime + Interval)).ToSimpleString("h:m:s"));
						}

						return false;
					}
				}
			}

			return true;
		}

		public virtual bool Vote(PlayerMobile voter, bool message = true)
		{
			if (voter == null || voter.Deleted || !CanVote(voter, message))
			{
				return false;
			}

			VoteProfile p = Voting.EnsureProfile(voter);

			if (p == null || p.Deleted)
			{
				if (message)
				{
					voter.SendMessage("Your vote profile can't be accessed right now.");
				}

				return false;
			}

			if (message)
			{
				voter.SendMessage("Thanks for voting, {0}! Every vote counts!", voter.RawName);
			}

			int tokens = Tokens;

			if (Voting.CMOptions.GiveBonusTokens && BonusTokens > 0 && Utility.RandomMinMax(0, 100) <= BonusTokensChance)
			{
				tokens += BonusTokens;

				if (message)
				{
					voter.SendMessage("You've just earned {0} bonus tokens!", BonusTokens);
				}
			}

			var e = new VoteRequestEventArgs(voter, this, tokens, message);
			VitaNexCore.TryCatch(e.Invoke, Voting.CMOptions.ToConsole);

			if (tokens != e.Tokens)
			{
				tokens = Math.Max(0, e.Tokens);
			}

			message = e.Message;

			if (!e.HandledTokens)
			{
				p.TransferTokens(this, tokens, message);
			}
			else
			{
				p.RegisterTokens(this, tokens);
			}

			Timer.DelayCall(Voting.CMOptions.BrowserDelay, () => voter.LaunchBrowser(Link));
			return true;
		}

		public bool ValidateLink()
		{
			if (String.IsNullOrWhiteSpace(_Link))
			{
				return false;
			}

			string link = _Link;

			if (!Insensitive.StartsWith(link, Uri.UriSchemeHttp) && !Insensitive.StartsWith(link, Uri.UriSchemeHttps))
			{
				link = Uri.UriSchemeHttp + link;
			}

			Uri test;

			if (Uri.TryCreate(link, UriKind.Absolute, out test))
			{
				_Link = test.ToString();
				return true;
			}

			return false;
		}

		public void Delete()
		{
			if (Deleted)
			{
				return;
			}

			Deleted = true;
			this.Remove();
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(UID);
						writer.Write(Enabled);
						writer.Write(Name);
						writer.Write(Link);
						writer.Write(Interval);
						writer.Write(Tokens);
						writer.Write(BonusTokens);
						writer.Write(BonusTokensChance);
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.GetVersion();

			switch (version)
			{
				case 0:
					{
						UID = reader.ReadInt();
						Enabled = reader.ReadBool();
						Name = reader.ReadString();
						Link = reader.ReadString();
						Interval = reader.ReadTimeSpan();
						Tokens = reader.ReadInt();
						BonusTokens = reader.ReadInt();
						BonusTokensChance = reader.ReadInt();
					}
					break;
			}
		}

		public override string ToString()
		{
			return Link;
		}

		public override int GetHashCode()
		{
			return UID;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj))
			{
				return false;
			}

			if (ReferenceEquals(this, obj))
			{
				return true;
			}

			if (obj.GetType() != GetType())
			{
				return false;
			}

			return Equals((IVoteSite)obj);
		}

		public override void Clear()
		{
			Enabled = false;
			Name = String.Empty;
			Link = String.Empty;
			Interval = TimeSpan.Zero;
			Tokens = 0;
		}

		public override void Reset()
		{
			Enabled = false;
			Name = String.Empty;
			Link = String.Empty;
			Interval = TimeSpan.FromHours(12);
			Tokens = 1;
		}

		public static bool operator ==(VoteSite left, IVoteSite right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(VoteSite left, IVoteSite right)
		{
			return !Equals(left, right);
		}

		public static bool operator ==(IVoteSite left, VoteSite right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(IVoteSite left, VoteSite right)
		{
			return !Equals(left, right);
		}

		public static bool operator ==(VoteSite left, VoteSite right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(VoteSite left, VoteSite right)
		{
			return !Equals(left, right);
		}
	}
}