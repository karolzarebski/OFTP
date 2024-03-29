﻿
namespace OFTP_Client
{
    partial class MainWindow
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.UsersListBox = new System.Windows.Forms.ListBox();
            this.AvailableUsersLabel = new System.Windows.Forms.Label();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.ConnectWithUserTextBox = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.LogoutButton = new System.Windows.Forms.Button();
            this.FilterUsersTextBox = new System.Windows.Forms.TextBox();
            this.StateLabel = new System.Windows.Forms.Label();
            this.LoggedInAsLabel = new System.Windows.Forms.Label();
            this.FilesTreeView = new System.Windows.Forms.TreeView();
            this.SendButton = new System.Windows.Forms.Button();
            this.SendFileProgressBar = new System.Windows.Forms.ProgressBar();
            this.SendFileProgressLabel = new System.Windows.Forms.Label();
            this.GeneralProgressBar = new System.Windows.Forms.ProgressBar();
            this.GeneralProgressLabel = new System.Windows.Forms.Label();
            this.ChooseDir = new System.Windows.Forms.Button();
            this.PauseButton = new System.Windows.Forms.Button();
            this.StopButton = new System.Windows.Forms.Button();
            this.FriendsListBox = new System.Windows.Forms.ListBox();
            this.FilterFriendsTextBox = new System.Windows.Forms.TextBox();
            this.FriendsCountLabel = new System.Windows.Forms.Label();
            this.AddOrRemoveFriendButton = new System.Windows.Forms.Button();
            this.UserEncryptionCheckBox = new System.Windows.Forms.CheckBox();
            this.ModifyAccountButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UsersListBox
            // 
            this.UsersListBox.FormattingEnabled = true;
            this.UsersListBox.ItemHeight = 15;
            this.UsersListBox.Location = new System.Drawing.Point(522, 32);
            this.UsersListBox.Name = "UsersListBox";
            this.UsersListBox.Size = new System.Drawing.Size(213, 379);
            this.UsersListBox.TabIndex = 0;
            this.UsersListBox.SelectedIndexChanged += new System.EventHandler(this.UsersListBox_SelectedIndexChanged);
            this.UsersListBox.DoubleClick += new System.EventHandler(this.UsersListBox_DoubleClick);
            // 
            // AvailableUsersLabel
            // 
            this.AvailableUsersLabel.AutoSize = true;
            this.AvailableUsersLabel.Location = new System.Drawing.Point(522, 13);
            this.AvailableUsersLabel.Name = "AvailableUsersLabel";
            this.AvailableUsersLabel.Size = new System.Drawing.Size(125, 15);
            this.AvailableUsersLabel.TabIndex = 1;
            this.AvailableUsersLabel.Text = "Aktywni użytkownicy: ";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(396, 128);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(120, 23);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Połącz";
            this.ConnectButton.UseVisualStyleBackColor = true;
            this.ConnectButton.Click += new System.EventHandler(this.ConnectButton_Click);
            // 
            // ConnectWithUserTextBox
            // 
            this.ConnectWithUserTextBox.Location = new System.Drawing.Point(70, 6);
            this.ConnectWithUserTextBox.Name = "ConnectWithUserTextBox";
            this.ConnectWithUserTextBox.ReadOnly = true;
            this.ConnectWithUserTextBox.Size = new System.Drawing.Size(270, 23);
            this.ConnectWithUserTextBox.TabIndex = 3;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(12, 9);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(52, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "Połącz z:";
            // 
            // LogoutButton
            // 
            this.LogoutButton.Location = new System.Drawing.Point(364, 6);
            this.LogoutButton.Name = "LogoutButton";
            this.LogoutButton.Size = new System.Drawing.Size(140, 23);
            this.LogoutButton.TabIndex = 5;
            this.LogoutButton.Text = "Wyloguj";
            this.LogoutButton.UseVisualStyleBackColor = true;
            this.LogoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
            // 
            // FilterUsersTextBox
            // 
            this.FilterUsersTextBox.Location = new System.Drawing.Point(522, 427);
            this.FilterUsersTextBox.Name = "FilterUsersTextBox";
            this.FilterUsersTextBox.Size = new System.Drawing.Size(213, 23);
            this.FilterUsersTextBox.TabIndex = 6;
            this.FilterUsersTextBox.Text = "Filtruj";
            this.FilterUsersTextBox.TextChanged += new System.EventHandler(this.FilterUsersTextBox_TextChanged);
            this.FilterUsersTextBox.Enter += new System.EventHandler(this.FilterUsersTextBox_Enter);
            this.FilterUsersTextBox.Leave += new System.EventHandler(this.FilterUsersTextBox_Leave);
            // 
            // StateLabel
            // 
            this.StateLabel.AutoSize = true;
            this.StateLabel.Location = new System.Drawing.Point(12, 47);
            this.StateLabel.Name = "StateLabel";
            this.StateLabel.Size = new System.Drawing.Size(102, 15);
            this.StateLabel.TabIndex = 7;
            this.StateLabel.Text = "Stan: Oczekiwanie";
            // 
            // LoggedInAsLabel
            // 
            this.LoggedInAsLabel.AutoSize = true;
            this.LoggedInAsLabel.Location = new System.Drawing.Point(13, 75);
            this.LoggedInAsLabel.Name = "LoggedInAsLabel";
            this.LoggedInAsLabel.Size = new System.Drawing.Size(101, 15);
            this.LoggedInAsLabel.TabIndex = 8;
            this.LoggedInAsLabel.Text = "Zalogowano jako:";
            // 
            // FilesTreeView
            // 
            this.FilesTreeView.CheckBoxes = true;
            this.FilesTreeView.Location = new System.Drawing.Point(13, 105);
            this.FilesTreeView.Name = "FilesTreeView";
            this.FilesTreeView.Size = new System.Drawing.Size(374, 291);
            this.FilesTreeView.TabIndex = 9;
            this.FilesTreeView.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.FilesTreeView_AfterCheck);
            // 
            // SendButton
            // 
            this.SendButton.Enabled = false;
            this.SendButton.Location = new System.Drawing.Point(396, 241);
            this.SendButton.Name = "SendButton";
            this.SendButton.Size = new System.Drawing.Size(120, 23);
            this.SendButton.TabIndex = 10;
            this.SendButton.Text = "Wyślij pliki";
            this.SendButton.UseVisualStyleBackColor = true;
            this.SendButton.Click += new System.EventHandler(this.SendButton_Click);
            // 
            // SendFileProgressBar
            // 
            this.SendFileProgressBar.Location = new System.Drawing.Point(99, 408);
            this.SendFileProgressBar.Name = "SendFileProgressBar";
            this.SendFileProgressBar.Size = new System.Drawing.Size(405, 18);
            this.SendFileProgressBar.TabIndex = 11;
            // 
            // SendFileProgressLabel
            // 
            this.SendFileProgressLabel.AutoSize = true;
            this.SendFileProgressLabel.Location = new System.Drawing.Point(15, 408);
            this.SendFileProgressLabel.Name = "SendFileProgressLabel";
            this.SendFileProgressLabel.Size = new System.Drawing.Size(49, 15);
            this.SendFileProgressLabel.TabIndex = 12;
            this.SendFileProgressLabel.Text = "Postęp: ";
            // 
            // GeneralProgressBar
            // 
            this.GeneralProgressBar.Location = new System.Drawing.Point(159, 432);
            this.GeneralProgressBar.Name = "GeneralProgressBar";
            this.GeneralProgressBar.Size = new System.Drawing.Size(345, 18);
            this.GeneralProgressBar.TabIndex = 13;
            // 
            // GeneralProgressLabel
            // 
            this.GeneralProgressLabel.AutoSize = true;
            this.GeneralProgressLabel.Location = new System.Drawing.Point(15, 435);
            this.GeneralProgressLabel.Name = "GeneralProgressLabel";
            this.GeneralProgressLabel.Size = new System.Drawing.Size(96, 15);
            this.GeneralProgressLabel.TabIndex = 14;
            this.GeneralProgressLabel.Text = "Wysłano plików: ";
            // 
            // ChooseDir
            // 
            this.ChooseDir.Location = new System.Drawing.Point(396, 191);
            this.ChooseDir.Name = "ChooseDir";
            this.ChooseDir.Size = new System.Drawing.Size(120, 23);
            this.ChooseDir.TabIndex = 15;
            this.ChooseDir.Text = "Wybór folderu";
            this.ChooseDir.UseVisualStyleBackColor = true;
            this.ChooseDir.Click += new System.EventHandler(this.ChooseDir_Click);
            // 
            // PauseButton
            // 
            this.PauseButton.Enabled = false;
            this.PauseButton.Location = new System.Drawing.Point(396, 298);
            this.PauseButton.Name = "PauseButton";
            this.PauseButton.Size = new System.Drawing.Size(120, 23);
            this.PauseButton.TabIndex = 16;
            this.PauseButton.Text = "Pauza";
            this.PauseButton.UseVisualStyleBackColor = true;
            this.PauseButton.Click += new System.EventHandler(this.PauseButton_Click);
            // 
            // StopButton
            // 
            this.StopButton.Enabled = false;
            this.StopButton.Location = new System.Drawing.Point(396, 327);
            this.StopButton.Name = "StopButton";
            this.StopButton.Size = new System.Drawing.Size(120, 23);
            this.StopButton.TabIndex = 17;
            this.StopButton.Text = "Stop";
            this.StopButton.UseVisualStyleBackColor = true;
            this.StopButton.Click += new System.EventHandler(this.StopButton_Click);
            // 
            // FriendsListBox
            // 
            this.FriendsListBox.FormattingEnabled = true;
            this.FriendsListBox.ItemHeight = 15;
            this.FriendsListBox.Location = new System.Drawing.Point(755, 32);
            this.FriendsListBox.Name = "FriendsListBox";
            this.FriendsListBox.Size = new System.Drawing.Size(213, 379);
            this.FriendsListBox.TabIndex = 18;
            this.FriendsListBox.SelectedIndexChanged += new System.EventHandler(this.FriendsListBox_SelectedIndexChanged);
            this.FriendsListBox.DoubleClick += new System.EventHandler(this.FriendsListBox_DoubleClick);
            // 
            // FilterFriendsTextBox
            // 
            this.FilterFriendsTextBox.Location = new System.Drawing.Point(755, 427);
            this.FilterFriendsTextBox.Name = "FilterFriendsTextBox";
            this.FilterFriendsTextBox.Size = new System.Drawing.Size(213, 23);
            this.FilterFriendsTextBox.TabIndex = 19;
            this.FilterFriendsTextBox.Text = "Filtruj";
            this.FilterFriendsTextBox.TextChanged += new System.EventHandler(this.FilterFriendsTextBox_TextChanged);
            this.FilterFriendsTextBox.Enter += new System.EventHandler(this.FilterFriendsTextBox_Enter);
            this.FilterFriendsTextBox.Leave += new System.EventHandler(this.FilterFriendsTextBox_Leave);
            // 
            // FriendsCountLabel
            // 
            this.FriendsCountLabel.AutoSize = true;
            this.FriendsCountLabel.Location = new System.Drawing.Point(755, 13);
            this.FriendsCountLabel.Name = "FriendsCountLabel";
            this.FriendsCountLabel.Size = new System.Drawing.Size(51, 15);
            this.FriendsCountLabel.TabIndex = 20;
            this.FriendsCountLabel.Text = "Znajomi";
            // 
            // AddOrRemoveFriendButton
            // 
            this.AddOrRemoveFriendButton.Location = new System.Drawing.Point(364, 32);
            this.AddOrRemoveFriendButton.Name = "AddOrRemoveFriendButton";
            this.AddOrRemoveFriendButton.Size = new System.Drawing.Size(140, 23);
            this.AddOrRemoveFriendButton.TabIndex = 21;
            this.AddOrRemoveFriendButton.Text = "Dodaj znajomego";
            this.AddOrRemoveFriendButton.UseVisualStyleBackColor = true;
            this.AddOrRemoveFriendButton.Click += new System.EventHandler(this.AddOrRemoveFriendButton_Click);
            // 
            // UserEncryptionCheckBox
            // 
            this.UserEncryptionCheckBox.AutoSize = true;
            this.UserEncryptionCheckBox.Checked = true;
            this.UserEncryptionCheckBox.CheckState = System.Windows.Forms.CheckState.Checked;
            this.UserEncryptionCheckBox.Location = new System.Drawing.Point(348, 74);
            this.UserEncryptionCheckBox.Name = "UserEncryptionCheckBox";
            this.UserEncryptionCheckBox.Size = new System.Drawing.Size(113, 19);
            this.UserEncryptionCheckBox.TabIndex = 22;
            this.UserEncryptionCheckBox.Text = "Użyj szyfrowania";
            this.UserEncryptionCheckBox.UseVisualStyleBackColor = true;
            this.UserEncryptionCheckBox.CheckedChanged += new System.EventHandler(this.UserEncryptionCheckBox_CheckedChanged);
            // 
            // ModifyAccountButton
            // 
            this.ModifyAccountButton.Location = new System.Drawing.Point(396, 372);
            this.ModifyAccountButton.Name = "ModifyAccountButton";
            this.ModifyAccountButton.Size = new System.Drawing.Size(120, 23);
            this.ModifyAccountButton.TabIndex = 23;
            this.ModifyAccountButton.Text = "Modyfikuj konto";
            this.ModifyAccountButton.UseVisualStyleBackColor = true;
            this.ModifyAccountButton.Click += new System.EventHandler(this.ModifyAccountButton_Click);
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(998, 460);
            this.Controls.Add(this.ModifyAccountButton);
            this.Controls.Add(this.UserEncryptionCheckBox);
            this.Controls.Add(this.AddOrRemoveFriendButton);
            this.Controls.Add(this.FriendsCountLabel);
            this.Controls.Add(this.FilterFriendsTextBox);
            this.Controls.Add(this.FriendsListBox);
            this.Controls.Add(this.StopButton);
            this.Controls.Add(this.PauseButton);
            this.Controls.Add(this.ChooseDir);
            this.Controls.Add(this.GeneralProgressLabel);
            this.Controls.Add(this.GeneralProgressBar);
            this.Controls.Add(this.SendFileProgressLabel);
            this.Controls.Add(this.SendFileProgressBar);
            this.Controls.Add(this.SendButton);
            this.Controls.Add(this.FilesTreeView);
            this.Controls.Add(this.LoggedInAsLabel);
            this.Controls.Add(this.StateLabel);
            this.Controls.Add(this.FilterUsersTextBox);
            this.Controls.Add(this.LogoutButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ConnectWithUserTextBox);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.AvailableUsersLabel);
            this.Controls.Add(this.UsersListBox);
            this.Name = "MainWindow";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "OFTP";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.MainWindow_FormClosing);
            this.Load += new System.EventHandler(this.MainWindow_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox UsersListBox;
        private System.Windows.Forms.Label AvailableUsersLabel;
        private System.Windows.Forms.Button ConnectButton;
        private System.Windows.Forms.TextBox ConnectWithUserTextBox;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button LogoutButton;
        private System.Windows.Forms.TextBox FilterUsersTextBox;
        private System.Windows.Forms.Label StateLabel;
        private System.Windows.Forms.Label LoggedInAsLabel;
        private System.Windows.Forms.TreeView FilesTreeView;
        private System.Windows.Forms.Button SendButton;
        private System.Windows.Forms.ProgressBar SendFileProgressBar;
        private System.Windows.Forms.Label SendFileProgressLabel;
        private System.Windows.Forms.ProgressBar GeneralProgressBar;
        private System.Windows.Forms.Label GeneralProgressLabel;
        private System.Windows.Forms.Button ChooseDir;
        private System.Windows.Forms.Button PauseButton;
        private System.Windows.Forms.Button StopButton;
        private System.Windows.Forms.ListBox FriendsListBox;
        private System.Windows.Forms.TextBox FilterFriendsTextBox;
        private System.Windows.Forms.Label FriendsCountLabel;
        private System.Windows.Forms.Button AddOrRemoveFriendButton;
        private System.Windows.Forms.CheckBox UserEncryptionCheckBox;
        private System.Windows.Forms.Button ModifyAccountButton;
    }
}