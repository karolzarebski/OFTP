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

        public void SendFile()
        {

        }
    }
}
