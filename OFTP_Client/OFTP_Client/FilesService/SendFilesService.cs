using OFTP_Client.Events;
using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        private int bufferLen = 25599; //25 KB

        public event EventHandler<SendProgressEvent> SendFileProgress;

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

                var code = new byte[3];
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
            await _client.GetStream().WriteAsync(encryptedData);
        }

        private async Task SendData2(byte[] data)
        {
            var encryptedData = await _cryptoService.EncryptData(data);
            //Debug.WriteLine(encryptedData.Length);
            await _client.GetStream().WriteAsync(encryptedData);
        }

        private async Task<string> ReceiveMessage(bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[16]; 
                await _client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);

                return await _cryptoService.DecryptData(codeBuffer);
            }
            else
            {
                var messageBuffer = new byte[1024];
                await _client.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
                return await _cryptoService.DecryptData(messageBuffer);
            }
        }

        public int Map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return Convert.ToInt32((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
        }

        public async Task SendFiles(List<string> files)
        {
            int filesSent = 0;

            await SendMessage($"{CodeNames.BeginFileTransmission}|{files.Count}");

            var responseCode = await ReceiveMessage(true);

            if (responseCode == CodeNames.AcceptFileTransmission)
            {
                foreach (var file in files)
                {
                    int i = 0;

                    FileInfo fi = new FileInfo(file);

                    var fileLength = $"{fi.Name}|{fi.Length}|";

                    int count = Convert.ToInt32(Math.Ceiling(fi.Length / Convert.ToDouble(bufferLen)));

                    using var fileStream = File.OpenRead(file);

                    while (fileLength.Length < 120)
                    {
                        fileLength += '0';
                    }

                    await SendMessage($"{CodeNames.FileLength}|{fileLength}");

                    SendFileProgress.Invoke(this, new SendProgressEvent 
                    {
                        Value = Map(filesSent++, 0, files.Count, 0, 100),
                        General = true, 
                        Receive = false 
                    });

                    while (fileStream.Position != fi.Length)
                    {
                        if (await ReceiveMessage(true) == CodeNames.NextPartialData)
                        {
                            if (fi.Length - fileStream.Position < bufferLen)
                            {
                                int len = Convert.ToInt32(fi.Length - fileStream.Position);
                                var buffer = new byte[len];

                                //Debug.WriteLine(len);

                                var nextDataLength = $"{CodeNames.NextDataLength}|{len}|";

                                while (nextDataLength.Length < 10)
                                {
                                    nextDataLength += '0';
                                }

                                await SendMessage(nextDataLength);

                                if (await ReceiveMessage(true) == CodeNames.OK)
                                {
                                    fileStream.Read(buffer, 0, buffer.Length);

                                    await SendData2(buffer);
                                }
                            }
                            else
                            {
                                var buffer = new byte[bufferLen];

                                var nextDataLength = $"{CodeNames.NextDataLength}|{bufferLen}|";

                                while (nextDataLength.Length < 10)
                                {
                                    nextDataLength += '0';
                                }

                                await SendMessage(nextDataLength);

                                if (await ReceiveMessage(true) == CodeNames.OK)
                                {
                                    fileStream.Read(buffer, 0, buffer.Length);

                                    //Debug.WriteLine(buffer.Length);

                                    await SendData2(buffer);
                                }
                            }

                            SendFileProgress.Invoke(this, new SendProgressEvent 
                            {
                                Value = Map(++i, 0, count, 0, 100), 
                                General = false, 
                                Receive = false 
                            });
                        }
                    }

                    SendFileProgress.Invoke(this, new SendProgressEvent
                    {
                        Value = Map(filesSent, 0, files.Count, 0, 100),
                        General = true,
                        Receive = false
                    });

                    var endMessage = $"{CodeNames.EndFileTransmission}|";

                    while (endMessage.Length < 10)
                    {
                        endMessage += '0';
                    }

                    await SendMessage(endMessage);

                    while (await ReceiveMessage(true) != CodeNames.OK) ;
                }
            }
        }
    }
}
