
namespace Scripts.Engines.Encryption 
{
	public interface IClientEncryption
	{
		// Encrypt outgoing data
		void serverEncrypt(ref byte[] buffer, int length);

		// Decrypt incoming data
		void clientDecrypt(ref byte[] buffer, int length);
	}
}
