using OFTP_Client.Events;
using OFTP_Client.Resources;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client.FilesService
{
    public class ReceiveFilesService : IDisposable
    {
        private TcpListener receiveFilesServer;
        private TcpClient _client;
        private CryptoService _cryptoService;
        private string _ipAddress;
        private int fileCount;
        private bool isPaused = false, isStopped = false;

        public string IncommingConnectionAddress { get; private set; }
        public event EventHandler<IncommingConnectionEvent> IncommingConnection;
        public event EventHandler<SendProgressEvent> SendFileProgressEvent;

        public ReceiveFilesService(string ipAddress)
        {
            _ipAddress = ipAddress;
            receiveFilesServer = new TcpListener(IPAddress.Any, 12138);
            receiveFilesServer.Start();
        }

        private async Task<string> ReceiveMessage()
        {
            var header = new byte[5];
            await _client.GetStream().ReadAsync(header, 0, 5);

            var len = header[3] * 256 + header[4];

            if (len != 0)
            {
                var message = new byte[len];
                await _client.GetStream().ReadAsync(message, 0, len);

                return $"{Encoding.UTF8.GetString(header.Take(3).ToArray())}|{await _cryptoService.DecryptData(message)}";
            }

            return Encoding.UTF8.GetString(header.Take(3).ToArray());
        }

        private async Task SendMessage(string code)
        {
            var encryptedMessage = new byte[5];
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            encryptedMessage[3] = 0;
            encryptedMessage[4] = 0;
            await _client.GetStream().WriteAsync(encryptedMessage);
        }

        //private async Task<string> ReceiveMessage2(int bytesToRead, bool isCodeReceived = false)
        //{
        //    if (isCodeReceived)
        //    {
        //        var codeBuffer = new byte[bytesToRead]; //TODO check length
        //        await _client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
        //        return await _cryptoService.DecryptData(codeBuffer);
        //    }
        //    else
        //    {
        //        var messageBuffer = new byte[1024];
        //        await _client.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
        //        return await _cryptoService.DecryptData(messageBuffer);
        //    }
        //}

        //private async Task SendMessage(string message)
        //{
        //    var encryptedData = await _cryptoService.EncryptData(message);
        //    await _client.GetStream().WriteAsync(encryptedData);
        //}

        private async Task<byte[]> ReceiveData2(int length)
        {
            var codeBuffer = new byte[length];

            await _client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);

            return await _cryptoService.DecryptDataB(codeBuffer);
        }

        private int Map(long x, long in_min, long in_max, long out_min, long out_max)
        {
            return Convert.ToInt32((x - in_min) * (out_max - out_min) / (in_max - in_min) + out_min);
        }

        public async Task<bool> WaitForIncomingConnection()
        {
            var isClientOk = false;

            while (!isClientOk)
            {
                _client = await receiveFilesServer.AcceptTcpClientAsync();

                if (!_client.Client.RemoteEndPoint.ToString().StartsWith(_ipAddress))
                {
                    _client.Close();
                    _client.Dispose();
                    isClientOk = false;
                }
                else
                {
                    isClientOk = true;

                    await _client.GetStream().WriteAsync(Encoding.UTF8.GetBytes($"{CodeNames.Connected}00"));

                    _cryptoService = new CryptoService();

                    var publicKey = _cryptoService.GeneratePublicKey();

                    var diffieHellmanMessage = new byte[publicKey.Length + 5];

                    Array.Copy(publicKey, 0, diffieHellmanMessage, 5, publicKey.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(CodeNames.DiffieHellmanKey), 0, diffieHellmanMessage, 0, 3);
                    diffieHellmanMessage[3] = 0;
                    diffieHellmanMessage[4] = 72;
                    await _client.GetStream().WriteAsync(diffieHellmanMessage);

                    byte[] clientPublicKey = new byte[77];
                    await _client.GetStream().ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

                    diffieHellmanMessage = new byte[21];

                    var iv = _cryptoService.GenerateIV(clientPublicKey.Skip(5).ToArray());

                    Array.Copy(iv, 0, diffieHellmanMessage, 5, iv.Length);
                    Array.Copy(Encoding.UTF8.GetBytes(CodeNames.DiffieHellmanIV), 0, diffieHellmanMessage, 0, 3);
                    diffieHellmanMessage[3] = 0;
                    diffieHellmanMessage[4] = 16;

                    await _client.GetStream().WriteAsync(diffieHellmanMessage);

                    return true;
                }
            }
            return false;
        }

        public async Task<bool> AcceptFiles()
        {
            var response = (await ReceiveMessage()).Split("|");

            if (response[0] == CodeNames.BeginFileTransmission)
            {
                switch (MessageBox.Show($"Czy chcesz odebrać {response[1]} plików?", "Odbiór plików",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        await SendMessage(CodeNames.AcceptFileTransmission);
                        fileCount = Convert.ToInt32(response[1]);
                        return true;
                    case DialogResult.No:
                        await SendMessage(CodeNames.RejectFileTransmission);
                        return false;
                }
            }
            if (response[0] == CodeNames.DisconnectFromClient)
            {
                return false;
            }

            return false;
        }

        public async Task<bool> ReceiveFiles()
        {
            try
            {
                var filePath = string.Empty;

                var t = new Thread(() =>
                {
                    FolderBrowserDialog fbd = new FolderBrowserDialog();
                    fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                    fbd.ShowNewFolderButton = true;
                    if (fbd.ShowDialog() == DialogResult.Cancel)
                        return;

                    filePath = fbd.SelectedPath;
                });

                t.SetApartmentState(ApartmentState.STA);
                t.Start();
                t.Join();

                for (int i = 0; i < fileCount; i++)
                {
                    var fileInfo = (await ReceiveMessage()).Split("|");

                    if (fileInfo[0] == CodeNames.FileLength)
                    {
                        int fileLen = Convert.ToInt32(fileInfo[2]);
                        int receivedDataLen = 0;

                        var fileDestination = Path.Combine(filePath, fileInfo[1]);

                        using FileStream fs = File.Create(fileDestination); //Need to check

                        SendFileProgressEvent.Invoke(this, new SendProgressEvent
                        {
                            Value = Map(i, 0, fileCount, 0, 100),
                            General = true,
                            Receive = true,
                            FilesCount = fileCount
                        });

                        while (true)
                        {
                            if (isStopped)
                            {
                                await SendMessage(CodeNames.FileTransmissionInterrupted);

                                return false;
                            }

                            if (!isPaused)
                            {
                                await SendMessage(CodeNames.NextPartialData);

                                var len = (await ReceiveMessage()).Split('|');

                                if (len[0] == CodeNames.NextDataLength)
                                {
                                    int realLength = Convert.ToInt32(len[1]);

                                    while (realLength % 16 != 0)
                                    {
                                        realLength++;
                                    }

                                    await SendMessage(CodeNames.OK);

                                    byte[] partialData = await ReceiveData2(realLength);

                                    fs.Write(partialData.Take(Convert.ToInt32(len[1])).ToArray());
                                    receivedDataLen += Convert.ToInt32(len[1]);

                                    SendFileProgressEvent.Invoke(this, new SendProgressEvent
                                    {
                                        Value = Map(receivedDataLen, 0, fileLen, 0, 100),
                                        General = false,
                                        Receive = true
                                    });
                                }
                                else if (len[0] == CodeNames.EndFileTransmission)
                                {
                                    break;
                                }
                                else if (len[0] == CodeNames.FileTransmissionInterrupted)
                                {
                                    MessageBox.Show("Klient przerwał transmisję plików", "Stop",
                                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                                    return false;
                                }
                            }
                        }

                        fs.Flush();

                        await SendMessage(CodeNames.OK);
                    }

                }

                SendFileProgressEvent.Invoke(this, new SendProgressEvent
                {
                    Value = Map(fileCount, 0, fileCount, 0, 100),
                    General = true,
                    Receive = true,
                    FilesCount = fileCount
                });

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Wystąpił błąd podczas transmisji plików", "Błąd transmisji",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                return false;
            }
        }

        public void Dispose()
        {
            receiveFilesServer.Stop();
            _client.Close();
        }

        public void PauseReceiving()
        {
            isPaused = !isPaused;
        }

        public void StopReceiving()
        {
            isStopped = true;
        }
    }
}
