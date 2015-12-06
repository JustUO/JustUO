
namespace Scripts.Engines.Encryption 
{
	public class NoEncryption : IClientEncryption
	{
		public void serverEncrypt(ref byte[] buffer, int length) 
		{
		}

		public void clientDecrypt(ref byte[] buffer, int length) 
		{
		}
	}
}
