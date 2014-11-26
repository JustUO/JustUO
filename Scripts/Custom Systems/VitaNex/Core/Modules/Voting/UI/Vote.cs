#region Header
//   Vorspire    _,-'/-'/  Vote.cs
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
using System.Drawing;
using System.Linq;
using System.Text;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.Voting
{
	public sealed class VoteGump : ListGump<IVoteSite>
	{
		public VoteGump(PlayerMobile user, Gump parent = null)
			: base(user, parent, emptyText: "There are no sites to display.", title: "Vote Site Listing")
		{
			ForceRecompile = true;
		}

		public override string GetSearchKeyFor(IVoteSite key)
		{
			return key != null ? key.Name : base.GetSearchKeyFor(null);
		}

		protected override int GetLabelHue(int index, int pageIndex, IVoteSite entry)
		{
			return entry != null
					   ? (entry.CanVote(User, false) ? HighlightHue : ErrorHue)
					   : base.GetLabelHue(index, pageIndex, null);
		}

		protected override string GetLabelText(int index, int pageIndex, IVoteSite entry)
		{
			return entry != null ? String.Format("{0}", entry.Name) : base.GetLabelText(index, pageIndex, null);
		}

		protected override void CompileList(List<IVoteSite> list)
		{
			list.Clear();
			list.AddRange(Voting.VoteSites.Values.Where(s => s != null && !s.Deleted && s.Enabled && s.Valid));

			base.CompileList(list);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.AppendEntry(new ListGumpEntry("View Profiles", ShowProfiles));
			list.AppendEntry(new ListGumpEntry("Help", ShowHelp));

			base.CompileMenuOptions(list);
		}

		protected override void SelectEntry(GumpButton button, IVoteSite entry)
		{
			base.SelectEntry(button, entry);

			var opts = new MenuGumpOptions();

			if (entry.CanVote(User, false))
			{
				opts.AppendEntry(new ListGumpEntry("Cast Vote", CastVote, HighlightHue));
			}

			opts.AppendEntry(new ListGumpEntry("Cancel", b => { }));

			Send(new MenuGump(User, Refresh(), opts, button));
		}

		private void CastVote()
		{
			if (Selected != null)
			{
				Selected.Vote(User);
			}

			Refresh(true);
		}

		private void ShowProfiles(GumpButton button)
		{
			if (User == null || User.Deleted)
			{
				return;
			}

			Send(new VoteProfilesGump(User, Hide()));
		}

		private void ShowHelp(GumpButton button)
		{
			if (User == null || User.Deleted)
			{
				return;
			}

			StringBuilder sb = VoteGumpUtility.GetHelpText(User);
			var g = new HtmlPanelGump<StringBuilder>(User, Refresh())
			{
				Selected = sb,
				Title = "Voting Help",
				Html = sb.ToString(),
				HtmlColor = Color.SkyBlue
			};
			g.Send();
		}
	}
}