using System.Xml;

namespace Server
{
	public static class StartupReader
	{
		private static readonly string clientpath;
		private static readonly string shardname;

		static StartupReader()
		{
			XmlDocument xml = new XmlDocument();
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
