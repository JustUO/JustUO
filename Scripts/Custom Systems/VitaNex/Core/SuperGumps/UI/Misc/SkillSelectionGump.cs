#region Header
//   Vorspire    _,-'/-'/  SkillSelectionGump.cs
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

using Server;
using Server.Gumps;
using Server.Mobiles;
#endregion

namespace VitaNex.SuperGumps.UI
{
	public class SkillSelectionGump : ListGump<SkillName>
	{
		private static readonly SkillName[] _Skills = ((SkillName)0).GetValues<SkillName>();

		public int Limit { get; set; }

		public virtual Action<GumpButton> AcceptHandler { get; set; }
		public virtual Action<GumpButton> CancelHandler { get; set; }

		public virtual Action<SkillName[]> Callback { get; set; }

		public List<SkillName> SelectedSkills { get; set; }
		public List<SkillName> IgnoredSkills { get; set; }

		public SkillSelectionGump(
			PlayerMobile user,
			Gump parent = null,
			int limit = 1,
			Action<GumpButton> onAccept = null,
			Action<GumpButton> onCancel = null,
			Action<SkillName[]> callback = null,
			params SkillName[] ignoredSkills)
			: base(user, parent, emptyText: "No skills to display.", title: "Skill Selection")
		{
			EntriesPerPage = 30;

			Limit = limit;
			Callback = callback;
			SelectedSkills = new List<SkillName>();
			IgnoredSkills = new List<SkillName>(ignoredSkills);

			if (Limit > 0)
			{
				Title += ": (" + Limit + " Max)";
			}

			AcceptHandler = onAccept;
			CancelHandler = onCancel;
		}

		protected override void CompileList(List<SkillName> list)
		{
			List.Clear();

			foreach (SkillName skill in _Skills.Where(skill => !IgnoredSkills.Contains(skill)))
			{
				List.Add(skill);
			}

			base.CompileList(list);

			List.Sort((a, b) => String.Compare(a.ToString(), b.ToString(), StringComparison.Ordinal));
		}

		protected override sealed void CompileMenuOptions(MenuGumpOptions list)
		{ }

		protected override sealed void ShowOptionMenu(GumpButton button)
		{
			OnAccept(button);
		}

		protected virtual void OnAccept(GumpButton button)
		{
			if (AcceptHandler != null)
			{
				AcceptHandler(button);
			}

			if (Callback != null)
			{
				Callback(SelectedSkills.ToArray());
			}
		}

		protected virtual void OnCancel(GumpButton button)
		{
			if (CancelHandler != null)
			{
				CancelHandler(button);
			}
		}

		protected override void CompileLayout(SuperGumpLayout layout)
		{
			base.CompileLayout(layout);

			layout.Remove("button/header/options");

			layout.Add("button/header/done", () => AddButton(15, 15, 248, 249, ShowOptionMenu));

			if (Minimized)
			{
				return;
			}

			layout.AddReplace("background/body/base", () => AddBackground(0, 55, 420, 330, 9270));
			layout.Remove("imagetiled/body/vsep/0");
		}

		protected override void CompileEntryLayout(
			SuperGumpLayout layout, int length, int index, int pIndex, int yOffset, SkillName entry)
		{
			int xOffset = 0;

			if (pIndex < EntriesPerPage - 20)
			{
				xOffset = 10;
			}
			else if (pIndex < EntriesPerPage - 10)
			{
				xOffset = 145;
				yOffset = 70 + (pIndex - 10) * 30;
			}
			else if (pIndex < EntriesPerPage)
			{
				xOffset = 280;
				yOffset = 70 + (pIndex - 20) * 30;
			}

			layout.AddReplace(
				"check/list/select/" + index,
				() => AddButton(
					xOffset,
					yOffset,
					5033,
					5033,
					b =>
					{
						if (SelectedSkills.Contains(entry))
						{
							SelectedSkills.Remove(entry);
						}
						else
						{
							if (SelectedSkills.Count < Limit)
							{
								SelectedSkills.Add(entry);
							}
							else
							{
								Send(
									new NoticeDialogGump(
										User,
										Refresh(true),
										title: "Limit Reached",
										html:
											"You have selected the maximum of " + Limit +
											" skills.\nIf you are happy with your selection, click the 'Okay' button."));
								return;
							}
						}

						Refresh(true);
					}));

			if (SelectedSkills.Contains(entry))
			{
				layout.Add(
					"imagetiled/list/entry/" + index,
					() =>
					{
						AddImageTiled(xOffset, yOffset, 128, 28, 3004);
						AddImageTiled(4 + xOffset, 4 + yOffset, 120, 20, 9274);
					});
			}
			else
			{
				layout.Add("imagetiled/list/entry/" + index, () => AddImageTiled(xOffset, yOffset, 128, 28, 9274));
			}

			layout.Add(
				"html/list/entry/" + index,
				() =>
				AddHtml(
					4 + xOffset,
					4 + yOffset,
					120,
					20,
					String.Format(
						"<center><big><basefont color=#{0:X6}>{1}</big></center>",
						GetLabelHue(index, pIndex, entry),
						GetLabelText(index, pIndex, entry)),
					false,
					false));
		}

		protected override int GetLabelHue(int index, int pageIndex, SkillName entry)
		{
			return SelectedSkills.Contains(entry) ? Color.Cyan.ToArgb() : Color.White.ToArgb();
		}
	}
}