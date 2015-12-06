//using Server.ClientConfiguration;

namespace Scripts.Engines.Encryption
{
    public class Configuration
    {
        // Set this to true to enable this subsystem.
        public static bool Enabled = true;

        // Set this to false to disconnect unencrypted connections.
        public static bool AllowUnencryptedClients = true;

        // This is the list of supported game encryption keys.
        // You can use the utility found at http://www.hartte.de/ExtractKeys.exe
        // to extract the neccesary keys from any client version.
        public static LoginKey[] LoginKeys = new LoginKey[]
		{
            new LoginKey("4.0.45.0", 0x27C4752D, 0xA6AA1E7F),
            new LoginKey("4.0.43.0", 0x27BC873D, 0xA692127F),
            new LoginKey("4.0.43.0", 0x2775D54D, 0xA6FA9E7F),
            new LoginKey("4.0.40.0", 0x26DF277D, 0xA633527F),
            new LoginKey("4.0.34.22", 0x296AAFDD, 0xA197227F),
            new LoginKey("4.0.34.0", 0x296AAFDD, 0xA197227F),
            new LoginKey("4.0.18.0", 0x2DE12CDD, 0xA3E8227F),
            new LoginKey("4.0.11.2", 0x2C7B574D, 0xA32D9E7F),          
            new LoginKey("7.0.45 2D",0x2644752D, 0xA66A1E7F),
            new LoginKey("7.0.43 2D",0x26F5D54D, 0xA63A9E7F),
            /*
            new LoginKey("7.0.10", 0x1f9c9575, 0x1bd26d6b),
            new LoginKey("7.0.1.1", 0x2FABA7ED , 0xA2C17E7F ),
            new LoginKey("7.0.11", 0x2FABA7ED , 0xA2C17E7F ),
            new LoginKey("7.0.4", 0x2FABA7ED , 0xA2C17E7F ),
            new LoginKey("7.0.3", 0x2FABA7ED , 0xA2C17E7F ),
            new LoginKey("7.0.2", 0x2FABA7ED , 0xA2C17E7F ),
            new LoginKey("7.0.0", 0x2F93A5FD , 0xA2DD527F ),
			new LoginKey("6.0.14", 0x2C022D1D, 0xA31DA27F),
			new LoginKey("6.0.13", 0x2DCAF72D, 0xA3F71E7F),
		    new LoginKey("6.0.12", 0x2DB2853D, 0xA3CA127F),
			new LoginKey("6.0.11", 0x2D7B574D, 0xA3AD9E7F),
			new LoginKey("6.0.10", 0x2D236D5D, 0xA380A27F),
			new LoginKey("6.0.9", 0x2EEB076D, 0xA263BE7F),
			new LoginKey("6.0.8", 0x2ED3257D, 0xA27F527F),
			new LoginKey("6.0.7", 0x2E9BC78D, 0xA25BFE7F),
			new LoginKey("6.0.6", 0x2E43ED9D, 0xA234227F),
			new LoginKey("6.0.5", 0x2E0B97AD, 0xA210DE7F),
			new LoginKey("6.0.4", 0x2FF385BD, 0xA2ED127F),
			new LoginKey("6.0.3", 0x2FBBB7CD, 0xA2C95E7F),
			new LoginKey("6.0.2", 0x2F63ADDD, 0xA2A5227F),
			new LoginKey("6.0.1", 0x2F2BA7ED, 0xA2817E7F),
			new LoginKey("6.0.0", 0x2f13a5fd, 0xa29d527f),
			new LoginKey("5.0.9", 0x2F6B076D, 0xA2A3BE7F),
			new LoginKey("5.0.8", 0x2F53257D, 0xA2BF527F),
			new LoginKey("5.0.7", 0x10140441, 0xA29BFE7F),
			new LoginKey("5.0.6", 0x2fc3ed9c, 0xa2f4227f),
			new LoginKey("5.0.5", 0x2f8b97ac, 0xa2d0de7f),
			new LoginKey("5.0.4", 0x2e7385bc, 0xa22d127f),
			new LoginKey("5.0.3", 0x2e3bb7cc, 0xa2095e7f),
			new LoginKey("5.0.2", 0x2ee3addc, 0xa265227f),
			new LoginKey("5.0.1", 0x2eaba7ec, 0xa2417e7f),
			new LoginKey("5.0.0", 0x2E93A5FC, 0xA25D527F),
			new LoginKey("4.0.11", 0x2C7B574C, 0xA32D9E7F),
			new LoginKey("4.0.10", 0x2C236D5C, 0xA300A27F),
			new LoginKey("4.0.9", 0x2FEB076C, 0xA2E3BE7F),
			new LoginKey("4.0.8", 0x2FD3257C, 0xA2FF527F),
			new LoginKey("4.0.7", 0x2F9BC78D, 0xA2DBFE7F),
			new LoginKey("4.0.6", 0x2F43ED9C, 0xA2B4227F),
			new LoginKey("4.0.5", 0x2F0B97AC, 0xA290DE7F),
			new LoginKey("4.0.4", 0x2EF385BC, 0xA26D127F),
			new LoginKey("4.0.3", 0x2EBBB7CC, 0xA2495E7F),
			new LoginKey("4.0.2", 0x2E63ADDC, 0xA225227F),
			new LoginKey("4.0.1", 0x2E2BA7EC, 0xA2017E7F),
			new LoginKey("4.0.0", 0x2E13A5FC, 0xA21D527F),
			new LoginKey("3.0.8", 0x2C53257C, 0xA33F527F),
			new LoginKey("3.0.7", 0x2C1BC78C, 0xA31BFE7F),
			new LoginKey("3.0.6", 0x2CC3ED9C, 0xA374227F),
			new LoginKey("3.0.5", 0x2C8B97AC, 0xA350DE7F),
             */
		};
    }
}
