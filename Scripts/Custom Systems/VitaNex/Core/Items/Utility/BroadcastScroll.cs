#region Header
//   Vorspire    _,-'/-'/  BroadcastScroll.cs
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
using Server.Items;
using Server.Mobiles;
using Server.Network;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Items
{
	public class BroadcastScrollGump : TextInputPanelGump<BroadcastScroll>
	{
		public bool UseConfirmDialog { get; set; }

		public BroadcastScrollGump(
			PlayerMobile user, Gump parent = null, BroadcastScroll scroll = null, bool useConfirm = true)
			: base(
				user,
				parent,
				width: 400,
				height: 150,
				limit: 200,
				selected: scroll,
				emptyText: "No scroll selected.",
				title: "Broadcast Scroll Message (200 Chars)")
		{
			UseConfirmDialog = useConfirm;
		}

		protected override void Compile()
		{
			base.Compile();

			if (Selected != null)
			{
				Input = Selected.Message;
			}
		}

		protected override void ParseInput(string text)
		{
			if (Selected != null)
			{
				Selected.Message = text ?? String.Empty;
			}

			base.ParseInput(text);
		}

		protected override void CompileMenuOptions(MenuGumpOptions list)
		{
			if (Selected != null)
			{
				list.AppendEntry(
					new ListGumpEntry(
						"Broadcast",
						subButton =>
						{
							Selected.Message = Input ?? String.Empty;
							Selected.Broadcast(User);
							Refresh(true);
						},
						HighlightHue));

				list.AppendEntry(
					new ListGumpEntry(
						"Clear",
						subButton =>
						{
							Selected.Message = Input = String.Empty;
							Refresh(true);
						},
						ErrorHue));
			}

			base.CompileMenuOptions(list);
		}
	}

	[Flipable(0xE34, 0xEF3)]
	public class BroadcastScroll : Item, IUsesRemaining
	{
		private bool _ShowUsesRemaining = true;
		private int _UsesRemaining = 10;

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual string Message { get; set; }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int MessageHue { get; set; }

		public override bool DisplayLootType { get { return false; } }
		public override bool DisplayWeight { get { return false; } }

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual int UsesRemaining
		{
			get { return _UsesRemaining; }
			set
			{
				_UsesRemaining = value;
				InvalidateProperties();
			}
		}

		[CommandProperty(AccessLevel.GameMaster)]
		public virtual bool ShowUsesRemaining
		{
			get { return _ShowUsesRemaining; }
			set
			{
				_ShowUsesRemaining = value;
				InvalidateProperties();
			}
		}

		[Constructable]
		public BroadcastScroll()
			: this(1)
		{ }

		[Constructable]
		public BroadcastScroll(int uses)
			: base(0xE34)
		{
			UsesRemaining = Math.Max(1, uses);

			Name = "Broadcast Scroll";
			LootType = LootType.Blessed;
			Stackable = false;
			Weight = 0;
		}

		public BroadcastScroll(Serial serial)
			: base(serial)
		{ }

		public void Broadcast(Mobile from)
		{
			if (from == null || from.Deleted)
			{
				return;
			}

			if (!IsChildOf(from.Backpack) || !IsAccessibleTo(from) || !from.CanSee(this) || InSecureTrade)
			{
				return;
			}

			if (String.IsNullOrWhiteSpace(Message))
			{
				if (from is PlayerMobile)
				{
					SuperGump.Send(
						new NoticeDialogGump(
							from as PlayerMobile,
							title: "Empty Message",
							html: "Your broadcast message can't be blank and can't consist only of white-space."));
				}

				return;
			}

			if (Deleted || UsesRemaining <= 0)
			{
				if (from is PlayerMobile)
				{
					SuperGump.Send(
						new NoticeDialogGump(
							(PlayerMobile)from,
							title: "Scroll Exhausted",
							html: "Your broadcast scroll has been exhausted, you can't send another message."));
				}

				return;
			}

			//Send the message to all online players, including staff.
			int reach = 0, staff = 0;
			NetState.Instances.ForEach(
				state =>
				{
					if (state == null || !state.Running || state.Mobile == null)
					{
						return;
					}

					state.Mobile.SendMessage(MessageHue > 0 ? MessageHue : from.SpeechHue, "Message from {0}:", from.RawName);
					state.Mobile.SendMessage(MessageHue > 0 ? MessageHue : from.SpeechHue, Message);

					if (state.Mobile == from || (DateTime.UtcNow - state.ConnectedOn) < TimeSpan.FromMinutes(1))
					{
						return;
					}

					//If receiver is not sender and receiver has been logged in for over 1 minute, include them in total reached.
					reach++;

					if (state.Mobile.AccessLevel >= AccessLevel.Counselor)
					{
						staff++;
					}
				});

			//If we reached people and they weren't just staff, charge for the message.
			if (reach - staff > 0)
			{
				from.SendMessage(0x55, "Your broadcast was seen by {0:#,0} people!", reach);

				if (--UsesRemaining <= 0)
				{
					from.SendMessage(0x22, "Your broadcast scroll has been exhausted, you throw away the scrap paper.");
					Delete();
				}
			}
			else
			{
				from.SendMessage(0x22, "Your broadcast was not seen by anyone, your scroll was not consumed this time.");
			}
		}

		public override void OnDoubleClick(Mobile from)
		{
			base.OnDoubleClick(from);

			if (!this.CheckDoubleClick(from, true, true, 2, true))
			{
				return;
			}

			if (from is PlayerMobile && UsesRemaining > 0)
			{
				new BroadcastScrollGump((PlayerMobile)from, scroll: this).Send();
			}
		}

		public override void GetProperties(ObjectPropertyList list)
		{
			base.GetProperties(list);

			if (ShowUsesRemaining)
			{
				// uses remaining: ~1_val~
				list.Add(1060584, UsesRemaining == Int32.MaxValue ? "unlimited" : String.Format("{0:#,0}", UsesRemaining));
			}

			list.Add("Use: Broadcast A Global Message".WrapUOHtmlColor(Color.SkyBlue));
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Message);
						writer.Write(_UsesRemaining);
						writer.Write(_ShowUsesRemaining);
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
						Message = reader.ReadString();
						_UsesRemaining = reader.ReadInt();
						_ShowUsesRemaining = reader.ReadBool();
					}
					break;
			}
		}
	}

	public class BroadcastScroll_Unlimited : BroadcastScroll
	{
		public override int UsesRemaining { get { return Int32.MaxValue; } set { } }

		[Constructable]
		public BroadcastScroll_Unlimited()
			: base(Int32.MaxValue)
		{
			ShowUsesRemaining = false;
		}

		public BroadcastScroll_Unlimited(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class BroadcastScroll_3Uses : BroadcastScroll
	{
		[Constructable]
		public BroadcastScroll_3Uses()
			: base(3)
		{ }

		public BroadcastScroll_3Uses(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class BroadcastScroll_5Uses : BroadcastScroll
	{
		[Constructable]
		public BroadcastScroll_5Uses()
			: base(5)
		{ }

		public BroadcastScroll_5Uses(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class BroadcastScroll_10Uses : BroadcastScroll
	{
		[Constructable]
		public BroadcastScroll_10Uses()
			: base(10)
		{ }

		public BroadcastScroll_10Uses(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class BroadcastScroll_30Uses : BroadcastScroll
	{
		[Constructable]
		public BroadcastScroll_30Uses()
			: base(30)
		{ }

		public BroadcastScroll_30Uses(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class BroadcastScroll_50Uses : BroadcastScroll
	{
		[Constructable]
		public BroadcastScroll_50Uses()
			: base(50)
		{ }

		public BroadcastScroll_50Uses(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}

	public class BroadcastScroll_100Uses : BroadcastScroll
	{
		[Constructable]
		public BroadcastScroll_100Uses()
			: base(100)
		{ }

		public BroadcastScroll_100Uses(Serial serial)
			: base(serial)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();
		}
	}
}