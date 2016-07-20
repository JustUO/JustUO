using System;
using Server.Network;

namespace Server.Gumps
{
	public abstract class GumpEntry
	{
		private Gump m_Parent;

		public GumpEntry()
		{
		}

		protected void Delta( ref int var, int val )
		{
			if ( var != val )
				var = val;
		}

		protected void Delta( ref bool var, bool val )
		{
			if ( var != val )
				var = val;
		}

		protected void Delta( ref string var, string val )
		{
			if ( var != val )
				var = val;
		}

		public Gump Parent
		{
			get
			{
				return m_Parent;
			}
			set
			{
				if ( m_Parent != value )
				{
					if ( m_Parent != null )
						m_Parent.Remove( this );

					m_Parent = value;

					m_Parent.Add( this );
				}
			}
		}

		public abstract string Compile();
		public abstract void AppendTo( IGumpWriter disp );
	}
}