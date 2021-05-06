using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public interface ICryptoService
    {
        Task<byte[]> EncryptPassword(string password);
        Task<string> DecryptPassword(byte[] encryptedPassword);
    }
}
