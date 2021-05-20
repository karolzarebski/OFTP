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
        private DictionaryService _dictionaryService;

        public string IncommingConnectionAddress { get; private set; }
        public event EventHandler<IncommingConnectionEvent> IncommingConnection;

        public ReceiveFilesService(DictionaryService dictionaryService)
        {
            _dictionaryService = dictionaryService;
            receiveFilesServer = new TcpListener(IPAddress.Any, 12137);
            receiveFilesServer.Start();
        }

        public async Task<bool> WaitForIncommingConnection()
        {
            TcpClient client = await receiveFilesServer.AcceptTcpClientAsync();

            byte[] codeBuffer = new byte[3];

            await client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);

            byte[] responseCode;

            if (Encoding.UTF8.GetString(codeBuffer) == "100") //incomming connection code
            {
                IncommingConnection.Invoke(this, new IncommingConnectionEvent { Message = "Stan: Połączenie przychodzące" });

                IncommingConnectionAddress = client.Client.RemoteEndPoint.ToString();

                var clientName = _dictionaryService.GetKeyByValue(IncommingConnectionAddress);

                switch (MessageBox.Show($"Użytkownik {clientName} chce wysłać Ci pliki\nAkceptować?", "Przesyłanie plików",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        IncommingConnection.Invoke(this, new IncommingConnectionEvent { Message = $"Stan: Połączono z {clientName}" });
                        responseCode = Encoding.UTF8.GetBytes("101"); //accepted connection code
                        await client.GetStream().WriteAsync(responseCode);
                        return true;
                    case DialogResult.No:
                        IncommingConnection.Invoke(this, new IncommingConnectionEvent { Message = "Połączenie odrzucone" });
                        responseCode = Encoding.UTF8.GetBytes("102"); //rejected connection code
                        await client.GetStream().WriteAsync(responseCode);
                        return false;
                }
            }
            return false;
        }
    }
}
