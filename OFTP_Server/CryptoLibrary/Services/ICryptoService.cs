namespace CryptoLibrary.Services
{
    public interface ICryptoService
    {
        byte[] Encrypt(string data);
        string Decrypt(byte[] data);
    }
}
