#region Header
//   Vorspire    _,-'/-'/  MOTD_Init.cs
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
using System.Globalization;
using System.Xml;

using Server;

using VitaNex.Crypto;
using VitaNex.IO;
#endregion

namespace VitaNex.Modules.MOTD
{
	[CoreModule("MOTD", "1.0.0.0")]
	public static partial class MOTD
	{
		static MOTD()
		{
			CMOptions = new MOTDOptions();

			Messages = new XmlDataStore<string, MOTDMessage>(VitaNexCore.SavesDirectory + "/MOTD", "Messages")
			{
				OnSerialize = Serialize,
				OnDeserialize = Deserialize
			};
		}

		private static void CMConfig()
		{
			EventSink.Login += OnLogin;
		}

		private static void CMEnabled()
		{
			EventSink.Login += OnLogin;
			CommandUtility.Register(CMOptions.PopupCommand, Access, CMOptions.HandlePopupCommand);
		}

		private static void CMDisabled()
		{
			EventSink.Login -= OnLogin;
			CommandUtility.Unregister(CMOptions.PopupCommand);
		}

		private static void CMSave()
		{
			Messages.Import();
			DataStoreResult result = Messages.Export();
			CMOptions.ToConsole("{0} messages saved, {1}.", Messages.Count > 0 ? Messages.Count.ToString("#,#") : "0", result);
		}

		private static void CMLoad()
		{
			DataStoreResult result = Messages.Import();
			CMOptions.ToConsole("{0} messages loaded, {1}.", Messages.Count > 0 ? Messages.Count.ToString("#,#") : "0", result);
		}

		private static bool Serialize(XmlDocument doc)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					if (Messages == null || Messages.Count == 0)
					{
						return;
					}

					XmlElement root = doc.CreateElement("messages");
					XmlElement msgRoot = null, msgSub = null;

					foreach (MOTDMessage message in Messages.Values)
					{
						VitaNexCore.TryCatch(
							() =>
							{
								msgRoot = doc.CreateElement("entry");
								msgRoot.SetAttribute("timestamp", message.Date.Stamp.ToString(CultureInfo.InvariantCulture));
								msgRoot.SetAttribute("title", message.Title);
								msgRoot.SetAttribute("author", message.Author);
								msgRoot.SetAttribute("published", message.Published.ToString());
								msgRoot.SetAttribute("uid", message.UniqueID);

								msgSub = doc.CreateElement("content");
								msgSub.InnerText = message.Content;
								msgRoot.AppendChild(msgSub);

								root.AppendChild(msgRoot);
								msgRoot = null;
								msgSub = null;
							},
							CMOptions.ToConsole);
					}

					doc.AppendChild(root);
				},
				CMOptions.ToConsole);

			return true;
		}

		private static bool Deserialize(XmlDocument doc)
		{
			VitaNexCore.TryCatch(
				() =>
				{
					if (doc.FirstChild == null || doc.FirstChild.Name != "messages")
					{
						return;
					}

					XmlElement root = doc["messages"];

					if (root == null)
					{
						return;
					}

					MOTDMessage message;
					TimeStamp date;
					bool published;
					string author, title, content, uid;

					foreach (XmlElement node in root)
					{
						VitaNexCore.TryCatch(
							() =>
							{
								date = node.HasAttribute("timestamp")
										   ? (TimeStamp)Double.Parse(node.GetAttribute("timestamp"))
										   : TimeStamp.UtcNow;
								published = !node.HasAttribute("published") || Boolean.Parse(node.GetAttribute("published"));
								author = node.HasAttribute("author") ? node.GetAttribute("author") : "Anonymous";
								title = node.HasAttribute("title") ? node.GetAttribute("title") : "Update";
								content = node["content"] != null ? node["content"].InnerText.Replace(@"\r\n", "[br]") : String.Empty;

								uid = node.HasAttribute("uid")
										  ? node.GetAttribute("uid")
										  : CryptoGenerator.GenString(CryptoHashType.MD5, String.Format("{0}", date.Stamp)).Replace("-", "");

								message = new MOTDMessage(uid, date, title, content, author, published);

								if (Messages.ContainsKey(uid))
								{
									Messages[uid] = message;
								}
								else
								{
									Messages.Add(uid, message);
								}
							},
							CMOptions.ToConsole);
					}
				},
				CMOptions.ToConsole);

			return true;
		}
	}
}