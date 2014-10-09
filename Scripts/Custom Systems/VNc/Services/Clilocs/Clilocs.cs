#region Header
//   Vorspire    _,-'/-'/  Clilocs.cs
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
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml;

using Server;
using Server.Commands;
using Server.Network;

using VitaNex.IO;
#endregion

namespace VitaNex
{
	public static partial class Clilocs
	{
		public static readonly Regex VarPattern = new Regex(@"(~(\d+)_(\w+)~)", RegexOptions.IgnoreCase);

		public static ClilocLNG DefaultLanguage = ClilocLNG.ENU;

		private static CoreServiceOptions _CSOptions = new CoreServiceOptions(typeof(Clilocs));

		private static readonly List<string> _Languages = new List<string>
		{
			"ENU",
			"DEU",
			"ESP",
			"FRA",
			"JPN",
			"KOR",
			"CHT"
		};

		private static readonly Dictionary<ClilocLNG, ClilocTable> _Tables = new Dictionary<ClilocLNG, ClilocTable>
		{
			{ClilocLNG.ENU, new ClilocTable()},
			{ClilocLNG.DEU, new ClilocTable()},
			{ClilocLNG.ESP, new ClilocTable()},
			{ClilocLNG.FRA, new ClilocTable()},
			{ClilocLNG.JPN, new ClilocTable()},
			{ClilocLNG.KOR, new ClilocTable()},
			{ClilocLNG.CHT, new ClilocTable()}
		};

		public static CoreServiceOptions CSOptions { get { return _CSOptions ?? (_CSOptions = new CoreServiceOptions(typeof(Clilocs))); } }

		public static Dictionary<ClilocLNG, ClilocTable> Tables { get { return _Tables; } }

		private static void ExportCommand(CommandEventArgs e)
		{
			if (e.Mobile == null || e.Mobile.Deleted)
			{
				return;
			}

			e.Mobile.SendMessage(0x55, "Export requested...");

			if (e.Arguments == null || e.Arguments.Length == 0 || String.IsNullOrWhiteSpace(e.Arguments[0]))
			{
				e.Mobile.SendMessage("Usage: {0}{1} <{2}>", CommandSystem.Prefix, e.Command, String.Join(" | ", _Languages));
				return;
			}

			ClilocLNG lng;

			if (Enum.TryParse(e.Arguments[0], true, out lng) && lng != ClilocLNG.NULL)
			{
				VitaNexCore.TryCatch(
					() =>
					{
						FileInfo file = Export(lng);

						if (file != null && file.Exists && file.Length > 0)
						{
							e.Mobile.SendMessage(0x55, "{0} clilocs have been exported to: {1}", lng, file.FullName);
						}
						else
						{
							e.Mobile.SendMessage(0x22, "Could not export clilocs for {0}", lng);
						}
					},
					ex =>
					{
						e.Mobile.SendMessage(0x22, "A fatal exception occurred, check the console for details.");
						CSOptions.ToConsole(ex);
					});
			}
			else
			{
				e.Mobile.SendMessage("Usage: {0}{1} <{2}>", CommandSystem.Prefix, e.Command, String.Join(" | ", _Languages));
			}
		}

		public static FileInfo Export(ClilocLNG lng)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			var list = new XmlDataStore<int, ClilocData>(VitaNexCore.DataDirectory + "/Exported Clilocs/", lng.ToString());
			ClilocTable table = _Tables[lng];

			list.OnSerialize = doc =>
			{
				XmlNode node;
				XmlCDataSection cdata;
				ClilocInfo info;

				XmlNode root = doc.CreateElement("clilocs");

				XmlAttribute attr = doc.CreateAttribute("len");
				attr.Value = table.Count.ToString(CultureInfo.InvariantCulture);

				if (root.Attributes != null)
				{
					root.Attributes.Append(attr);
				}

				attr = doc.CreateAttribute("lng");
				attr.Value = table.Language.ToString();

				if (root.Attributes != null)
				{
					root.Attributes.Append(attr);
				}

				foreach (ClilocData d in table.Where(d => d.Length > 0))
				{
					info = d.Lookup(table.InputFile, true);

					if (info == null || String.IsNullOrWhiteSpace(info.Text))
					{
						continue;
					}

					node = doc.CreateElement("cliloc");

					attr = doc.CreateAttribute("idx");
					attr.Value = d.Index.ToString(CultureInfo.InvariantCulture);

					if (node.Attributes != null)
					{
						node.Attributes.Append(attr);
					}

					attr = doc.CreateAttribute("len");
					attr.Value = d.Length.ToString(CultureInfo.InvariantCulture);

					if (node.Attributes != null)
					{
						node.Attributes.Append(attr);
					}

					cdata = doc.CreateCDataSection(info.Text);
					node.AppendChild(cdata);

					root.AppendChild(node);
				}

				doc.AppendChild(root);
				table.Clear();

				return true;
			};

