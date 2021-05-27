using OFTP_Client.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class ConnectionForm : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private CryptoService _cryptoService = new CryptoService();
        private List<string> availableUsers = new List<string>();
        private bool connected = false, isLoggedIn = false;

        public ConnectionForm()
        {
            InitializeComponent();

            ServerIpTextBox.Text = "127.0.0.1";
            LoginTextBox.Text = "karol123";
            PasswordTextBox.Text = "Ww123456789";
        }

        private IPAddress IsServerConfigurationCorrect(string ipAddress)
        {
            try
            {
                return IPAddress.Parse(ipAddress);
            }
            catch
            {
                return default;
            }
        }

        private async Task<string> ReceiveMessage(bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[256]; //TODO check length
                await stream.ReadAsync(codeBuffer, 0, codeBuffer.Length);
                return await _cryptoService.DecryptData(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
            }
            else
            {
                var messageBuffer = new byte[1024];
                await stream.ReadAsync(messageBuffer, 0, messageBuffer.Length);
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
            await stream.WriteAsync(encryptedMessage);
        }

        private async void ConnectToServer(IPAddress ipAddress)
        {
            try
            {
                await (client = new TcpClient()).ConnectAsync(ipAddress, 12137);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Błąd podczas łączenia się z serwerem\nTreść błędu: {ex.Message}",
                    "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            stream = client.GetStream();

            var code = new byte[3]; //TODO check size
            await stream.ReadAsync(code, 0, code.Length);

            if (Encoding.UTF8.GetString(code) == Resources.CodeNames.Connected)
            {
                LoginButton.Enabled = true;
                RegisterButton.Enabled = true;
                ConnectButton.Text = "Rozłącz";
                connected = true;

                byte[] publicKey = new byte[72];
                await stream.ReadAsync(publicKey, 0, publicKey.Length);

                var clientPublicKey = _cryptoService.GeneratePublicKey();
                await stream.WriteAsync(clientPublicKey);

                byte[] iv = new byte[16];
                await stream.ReadAsync(iv, 0, iv.Length);
                _cryptoService.AssignIV(publicKey, iv);
            }
        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            if (connected)
            {
                await SendMessage(Resources.CodeNames.Disconnect);
                LoginButton.Enabled = false;
                RegisterButton.Enabled = false;
                ConnectButton.Text = "Połącz";
                LoginTextBox.Text = "";
                PasswordTextBox.Text = "";
                connected = false;
            }
            else
            {
                var serwerIP = IsServerConfigurationCorrect(ServerIpTextBox.Text);

                if (serwerIP != default)
                {
                    ConnectToServer(serwerIP);
                }
                else
                {
                    MessageBox.Show("Niepoprawny adres IP serwera\nPodaj inny i spróbuj ponownie", "Błędny adres IP",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }
        

        private async void LoginButton_ClickAsync(object sender, EventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            if (!string.IsNullOrWhiteSpace(login) || !string.IsNullOrWhiteSpace(password))
            {
                await SendMessage($"{Resources.CodeNames.Login}|{login}|{password}");
                var message = await ReceiveMessage(true);

                if (message == Resources.CodeNames.CorrectLoginData)
                {
                    MessageBox.Show("Zalogowano", "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);

                    var availableUsersCount = (await ReceiveMessage(false)).Split('|');

                    if (availableUsersCount[0] == Resources.CodeNames.ActiveUsers)
                    {
                        var processedUsersCount = Convert.ToInt32(availableUsersCount[1]);

                        await SendMessage(Resources.CodeNames.ActiveUsers);

                        while (processedUsersCount >= 0)
                        {
                            var users = (await ReceiveMessage(false)).Split('\n');

                            foreach (var craftedUser in users)
                            {
                                availableUsers.Add(craftedUser);
                            }

                            processedUsersCount -= 100; //server sends 100 users in a row
                        }

                    }

                    InitMainWindow();
                }
                else if (message == Resources.CodeNames.WrongLoginData)
                {
                    MessageBox.Show("Błędne dane logowania\nPodaj nowe i spróbuj ponowne",
                        "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Dane do logowania nie mogą być puste\nPodaj nowe i spróbuj ponownie", "Puste dane",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private async void RegisterButton_Click(object sender, EventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            if (!string.IsNullOrWhiteSpace(login) || !string.IsNullOrWhiteSpace(password))
            {
                await SendMessage($"{Resources.CodeNames.Register}|{login}|{password}");

                var message = await ReceiveMessage(true);
                if (message == Resources.CodeNames.CorrectRegisterData)
                {
                    MessageBox.Show("Pomyślnie zarejestrowano", "Rejestracja",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    InitMainWindow();
                }
                else if (message == Resources.CodeNames.RegistrationLoginExists)
                {
                    MessageBox.Show("Błąd rejestracji\nKonto o podanym loginie już istnieje\nPodaj nowe i spróbuj ponowne",
                            "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (message == Resources.CodeNames.RegistrationPasswordWrong)
                {
                    MessageBox.Show("Błąd rejestracji\nHasło nie spełnia polityki\nPodaj nowe i spróbuj ponowne",
                        "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Dane rejestracyjne nie mogą być puste\nPodaj nowe i spróbuj ponownie", "Puste dane",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void InitMainWindow()
        {
            isLoggedIn = true;
            var mainWindow = new MainWindow(client, _cryptoService, availableUsers);

            mainWindow.FormClosing += (sender, e) =>
            {

                Show();
                LoginButton.Enabled = false;
                RegisterButton.Enabled = false;
                ConnectButton.Text = "Połącz";
                LoginTextBox.Text = "";
                PasswordTextBox.Text = "";
                connected = false;
            };

            mainWindow.Show();
            Hide();
        }
    }
}
