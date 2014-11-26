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
using Server;

using VitaNex.IO;
using VitaNex.MySQL;
#endregion

namespace VitaNex.Modules.AutoDonate
{
	public sealed class AutoDonateOptions : CoreModuleOptions
	{
		public AutoDonateOptions()
			: base(typeof(AutoDonate))
		{
			MySQL = new MySQLConnectionInfo("localhost", 3306, "root", "", ODBCVersion.V_5_1, "donate_db");

			MoneySymbol = '$';
			MoneyAbbr = "USD";
			TableName = "donate_trans";
			ShowHistory = false;
			GiftingEnabled = true;
			ExchangeRate = 1.0;
			CurrencyType = "Gold";
		}

		public AutoDonateOptions(GenericReader reader)
			: base(reader)
		{ }

		[CommandProperty(AutoDonate.Access)]
		public bool Connected { get { return AutoDonate.Connection.Connected; } }

		[CommandProperty(AccessLevel.Administrator)]
		public int ProfileTotal { get { return AutoDonate.Profiles.Count; } }

		[CommandProperty(AccessLevel.Administrator)]
		public DataStoreStatus Status { get { return AutoDonate.Profiles.Status; } }

		[CommandProperty(AutoDonate.Access)]
		public MySQLConnectionInfo MySQL { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public string TableName { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public char MoneySymbol { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public string MoneyAbbr { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public bool GiftingEnabled { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public bool ShowHistory { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public double ExchangeRate { get; set; }

		[CommandProperty(AutoDonate.Access)]
		public ItemTypeSelectProperty CurrencyType { get; set; }

		public override void Clear()
		{
			base.Clear();

			MySQL = new MySQLConnectionInfo("", 0, "", "", ODBCVersion.V_5_1, "");
			MoneySymbol = ' ';
			MoneyAbbr = "";
			TableName = "";
			ShowHistory = false;
			GiftingEnabled = false;
			ExchangeRate = 1.0;
			CurrencyType = "Gold";
		}

		public override void Reset()
		{
			base.Reset();

			MySQL = new MySQLConnectionInfo("localhost", 3306, "root", "", ODBCVersion.V_5_1, "donate_db");
			MoneySymbol = '$';
			MoneyAbbr = "USD";
			TableName = "donate_trans";
			ShowHistory = false;
			GiftingEnabled = true;
			ExchangeRate = 1.0;
			CurrencyType = "Gold";
		}

		public override string ToString()
		{
			return "Donation Config";
		}

		public override void Serialize(GenericWriter writer)
		{
			base.Serialize(writer);

			int version = writer.SetVersion(0);

			switch (version)
			{
				case 0:
					{
						MySQL.Serialize(writer);
						CurrencyType.Serialize(writer);

						writer.Write(TableName);
						writer.Write(ShowHistory);
						writer.Write(ExchangeRate);
						writer.Write(MoneySymbol);
						writer.Write(MoneyAbbr);
						writer.Write(GiftingEnabled);
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
						MySQL = new MySQLConnectionInfo(reader);
						CurrencyType = new ItemTypeSelectProperty(reader);

						TableName = reader.ReadString();
						ShowHistory = reader.ReadBool();
						ExchangeRate = reader.ReadDouble();
						MoneySymbol = reader.ReadChar();
						MoneyAbbr = reader.ReadString();
						GiftingEnabled = reader.ReadBool();
					}
					break;
			}
		}
	}
}