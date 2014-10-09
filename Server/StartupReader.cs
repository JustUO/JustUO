using System;
using System.Xml;

namespace Server
{
    public class StartupReader
    {

        public static string GetClientPath()
        {
            XmlDocument xml = new XmlDocument();
            string filePath = "startup.xml";
            xml.Load(filePath);
            string clientpath = xml.SelectSingleNode("startupinfo/clientpath").InnerText;
            return clientpath;
        }

        public static string GetShardName()
        {
            XmlDocument xml = new XmlDocument();
            string filePath = "startup.xml";
            xml.Load(filePath);
            string shardname = xml.SelectSingleNode("startupinfo/shardname").InnerText;
            return shardname;
        }
    }
}
