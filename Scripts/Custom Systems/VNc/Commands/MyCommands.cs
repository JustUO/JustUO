#region Header
//   Vorspire    _,-'/-'/  MyCommands.cs
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
using System.Globalization;
using System.Linq;

using Server;
using Server.Commands;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Commands
{
	public static class MyCommandsCommand
	{
		public static void Initialize()
		{
			CommandSystem.Register(
				"MyCommands",
				AccessLevel.Player,
				e =>
				{
					if (e == null || e.Mobile == null || !(e.Mobile is PlayerMobile))
					{
						return;
					}

					SuperGump.Send(new MyCommandsGump(e.Mobile as PlayerMobile));
				});
		}
	}

	public class MyCommandsGump : ListGump<CommandEntry>
	{
		public MyCommandsGump(PlayerMobile user, Gump parent = null)
			: base(user, parent, title: "My Commands", emptyText: "No commands to display.")
		{
			Sorted = true;
			Modal = false;
			CanMove = false;
			CanResize = false;
		}

		public override string GetSearchKeyFor(CommandEntry key)
		{
			return key != null ? key.Command : base.GetSearchKeyFor(null);
		}

		public override int SortCompare(CommandEntry a, CommandEntry b)
		{
			if (a == null && b == null)
			{
				return 0;
			}

			if (a == null)
			{
				return 1;
			}

			if (b == null)
			{
				return -1;
			}

			if (a.AccessLevel > b.AccessLevel)
			{
				return -1;
			}

			if (a.AccessLevel < b.AccessLevel)
			{
				return 1;
			}

			return String.Compare(a.Command, b.Command, StringComparison.OrdinalIgnoreCase);
		}

		protected override void CompileList(List<CommandEntry> list)
		{
			list.Clear();

			list.AddRange(
				CommandSystem.Entries.Values.Where(
					cmd =>
					cmd.AccessLevel <= User.AccessLevel && !String.IsNullOrWhiteSpace(cmd.Command) &&
					!Insensitive.Equals(cmd.Command, "MyCommands")));

			base.CompileList(list);
		}

		protected override void SelectEntry(GumpButton button, CommandEntry entry)
		{
			base.SelectEntry(button, entry);

			User.SendMessage(0x55, "Using Command: {0}", entry.Command);
			CommandSystem.Handle(User, String.Format("{0}{1}", CommandSystem.Prefix, entry.Command));
			Refresh();
		}

		protected override string GetLabelText(int index, int pageIndex, CommandEntry entry)
		{
			return entry != null && !String.IsNullOrWhiteSpace(entry.Command)
					   ? entry.Command[0].ToString(CultureInfo.InvariantCulture).ToUpper() + entry.Command.Substring(1)
					   : base.GetLabelText(index, pageIndex, entry);
		}

		protected override int GetLabelHue(int index, int pageIndex, CommandEntry entry)
		{
			if (entry == null)
			{
				return base.GetLabelHue(index, pageIndex, null);
			}

			if (entry.AccessLevel >= AccessLevel.Administrator)
			{
				return 0x516;
			}

			if (entry.AccessLevel > AccessLevel.GameMaster)
			{
				return 0x144;
			}

			if (entry.AccessLevel > AccessLevel.Counselor)
			{
				return 0x21;
			}

			if (entry.AccessLevel > AccessLevel.Player)
			{
				return 0x30;
			}

			return TextHue;
		}
	}
}