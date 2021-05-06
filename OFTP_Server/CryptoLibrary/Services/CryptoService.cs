using System;
using System.IO;
using System.Text;
using System.Security.Cryptography;

namespace CryptoLibrary.Services
{
    class CryptoService : ICryptoService
    {
        private readonly Aes aes;
        private readonly CryptoConfiguration cryptoConfiguration;

        // Karol pewnie wykorzysta
        //private readonly ILogger<LoginService> _logger;

        // Karol pewnie wykorzysta
        public CryptoService(CryptoConfiguration _cryptoConfiguration)//, ILogger<LoginService> logger)
        {
            // Karol pewnie wykorzysta
            //_logger = logger;
            cryptoConfiguration = _cryptoConfiguration;

            aes = Aes.Create();
        }


        public byte[] Encrypt(string data)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateEncryptor(cryptoConfiguration.Key, cryptoConfiguration.IV), CryptoStreamMode.Write))
                {
                    byte[] binaryData = new byte[data.Length];

                    for (int i = 0; i < data.Length; i++)
                    {
                        binaryData[i] = (byte)data[i];
                    }

                    cryptoStream.Write(binaryData, 0, binaryData.Length);
                    cryptoStream.FlushFinalBlock();
                    return memoryStream.ToArray();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                // Karol pewnie wykorzysta
                //_logger.LogInformation(e.Message);
            }
            return null;
        }

        public string Decrypt(byte[] data)
        {
            try
            {
                using (var memoryStream = new MemoryStream())
                using (var cryptoStream = new CryptoStream(memoryStream, aes.CreateDecryptor(cryptoConfiguration.Key, cryptoConfiguration.IV), CryptoStreamMode.Write))
                {
                    cryptoStream.Write(data, 0, data.Length);
                    cryptoStream.FlushFinalBlock();
                    return Encoding.ASCII.GetString(memoryStream.ToArray());
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);

                // Karol pewnie wykorzysta
                //_logger.LogInformation(e.Message);
            }
            return null;
        }
    }
}
