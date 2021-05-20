using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public interface ICryptoService
    {
        Task<byte[]> EncryptData(string password);
        Task<string> DecryptData(byte[] encryptedPassword);
        byte[] GeneratePublicKey();
        byte[] GenerateIV(byte[] publicKey);
    }
}
