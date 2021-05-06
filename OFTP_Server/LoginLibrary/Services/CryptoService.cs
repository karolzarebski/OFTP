using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly CryptoConfiguration _cryptoConfiguration;
        private readonly Aes aes;

        public CryptoService(CryptoConfiguration cryptoConfiguration)
        {
            _cryptoConfiguration = cryptoConfiguration;

            aes = Aes.Create();
        }

        /// <summary>
        /// Decrypt password
        /// </summary>
        /// <param name="encryptedPassword">Encrypted password</param>
        /// <returns>Decrypted password</returns>
        public Task<string> DecryptPassword(byte[] encryptedPassword)
        {
            using (MemoryStream msDecrypt = new MemoryStream(encryptedPassword))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(_cryptoConfiguration.Key,
                    _cryptoConfiguration.IV), CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return Task.FromResult(srDecrypt.ReadToEnd());
                    }
                }
            }
        }

        /// <summary>
        /// Encrypt password
        /// </summary>
        /// <param name="password">Password to encrypt</param>
        /// <returns>Encrypted password</returns>
        public Task<byte[]> EncryptPassword(string password)
        {
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(_cryptoConfiguration.Key,
                    _cryptoConfiguration.IV), CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(password);
                    }

                    return Task.FromResult(msEncrypt.ToArray());
                }
            }
        }
    }
}
