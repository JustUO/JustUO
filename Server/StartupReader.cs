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
using System.Xml;
#endregion

namespace Server
{
	public static class StartupReader
	{
		private static readonly string clientpath;
		private static readonly string shardname;

		static StartupReader()
		{
			var xml = new XmlDocument();
			string filePath = "startup.xml";
			xml.Load(filePath);
			clientpath = xml.SelectSingleNode("startupinfo/clientpath").InnerText;
			shardname = xml.SelectSingleNode("startupinfo/shardname").InnerText;
		}

		public static string GetClientPath()
		{
			return clientpath;
		}

		public static string GetShardName()
		{
			return shardname;
		}
	}
}