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
            this.SuspendLayout();
            // 
            // UsersListBox
            // 
            this.UsersListBox.FormattingEnabled = true;
            this.UsersListBox.ItemHeight = 15;
            this.UsersListBox.Location = new System.Drawing.Point(522, 32);
            this.UsersListBox.Name = "UsersListBox";
            this.UsersListBox.Size = new System.Drawing.Size(266, 364);
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
            this.ConnectButton.Location = new System.Drawing.Point(343, 5);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(161, 23);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Połącz z użytkownikiem";
            this.ConnectButton.UseVisualStyleBackColor = true;
            // 
            // ConnectWithUserTextBox
            // 
            this.ConnectWithUserTextBox.Location = new System.Drawing.Point(70, 6);
            this.ConnectWithUserTextBox.Name = "ConnectWithUserTextBox";
            this.ConnectWithUserTextBox.Size = new System.Drawing.Size(152, 23);
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
            this.LogoutButton.Location = new System.Drawing.Point(240, 5);
            this.LogoutButton.Name = "LogoutButton";
            this.LogoutButton.Size = new System.Drawing.Size(75, 23);
            this.LogoutButton.TabIndex = 5;
            this.LogoutButton.Text = "Wyloguj";
            this.LogoutButton.UseVisualStyleBackColor = true;
            this.LogoutButton.Click += new System.EventHandler(this.LogoutButton_Click);
            // 
            // FilterUsersTextBox
            // 
            this.FilterUsersTextBox.Location = new System.Drawing.Point(522, 403);
            this.FilterUsersTextBox.Name = "FilterUsersTextBox";
            this.FilterUsersTextBox.Size = new System.Drawing.Size(266, 23);
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
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.StateLabel);
            this.Controls.Add(this.FilterUsersTextBox);
            this.Controls.Add(this.LogoutButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.ConnectWithUserTextBox);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.AvailableUsersLabel);
            this.Controls.Add(this.UsersListBox);
            this.Name = "MainWindow";
            this.Text = "FileTransferForm";
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
    }
}