			list.Export();
			list.Clear();

			return list.Document;
		}

		public static ClilocInfo Lookup(this ClilocLNG lng, int index)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			return _Tables.ContainsKey(lng) && _Tables[lng] != null ? _Tables[lng].Lookup(index) : null;
		}

		public static ClilocInfo Lookup(this ClilocLNG lng, Type t)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			return VitaNexCore.TryCatchGet(
				() =>
				{
					if (t == null)
					{
						return null;
					}

					PropertyInfo p = t.GetProperty("LabelNumber");

					if (p == null || !p.CanRead)
					{
						return null;
					}

					object o = t.CreateInstanceSafe<object>();

					if (o == null)
					{
						return null;
					}

					var index = (int)p.GetValue(o, null); // LabelNumber_get()

					MethodInfo m = t.GetMethod("Delete");

					if (m != null)
					{
						m.Invoke(o, new object[0]); // Delete_call()
					}

					return Lookup(lng, index);
				});
		}

		public static string GetRawString(this ClilocLNG lng, int index)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			return _Tables.ContainsKey(lng) && _Tables[lng] != null && !_Tables[lng].IsNullOrWhiteSpace(index)
					   ? _Tables[lng][index].Text
					   : String.Empty;
		}

		public static string GetString(this ClilocLNG lng, int index, string args)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			ClilocInfo info = Lookup(lng, index);

			return info == null ? String.Empty : info.ToString(args);
		}

		public static string GetString(this ClilocLNG lng, int index, params string[] args)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			ClilocInfo info = Lookup(lng, index);

			return info == null ? String.Empty : info.ToString(args);
		}

		public static string GetString(this TextDefinition text)
		{
			return GetString(text, DefaultLanguage);
		}

		public static string GetString(this TextDefinition text, Mobile m)
		{
			return GetString(text, m != null ? m.GetLanguage() : DefaultLanguage);
		}

		public static string GetString(this TextDefinition text, ClilocLNG lng)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			return text.Number > 0 ? lng.GetString(text.Number) : (text.String ?? String.Empty);
		}

		public static bool IsNullOrEmpty(this TextDefinition text)
		{
			return text.Number <= 0 && String.IsNullOrEmpty(text.String) && String.IsNullOrEmpty(text.GetString());
		}

		public static bool IsNullOrWhiteSpace(this TextDefinition text)
		{
			return text.Number <= 0 && String.IsNullOrWhiteSpace(text.String) && String.IsNullOrWhiteSpace(text.GetString());
		}

		public static string[] DecodePropertyList(this ObjectPropertyList list, ClilocLNG lng)
		{
			if (lng == ClilocLNG.NULL)
			{
				lng = DefaultLanguage;
			}

			int length;
			var data = list.Compile(false, out length);
			var msgs = new List<string>();

			var reader = new PacketReader(data, length, false);
			reader.Seek(15, SeekOrigin.Begin);

			for (int i = 15; i < data.Length - 4;)
			{
				int index = reader.ReadInt32();
				int paramLength = reader.ReadInt16() / 2;

				string param = String.Empty;

				if (paramLength > 0)
				{
					param = reader.ReadUnicodeStringLE(paramLength);
				}

				msgs.Add(GetString(lng, index, param));

				i += (6 + paramLength);
			}

			return msgs.ToArray();
		}

		public static string ReadUnicodeStringLE(this PacketReader reader, int length)
		{
			int index = reader.Seek(0, SeekOrigin.Current);
			int bound = index + (length << 1);
			int end = bound;

			if (bound > reader.Size)
			{
				bound = reader.Size;
			}

			int c;

			var buffer = new List<byte>();

			while (reader.Seek(0, SeekOrigin.Current) + 1 < bound &&
				   (c = reader.Buffer[reader.Seek(1, SeekOrigin.Current)] | reader.Buffer[reader.Seek(1, SeekOrigin.Current)] << 8) !=
				   0)
			{
				buffer.Add((byte)c);
			}

			index = end;
			reader.Seek(index, SeekOrigin.Begin);

			return Encoding.Unicode.GetString(buffer.ToArray());
		}

		public static ClilocLNG GetLanguage(this Mobile m)
		{
			ClilocLNG lng;

			return !Enum.TryParse(m.Language, out lng) ? ClilocLNG.ENU : lng;
		}
	}
}