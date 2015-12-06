using System;

namespace Scripts.Engines.Encryption
{
	public class LoginKey
	{
		private uint m_Key1;
		private uint m_Key2;
		private String m_Name;

		public LoginKey(String name, uint key1, uint key2)
		{
			m_Key1 = key1;
			m_Key2 = key2;
			m_Name = name;
		}

		public String Name 
		{
			get 
			{
				return m_Name;
			}
		}

		public uint Key1 
		{
			get 
			{
				return m_Key1;
			}
		}

		public uint Key2
		{
			get 
			{
				return m_Key2;
			}
		}
	}
}
