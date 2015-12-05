using System;
using Server.Network;

namespace Server.Gumps
{
	public class GumpGroup : GumpEntry
	{
		private int m_Group;

		public GumpGroup( int group )
		{
			m_Group = group;
		}

		public int Group
		{
			get
			{
				return m_Group;
			}
			set
			{
				Delta( ref m_Group, value );
			}
		}

		public override string Compile()
		{
			return String.Format( "{{ group {0} }}", m_Group );
		}

		private static byte[] m_LayoutName = Gump.StringToBuffer( "group" );

		public override void AppendTo( IGumpWriter disp )
		{
			disp.AppendLayout( m_LayoutName );
			disp.AppendLayout( m_Group );
		}
	}
}