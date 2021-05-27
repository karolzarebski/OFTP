using OFTP_Client.Events;
using OFTP_Client.FilesService;
using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
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

        public MainWindow(TcpClient tcpClient, CryptoService cryptoService, List<string> availableUsers, string loggedInAs)
        {
            InitializeComponent();

            _cryptoService = cryptoService;
            _availableUsers = availableUsers;
            _tcpClient = tcpClient;

            LoggedInAsLabel.Text = $"Zalogowano jako: {loggedInAs}";

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
                           var data = (await cryptoService.DecryptData(buffer.Skip(2).Take(buffer[0] * 256 + buffer[1]).ToArray())).Split('|');
                           if (data[0] == CodeNames.NewUser)
                           {
                               UsersChanged(data[1]);
                           }
                           else if (data[0] == CodeNames.AskUserForConnection)
                           {
                               switch (MessageBox.Show($"Czy chcesz akceptować połączenie od: {data[1]}?", "Połączenie przychodzące",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                               {
                                   case DialogResult.Yes:
                                       await SendMessage(CodeNames.AcceptedIncomingConnection);

                                       break;
                                   case DialogResult.No:
                                       await SendMessage(CodeNames.RejectedIncomingConnection);
                                       break;
                               }
                               string ip = await ReceiveMessage();

                               receiveFilesService = new ReceiveFilesService(ip);

                               await receiveFilesService.WaitForIncomingConnection(); //FIX THIS SHIT
                           }
                           else if (data[0] == CodeNames.AcceptedIncomingConnection)
                           {
                               string ip = await ReceiveMessage();

                               sendFilesService = new SendFilesService(ip);

                               if(await sendFilesService.Connect())
                               {
                                   using (OpenFileDialog openFileDialog = new OpenFileDialog())
                                   {
                                       openFileDialog.InitialDirectory = "c:\\";
                                       openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                                       openFileDialog.FilterIndex = 2;
                                       openFileDialog.RestoreDirectory = true;

                                       if (openFileDialog.ShowDialog() == DialogResult.OK)
                                       {
                                           //Get the path of specified file
                                           var filePath = openFileDialog.FileName;

                                           //Read the contents of the file into a stream
                                           //var fileStream = openFileDialog.OpenFile();

                                           //using (StreamReader reader = new StreamReader(fileStream))
                                           //{
                                           //   fileContent = reader.ReadToEnd();
                                           //}
                                       }
                                   }
                               }
                               else
                               {
                                   MessageBox.Show("Wystąpił błąd podczas łącznia klientem", "Błąd połączenia",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                               }
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
        
        private async Task SendMessage(string message)
        {
            var encryptedData = await _cryptoService.EncryptData(message);
            var encryptedMessage = new byte[encryptedData.Length + 2];
            Array.Copy(encryptedData, 0, encryptedMessage, 2, encryptedData.Length);
            var len = encryptedData.Length;

            encryptedMessage[0] = (byte)(len / 256);
            encryptedMessage[1] = (byte)(len  %  256);
            await _tcpClient.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task<string> ReceiveMessage(bool isCodeReceived = false)
        {
            if (isCodeReceived)
            {
                var codeBuffer = new byte[256]; //TODO check length
                await _tcpClient.GetStream().ReadAsync(codeBuffer, 0, codeBuffer.Length);
                return await _cryptoService.DecryptData(codeBuffer.Skip(2).Take(codeBuffer[0] * 256 + codeBuffer[1]).ToArray());
            }
            else
            {
                var messageBuffer = new byte[1024];
                await _tcpClient.GetStream().ReadAsync(messageBuffer, 0, messageBuffer.Length);
                return await _cryptoService.DecryptData(messageBuffer.Skip(2)
                        .Take(messageBuffer[0] * 256 + messageBuffer[1]).ToArray());
            }
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

            await SendMessage(CodeNames.LogOut);

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

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            await SendMessage($"{CodeNames.AskUserForConnection}");
            await SendMessage($"{UsersListBox.SelectedItem}");
            StateLabel.Text = $"Stan: Oczekiwanie na akceptację od {UsersListBox.SelectedItem}";
        }

        private void ReceiveFilesService_IncommingConnection(object sender, IncommingConnectionEvent e)
        {
            StateLabel.Text = e.Message;
        }
    }
}
