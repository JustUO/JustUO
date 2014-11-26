#region Header
//   Vorspire    _,-'/-'/  SuperGump_Props.cs
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
using System.Reflection;

using Server;
using Server.Gumps;
using Server.Mobiles;
using Server.Network;
#endregion

namespace VitaNex.SuperGumps
{
	public abstract partial class SuperGump
	{
		private static readonly MethodInfo _AddItemPropertyImpl = typeof(Gump).GetMethod(
			"AddItemProperty", new[] {typeof(int)});

		public void AddProperties(Item item)
		{
			if (item == null || item.Deleted)
			{
				return;
			}

			if (User.IsOnline())
			{
				User.Send(item.OPLPacket);
			}

			AddProperties(item.Serial);
		}

		public void AddProperties(Mobile mob)
		{
			if (mob == null || mob.Deleted)
			{
				return;
			}

			if (User.IsOnline())
			{
				User.Send(mob.OPLPacket);
			}

			AddProperties(mob.Serial);
		}

		public void AddProperties(Serial serial)
		{
			if (!serial.IsValid)
			{
				return;
			}

			if (_AddItemPropertyImpl != null)
			{
				try
				{
					_AddItemPropertyImpl.Invoke(this, new object[] {serial.Value});
					return;
				}
				catch
				{ }
			}

			Add(new GumpOPL(serial));
		}
	}

	public class GumpOPL : GumpEntry
	{
		private int m_Serial;

		public GumpOPL(int serial)
		{
			m_Serial = serial;
		}

		public int Serial { get { return m_Serial; } set { Delta(ref m_Serial, value); } }

		public override string Compile()
		{
			return String.Format("{{ itemproperty {0} }}", m_Serial);
		}

		private static readonly byte[] m_LayoutName = Gump.StringToBuffer("itemproperty");

		public override void AppendTo(IGumpWriter disp)
		{
			disp.AppendLayout(m_LayoutName);
			disp.AppendLayout(m_Serial);
		}
	}
}