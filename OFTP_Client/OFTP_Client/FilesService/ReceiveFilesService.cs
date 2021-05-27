using OFTP_Client.Events;
using System;
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
        private string _ipAddress;

        public string IncommingConnectionAddress { get; private set; }
        public event EventHandler<IncommingConnectionEvent> IncommingConnection;

        public ReceiveFilesService(string ipAddress)
        {
            _ipAddress = ipAddress;
            receiveFilesServer = new TcpListener(IPAddress.Any, 12138);
            receiveFilesServer.Start();
        }

        public async Task<bool> WaitForIncomingConnection()
        {
            var isClientOk = false;

            while (!isClientOk)
            {
                TcpClient client = await receiveFilesServer.AcceptTcpClientAsync();

                if (!client.Client.RemoteEndPoint.ToString().StartsWith(_ipAddress))
                {
                    client.Close();
                    client.Dispose();
                    isClientOk = false;
                }
                else
                {
                    isClientOk = true;

                    await client.GetStream().WriteAsync(Encoding.UTF8.GetBytes(Resources.CodeNames.Connected));

                    var cryptoService = new CryptoService();

                    var publicKey = cryptoService.GeneratePublicKey();
                    await client.GetStream().WriteAsync(publicKey);

                    byte[] clientPublicKey = new byte[72];
                    await client.GetStream().ReadAsync(clientPublicKey, 0, clientPublicKey.Length);

                    await client.GetStream().WriteAsync(cryptoService.GenerateIV(clientPublicKey));

                    //Odbierz nazwę pliku
                    //Wyświetl czy chce taki plik
                    //Zacznij odbieranie pliku
                    //Podziękuj drugiemu użytkownikowi za nowe wirusy <3 ++1
                    //Rozłącz się
                }
            }          
            return false;
        }
    }
}
