using System.Net.Sockets;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client.FilesService
{
    public class SendFilesService
    {
        private readonly string _serverIP;
        private TcpClient client;

        public SendFilesService(string ServerIP)
        {
            _serverIP = ServerIP;
        }

        public async Task<bool> Connect()
        {
            try
            {
                await (client = new TcpClient()).ConnectAsync(_serverIP, 12137);
                return true;
            }
            catch (SocketException ex)
            {
                MessageBox.Show($"Błąd łączenia z klientem\n{ex.Message}", "Błąd połączenia",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }
        }

        public void SendFile()
        {

        }
    }
}
