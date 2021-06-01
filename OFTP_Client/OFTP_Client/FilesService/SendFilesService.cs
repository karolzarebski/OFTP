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
        private int bufferLen = 25599; //100 KB

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

                var code = new byte[3]; //TODO check size
                await _client.GetStream().ReadAsync(code, 0, code.Length);

                byte[] publicKey = new byte[72];
                await _client.GetStream().ReadAsync(publicKey, 0, publicKey.Length);

                var clientPublicKey = _cryptoService.GeneratePublicKey();
                await _client.GetStream().WriteAsync(clientPublicKey);

                byte[] iv = new byte[16];
                await _client.GetStream().ReadAsync(iv, 0, iv.Length);
                _cryptoService.AssignIV(publicKey, iv);

                //foreach (var item in publicKey)
                //{
                //    Debug.Write($"{item}\t");
                //}

                //Debug.WriteLine("");

                //foreach (var item in iv)
                //{
                //    Debug.Write($"{item}\t");
                //}

                //Debug.WriteLine("");

                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Błąd łączenia z klientem\n{ex.Message}", "Błąd połączenia",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        //private async Task SendMessage(string message)
        //{
        //    var encryptedData = await _cryptoService.EncryptData(message);
        //    var encryptedMessage = new byte[encryptedData.Length + 2];
        //    Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);
        //    var len = encryptedData.Length;
        //    Debug.WriteLine(len);
        //    encryptedMessage[0] = (byte)(len / 256);
        //    encryptedMessage[1] = (byte)(len % 256);
        //    await _client.GetStream().WriteAsync(encryptedMessage);
        //} 

        private async Task SendMessage(string message)
        {
            var encryptedData = await _cryptoService.EncryptData(message);
            await _client.GetStream().WriteAsync(encryptedData);

            //var encryptedMessage = new byte[encryptedData.Length + 2];
            //Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);
            //var len = encryptedData.Length;
            //Debug.WriteLine(len);
            //encryptedMessage[0] = (byte)(len / 256);
            //encryptedMessage[1] = (byte)(len % 256);
        }

        private async Task SendData(byte[] data)
        {
            var encryptedData = await _cryptoService.EncryptData(data);

            //Debug.WriteLine($"{encryptedData[0]}\t{encryptedData[1]}");

            var encryptedMessage = new byte[encryptedData.Length + 2];
            Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);
            var len = encryptedData.Length;

            //await Task.Delay(1);

            //encryptedMessage[0] = (byte)((encryptedData.Length + 2) / 256);
            //encryptedMessage[1] = (byte)((encryptedData.Length + 2) % 256);
            encryptedMessage[0] = (byte)(len / 256);
            encryptedMessage[1] = (byte)(len % 256);

            //Debug.WriteLine($"{encryptedMessage[0]}\t{encryptedMessage[1]}\t{encryptedMessage[2]}\t{encryptedMessage[3]}");

            var packetSize = new byte[2];

            packetSize[0] = (byte)((encryptedData.Length + 2) / 256);
            packetSize[1] = (byte)((encryptedData.Length + 2) % 256);
            Debug.WriteLine(encryptedMessage.Length);

            await _client.GetStream().FlushAsync();
            await _client.GetStream().WriteAsync(packetSize);

            await _client.GetStream().FlushAsync();
            await _client.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task SendData2(byte[] data)
        {
            var encryptedData = await _cryptoService.EncryptData(data);
            Debug.WriteLine(encryptedData.Length);
            await _client.GetStream().WriteAsync(encryptedData);
        }

        private async Task<string> ReceiveMessage(bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[16]; //PC - 256
                await _client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);

                //foreach (var item in codeBuffer)
                //{
                //    Debug.Write($"{item}\t");
                //}

                //Debug.WriteLine("");

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
                    FileInfo fi = new FileInfo(file);

                    var fileLength = $"{fi.Name}|{fi.Length}|";

                    using var fileStream = File.OpenRead(file);

                    while (fileLength.Length < 120)
                    {
                        fileLength += '0';
                    }

                    await SendMessage($"{CodeNames.FileLength}|{fileLength}");

                    while (fileStream.Position != fi.Length)
                    {
                        if (await ReceiveMessage(true) == CodeNames.NextPartialData)
                        {
                            if (fi.Length - fileStream.Position < bufferLen)
                            {
                                int len = Convert.ToInt32(fi.Length - fileStream.Position);
                                var buffer = new byte[len];

                                //while (len % 16 != 0)
                                //{
                                //    len++;
                                //}

                                Debug.WriteLine(len);

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

                                    Debug.WriteLine(buffer.Length);

                                    await SendData2(buffer);
                                }

                                //Debug.WriteLine($"{buffer[0]}\t{buffer[1]}");
                            }
                        }
                    }

                    var endMessage = $"{CodeNames.EndFileTransmission}|";

                    while (endMessage.Length < 10)
                    {
                        endMessage += '0';
                    }

                    await SendMessage(endMessage);

                    while (await ReceiveMessage(true) != CodeNames.OK) ;
                }
            }

            /*
            int filesSent = 0;

            await SendMessage($"{CodeNames.BeginFileTransmission}|{files.Count}");

            var responseCode = await ReceiveMessage(true);

            if (responseCode == CodeNames.AcceptFileTransmission)
            {
                foreach (var file in files)
                {
                    FileInfo fi = new FileInfo(file);

                    await SendMessage($"{fi.Name}|{fi.Length}");

                    using var fileStream = File.OpenRead(file);

                    int count = Convert.ToInt32(Math.Ceiling(fi.Length / Convert.ToDouble(bufferLen)));

                    int i = 0;

                    SendFileProgress.Invoke(this, new SendProgressEvent { Value = Map(filesSent++, 0, files.Count, 0, 100), General = true, Receive = false });

                    while (fileStream.Position != fi.Length)
                    {
                        var code = await ReceiveMessage(true);

                        if (code == CodeNames.NextPartialData)
                        {
                            if (fi.Length - fileStream.Position < bufferLen)
                            {
                                int len = Convert.ToInt32(fi.Length - fileStream.Position);
                                var buffer = new byte[len + 2];

                                buffer[0] = (byte)(len / 256);
                                buffer[1] = (byte)(len % 256);

                                fileStream.Read(buffer, 2, buffer.Length - 2);
                                await SendData(buffer);
                            }
                            else
                            {
                                var buffer = new byte[bufferLen + 2];
                                buffer[0] = (byte)(bufferLen / 256);
                                buffer[1] = (byte)(bufferLen % 256);

                                //Debug.WriteLine($"{buffer[0]}\t{buffer[1]}");

                                fileStream.Read(buffer, 2, buffer.Length - 2);
                                await SendData(buffer);
                            }

                            SendFileProgress.Invoke(this, new SendProgressEvent { Value = Map(++i, 0, count, 0, 100), General = false, Receive = false });
                        }
                    }

                    await Task.Delay(1);
                    await SendData(Encoding.UTF8.GetBytes(CodeNames.EndFileTransmission));

                    //await Task.Delay(10);
                    Debug.WriteLine("");
                }

                SendFileProgress.Invoke(this, new SendProgressEvent { Value = Map(filesSent, 0, files.Count, 0, 100), General = true, Receive = false });
            }
            else
            {
                MessageBox.Show("Klient odmówił transmisji plików", "Odmowa", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            */
        }
    }
}
