#region Header
//   Vorspire    _,-'/-'/  RestrictionList.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2014  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System.Collections.Generic;

using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.AutoPvP
{
	public class PvPRestrictionListGump<TKey> : ListGump<TKey>
	{
		public static string HelpText = "Restrictions: Lists specific restrictions for this battle.";

		public PvPRestrictionListGump(
			PlayerMobile user,
			PvPBattleRestrictionsBase<TKey> res,
			Gump parent = null,
			bool locked = true,
			bool useConfirm = true)
			: base(
				user,
				parent,
				emptyText: "There are no restrictions to display.",
				title: (res != null) ? res.ToString() : "Restrictions")
		{
			Restrictions = res;
			Locked = locked;
			UseConfirmDialog = useConfirm;

			ForceRecompile = true;
		}

		public PvPBattleRestrictionsBase<TKey> Restrictions { get; set; }

		public virtual bool Locked { get; set; }
		public virtual bool UseConfirmDialog { get; set; }

		protected override void CompileList(List<TKey> list)
		{
			list.Clear();
			list.AddRange(Restrictions.List.Keys);
			base.CompileList(list);
		}

		protected override int GetLabelHue(int index, int pageIndex, TKey entry)
		{
			return entry != null
					   ? (Restrictions.IsRestricted(entry) ? ErrorHue : HighlightHue)
					   : base.GetLabelHue(index, pageIndex, default(TKey));
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			list.AppendEntry(new ListGumpEntry("Restrict All", OnRestrictAll, HighlightHue));
			list.AppendEntry(new ListGumpEntry("Unrestrict All", OnUnrestrictAll, HighlightHue));
			list.AppendEntry(new ListGumpEntry("Invert All", OnInvertAll, HighlightHue));

			if (!Locked && User.AccessLevel >= AutoPvP.Access)
			{
				list.AppendEntry(new ListGumpEntry("Delete All", OnDeleteAll, HighlightHue));
				list.AppendEntry(new ListGumpEntry("Add Entry", OnAddRestriction, HighlightHue));
			}

			list.AppendEntry(new ListGumpEntry("Help", ShowHelp));

			base.CompileMenuOptions(list);
		}

		protected virtual void ShowHelp(GumpButton button)
		{
			if (User != null && !User.Deleted)
			{
				Send(new NoticeDialogGump(User, this, title: "Help", html: HelpText));
			}
		}

		protected virtual void OnDeleteAll(GumpButton button)
		{
			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						this,
						title: "Delete All Entries?",
						html:
							"All entries in the " + Restrictions +
							" will be deleted, erasing all data associated with them.\nThis action can not be reversed.\n\nDo you want to continue?",
						onAccept: subButton =>
						{
							Restrictions.Clear();
							Refresh(true);
						},
						onCancel: b => Refresh(true)));
			}
			else
			{
				Restrictions.Clear();
				Refresh(true);
			}
		}

		protected virtual void OnRestrictAll(GumpButton button)
		{
			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						this,
						title: "Restrict All Entries?",
						html:
							"All entries in the " + Restrictions +
							" will be restricted.\nThis action can not be reversed.\n\nDo you want to continue?",
						onAccept: subButton =>
						{
							Restrictions.Reset(true);
							Refresh(true);
						},
						onCancel: b => Refresh(true)));
			}
			else
			{
				Restrictions.Reset(true);
				Refresh(true);
			}
		}

		protected virtual void OnUnrestrictAll(GumpButton button)
		{
			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						this,
						title: "Unrestrict All Entries?",
						html:
							"All entries in the " + Restrictions +
							" will be unrestricted.\nThis action can not be reversed.\n\nDo you want to continue?",
						onAccept: subButton =>
						{
							Restrictions.Reset(false);
							Refresh(true);
						},
						onCancel: b => Refresh(true)));
			}
			else
			{
				Restrictions.Reset(false);
				Refresh(true);
			}
		}

		protected virtual void OnInvertAll(GumpButton button)
		{
			if (UseConfirmDialog)
			{
				Send(
					new ConfirmDialogGump(
						User,
						this,
						title: "Invert All Entries?",
						html:
							"All entries in the " + Restrictions +
							" will be toggled to their opposite setting..\nThis action can not be reversed.\n\nDo you want to continue?",
						onAccept: subButton =>
						{
							Restrictions.Invert();
							Refresh(true);
						},
						onCancel: b => Refresh(true)));
			}
			else
			{
				Restrictions.Invert();
				Refresh(true);
			}
		}

		protected virtual void OnAddRestriction(GumpButton button)
		{
			Send(
				new InputDialogGump(
					User,
					this,
					title: "Add Restriction",
					html: OnAddEntryGetHtml(),
					callback: OnAddEntryConfirm,
					onCancel: b => Refresh(true)));
		}

		protected virtual string OnAddEntryGetHtml()
		{
			return "Add an entry by name.";
		}

		protected virtual void OnAddEntryConfirm(GumpButton b, string text)
		{ }

		protected override void SelectEntry(GumpButton button, TKey entry)
		{
			base.SelectEntry(button, entry);

			if (button != null && entry != null)
			{
				Send(new PvPRestrictionListEntryGump<TKey>(User, Restrictions, Refresh(), button, entry, Locked, UseConfirmDialog));
			}
		}
	}
}