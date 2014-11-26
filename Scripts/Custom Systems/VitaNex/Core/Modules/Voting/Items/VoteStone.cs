#region Header
//   Vorspire    _,-'/-'/  VoteStone.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Drawing;

using Server;
using Server.Misc;
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.Voting
{
	public class VotingStone : Item
	{
		private int _SiteUID;

		[Constructable]
		public VotingStone()
			: this(4963)
		{ }

		[Constructable]
		public VotingStone(int siteUID)
			: base(4963)
		{
			SiteUID = siteUID;
			UsageColor = KnownColor.SkyBlue;

			Name = "Voting Stone";
			LootType = LootType.Blessed;
			Weight = 0;
		}

		public VotingStone(Serial serial)
			: base(serial)
		{ }

		public override bool DisplayLootType { get { return false; } }
		public override bool DisplayWeight { get { return false; } }

		[CommandProperty(Voting.Access)]
		public int SiteUID
		{
			get { return _SiteUID; }
			set
			{
				_SiteUID = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(Voting.Access)]
		public KnownColor UsageColor { get; set; }

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			IVoteSite site = Voting.FindSite(SiteUID);

			if (site != null && !site.Deleted)
			{
				int color = Color.FromKnownColor(UsageColor).ToArgb();

				list.Add(
					"<basefont color=#{0:X6}>Use: Cast a vote for {1} at {2}<basefont color=#ffffff>",
					color,
					ServerList.ServerName,
					site.Name);
			}
			else
			{
				list.Add("<basefont color=#{0:X6}>[No Vote Site Available]<basefont color=#ffffff>", Color.Red.ToArgb());
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			PlayerMobile voter = from as PlayerMobile;

			if (voter == null || voter.Deleted)
			{
				return;
			}

			IVoteSite site = Voting.FindSite(SiteUID);

			if (site != null)
			{
				site.Vote(voter);
			}
			else if (voter.AccessLevel >= Voting.Access)
			{
				SuperGump.Send(new VoteAdminGump(voter));
			}
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.WriteFlag(UsageColor);
						writer.Write(SiteUID);
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
						UsageColor = reader.ReadFlag<KnownColor>();
						SiteUID = reader.ReadInt();
					}
					break;
			}
		}
	}
}