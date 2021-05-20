using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace LoginLibrary.Services
{
    public class CryptoService : ICryptoService
    {
        private readonly Aes aes;
        ECDiffieHellmanCng dh;

        public CryptoService()
        {
            aes = Aes.Create();
        }

        public byte[] GeneratePublicKey()
        {
            dh = new ECDiffieHellmanCng(256)
            {
                KeyDerivationFunction = ECDiffieHellmanKeyDerivationFunction.Hash,
                HashAlgorithm = CngAlgorithm.Sha256
            };

            return dh.PublicKey.ToByteArray();
        }

        public byte[] GenerateIV(byte[] publicKey)
        {
            aes.Key = dh.DeriveKeyMaterial(CngKey.Import(publicKey, CngKeyBlobFormat.EccPublicBlob));
            aes.GenerateIV();
            return aes.IV;
        }

        public Task<string> DecryptData(byte[] encryptedData)
        {
            using (MemoryStream msDecrypt = new MemoryStream(encryptedData))
            {
                using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, aes.CreateDecryptor(aes.Key,
                    aes.IV), CryptoStreamMode.Read))
                {
                    using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                    {
                        return Task.FromResult(srDecrypt.ReadToEnd());
                    }
                }
            }
        }

        public Task<byte[]> EncryptData(string dataToEncrypt)
        {
            using (MemoryStream msEncrypt = new MemoryStream())
            {
                using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, aes.CreateEncryptor(aes.Key,
                    aes.IV), CryptoStreamMode.Write))
                {
                    using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                    {
                        swEncrypt.Write(dataToEncrypt);
                    }

                    return Task.FromResult(msEncrypt.ToArray());
                }
            }
        }

        public byte[] CreateHash(string password, string salt)
        {
            using var shaHasher = SHA256.Create();
            return shaHasher.ComputeHash(Encoding.UTF8.GetBytes($"{password}{salt}"));
        }
    }
}
