using OFTP_Client.Events;
using OFTP_Client.Resources;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client.FilesService
{
    public class ReceiveFilesService
    {
        private TcpListener receiveFilesServer;
        private TcpClient _client;
        private CryptoService _cryptoService;
        private string _ipAddress;
        private int fileCount;
        private int bufferLen = 102400; //50 KB

        public string IncommingConnectionAddress { get; private set; }
        public event EventHandler<IncommingConnectionEvent> IncommingConnection;

        public ReceiveFilesService(string ipAddress)
        {
            _ipAddress = ipAddress;
            receiveFilesServer = new TcpListener(IPAddress.Any, 12138);
            receiveFilesServer.Start();
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

        private async Task<byte[]> ReceiveData()
        {
            var len = new byte[2];
            await _client.GetStream().ReadAsync(len, 0, 2);

            int bufLen = len[0] * 256 + len[1];

            var codeBuffer = new byte[bufLen]; //TODO check length // I think we done it :P
            await _client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
            return await _cryptoService.DecryptDataB(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
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

                    await _client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(CodeNames.Connected));

                    _cryptoService = new CryptoService();

                    var publicKey = _cryptoService.GeneratePublicKey();
                    await _client.GetStream().WriteAsync(publicKey);

                    byte[] clientPublicKey = new byte[72];
                    await _client.GetStream().ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

                    await _client.GetStream().WriteAsync(_cryptoService.GenerateIV(clientPublicKey));

                    return true;
                    //Odbierz nazwę pliku
                    //Wyświetl czy chce taki plik
                    //Zacznij odbieranie pliku
                    //Podziękuj drugiemu użytkownikowi za nowe wirusy <3 ++1
                    //Rozłącz się
                }
            }          
            return false;
        }

        public async Task<bool> AcceptFiles()
        {
            var response = (await ReceiveMessage(true)).Split("|");

            if(response[0] == CodeNames.BeginFileTransmission)
            {
                switch(MessageBox.Show($"Czy chcesz odebrać {response[1]} plików?", "Odbiór plików",
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

            return false;
        }

        public async Task ReceiveFiles()
        {
            for(int i = 0; i < fileCount; i++)
            {
                var fileInfo = (await ReceiveMessage(true)).Split("|");

                //int fileLen = Convert.ToInt32(fileInfo[1]);
                //int receivedDataLen = 0;

                using FileStream fs = File.Create(fileInfo[0]);
                while (true)
                {
                    await SendMessage(CodeNames.NextPartialData);

                    byte[] partialData = await ReceiveData();

                    var dataToWrite = partialData.Skip(2).Take(partialData[0] * 256 + partialData[1]).ToArray();

                    var code = Encoding.UTF8.GetString(dataToWrite);

                    if (code != CodeNames.EndFileTransmission)
                    {
                        fs.Write(dataToWrite);
                        //receivedDataLen += partialData[0] * 256 + partialData[1];
                    }
                    else
                    {
                        break;
                    }

                    //Array.Copy(partialData, 0, receivedData, receivedDataLen, partialData.Length);
                }
                fs.Flush();
                MessageBox.Show($"Odebrano plik {fileInfo[0]}");
            }
        }
    }
}
