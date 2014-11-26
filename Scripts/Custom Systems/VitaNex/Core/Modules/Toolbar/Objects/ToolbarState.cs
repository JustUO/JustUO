#region Header
//   Vorspire    _,-'/-'/  ToolbarState.cs
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
using Server.Mobiles;

using VitaNex.SuperGumps;
#endregion

namespace VitaNex.Modules.Toolbar
{
	public class ToolbarState : Grid<ToolbarEntry>
	{
		private SuperGump _ToolbarGump;

		[CommandProperty(Toolbars.Access)]
		public int X { get; set; }

		[CommandProperty(Toolbars.Access)]
		public int Y { get; set; }

		[CommandProperty(Toolbars.Access)]
		public PlayerMobile User { get; set; }

		[CommandProperty(Toolbars.Access)]
		public bool Minimized { get; set; }

		[CommandProperty(Toolbars.Access)]
		public ToolbarTheme Theme { get; set; }

		public ToolbarState(
			PlayerMobile user, int x = 0, int y = 28, int cols = 0, int rows = 0, ToolbarTheme theme = ToolbarTheme.Default)
			: base(cols < 1 ? Toolbars.CMOptions.DefaultWidth : cols, rows < 1 ? Toolbars.CMOptions.DefaultHeight : rows)
		{
			User = user;
			X = x;
			Y = y;
			Theme = theme;
		}

		public ToolbarState(GenericReader reader)
			: base(reader)
		{ }

		public virtual void SetToolbarGump(SuperGump tb)
		{
			_ToolbarGump = tb;
		}

		public virtual SuperGump GetToolbarGump()
		{
			if (User == null || User.Deleted || User.NetState == null)
			{
				if (_ToolbarGump != null)
				{
					_ToolbarGump.Dispose();
				}

				return _ToolbarGump = null;
			}

			return _ToolbarGump == null || _ToolbarGump.IsDisposed ? (_ToolbarGump = new ToolbarGump(this)) : _ToolbarGump;
		}

		public virtual void SetDefaultSize()
		{
			Resize(Toolbars.CMOptions.DefaultWidth, Toolbars.CMOptions.DefaultHeight);
		}

		public virtual void SetDefaultEntries()
		{
			SetDefaultSize();

			if (this == Toolbars.DefaultEntries)
			{
				Toolbars.LoadDefaultEntries();
				return;
			}

			for (int x = 0; x < Toolbars.DefaultEntries.Width; x++)
			{
				for (int y = 0; y < Toolbars.DefaultEntries.Height; y++)
				{
					ToolbarEntry entry = Toolbars.DefaultEntries[x, y];

					if (entry != null && entry.ValidateState(this))
					{
						SetContent(x, y, entry.Clone());
					}
				}
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
						if (this == Toolbars.DefaultEntries)
						{
							writer.Write(false);
						}
						else
						{
							writer.Write(true);
							writer.Write(User);
						}

						writer.Write(Minimized);
						writer.Write(X);
						writer.Write(Y);
						writer.WriteFlag(Theme);
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
						if (reader.ReadBool())
						{
							User = reader.ReadMobile<PlayerMobile>();
						}

						Minimized = reader.ReadBool();
						X = reader.ReadInt();
						Y = reader.ReadInt();
						Theme = reader.ReadFlag<ToolbarTheme>();
					}
					break;
			}
		}

		public override void SerializeContent(GenericWriter writer, ToolbarEntry content, int x, int y)
		{
			writer.WriteType(
				content,
				t =>
				{
					if (t != null)
					{
						content.Serialize(writer);
					}
				});
		}

		public override ToolbarEntry DeserializeContent(GenericReader reader, Type type, int x, int y)
		{
			return reader.ReadTypeCreate<ToolbarEntry>(reader);
		}
	}
}