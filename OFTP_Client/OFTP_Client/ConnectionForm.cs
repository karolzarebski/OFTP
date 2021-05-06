using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class ConnectionForm : Form
    {
        private TcpClient client;
        private NetworkStream stream;
        private bool connected = false;

        public ConnectionForm()
        {
            InitializeComponent();
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

            var buffer = new byte[1];

            await stream.ReadAsync(buffer, 0, 1);

            if (Encoding.UTF8.GetString(buffer) == "1")
            {
                LoginButton.Enabled = true;
                RegisterButton.Enabled = true;
                ConnectButton.Text = "Rozłącz";
                connected = true;
            }
        }

        private void ConnectButton_Click(object sender, EventArgs e)
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

        private async void LoginButton_ClickAsync(object sender, EventArgs e)
        {
            var codeBuffer = new byte[1];

            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            if (!string.IsNullOrWhiteSpace(login) || !string.IsNullOrWhiteSpace(password))
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes($"2|{LoginTextBox.Text}|{PasswordTextBox.Text}"));

                await stream.ReadAsync(codeBuffer, 0, codeBuffer.Length);

                switch (Encoding.UTF8.GetString(codeBuffer))
                {
                    case "5":
                        Array.Clear(codeBuffer, 0, codeBuffer.Length);
                        MessageBox.Show("Zalogowano",
                             "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case "4":
                        MessageBox.Show("Błędne dane logowania\nPodaj nowe i spróbuj ponowne",
                            "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Array.Clear(codeBuffer, 0, codeBuffer.Length);
                        break;
                    default:
                        break;
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
            var codeBuffer = new byte[1];

            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            if (!string.IsNullOrWhiteSpace(login) || !string.IsNullOrWhiteSpace(password))
            {
                await stream.WriteAsync(Encoding.UTF8.GetBytes($"3|{login}|{password}"));

                await stream.ReadAsync(codeBuffer, 0, codeBuffer.Length);

                switch (Encoding.UTF8.GetString(codeBuffer))
                {
                    case "6":
                        MessageBox.Show("Zarejestrowano\nZaloguj się",
                             "OK", MessageBoxButtons.OK, MessageBoxIcon.Information);
                        Array.Clear(codeBuffer, 0, codeBuffer.Length);
                        break;
                    case "7":
                        MessageBox.Show("Błąd rejestracji\nKonto o podanym loginie już istnieje\nPodaj nowe i spróbuj ponowne",
                            "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Array.Clear(codeBuffer, 0, codeBuffer.Length);
                        break;
                    case "8":
                        MessageBox.Show("Błąd rejestracji\nHasło nie spełnia polityki\nPodaj nowe i spróbuj ponowne",
                            "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        Array.Clear(codeBuffer, 0, codeBuffer.Length);
                        break;
                    default:
                        break;
                }
            }
            else
            {
                MessageBox.Show("Dane rejestracyjne nie mogą być puste\nPodaj nowe i spróbuj ponownie", "Puste dane", 
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }
    }
}
