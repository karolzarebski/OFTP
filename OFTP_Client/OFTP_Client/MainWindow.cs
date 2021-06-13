using OFTP_Client.Events;
using OFTP_Client.FilesService;
using OFTP_Client.Resources;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class MainWindow : Form
    {
        private bool isConnected = false, isLoggedIn = true, isPaused = false;
        private List<string> _availableUsers = new List<string>();
        private List<string> _friends = new List<string>();
        private SendFilesService sendFilesService;
        private ReceiveFilesService receiveFilesService;

        private string filePath = "", _loggedInAs = string.Empty;
        private List<string> selectedFilesPath = new List<string>();

        public TcpClient _tcpClient;
        private CryptoService _cryptoService;
        private CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();

        public event EventHandler<SendEmailEvent> SendEmailEvent;

        public MainWindow(TcpClient tcpClient, CryptoService cryptoService, List<string> availableUsers,
             List<string> friends, string loggedInAs)
        {
            InitializeComponent();

            _cryptoService = cryptoService;
            _availableUsers = availableUsers;
            _tcpClient = tcpClient;
            _friends = friends;
            _loggedInAs = loggedInAs;

            LoggedInAsLabel.Text = $"Zalogowano jako: {loggedInAs}";

            var cancellationToken = cancellationTokenSource.Token;

            Task.Run(async () =>
           {
               var buffer = new byte[5];

               bool accepted = false;

               while (isLoggedIn)
               {
                   try
                   {
                       await _tcpClient.GetStream().ReadAsync(buffer, 0, 5, cancellationToken);

                       if (!cancellationToken.IsCancellationRequested)
                       {
                           var msgLength = buffer[3] * 256 + buffer[4];
                           var code = Encoding.UTF8.GetString(buffer.Take(3).ToArray());

                           if (msgLength != 0)
                           {
                               buffer = new byte[msgLength];

                               await _tcpClient.GetStream().ReadAsync(buffer, 0, buffer.Length);

                               var data = (await cryptoService.DecryptData(buffer)).Split('|');
                               var login = string.Empty;

                               if (code == ServerRequestCodes.NewUser)
                               {
                                   login = data[0];
                                   UsersChanged(login);
                               }
                               else if (code == FriendshipCodes.NewFriend)
                               {
                                   FriendsChanged(data[0]);
                               }
                               else if (code == UserConnectionCodes.AskUserForConnection)
                               {
                                   login = data[0];

                                   switch (MessageBox.Show($"Czy chcesz akceptować połączenie od: {login}?", "Połączenie przychodzące",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                                   {
                                       case DialogResult.Yes:
                                           await SendMessage(UserConnectionCodes.AcceptedIncomingConnection);

                                           accepted = true;

                                           StateLabel.Invoke((MethodInvoker)delegate
                                           {
                                               StateLabel.Text = $"Połączono z: {login}"; //don't know if it's correct
                                           });

                                           break;
                                       case DialogResult.No:
                                           accepted = false;

                                           await SendMessage(UserConnectionCodes.RejectedIncomingConnection);
                                           break;
                                   }
                                   if (accepted)
                                   {
                                       string ip = (await ReceiveMessage()).Split('|')[2];

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

                                           receiveFilesService.SendFileProgressEvent -= SendFilesService_SendFileProgress;

                                           receiveFilesService.Dispose();
                                       }
                                   }
                               }
                               else if (code == UserConnectionCodes.AcceptedIncomingConnection)
                               {
                                   login = data[0];

                                   StateLabel.Invoke((MethodInvoker)delegate
                                   {
                                       StateLabel.Text = $"Połączono z: {login}"; //don't know if it's correct
                                   });

                                   string ip = data[1];

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
                               else if (code == FriendshipCodes.AskForFriendship)
                               {
                                   switch (MessageBox.Show($"Czy chcesz dodać {data[0]} do listy znajomych?", "Nowy znajomy",
                                       MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                                   {
                                       case DialogResult.Yes:
                                           await SendMessage(FriendshipCodes.AddToFriendsAccepted);

                                           break;
                                       case DialogResult.No:
                                           await SendMessage(FriendshipCodes.AddToFriendsRejected);

                                           break;
                                   }
                               }
                               else if (code == FriendshipCodes.AddToFriendsAccepted)
                               {
                                   MessageBox.Show("Pomyślnie dodano użytkownika do znajomych", "Nowy znajomy",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                               }
                           }
                           else
                           {
                               if (code == UserConnectionCodes.RejectedIncomingConnection)
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
                               else if (code == FriendshipCodes.AddToFriendsRejected)
                               {
                                   MessageBox.Show("Użytkownik odmówił znjomości", "Nowy znajomy odrzucony",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                               }
                               else if (code == EmailCodes.SendEmailSuccess)
                               {
                                   MessageBox.Show("Email został pomyślnie wysłany", "Powodzenie",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
                               }
                               else if (code == EmailCodes.SendEmailFailure)
                               {
                                   MessageBox.Show("Wystąpił błąd podczas próby wysłania Emaila", "Błąd",
                                       MessageBoxButtons.OK, MessageBoxIcon.Information);
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

                        if (e.Value == 100)
                        {
                            MessageBox.Show("Pomyślnie odebrano pliki", "Transfer zakończony",
                                MessageBoxButtons.OK, MessageBoxIcon.Information);
                        }
                    }
                    else
                    {
                        GeneralProgressLabel.Text = $"Wysłano plików: {(e.Value * selectedFilesPath.Count) / 100}/{selectedFilesPath.Count}";

                        if (e.Value == 100)
                        {
                            Task.Run(() => MessageBox.Show("Pomyślnie wysłano pliki", "Transfer zakończony",
                                MessageBoxButtons.OK, MessageBoxIcon.Information));

                            selectedFilesPath.Clear();
                        }
                    }
                }
            });
        }

        private async Task<string> ReceiveMessage()
        {
            var header = new byte[5];
            await _tcpClient.GetStream().ReadAsync(header, 0, 5);

            var len = header[3] * 256 + header[4];

            if (len != 0)
            {
                var message = new byte[len];
                await _tcpClient.GetStream().ReadAsync(message, 0, len);

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
            await _tcpClient.GetStream().WriteAsync(encryptedMessage);
        }

        private async Task SendMessage(string code)
        {
            var encryptedMessage = new byte[5];
            Array.Copy(Encoding.UTF8.GetBytes(code), 0, encryptedMessage, 0, 3);
            encryptedMessage[3] = 0;
            encryptedMessage[4] = 0;
            await _tcpClient.GetStream().WriteAsync(encryptedMessage);
        }

        private void FriendsChanged(string friendName)
        {
            if (_friends.Contains(friendName))
            {
                _friends.Remove(friendName);

                MessageBox.Show($"Usunięto {friendName} z listy znajomych", "Znajomy usunięty",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                _friends.Add(friendName);
            }

            FriendsListBox.Invoke((MethodInvoker)delegate
            {
                FriendsCountLabel.Text = $"Znajomi: {_friends.Count}";
                FriendsListBox.Items.Clear();
                FriendsListBox.Items.AddRange(_friends.ToArray());
            });
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
                AvailableUsersLabel.Text = $"Dostępni użytkownicy: {_availableUsers.Count}";
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

            await SendMessage(ServerRequestCodes.LogOut);

            cancellationTokenSource.Cancel();

            _tcpClient.Close();

            isLoggedIn = false;

            UsersListBox.Items.Clear();
            _availableUsers.Clear();
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
            var selectedUser = UsersListBox.SelectedItem;

            if (selectedUser != null)
            {
                if (!isConnected)
                {
                    ConnectWithUserTextBox.Text = selectedUser.ToString();
                }

                if (_friends.Contains(selectedUser))
                {
                    FriendsListBox.SelectedItem = selectedUser;

                    AddOrRemoveFriendButton.Text = "Usuń znajomego";
                }
                else
                {
                    AddOrRemoveFriendButton.Text = "Dodaj znajomego";
                }

                AddOrRemoveFriendButton.Enabled = true;
            }
            else
            {
                AddOrRemoveFriendButton.Enabled = false;
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
            FriendsListBox.Items.AddRange(_friends.ToArray());

            AvailableUsersLabel.Text = $"Dostępni użytownicy: {_availableUsers.Count}";
            FriendsCountLabel.Text = $"Znajomi: {_friends.Count}";
        }

        private async void ConnectButton_Click(object sender, EventArgs e)
        {
            if (!isConnected)
            {
                var selectedUser = UsersListBox.SelectedItem;

                if (selectedUser != null)
                {
                    isConnected = true;
                    await SendMessage(UserConnectionCodes.AskUserForConnection, selectedUser.ToString());
                    StateLabel.Text = $"Stan: Oczekiwanie na akceptację od {UsersListBox.SelectedItem}";
                    ConnectButton.Text = "Rozłącz";
                }
                else if (FriendsListBox.SelectedItem != null)
                {
                    var selectedFriend = FriendsListBox.SelectedItem;

                    if (!_availableUsers.Contains(selectedFriend))
                    {
                        FriendIsUnavailable(selectedFriend.ToString());
                    }
                }
                else
                {
                    MessageBox.Show("Wybierz użytkownika z listy obok", "Brak użytkownika",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            else
            {
                await sendFilesService.SendEndConnection();

                sendFilesService.SendFileProgress -= SendFilesService_SendFileProgress;

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

        private async void FilesTreeView_AfterCheck(object sender, TreeViewEventArgs e)
        {
            string path = filePath.Remove(filePath.LastIndexOf('\\') + 1) + e.Node.FullPath;

            SendButton.Enabled = true;

            await Task.Run(() =>
            {
                FilesTreeView.Invoke((MethodInvoker)delegate
                    {
                        if (Directory.Exists(path))
                        {
                            if (e.Node.Checked)
                            {
                                e.Node.Expand();

                                foreach (TreeNode i in e.Node.Nodes)
                                {
                                    if (!i.Checked)
                                        i.Checked = true;
                                }
                            }
                            else
                            {
                                e.Node.Collapse();

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
                    });
            });
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

        private void FilterFriendsTextBox_TextChanged(object sender, EventArgs e)
        {
            var text = FilterFriendsTextBox.Text.ToLower();

            if (text != "filtruj")
            {
                if (!string.IsNullOrWhiteSpace(text))
                {
                    FriendsListBox.Items.Clear();
                    var filteredUserList = _friends.Where(x => x.ToLower().Contains(text));

                    FriendsCountLabel.Text = $"Znajomi: {filteredUserList.Count()}";

                    FriendsListBox.Items.AddRange(filteredUserList.ToArray());
                }
                else
                {
                    FriendsListBox.Items.Clear();

                    FriendsCountLabel.Text = $"Znajomi: {_friends.Count()}";

                    FriendsListBox.Items.AddRange(_friends.ToArray());

                }
            }
        }

        private void FriendsListBox_DoubleClick(object sender, EventArgs e)
        {
            var selectedFriend = FriendsListBox.SelectedItem;

            if (selectedFriend != null)
            {
                if (_availableUsers.Contains(FriendsListBox.SelectedItem))
                {
                    ConnectButton.PerformClick();
                }
                else
                {
                    FriendIsUnavailable(selectedFriend.ToString());
                }
            }
        }

        private void FriendIsUnavailable(string unavailableUsername)
        {
            switch (MessageBox.Show($"{FriendsListBox.SelectedItem} nie jest dostępny\nCzy chcesz wysłać powiadomienie?",
                "Brak użytkownika", MessageBoxButtons.YesNo, MessageBoxIcon.Information))
            {
                case DialogResult.Yes:
                    SendEmailEvent.Invoke(this, new SendEmailEvent
                    {
                        UnavailableUsername = unavailableUsername,
                        Username = _loggedInAs
                    });
                    break;
                case DialogResult.No:
                    break;
            }
        }

        private void FriendsListBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            var selectedFriend = FriendsListBox.SelectedItem;

            if (selectedFriend != null)
            {
                if (_availableUsers.Contains(selectedFriend))
                {
                    UsersListBox.SelectedItem = selectedFriend;
                }

                if (_friends.Contains(FriendsListBox.SelectedItem))
                {
                    AddOrRemoveFriendButton.Text = "Usuń znajomego";
                }

                AddOrRemoveFriendButton.Enabled = true;
            }
            else
            {
                AddOrRemoveFriendButton.Enabled = false;
            }
        }

        private async void AddOrRemoveFriendButton_Click(object sender, EventArgs e)
        {
            var selectedUser = UsersListBox.SelectedItem;
            var selectedFriend = FriendsListBox.SelectedItem;

            if (selectedUser != null)
            {
                if (_friends.Contains(selectedUser))
                {
                    switch (MessageBox.Show($"Czy na pewno chcesz usunąć {selectedUser} z listy znajomych?",
                        "Potwierdzanie usuwania", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                    {
                        case DialogResult.Yes:
                            await SendMessage(FriendshipCodes.RemoveFriend, selectedUser.ToString());
                            break;
                        case DialogResult.No:
                            break;
                    }
                }
                else
                {
                    await SendMessage(FriendshipCodes.AskForFriendship, selectedUser.ToString());
                }
            }
            else
            {
                switch (MessageBox.Show($"Czy na pewno chcesz usunąć {selectedFriend} z listy znajomych?",
                    "Potwierdzanie usuwania", MessageBoxButtons.YesNo, MessageBoxIcon.Question))
                {
                    case DialogResult.Yes:
                        await SendMessage(FriendshipCodes.RemoveFriend, selectedFriend.ToString());
                        break;
                    case DialogResult.No:
                        break;
                }
            }
        }

        private void FilterFriendsTextBox_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(FilterFriendsTextBox.Text))
            {
                FilterFriendsTextBox.Text = "Filtruj";
            }
        }

        private void FilterFriendsTextBox_Enter(object sender, EventArgs e)
        {
            var filterTerm = FilterFriendsTextBox.Text;

            if (!string.IsNullOrEmpty(filterTerm) && filterTerm == "Filtruj")
            {
                FilterFriendsTextBox.Text = "";
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
                switch (MessageBox.Show("Przerwanie transmisji możliwe tylko podczas jej trwania\nCzy chcesz wznowić transmisję?",
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
