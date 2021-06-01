using OFTP_Client.Events;
using OFTP_Client.FilesService;
using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class MainWindow : Form
    {
        private bool isConnected = false, isLoggedIn = true, isPaused = false;
        private List<string> _availableUsers = new List<string>();
        private SendFilesService sendFilesService;
        private ReceiveFilesService receiveFilesService;

        private string filePath = "";
        private List<string> selectedFilesPath = new List<string>();

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

               bool accepted = false;

               while (isLoggedIn)
               {
                   try
                   {
                       await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length, cancellationToken);

                       if (!cancellationToken.IsCancellationRequested)
                       {
                           var data = (await cryptoService.DecryptData(buffer.Skip(2).Take(buffer[0] * 256 + buffer[1]).ToArray())).Split('|');
                           var login = string.Empty;

                           if (data[0] == CodeNames.NewUser)
                           {
                               login = data[1];
                               UsersChanged(login);
                           }
                           else if (data[0] == CodeNames.AskUserForConnection)
                           {
                               login = data[1];

                               switch (MessageBox.Show($"Czy chcesz akceptować połączenie od: {login}?", "Połączenie przychodzące",
                                   MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                               {
                                   case DialogResult.Yes:
                                       await SendMessage(CodeNames.AcceptedIncomingConnection);

                                       accepted = true;

                                       StateLabel.Invoke((MethodInvoker)delegate
                                       {
                                           StateLabel.Text = $"Połączono z: {login}"; //don't know if it's correct
                                       });

                                       break;
                                   case DialogResult.No:
                                       accepted = false;

                                       await SendMessage(CodeNames.RejectedIncomingConnection);
                                       break;
                               }
                               if (accepted)
                               {
                                   string ip = await ReceiveMessage();

                                   receiveFilesService = new ReceiveFilesService(ip);

                                   receiveFilesService.SendFileProgressEvent += SendFilesService_SendFileProgress;

                                   if (await receiveFilesService.WaitForIncomingConnection())
                                   {
                                       while (await receiveFilesService.AcceptFiles())
                                       {
                                           if (!await receiveFilesService.ReceiveFiles())
                                           {
                                               break;
                                           }
                                       }

                                       StateLabel.Invoke((MethodInvoker)delegate
                                       {
                                           StateLabel.Text = "Stan: Oczekiwanie";
                                           GeneralProgressBar.Value = 0;
                                           GeneralProgressLabel.Text = "Otrzymano plików: ";
                                           SendFileProgressBar.Value = 0;
                                           SendFileProgressLabel.Text = "Postęp: ";
                                       });

                                       receiveFilesService.Dispose();
                                   }
                               }
                           }
                           else if (data[0] == CodeNames.AcceptedIncomingConnection)
                           {
                               login = data[1];

                               StateLabel.Invoke((MethodInvoker)delegate
                               {
                                   StateLabel.Text = $"Połączono z: {login}"; //don't know if it's correct
                               });

                               string ip = await ReceiveMessage();

                               sendFilesService = new SendFilesService(ip);
                               sendFilesService.SendFileProgress += SendFilesService_SendFileProgress;

                               if (await sendFilesService.Connect())
                               {
                                   isConnected = true;

                                   filePath = string.Empty;

                                   var t = new Thread(() =>
                                   {
                                       FolderBrowserDialog fbd = new FolderBrowserDialog();
                                       fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                                       fbd.ShowNewFolderButton = true;
                                       if (fbd.ShowDialog() == DialogResult.Cancel)
                                           return;

                                       filePath = fbd.SelectedPath;
                                   });

                                   t.SetApartmentState(ApartmentState.STA);
                                   t.Start();
                                   t.Join();

                                   if (filePath != string.Empty)
                                   {
                                       FilesTreeView.Invoke((MethodInvoker)delegate
                                       {
                                           FilesTreeView.Nodes.Clear();
                                           DirectoryInfo di = new DirectoryInfo(filePath);
                                           TreeNode tds = FilesTreeView.Nodes.Add(di.Name);

                                           tds.Tag = di.FullName;
                                           tds.StateImageIndex = 0;
                                           LoadFiles(filePath, tds);
                                           LoadSubDirectories(filePath, tds);
                                       });
                                   }
                               }
                               else
                               {
                                   isConnected = false;
                                   SendButton.Enabled = false;
                                   MessageBox.Show("Wystąpił błąd podczas łącznia klientem", "Błąd połączenia",
                                       MessageBoxButtons.OK, MessageBoxIcon.Error);
                               }
                           }
                           else if (data[0] == CodeNames.RejectedIncomingConnection)
                           {
                               SendButton.Enabled = false;
                               MessageBox.Show("Klient odmówił połączenia", "Odmowa połączenia",
                                   MessageBoxButtons.OK, MessageBoxIcon.Information);

                               isConnected = false;

                               ConnectButton.Invoke((MethodInvoker)delegate
                               {
                                   ConnectButton.Text = "Połącz z użytkownikiem";
                                   StateLabel.Text = "Stan: Oczekiwanie";
                               });
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

        private void SendFilesService_SendFileProgress(object sender, SendProgressEvent e)
        {
            SendFileProgressBar.Invoke((MethodInvoker)delegate
            {
                if (!e.General)
                {
                    SendFileProgressBar.Value = e.Value;
                    SendFileProgressLabel.Text = $"Postęp: {e.Value} %";
                }
                else
                {
                    GeneralProgressBar.Value = e.Value;

                    if (e.Receive)
                    {
                        GeneralProgressLabel.Text = $"Otrzymano plików: {(e.Value * e.FilesCount) / 100}/{e.FilesCount}";
                        if (e.Value == e.FilesCount)
                        {
                            MessageBox.Show("Pomyślnie odebrano pliki", "Transfer zakończony", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            receiveFilesService.SendFileProgressEvent -= SendFilesService_SendFileProgress;
                        }
                    }
                    else
                    {
                        GeneralProgressLabel.Text = $"Wysłano plików: {(e.Value * selectedFilesPath.Count) / 100}/{selectedFilesPath.Count}";

                        if (e.Value == selectedFilesPath.Count)
                        {
                            MessageBox.Show("Pomyślnie wysłano pliki", "Transfer zakończony", 
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                            sendFilesService.SendFileProgress -= SendFilesService_SendFileProgress;

                            selectedFilesPath.Clear();
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
            encryptedMessage[1] = (byte)(len % 256);
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
            _tcpClient.Close();

            _tcpClient.Dispose();
            isLoggedIn = false;

            UsersListBox.Items.Clear();
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
            if (UsersListBox.SelectedItem != null)
            {
                ConnectButton.PerformClick();
            }
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

            UsersListBox.Items.AddRange(_availableUsers.ToArray());

            AvailableUsersLabel.Text = $"Dostępni użytownicy: {_availableUsers.Count}";
        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                isConnected = true;
                await SendMessage($"{CodeNames.AskUserForConnection}");
                await SendMessage($"{UsersListBox.SelectedItem}");
                StateLabel.Text = $"Stan: Oczekiwanie na akceptację od {UsersListBox.SelectedItem}";
                ConnectButton.Text = "Rozłącz z użytkownikiem";
            }
            else
            {
                await sendFilesService.SendEndConnection();

                isConnected = false;
                ConnectButton.Text = "Połącz z użytkownikiem";
                StateLabel.Text = "Stan: Oczekiwanie";

                GeneralProgressBar.Value = 0;
                SendFileProgressBar.Value = 0;

                GeneralProgressLabel.Text = "Wysłano plików: ";
                SendFileProgressLabel.Text = "Postęp: ";

                selectedFilesPath.Clear();
            }
        }

        private void FilesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            string path = filePath.Remove(filePath.LastIndexOf('\\') + 1) + e.Node.FullPath;

            SendButton.Enabled = true;

            if (Directory.Exists(path))
            {
                if (e.Node.Checked)
                {
                    foreach (TreeNode i in e.Node.Nodes)
                    {
                        if (!i.Checked)
                            i.Checked = true;
                    }
                }
                else
                {
                    foreach (TreeNode i in e.Node.Nodes)
                    {
                        if (i.Checked)
                            i.Checked = false;
                    }
                }
            }
            else
            {
                if (e.Node.Checked)
                {
                    selectedFilesPath.Add(path);
                }
                else
                {
                    selectedFilesPath.Remove(path);
                }
            }
        }

        private async void SendButton_Click(object sender, EventArgs e)
        {
            string allFiles = "";

            foreach (var i in selectedFilesPath)
            {
                allFiles += i + "\r\n";
            }

            MessageBox.Show("Wybrane pliki : \r\n" + allFiles, "Wybrane pliki", MessageBoxButtons.OK, MessageBoxIcon.Information);

            //SendButton.Enabled = false;
            //FilesTreeView.Nodes.Clear();

            var isClientConnected = false;

            await Task.Run(async () => isClientConnected = await sendFilesService.SendFiles(selectedFilesPath));

            if (!isClientConnected)
            {
                isConnected = false;
                ConnectButton.Text = "Połącz z użytkownikiem";
                StateLabel.Text = "Stan: Oczekiwanie";

                GeneralProgressBar.Value = 0;
                SendFileProgressBar.Value = 0;

                GeneralProgressLabel.Text = "Wysłano plików: ";
                SendFileProgressLabel.Text = "Postęp: ";
            }
        }

        private void LoadFiles(string dir, TreeNode td)
        {
            string[] Files = Directory.GetFiles(dir, "*.*");

            foreach (string file in Files)
            {
                FileInfo fi = new FileInfo(file);
                TreeNode tds = td.Nodes.Add(fi.Name);
                tds.Tag = fi.FullName;
                tds.StateImageIndex = 1;
            }
        }

        private void LoadSubDirectories(string dir, TreeNode td)
        {
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            foreach (string subdirectory in subdirectoryEntries)
            {
                DirectoryInfo di = new DirectoryInfo(subdirectory);
                TreeNode tds = td.Nodes.Add(di.Name);
                tds.StateImageIndex = 0;
                tds.Tag = di.FullName;
                LoadFiles(subdirectory, tds);
                LoadSubDirectories(subdirectory, tds);
            }
        }

        private void ChooseDir_Click(object sender, EventArgs e)
        {
            selectedFilesPath.Clear();

            filePath = string.Empty;

            var t = new Thread(() =>
            {
                FolderBrowserDialog fbd = new FolderBrowserDialog();
                fbd.RootFolder = Environment.SpecialFolder.MyComputer;
                fbd.ShowNewFolderButton = true;
                if (fbd.ShowDialog() == DialogResult.Cancel)
                    return;

                filePath = fbd.SelectedPath;
            });

            t.SetApartmentState(ApartmentState.STA);
            t.Start();
            t.Join();

            if (filePath != string.Empty)
            {
                FilesTreeView.Nodes.Clear();
                DirectoryInfo di = new DirectoryInfo(filePath);

                FilesTreeView.Invoke((MethodInvoker)delegate
                {
                    TreeNode tds = FilesTreeView.Nodes.Add(di.Name);

                    tds.Tag = di.FullName;
                    tds.StateImageIndex = 0;
                    LoadFiles(filePath, tds);
                    LoadSubDirectories(filePath, tds);
                });
            }
        }

        private void PauseButton_Click(object sender, EventArgs e)
        {
            if (receiveFilesService != null)
            {
                receiveFilesService.PauseReceiving();
            }
            else
            {
                sendFilesService.PauseSending();
            }

            if (!isPaused)
            {
                isPaused = true;
                PauseButton.Text = "Wznów";
                MessageBox.Show("Przesyłanie plików wstrzymane", "Pauza",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                isPaused = false;
                PauseButton.Text = "Pauza";
                MessageBox.Show("Przesyłanie plików wznowione", "Wznów",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void StopButton_Click(object sender, EventArgs e)
        {
            if (!isPaused)
            {
                switch (MessageBox.Show("Czy chcesz przerwać transmisję plików?", "Stop",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:

                        selectedFilesPath.Clear();

                        if (receiveFilesService != null)
                        {
                            receiveFilesService.StopReceiving();
                        }
                        else
                        {
                            sendFilesService.StopSending();
                        }

                        MessageBox.Show("Wysyłanie plików zostało pomyślnie przerwane", "Stop",
                            MessageBoxButtons.OK, MessageBoxIcon.Information);
                        break;
                    case DialogResult.No:
                        break;
                }
            }
            else
            {
                switch(MessageBox.Show("Przerwanie transmisji możliwe tylko podczas jej trwania\nCzy chcesz wznowić transmisję?",
                    "Przerywanie transmisji", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        PauseButton.PerformClick();
                        break;
                    case DialogResult.No:
                        break;
                }
            }
        }

        private void ReceiveFilesService_IncommingConnection(object sender, IncommingConnectionEvent e)
        {
            StateLabel.Text = e.Message;
        }
    }
}
