#region Header
//   Vorspire    _,-'/-'/  Utility.cs
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
using System.Drawing;
using System.Text;

using Server;
using Server.Commands;
using Server.Misc;
#endregion

namespace VitaNex.Modules.Voting
{
	public static class VoteGumpUtility
	{
		public static StringBuilder GetHelpText(Mobile m)
		{
			StringBuilder help = new StringBuilder();

			help.AppendFormat("<basefont color=#{0:X6}>", Color.SkyBlue.ToArgb());
			help.AppendLine(
				"The Voting service allows you to vote for " + ServerList.ServerName + " and receive rewards (usually tokens).");

			if (Voting.CMOptions.DailyLimit > 0)
			{
				help.AppendLine();
				help.AppendFormat("<basefont color=#{0:X6}>", Color.Orange.ToArgb());
				help.AppendLine(
					String.Format(
						"There is a daily limit of {0} tokens, when you reach this limit you can still vote, but will not receive any tokens.",
						Voting.CMOptions.DailyLimit.ToString("#,#")));
			}

			help.AppendLine();
			help.AppendFormat("<basefont color=#{0:X6}>", Color.Yellow.ToArgb());
			help.AppendLine("All successful votes will be logged to your personal vote profile.");
			help.AppendLine("These logs are separated by day and can be viewed at any time.");

			if (!String.IsNullOrWhiteSpace(Voting.CMOptions.ProfilesCommand))
			{
				help.AppendLine();
				help.AppendFormat("<basefont color=#{0:X6}>", Color.YellowGreen.ToArgb());
				help.AppendLine(
					String.Format(
						"To view vote profiles, use the <big>{0}{1}</big> command.",
						CommandSystem.Prefix,
						Voting.CMOptions.ProfilesCommand));

				if (m.AccessLevel >= Voting.Access)
				{
					help.AppendLine("You have access to administrate the voting system, you can also manage profiles.");
				}
			}

			if (m.AccessLevel >= Voting.Access)
			{
				if (!String.IsNullOrWhiteSpace(Voting.CMOptions.AdminCommand))
				{
					help.AppendLine();
					help.AppendFormat("<basefont color=#{0:X6}>", Color.LimeGreen.ToArgb());
					help.AppendLine(
						String.Format(
							"To administrate the voting system, use the <big>{0}{1}</big> command.",
							CommandSystem.Prefix,
							Voting.CMOptions.AdminCommand));
				}
			}

			return help;
		}
	}
}