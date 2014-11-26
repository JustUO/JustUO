#region Header
//   Vorspire    _,-'/-'/  VoteRequestEventArgs.cs
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

using Server;
#endregion

namespace VitaNex.Modules.Voting
{
	public class VoteRequestEventArgs : EventArgs
	{
		public VoteRequestEventArgs(Mobile from, IVoteSite site, int tokens, bool message)
		{
			Mobile = from;
			Site = site;
			Tokens = tokens;
			Message = message;
		}

		public Mobile Mobile { get; private set; }
		public IVoteSite Site { get; private set; }
		public int Tokens { get; set; }
		public bool Message { get; set; }
		public bool HandledTokens { get; set; }
	}
}