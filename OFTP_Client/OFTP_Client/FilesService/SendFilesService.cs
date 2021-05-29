using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client.FilesService
{
    public class SendFilesService
    {
        private readonly string _serverIP;
        private TcpClient _client;
        private CryptoService _cryptoService;
        private int bufferLen = 51200; //50 KB

        public SendFilesService(string serverIP)
        {
            _serverIP = serverIP;
            _cryptoService = new CryptoService();
        }

        public async Task<bool> Connect()
        {
            try
            {
                await (_client = new TcpClient()).ConnectAsync(_serverIP, 12138);

                var code = new byte[3]; //TODO check size
                await _client.GetStream().ReadAsync(code, 0, code.Length);

                byte[] publicKey = new byte[72];
                await _client.GetStream().ReadAsync(publicKey, 0, publicKey.Length);

                var clientPublicKey = _cryptoService.GeneratePublicKey();
                await _client.GetStream().WriteAsync(clientPublicKey);

                byte[] iv = new byte[16];
                await _client.GetStream().ReadAsync(iv, 0, iv.Length);
                _cryptoService.AssignIV(publicKey, iv);

                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Błąd łączenia z klientem\n{ex.Message}", "Błąd połączenia",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        private async Task SendMessage(string message)
        {
            var encryptedData = await _cryptoService.EncryptData(message);
            var encryptedMessage = new byte[encryptedData.Length + 2];
            Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);
            var len = encryptedData.Length;
            encryptedMessage[0] = (byte)(len / 256);
            encryptedMessage[1] = (byte)(len % 256);
            await _client.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task SendData(byte[] data)
        {
            var encryptedData = await _cryptoService.EncryptData(data);
            var encryptedMessage = new byte[encryptedData.Length + 2];
            Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);
            var len = encryptedData.Length;
            encryptedMessage[0] = (byte)(len / 256);
            encryptedMessage[1] = (byte)(len % 256);
            await _client.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task<string> ReceiveMessage(bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[256]; //TODO check length
                await _client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
                return await _cryptoService.DecryptData(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
            }
            else
            {
                var messageBuffer = new byte[1024];
                await _client.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
                return await _cryptoService.DecryptData(messageBuffer.Skip(2)
                        .Take(messageBuffer[0] * 256 + messageBuffer[1]).ToArray());
            }
        }

        public async void SendFile(List<string> files)
        {
            await SendMessage($"{CodeNames.BeginFileTransmission}|{files.Count}");

            var responseCode = await ReceiveMessage(true);

            if (responseCode == CodeNames.AcceptFileTransmission)
            {
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo(file);
                    await SendMessage($"{fi.Name}|{fi.Length}");

                    using var fileStream = File.OpenRead(file);

                    while(fileStream.Position != fi.Length)
                    {
                        if(fi.Length - fileStream.Position < bufferLen)
                        {
                            int len = Convert.ToInt32(fi.Length - fileStream.Position);
                            var buffer = new byte[len];
                            fileStream.Read(buffer, 0, buffer.Length);
                            await SendData(buffer);
                        }
                        else
                        {
                            var buffer = new byte[bufferLen];
                            fileStream.Read(buffer, 0, buffer.Length);
                            await SendData(buffer);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show("Klient odmówił transmisji plików", "Odmowa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }
    }
}
