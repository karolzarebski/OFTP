using OFTP_Client.Events;
using OFTP_Client.FilesService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class MainWindow : Form
    {
        private bool isConnected = false, isLoggedIn = true;
        private List<string> _availableUsers = new List<string>();
        private DictionaryService dictionaryService = new DictionaryService();
        private SendFilesService sendFilesService;
        private ReceiveFilesService receiveFilesService;

        public TcpClient _tcpClient;
        private CryptoService _cryptoService;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public MainWindow(TcpClient tcpClient, CryptoService cryptoService, List<string> availableUsers)
        {
            InitializeComponent();

            _cryptoService = cryptoService;
            _availableUsers = availableUsers;
            _tcpClient = tcpClient;

            var cancellationToken = cancellationTokenSource.Token;

            Task.Run(async () =>
           {
               var buffer = new byte[2048];

               while (isLoggedIn)
               {
                   try
                   {
                       await tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                       if (!cancellationToken.IsCancellationRequested)
                       {
                           var newUser = (await cryptoService.DecryptData(buffer.Skip(1).Take(buffer[0]).ToArray())).Split('|');
                           if (newUser[0] == Resources.CodeNames.NewUser)
                           {
                               UsersChanged(newUser[1]);
                           }

                       }
                   }
                   catch (Exception ex)
                   {
                       if (ex is OperationCanceledException)
                       {
                           isLoggedIn = false;
                           MessageBox.Show("Pomyślnie wylogowano", "Wylogowywanie", 
                               MessageBoxButtons.OK, MessageBoxIcon.Information);
                       }
                       else
                       {
                           throw new Exception(ex.Message);
                       }
                   }
               }
           });
        }

        private async Task<string> ReceiveMessage(bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[256]; //TODO check length
                await _tcpClient.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
                return await _cryptoService.DecryptData(codeBuffer.Skip(1).Take(codeBuffer[0]).ToArray());
            }
            else
            {
                var messageBuffer = new byte[1024];
                await _tcpClient.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
                return await _cryptoService.DecryptData(messageBuffer.Skip(1)
                        .Take(messageBuffer[0]).ToArray());
            }
        }

        private async Task SendMessage(string message)
        {
            var encryptedData = await _cryptoService.EncryptData(message);
            var encryptedMessage = new byte[encryptedData.Length + 1];
            Array.Copy(encryptedData, 0, encryptedMessage, 1, encryptedData.Length);
            encryptedMessage[0] = (byte)encryptedData.Length;
            await _tcpClient.GetStream().WriteAsync(encryptedMessage);
        }

        private void UsersChanged(string userName)
        {
            if (_availableUsers.Contains(userName))
            {
                _availableUsers.Remove(userName);
            }
            else
            {
                _availableUsers.Add(userName);
            }

            UsersListBox.Invoke((MethodInvoker)delegate
            {
                UsersListBox.Items.Clear();
                UsersListBox.Items.AddRange(_availableUsers.ToArray());
            });
        }

        private async void Logout(object eventArgs)
        {
            if (isConnected)
            {
                switch (MessageBox.Show("Połączenie nie zostało zakończone\nCzy chcesz zakończyć połączenie?",
                    "Wylogowywanie", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
                {
                    case DialogResult.Yes:

                        isConnected = false;

                        break;
                    case DialogResult.No:
                        if (eventArgs is FormClosingEventArgs)
                        {
                            (eventArgs as FormClosingEventArgs).Cancel = true;
                        }
                        break;
                }
            }

            await SendMessage(Resources.CodeNames.LogOut);

            cancellationTokenSource.Cancel();
            isLoggedIn = false;
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void FilterUsersTextBox_Enter(object sender, EventArgs e)
        {
            var filterTerm = FilterUsersTextBox.Text;

            if (!string.IsNullOrEmpty(filterTerm) && filterTerm == "Filtruj")
            {
                FilterUsersTextBox.Text = "";
            }
        }

        private void FilterUsersTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FilterUsersTextBox.Text))
            {
                FilterUsersTextBox.Text = "Filtruj";
            }
        }

        private void FilterUsersTextBox_TextChanged(object sender, EventArgs e)
        {
            var text = FilterUsersTextBox.Text.ToLower();

            if (text != "filtruj")
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    UsersListBox.Items.Clear();
                    var filteredUserList = _availableUsers.Where(x => x.ToLower().Contains(text));

                    AvailableUsersLabel.Text = $"Dostępni użytownicy: {filteredUserList.Count()}";

                    UsersListBox.Items.AddRange(filteredUserList.ToArray());
                }
                else
                {
                    UsersListBox.Items.Clear();

                    AvailableUsersLabel.Text = $"Dostępni użytownicy: {_availableUsers.Count()}";

                    UsersListBox.Items.AddRange(_availableUsers.ToArray());
                }
            }
        }

        private void UsersListBox_DoubleClick(object sender, EventArgs e)
        {
            ConnectButton.PerformClick();
        }

        private void UsersListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                ConnectWithUserTextBox.Text = UsersListBox.SelectedItem?.ToString();
            }
        }

        private void MainWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            Logout(e);
        }

        private void MainWindow_Load(object sender, EventArgs e)
        {
            MaximumSize = Size;
            MinimumSize = Size;

            //availableUsers = dictionaryService.users;

            UsersListBox.Items.AddRange(_availableUsers.ToArray());

            //UsersListBox.SelectedIndex = 0;

            AvailableUsersLabel.Text = $"Dostępni użytownicy: {_availableUsers.Count}";

            //receiveFilesService = new ReceiveFilesService(dictionaryService);
            //receiveFilesService.IncommingConnection += ReceiveFilesService_IncommingConnection;

            //InitReceiveService();
        }

        private async void InitReceiveService()
        {
            if (await receiveFilesService.WaitForIncommingConnection())
            {
                IncommingConnectionAccepted();
            }
            else
            {
                InitReceiveService();

                await Task.Delay(2000);
                StateLabel.Text = "Stan: oczekiwanie";
            }
        }

        private void IncommingConnectionAccepted()
        {
            UsersListBox.SelectedItem = dictionaryService.GetKeyByValue(receiveFilesService.IncommingConnectionAddress);

            isConnected = true;
            ConnectButton.Text = "Rozłącz";

            sendFilesService = new SendFilesService(receiveFilesService.IncommingConnectionAddress);
        }

        private void ReceiveFilesService_IncommingConnection(object sender, IncommingConnectionEvent e)
        {
            StateLabel.Text = e.Message;
        }
    }
}
