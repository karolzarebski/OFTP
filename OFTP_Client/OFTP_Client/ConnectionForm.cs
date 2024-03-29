﻿using OFTP_Client.Events;
using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class ConnectionForm : Form
    {
        private TcpClient client = new TcpClient();
        private NetworkStream stream;
        private CryptoService _cryptoService = new CryptoService();
        private List<string> availableUsers = new List<string>();
        private List<string> friendsList = new List<string>();
        private bool connected = false, loginSite = true, passwordOk = false, emailOk = false;

        private string serverIpAddress = "192.168.1.14";

        public ConnectionForm()
        {
            InitializeComponent();

            //ServerIpTextBox.Text = "192.168.1.14";
            //LoginTextBox.Text = "karol123";
            //PasswordTextBox.Text = "Ww123456789";

            ConnectToServer();
        }

        private async Task<string> ReceiveMessage()
        {
            var header = new byte[5];
            await client.GetStream().ReadAsync(header, 0, 5);

            var len = header[3] * 256 + header[4];

            if (len != 0)
            {
                var message = new byte[len];
                await client.GetStream().ReadAsync(message, 0, len);

                return $"{Encoding.UTF8.GetString(header.Take(3).ToArray())}|{await _cryptoService.DecryptData(message)}";
            }

            return Encoding.UTF8.GetString(header.Take(3).ToArray());

            //if (isCodeReceived)
            //{
            //    var codeBuffer = new byte[256]; //TODO check length
            //    await client.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
            //    return await clients[client].DecryptData(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
            //}
            //else
            //{
            //    var messageBuffer = new byte[1024];
            //    await client.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
            //    return await clients[client].DecryptData(messageBuffer.Skip(2)
            //            .Take(messageBuffer[0] * 256 + messageBuffer[1]).ToArray());
            //}
        }

        private async Task SendMessage(string code, string message)
        {
            var encryptedData = await _cryptoService.EncryptData(message);
            var encryptedMessage = new byte[encryptedData.Length + 5];
            Array.Copy(encryptedData, 0, encryptedMessage, 5, encryptedData.Length);
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            var len = encryptedData.Length;
            encryptedMessage[3] = (byte)(len / 256);
            encryptedMessage[4] = (byte)(len % 256);
            await stream.WriteAsync(encryptedMessage);
        }

        private async Task SendMessage(string code)
        {
            var encryptedMessage = new byte[5];
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            encryptedMessage[3] = 0;
            encryptedMessage[4] = 0;
            await stream.WriteAsync(encryptedMessage);
        }

        private async void ConnectToServer()
        {
            try
            {
                TryAgainButton.Visible = false;

                await Task.Delay(1000);

                TryAgainButton.Text = "Łączenie";
                TryAgainButton.Enabled = false;

                ServerConnectionLabel.Text = "Stan połączenia: Łączenie";
                ServerConnectionLabel.ForeColor = Color.Orange;

                await (client = new TcpClient()).ConnectAsync(serverIpAddress, 12137);

                ServerConnectionLabel.ForeColor = Color.Green;
                ServerConnectionLabel.Text = "Stan połączenia: Połączono";
            }
            catch (Exception ex)
            {
                TryAgainButton.Text = "Spróbuj ponownie";
                TryAgainButton.Enabled = true;

                ServerConnectionLabel.ForeColor = Color.Red;
                ServerConnectionLabel.Text = "Stan połączenia: Rozłączono";

                TryAgainButton.Visible = true;

                MessageBox.Show($"Błąd podczas łączenia się z serwerem\nTreść błędu: {ex.Message}",
                    "Błąd połączenia", MessageBoxButtons.OK, MessageBoxIcon.Error);

                return;
            }

            stream = client.GetStream();

            var code = new byte[5]; //TODO check size
            await stream.ReadAsync(code, 0, code.Length);

            if (Encoding.UTF8.GetString(code.Take(3).ToArray()) == ServerRequestCodes.Connected)
            {
                LoginButton.Enabled = true;
                RegisterButton.Enabled = true;
                TryAgainButton.Text = "Rozłącz";
                TryAgainButton.Enabled = true;
                connected = true;

                byte[] publicKey = new byte[77];
                await stream.ReadAsync(publicKey, 0, publicKey.Length);

                var clientPublicKey = new byte[77];

                Array.Copy(_cryptoService.GeneratePublicKey(), 0, clientPublicKey, 5, 72);
                Array.Copy(Encoding.UTF8.GetBytes(ServerRequestCodes.DiffieHellmanKey), 0, clientPublicKey, 0, 3);
                clientPublicKey[3] = 0;
                clientPublicKey[4] = 72;
                await client.GetStream().WriteAsync(clientPublicKey);

                byte[] iv = new byte[21];
                await stream.ReadAsync(iv, 0, iv.Length);
                _cryptoService.AssignIV(publicKey.Skip(5).ToArray(), iv.Skip(5).ToArray());
            }
        }

        private void TryAgainButton_Click(object sender, EventArgs e)
        {
            //if (connected)
            //{
            //    await SendMessage(CodeNames.Disconnect);

            //    ServerConnectionLabel.ForeColor = Color.Red;
            //    ServerConnectionLabel.Text = "Stan: Rozłączno";

            //    LoginButton.Enabled = false;
            //    RegisterButton.Enabled = false;
            //    TryAgainButton.Text = "Połącz";
            //    LoginTextBox.Text = "";
            //    PasswordTextBox.Text = "";
            //    connected = false;
            //}
            //else
            //{
            ConnectToServer();
            //}
        }


        private async void LoginButton_ClickAsync(object sender, EventArgs e)
        {
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;

            if (!string.IsNullOrWhiteSpace(login) || !string.IsNullOrWhiteSpace(password))
            {
                await SendMessage(ServerRequestCodes.Login, $"{login}|{password}");
                var message = await ReceiveMessage();

                if (message == ServerRequestCodes.CorrectLoginData)
                {
                    MessageBox.Show("Pomyślnie zalogowano do serwera", "Logowanie",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await SendMessage(ServerRequestCodes.ActiveUsers);

                    var availableUsersCount = (await ReceiveMessage()).Split('|');

                    if (availableUsersCount[0] == ServerRequestCodes.ActiveUsers)
                    {
                        var processedUsersCount = Convert.ToInt32(availableUsersCount[1]);

                        if (processedUsersCount > 0)
                        {
                            await SendMessage(ServerRequestCodes.ActiveUsers);

                            while (processedUsersCount >= 0)
                            {
                                var users = (await ReceiveMessage()).Split('|')[1].Split('\n');

                                foreach (var craftedUser in users)
                                {
                                    availableUsers.Add(craftedUser);
                                }

                                processedUsersCount -= 100; //server sends 100 users in a row
                            }
                        }
                    }

                    await SendMessage(ServerRequestCodes.Friends);

                    var friendsCount = (await ReceiveMessage()).Split('|');

                    if (friendsCount[0] == ServerRequestCodes.Friends)
                    {
                        var processedFriendsCount = Convert.ToInt32(friendsCount[1]);

                        if (processedFriendsCount > 0)
                        {
                            await SendMessage(ServerRequestCodes.Friends);

                            while (processedFriendsCount >= 0)
                            {
                                var friends = (await ReceiveMessage()).Split('|')[1].Split('\n');

                                foreach (var craftedFriend in friends)
                                {
                                    friendsList.Add(craftedFriend);
                                }

                                processedFriendsCount -= 100;
                            }
                        }
                    }

                    InitMainWindow();
                }
                else if (message == ServerRequestCodes.WrongLoginData)
                {
                    MessageBox.Show("Błędne dane logowania\nPodaj nowe i spróbuj ponowne",
                        "Błąd logowania", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (message == ServerRequestCodes.UserAlreadyLoggedIn)
                {
                    MessageBox.Show($"Użytkownik {login} jest już zalogowany", "Błąd logowania",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            string emailAddress = EmailAddressTextBox.Text;

            if (login.Length > 32)
            {
                MessageBox.Show("Login jest zbyt długi\nPodaj nowy i spróbuj ponownie", "Puste dane",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }
            if (Regex.IsMatch(login, "[^a-zA-z0-9]"))
            {
                MessageBox.Show("Login zawiera niedozwolone znaki\nPodaj nowy i spróbuj ponownie", "Puste dane",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);

                return;
            }

            if (!string.IsNullOrWhiteSpace(login) || !string.IsNullOrWhiteSpace(password))
            {
                await SendMessage(ServerRequestCodes.Register, $"{login}|{password}|{emailAddress}");

                var message = await ReceiveMessage();

                if (message == ServerRequestCodes.CorrectRegisterData)
                {
                    MessageBox.Show("Pomyślnie zarejestrowano", "Rejestracja",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                    await SendMessage(ServerRequestCodes.ActiveUsers);

                    var availableUsersCount = (await ReceiveMessage()).Split('|');

                    if (availableUsersCount[0] == ServerRequestCodes.ActiveUsers)
                    {
                        var processedUsersCount = Convert.ToInt32(availableUsersCount[1]);

                        if (processedUsersCount > 0)
                        {
                            await SendMessage(ServerRequestCodes.ActiveUsers);

                            while (processedUsersCount >= 0)
                            {
                                var users = (await ReceiveMessage()).Split('|')[1].Split('\n');

                                foreach (var craftedUser in users)
                                {
                                    availableUsers.Add(craftedUser);
                                }

                                processedUsersCount -= 100; //server sends 100 users in a row
                            }
                        }
                    }

                    await SendMessage(ServerRequestCodes.Friends);

                    var friendsCount = (await ReceiveMessage()).Split('|');

                    if (friendsCount[0] == ServerRequestCodes.Friends)
                    {
                        var processedFriendsCount = Convert.ToInt32(friendsCount[1]);

                        if (processedFriendsCount > 0)
                        {
                            await SendMessage(ServerRequestCodes.Friends);

                            while (processedFriendsCount >= 0)
                            {
                                var friends = (await ReceiveMessage()).Split('|')[1].Split('\n');

                                foreach (var craftedFriend in friends)
                                {
                                    friendsList.Add(craftedFriend);
                                }

                                processedFriendsCount -= 100;
                            }
                        }
                    }

                    InitMainWindow();
                }
                else if (message == RegisterCodes.RegistrationLoginExists)
                {
                    MessageBox.Show("Błąd rejestracji\nKonto o podanym loginie już istnieje\nPodaj nowe i spróbuj ponowne",
                            "Błąd rejestracji", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (message == RegisterCodes.RegistrationEmailExists)
                {
                    MessageBox.Show("Błąd rejestracji\nKonto o podanym adresie email już istnieje\nPodaj nowe i spróbuj ponowne",
                            "Błąd rejestracji", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                else if (message == RegisterCodes.RegistrationPasswordWrong)
                {
                    MessageBox.Show("Błąd rejestracji\nHasło nie spełnia polityki\n" +
                        "Hasło musi składać się z min. 10 znaków, 1 wielka litera, 1 mała litera, 1 cyfra\n" +
                        "Podaj nowe i spróbuj ponowne",
                        "Błąd rejestracji", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
            else
            {
                MessageBox.Show("Dane rejestracyjne nie mogą być puste\nPodaj nowe i spróbuj ponownie", "Puste dane",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void ConnectionForm_Load(object sender, EventArgs e)
        {
            ServerConnectionLabel.ForeColor = Color.Red;

            RepeatPasswordLabel.Visible = false;
            RepeatPasswordTextBox.Visible = false;
            EmailAddressTextBox.Visible = false;
            EmailAddressLabel.Visible = false;

            RegisterButton.Visible = false;

            TryAgainButton.Visible = false;

            Size = new Size(320, 335);
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowPasswordCheckBox.Checked)
            {
                PasswordTextBox.PasswordChar = '\0';
                RepeatPasswordTextBox.PasswordChar = '\0';
            }
            else
            {
                PasswordTextBox.PasswordChar = '*';
                RepeatPasswordTextBox.PasswordChar = '*';
            }
        }

        private void RegisterOrLoginLabel_Click(object sender, EventArgs e)
        {
            if (loginSite)
            {
                loginSite = false;

                RegisterOrLoginLabel.Location = new Point(28, 327);
                RegisterOrLoginLabel.Text = "Zaloguj się";
                LoginButton.Visible = false;
                RegisterButton.Visible = true;
                RegisterButton.Enabled = false;
                RegisterLoginLabel.Visible = true;

                RepeatPasswordLabel.Visible = true;
                RepeatPasswordTextBox.Visible = true;
                EmailAddressTextBox.Visible = true;
                EmailAddressLabel.Visible = true;

                LoginTextBox.TextChanged += LoginTextBox_TextChanged;

                Size = new Size(320, 424);
            }
            else
            {
                loginSite = true;

                RegisterOrLoginLabel.Location = new Point(28, 226);
                RegisterOrLoginLabel.Text = "Zarejestruj się";
                LoginButton.Visible = true;
                RegisterButton.Visible = false;
                RegisterLoginLabel.Visible = false;

                RepeatPasswordTextBox.Visible = false;
                RepeatPasswordLabel.Visible = false;
                EmailAddressTextBox.Visible = false;
                EmailAddressLabel.Visible = false;

                LoginTextBox.TextChanged -= LoginTextBox_TextChanged;

                Size = new Size(320, 335);
            }
        }

        private void LoginTextBox_TextChanged(object sender, EventArgs e)
        {
            string login = LoginTextBox.Text;
            if (login.Length <= 32 && login.Length > 0 && !Regex.IsMatch(login, "[^a-zA-z0-9]"))
            {
                RegisterLoginLabel.BackColor = Color.PaleGreen;
            }
            else
            {
                RegisterLoginLabel.BackColor = Color.Tomato;
            }
        }

        private void RepeatPasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            if (PasswordTextBox.Text != RepeatPasswordTextBox.Text)
            {
                RepeatPasswordTextBox.BackColor = Color.Tomato;
                passwordOk = false;
                RegisterButton.Enabled = false;
            }
            else
            {
                RepeatPasswordTextBox.BackColor = Color.PaleGreen;
                passwordOk = true;

                if (connected && passwordOk && emailOk)
                {
                    RegisterButton.Enabled = true;
                }
            }
        }

        private void InitMainWindow()
        {
            var mainWindow = new MainWindow(client, _cryptoService, availableUsers, friendsList, LoginTextBox.Text);

            mainWindow.SendEmailEvent += MainWindow_SendEmailEvent;

            mainWindow.FormClosing += (sender, e) =>
            {
                ServerConnectionLabel.ForeColor = Color.Red;
                ServerConnectionLabel.Text = "Stan: Rozłączono";

                LoginButton.Enabled = false;
                RegisterButton.Enabled = false;
                TryAgainButton.Enabled = true;
                TryAgainButton.Text = "Połącz";
                TryAgainButton.Visible = true;
                LoginTextBox.Text = "";
                PasswordTextBox.Text = "";
                connected = false;
                mainWindow.SendEmailEvent -= MainWindow_SendEmailEvent;
                Show();
            };

            mainWindow.Show();
            Hide();
        }

        public bool IsEmailCorrect(string email)
        {
            if (email != string.Empty)
            {
                System.Net.Mail.MailAddress eMailValidator = null;

                if (System.Net.Mail.MailAddress.TryCreate(email, out eMailValidator))
                {
                    if (Regex.IsMatch(eMailValidator.Host, "[a-zA-z]+\\.[a-zA-z]+"))
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
                else
                {
                    return false;
                }
            }

            return false;
        }

        private void EmailAddressTextBox_TextChanged(object sender, EventArgs e)
        {
            if (IsEmailCorrect(EmailAddressTextBox.Text))
            {
                EmailAddressTextBox.BackColor = Color.PaleGreen;

                emailOk = true;

                if (connected && passwordOk && emailOk)
                {
                    RegisterButton.Enabled = true;
                }
            }
            else
            {
                EmailAddressTextBox.BackColor = Color.Tomato;
                emailOk = false;
                RegisterButton.Enabled = false;
            }
        }

        private async void MainWindow_SendEmailEvent(object sender, SendEmailEvent e)
        {
            await SendMessage(EmailCodes.SendEmail, $"{e.UnavailableUsername}|{e.Username}");
        }
    }
}
