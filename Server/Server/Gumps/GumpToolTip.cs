using System;
using Server.Network;

namespace Server.Gumps
{
	public class GumpTooltip : GumpEntry
	{
		private int m_Number;
		private string m_Args;

		public GumpTooltip( int number )
			: this( number, null )
		{
		}

		public GumpTooltip( int number, string args )
		{
			m_Number = number;
			m_Args = args;
		}

		public int Number
		{
			get
			{
				return m_Number;
			}
			set
			{
				base.Delta( ref m_Number, value );
			}
		}

		public string Args
		{
			get
			{
				return m_Args;
			}
			set
			{
				Delta( ref m_Args, value );
			}
		}

		public override string Compile()
		{
			if ( string.IsNullOrEmpty( m_Args ) )
				return string.Format( "{{ tooltip {0} }}", m_Number );
			else
				return string.Format( "{{ tooltip {0} @{1}@ }}", m_Number, m_Args );
		}

		private static byte[] m_LayoutName = Gump.StringToBuffer( "tooltip" );

		public override void AppendTo( IGumpWriter disp )
		{
			disp.AppendLayout( m_LayoutName );
			disp.AppendLayout( m_Number );

			if ( !string.IsNullOrEmpty( m_Args ) )
				disp.AppendLayout( m_Args );
		}
	}
}
