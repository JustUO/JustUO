#region Header
//   Vorspire    _,-'/-'/  ToolbarLink.cs
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

using Server;
using Server.Gumps;
using Server.Mobiles;

using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.Toolbar
{
	public class ToolbarLink : ToolbarEntry, IEquatable<ToolbarLink>
	{
		public ToolbarLink()
		{ }

		public ToolbarLink(
			string url,
			string label = null,
			bool canDelete = true,
			bool canEdit = true,
			bool highlight = false,
			Color? labelColor = null)
			: base(url, label, canDelete, canEdit, highlight, labelColor)
		{ }

		public ToolbarLink(GenericReader reader)
			: base(reader)
		{ }

		public override string GetDisplayLabel()
		{
			return "<u>" + base.GetDisplayLabel() + "</u>";
		}

		protected override void CompileOptions(ToolbarGump toolbar, GumpButton clicked, Point loc, MenuGumpOptions opts)
		{
			if (toolbar == null)
			{
				return;
			}

			base.CompileOptions(toolbar, clicked, loc, opts);

			PlayerMobile user = toolbar.State.User;

			if (!CanEdit && user.AccessLevel < Toolbars.Access)
			{
				return;
			}

			opts.Replace(
				"Set Value",
				new ListGumpEntry(
					"Set URL",
					b => new InputDialogGump(user, toolbar)
					{
						Title = "Set URL",
						Html = "Set the URL for this Link entry.",
						InputText = base.Value,
						Callback = (cb, text) =>
						{
							Value = text;
							toolbar.Refresh(true);
						}
					}.Send(),
					toolbar.HighlightHue));
		}

		public override bool ValidateState(ToolbarState state)
		{
			if (!base.ValidateState(state))
			{
				return false;
			}

			return !String.IsNullOrWhiteSpace(base.Value);
		}

		public override void Invoke(ToolbarState state)
		{
			if (state == null)
			{
				return;
			}

			PlayerMobile user = state.User;

			if (user == null || user.Deleted || user.NetState == null)
			{
				return;
			}

			VitaNexCore.TryCatch(
				() => user.LaunchBrowser(FullValue),
				ex =>
				{
					Console.WriteLine("{0} => {1} => ({2}) => {3}", user, GetType().Name, FullValue, ex);
					Toolbars.CMOptions.ToConsole(ex);
				});
		}

		public bool Equals(ToolbarLink other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && Equals(Value, other.Value);
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

			var other = obj as ToolbarLink;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			return base.GetHashCode();
		}

		public static bool operator ==(ToolbarLink left, ToolbarLink right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ToolbarLink left, ToolbarLink right)
		{
			return !Equals(left, right);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
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
					break;
			}
		}
	}
}