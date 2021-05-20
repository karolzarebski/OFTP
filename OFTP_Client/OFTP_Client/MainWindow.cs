using OFTP_Client.Events;
using OFTP_Client.FilesService;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class MainWindow : Form
    {
        private bool isConnected = false;
        private Dictionary<string, IPAddress> availableUsers = new Dictionary<string, IPAddress>();
        private DictionaryService dictionaryService = new DictionaryService();
        private SendFilesService sendFilesService;
        private ReceiveFilesService receiveFilesService;

        public MainWindow()
        {
            InitializeComponent();
        }

        public void MainWindow_UsersChanged(object sender, UsersListChangedEvent e)
        {
            if (availableUsers.ContainsKey(e.username))
            {
                availableUsers.Remove(e.username);
            }
            else
            {
                availableUsers.Add(e.username, IPAddress.Parse(e.IPAddress));
            }

            Invoke(new Action(() =>
            {

            }));
        }

        private void Logout(object eventArgs)
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
        }

        private void LogoutButton_Click(object sender, EventArgs e)
        {
            Logout(e);
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
                    var filteredUserList = availableUsers.Keys.Where(x => x.ToLower().Contains(text));

                    AvailableUsersLabel.Text = $"Dostępni użytownicy: {filteredUserList.Count()}";

                    UsersListBox.Items.AddRange(filteredUserList.ToArray());
                }
                else
                {
                    UsersListBox.Items.Clear();

                    AvailableUsersLabel.Text = $"Dostępni użytownicy: {availableUsers.Count()}";

                    UsersListBox.Items.AddRange(availableUsers.Keys.ToArray());
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

            availableUsers = dictionaryService.users;

            UsersListBox.Items.AddRange(availableUsers.Keys.ToArray());

            UsersListBox.SelectedIndex = 0;

            AvailableUsersLabel.Text = $"Dostępni użytownicy: {availableUsers.Count}";

            receiveFilesService = new ReceiveFilesService(dictionaryService);
            receiveFilesService.IncommingConnection += ReceiveFilesService_IncommingConnection;

            InitReceiveService();
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
