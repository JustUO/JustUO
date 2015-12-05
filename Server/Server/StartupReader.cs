#region Header
// **************************************\
//     _  _   _   __  ___  _   _   ___   |
//    |# |#  |#  |## |### |#  |#  |###   |
//    |# |#  |# |#    |#  |#  |# |#  |#  |
//    |# |#  |#  |#   |#  |#  |# |#  |#  |
//   _|# |#__|#  _|#  |#  |#__|# |#__|#  |
//  |##   |##   |##   |#   |##    |###   |
//        [http://www.playuo.org]        |
// **************************************/
//  [2014] StartupReader.cs
// ************************************/
#endregion

#region References
using System;
using System.Xml;
#endregion

namespace Server
{
	public static class StartupReader
	{
		private static readonly string clientpath;
		private static readonly string shardname;
	    private static readonly double saves;
	    private static readonly double rewards;
	    private static readonly int skillcap;
	    private static readonly int statcap;
	    private static readonly double housedecay;
        private static readonly double bonding;

		static StartupReader()
		{
			var xml = new XmlDocument();
			string filePath = "startup.xml";
			xml.Load(filePath);
			clientpath = xml.SelectSingleNode("startupinfo/clientpath").InnerText;
            shardname = xml.SelectSingleNode("startupinfo/shardname").InnerText;
            saves = Convert.ToDouble(xml.SelectSingleNode("startupinfo/saves").InnerText);
            rewards = Convert.ToDouble(xml.SelectSingleNode("startupinfo/rewards").InnerText);
            skillcap = Convert.ToInt32(xml.SelectSingleNode("startupinfo/skillcap").InnerText);
            statcap = Convert.ToInt32(xml.SelectSingleNode("startupinfo/statcap").InnerText);
            housedecay = Convert.ToDouble(xml.SelectSingleNode("startupinfo/housedecay").InnerText);
            bonding = Convert.ToDouble(xml.SelectSingleNode("startupinfo/bonding").InnerText);
    	}

		public static string GetClientPath()
		{
			return clientpath;
		}

		public static string GetShardName()
		{
			return shardname;
		}
        public static double GetSaves()
        {
            return saves;
        }
        public static double GetRewards()
        {
            return rewards;
        }
        public static int GetSkillcap()
        {
            return skillcap;
        }
        public static int GetStatcap()
        {
            return statcap;
        }
        public static double GetHousedecay()
        {
            return housedecay;
        }
        public static double GetBonding()
        {
            return bonding;
        }
	}
}