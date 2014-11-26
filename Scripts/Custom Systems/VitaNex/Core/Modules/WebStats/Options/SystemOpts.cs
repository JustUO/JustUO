#region Header
//   Vorspire    _,-'/-'/  SystemOpts.cs
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

namespace VitaNex.Modules.WebStats
{
	public class WebStatsOptions : CoreModuleOptions
	{
		[CommandProperty(WebStats.Access)]
		public short Port { get; set; }

		[CommandProperty(WebStats.Access)]
		public int MaxConnections { get; set; }

		[CommandProperty(WebStats.Access)]
		public TimeSpan UpdateInterval { get; set; }

		[CommandProperty(WebStats.Access, true)]
		public WebStatsRequestFlags RequestFlags { get; private set; }

		[CommandProperty(WebStats.Access)]
		public bool DisplayServer { get { return GetRequestFlag(WebStatsRequestFlags.Server); } set { SetRequestFlag(WebStatsRequestFlags.Server, value); } }

		[CommandProperty(WebStats.Access)]
		public bool DisplayStats { get { return GetRequestFlag(WebStatsRequestFlags.Stats); } set { SetRequestFlag(WebStatsRequestFlags.Stats, value); } }

		[CommandProperty(WebStats.Access)]
		public bool DisplayGuilds { get { return GetRequestFlag(WebStatsRequestFlags.Guilds); } set { SetRequestFlag(WebStatsRequestFlags.Guilds, value); } }

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayers { get { return GetRequestFlag(WebStatsRequestFlags.Players); } set { SetRequestFlag(WebStatsRequestFlags.Players, value); } }

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerStats { get { return GetRequestFlag(WebStatsRequestFlags.PlayerStats); } set { SetRequestFlag(WebStatsRequestFlags.PlayerStats, value); } }

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerSkills { get { return GetRequestFlag(WebStatsRequestFlags.PlayerSkills); } set { SetRequestFlag(WebStatsRequestFlags.PlayerSkills, value); } }

		[CommandProperty(WebStats.Access)]
		public bool DisplayPlayerEquip { get { return GetRequestFlag(WebStatsRequestFlags.PlayerEquip); } set { SetRequestFlag(WebStatsRequestFlags.PlayerEquip, value); } }

		private bool GetRequestFlag(WebStatsRequestFlags flag)
		{
			return RequestFlags.HasFlag(flag);
		}

		private void SetRequestFlag(WebStatsRequestFlags flag, bool value)
		{
			if (flag == WebStatsRequestFlags.None)
			{
				RequestFlags = value ? flag : WebStatsRequestFlags.All;
				return;
			}

			if (flag == WebStatsRequestFlags.All)
			{
				RequestFlags = value ? flag : WebStatsRequestFlags.None;
				return;
			}

			if (value)
			{
				RequestFlags |= flag;
			}
			else
			{
				RequestFlags &= ~flag;
			}
		}

		public WebStatsOptions()
			: base(typeof(WebStats))
		{
			Port = 3000;
			MaxConnections = 100;
			UpdateInterval = TimeSpan.FromSeconds(30.0);
			RequestFlags = WebStatsRequestFlags.Server | WebStatsRequestFlags.Stats;
		}

		public WebStatsOptions(GenericReader reader)
			: base(reader)
		{ }

		public override void Clear()
		{
			base.Clear();

			Port = 3000;
			MaxConnections = 100;
			UpdateInterval = TimeSpan.Zero;
			RequestFlags = WebStatsRequestFlags.None;
		}

		public override void Reset()
		{
			base.Reset();

			Port = 3000;
			MaxConnections = 100;
			UpdateInterval = TimeSpan.FromSeconds(30.0);
			RequestFlags = WebStatsRequestFlags.Server | WebStatsRequestFlags.Stats;
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						writer.Write(Port);
						writer.Write(MaxConnections);
						writer.Write(UpdateInterval);
						writer.WriteFlag(RequestFlags);
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
						Port = reader.ReadShort();
						MaxConnections = reader.ReadInt();
						UpdateInterval = reader.ReadTimeSpan();
						RequestFlags = reader.ReadFlag<WebStatsRequestFlags>();
					}
					break;
			}
		}
	}
}