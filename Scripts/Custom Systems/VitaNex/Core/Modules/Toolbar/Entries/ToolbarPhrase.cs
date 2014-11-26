#region Header
//   Vorspire    _,-'/-'/  ToolbarPhrase.cs
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
using Server.Network;

using VitaNex.SuperGumps;
using VitaNex.SuperGumps.UI;
#endregion

namespace VitaNex.Modules.Toolbar
{
	public class ToolbarPhrase : ToolbarEntry, IEquatable<ToolbarPhrase>
	{
		[CommandProperty(Toolbars.Access)]
		public virtual MessageType TextType { get; set; }

		[CommandProperty(Toolbars.Access)]
		public TimeSpan SpamDelay { get; set; }

		public ToolbarPhrase()
		{
			SpamDelay = TimeSpan.FromSeconds(3.0);
			TextType = MessageType.Regular;
		}

		public ToolbarPhrase(
			string phrase,
			string label = null,
			bool canDelete = true,
			bool canEdit = true,
			bool highlight = false,
			Color? labelColor = null,
			MessageType type = MessageType.Regular)
			: base(phrase, label, canDelete, canEdit, highlight, labelColor)
		{
			SpamDelay = TimeSpan.FromSeconds(3.0);
			TextType = type;
		}

		public ToolbarPhrase(GenericReader reader)
			: base(reader)
		{ }

		public override string GetDisplayLabel()
		{
			return "\"" + base.GetDisplayLabel() + "\"";
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

			opts.AppendEntry(
				new ListGumpEntry(
					"Set Type",
					b =>
					{
						MenuGumpOptions tOpts = new MenuGumpOptions();

						if (TextType != MessageType.Regular)
						{
							tOpts.AppendEntry(
								new ListGumpEntry(
									"Regular",
									tb =>
									{
										TextType = MessageType.Regular;
										toolbar.Refresh(true);
									}));
						}

						if (TextType != MessageType.Whisper)
						{
							tOpts.AppendEntry(
								new ListGumpEntry(
									"Whisper",
									tb =>
									{
										TextType = MessageType.Whisper;
										toolbar.Refresh(true);
									}));
						}

						if (TextType != MessageType.Yell)
						{
							tOpts.AppendEntry(
								new ListGumpEntry(
									"Yell",
									tb =>
									{
										TextType = MessageType.Yell;
										toolbar.Refresh(true);
									}));
						}

						SuperGump.Send(new MenuGump(user, clicked.Parent, tOpts, clicked));
					},
					toolbar.HighlightHue));

			opts.Replace(
				"Set Value",
				new ListGumpEntry(
					"Set Phrase",
					b =>
					SuperGump.Send(
						new InputDialogGump(
							user,
							toolbar,
							title: "Set Phrase",
							html: "Set the text for this Phrase entry.",
							input: Value,
							callback: (cb, text) =>
							{
								Value = text;
								toolbar.Refresh(true);
							})),
					toolbar.HighlightHue));
		}

		public override bool ValidateState(ToolbarState state)
		{
			return base.ValidateState(state) && !String.IsNullOrWhiteSpace(base.Value);
		}

		protected override void OnCloned(ToolbarEntry clone)
		{
			base.OnCloned(clone);

			if (clone == null)
			{
				return;
			}

			ToolbarPhrase phrase = clone as ToolbarPhrase;

			if (phrase == null)
			{
				return;
			}

			phrase.SpamDelay = SpamDelay;
			phrase.TextType = TextType;
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
				() =>
				{
					if (!user.CanBeginAction(this))
					{
						return;
					}

					switch (TextType)
					{
						case MessageType.Whisper:
							user.DoSpeech(FullValue, new int[0], TextType, user.WhisperHue);
							break;
						case MessageType.Yell:
							user.DoSpeech(FullValue, new int[0], TextType, user.YellHue);
							break;
						default:
							user.DoSpeech(FullValue, new int[0], TextType, user.SpeechHue);
							break;
					}

					user.BeginAction(this);
					Timer.DelayCall(SpamDelay, () => user.EndAction(this));
				},
				ex =>
				{
					Console.WriteLine("{0} => {1} => ({2}) => {3}", user, GetType().Name, FullValue, ex);
					Toolbars.CMOptions.ToConsole(ex);
				});
		}

		public bool Equals(ToolbarPhrase other)
		{
			if (ReferenceEquals(null, other))
			{
				return false;
			}

			if (ReferenceEquals(this, other))
			{
				return true;
			}

			return base.Equals(other) && Equals(Value, other.Value) && TextType == other.TextType &&
				   SpamDelay.Equals(other.SpamDelay);
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

			var other = obj as ToolbarPhrase;
			return other != null && Equals(other);
		}

		public override int GetHashCode()
		{
			unchecked
			{
				int hashCode = base.GetHashCode();
				hashCode = (hashCode * 397) ^ (int)TextType;
				hashCode = (hashCode * 397) ^ SpamDelay.GetHashCode();
				return hashCode;
			}
		}

		public static bool operator ==(ToolbarPhrase left, ToolbarPhrase right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(ToolbarPhrase left, ToolbarPhrase right)
		{
			return !Equals(left, right);
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(1);

			switch (version)
			{
				case 1:
				case 0:
					{
						writer.Write(SpamDelay);

						if (version > 0)
						{
							writer.WriteFlag(TextType);
						}
						else
						{
							writer.Write((int)TextType);
						}
					}
					break;
			}
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			int version = reader.ReadInt();

			switch (version)
			{
				case 1:
				case 0:
					{
						SpamDelay = reader.ReadTimeSpan();

						MessageType mt = version > 0 ? reader.ReadFlag<MessageType>() : (MessageType)reader.ReadInt();

						TextType = mt;
					}
					break;
			}
		}
	}
}