#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
//   .      __,-; ,'( '/
//    \.    `-.__`-._`:_,-._       _ , . ``
//     `:-._,------' ` _,`--` -: `_ , ` ,' :
//        `---..__,,--'  (C) 2013  ` -'. -'
//        #  Vita-Nex [http://core.vita-nex.com]  #
//  {o)xxx|===============-   #   -===============|xxx(o}
//        #        The MIT License (MIT)          #
#endregion

#region References
using System;
using System.Globalization;

using Server;
using Server.Commands;
#endregion

namespace VitaNex.Modules.WorldChat
{
	public sealed class WorldChatOptions : CoreModuleOptions
	{
		public const char DefaultPrefix = ';';

		private char _ChatPrefix;

		[CommandProperty(WorldChat.Access)]
		public WorldChatAccessPrefixOptions AccessPrefixes { get; set; }

		[CommandProperty(WorldChat.Access)]
		public char ChatPrefix
		{
			get { return _ChatPrefix; }
			set
			{
				_ChatPrefix = (Char.IsWhiteSpace(value) ||
							   Insensitive.StartsWith(CommandSystem.Prefix, value.ToString(CultureInfo.InvariantCulture)))
								  ? DefaultPrefix
								  : value;
			}
		}

		[CommandProperty(WorldChat.Access)]
		public byte HistoryBuffer { get; set; }

		public WorldChatOptions()
			: base(typeof(WorldChat))
		{
			AccessPrefixes = new WorldChatAccessPrefixOptions();

			ChatPrefix = DefaultPrefix;
		}

		public WorldChatOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			writer.SetVersion(0);

			AccessPrefixes.Serialize(writer);

			writer.Write(ChatPrefix);
			writer.Write(HistoryBuffer);
		}

		public override void Deserialize(GenericReader reader)
		{
			base.Deserialize(reader);

			reader.GetVersion();

			AccessPrefixes = new WorldChatAccessPrefixOptions(reader);

			ChatPrefix = reader.ReadChar();
			HistoryBuffer = reader.ReadByte();
		}
	}
}