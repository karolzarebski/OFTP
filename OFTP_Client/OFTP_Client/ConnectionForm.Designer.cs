
namespace OFTP_Client
{
    partial class ConnectionForm
    {
        /// <summary>
        ///  Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        ///  Clean up any resources being used.
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
        ///  Required method for Designer support - do not modify
        ///  the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.LoginButton = new System.Windows.Forms.Button();
            this.LoginTextBox = new System.Windows.Forms.TextBox();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.TryAgainButton = new System.Windows.Forms.Button();
            this.RegisterButton = new System.Windows.Forms.Button();
            this.ServerConnectionLabel = new System.Windows.Forms.Label();
            this.ShowPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this.RegisterOrLoginLabel = new System.Windows.Forms.Label();
            this.RepeatPasswordTextBox = new System.Windows.Forms.TextBox();
            this.RepeatPasswordLabel = new System.Windows.Forms.Label();
            this.EmailAddressLabel = new System.Windows.Forms.Label();
            this.EmailAddressTextBox = new System.Windows.Forms.TextBox();
            this.RegisterLoginLabel = new System.Windows.Forms.Label();
            this.SuspendLayout();
            // 
            // LoginButton
            // 
            this.LoginButton.Enabled = false;
            this.LoginButton.Location = new System.Drawing.Point(49, 250);
            this.LoginButton.Name = "LoginButton";
            this.LoginButton.Size = new System.Drawing.Size(212, 23);
            this.LoginButton.TabIndex = 0;
            this.LoginButton.Text = "Zaloguj się";
            this.LoginButton.UseVisualStyleBackColor = true;
            this.LoginButton.Click += new System.EventHandler(this.LoginButton_ClickAsync);
            // 
            // LoginTextBox
            // 
            this.LoginTextBox.Location = new System.Drawing.Point(28, 152);
            this.LoginTextBox.Name = "LoginTextBox";
            this.LoginTextBox.Size = new System.Drawing.Size(249, 23);
            this.LoginTextBox.TabIndex = 1;
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(28, 200);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(249, 23);
            this.PasswordTextBox.TabIndex = 2;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(28, 134);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(42, 15);
            this.label1.TabIndex = 3;
            this.label1.Text = "LOGIN";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(28, 182);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 15);
            this.label2.TabIndex = 4;
            this.label2.Text = "HASŁO";
            // 
            // TryAgainButton
            // 
            this.TryAgainButton.Location = new System.Drawing.Point(95, 81);
            this.TryAgainButton.Name = "TryAgainButton";
            this.TryAgainButton.Size = new System.Drawing.Size(133, 23);
            this.TryAgainButton.TabIndex = 7;
            this.TryAgainButton.Text = "Spróbuj ponownie";
            this.TryAgainButton.UseVisualStyleBackColor = true;
            this.TryAgainButton.Click += new System.EventHandler(this.TryAgainButton_Click);
            // 
            // RegisterButton
            // 
            this.RegisterButton.Enabled = false;
            this.RegisterButton.Location = new System.Drawing.Point(49, 346);
            this.RegisterButton.Name = "RegisterButton";
            this.RegisterButton.Size = new System.Drawing.Size(212, 23);
            this.RegisterButton.TabIndex = 8;
            this.RegisterButton.Text = "Zarejestruj się";
            this.RegisterButton.UseVisualStyleBackColor = true;
            this.RegisterButton.Click += new System.EventHandler(this.RegisterButton_Click);
            // 
            // ServerConnectionLabel
            // 
            this.ServerConnectionLabel.AutoSize = true;
            this.ServerConnectionLabel.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.ServerConnectionLabel.Location = new System.Drawing.Point(58, 46);
            this.ServerConnectionLabel.Name = "ServerConnectionLabel";
            this.ServerConnectionLabel.Size = new System.Drawing.Size(204, 21);
            this.ServerConnectionLabel.TabIndex = 9;
            this.ServerConnectionLabel.Text = "Stan połączenia: Rozłączono";
            // 
            // ShowPasswordCheckBox
            // 
            this.ShowPasswordCheckBox.AutoSize = true;
            this.ShowPasswordCheckBox.Location = new System.Drawing.Point(173, 178);
            this.ShowPasswordCheckBox.Name = "ShowPasswordCheckBox";
            this.ShowPasswordCheckBox.Size = new System.Drawing.Size(104, 19);
            this.ShowPasswordCheckBox.TabIndex = 10;
            this.ShowPasswordCheckBox.Text = "Wyświetl hasło";
            this.ShowPasswordCheckBox.UseVisualStyleBackColor = true;
            this.ShowPasswordCheckBox.CheckedChanged += new System.EventHandler(this.ShowPasswordCheckBox_CheckedChanged);
            // 
            // RegisterOrLoginLabel
            // 
            this.RegisterOrLoginLabel.AutoSize = true;
            this.RegisterOrLoginLabel.Location = new System.Drawing.Point(28, 226);
            this.RegisterOrLoginLabel.Name = "RegisterOrLoginLabel";
            this.RegisterOrLoginLabel.Size = new System.Drawing.Size(79, 15);
            this.RegisterOrLoginLabel.TabIndex = 11;
            this.RegisterOrLoginLabel.Text = "Zarejestruj się";
            this.RegisterOrLoginLabel.Click += new System.EventHandler(this.RegisterOrLoginLabel_Click);
            // 
            // RepeatPasswordTextBox
            // 
            this.RepeatPasswordTextBox.Location = new System.Drawing.Point(28, 251);
            this.RepeatPasswordTextBox.Name = "RepeatPasswordTextBox";
            this.RepeatPasswordTextBox.PasswordChar = '*';
            this.RepeatPasswordTextBox.Size = new System.Drawing.Size(249, 23);
            this.RepeatPasswordTextBox.TabIndex = 12;
            this.RepeatPasswordTextBox.TextChanged += new System.EventHandler(this.RepeatPasswordTextBox_TextChanged);
            // 
            // RepeatPasswordLabel
            // 
            this.RepeatPasswordLabel.AutoSize = true;
            this.RepeatPasswordLabel.Location = new System.Drawing.Point(28, 233);
            this.RepeatPasswordLabel.Name = "RepeatPasswordLabel";
            this.RepeatPasswordLabel.Size = new System.Drawing.Size(105, 15);
            this.RepeatPasswordLabel.TabIndex = 13;
            this.RepeatPasswordLabel.Text = "POWTÓRZ HASŁO";
            // 
            // EmailAddressLabel
            // 
            this.EmailAddressLabel.AutoSize = true;
            this.EmailAddressLabel.Location = new System.Drawing.Point(28, 283);
            this.EmailAddressLabel.Name = "EmailAddressLabel";
            this.EmailAddressLabel.Size = new System.Drawing.Size(84, 15);
            this.EmailAddressLabel.TabIndex = 15;
            this.EmailAddressLabel.Text = "ADRES E-MAIL";
            // 
            // EmailAddressTextBox
            // 
            this.EmailAddressTextBox.Location = new System.Drawing.Point(28, 301);
            this.EmailAddressTextBox.Name = "EmailAddressTextBox";
            this.EmailAddressTextBox.Size = new System.Drawing.Size(249, 23);
            this.EmailAddressTextBox.TabIndex = 14;
            this.EmailAddressTextBox.TextChanged += new System.EventHandler(this.EmailAddressTextBox_TextChanged);
            // 
            // RegisterLoginLabel
            // 
            this.RegisterLoginLabel.AutoSize = true;
            this.RegisterLoginLabel.Location = new System.Drawing.Point(76, 134);
            this.RegisterLoginLabel.Name = "RegisterLoginLabel";
            this.RegisterLoginLabel.Size = new System.Drawing.Size(139, 15);
            this.RegisterLoginLabel.TabIndex = 16;
            this.RegisterLoginLabel.Text = "(max 32 litery oraz liczby)";
            this.RegisterLoginLabel.Visible = false;
            // 
            // ConnectionForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(304, 386);
            this.Controls.Add(this.RegisterLoginLabel);
            this.Controls.Add(this.EmailAddressLabel);
            this.Controls.Add(this.EmailAddressTextBox);
            this.Controls.Add(this.RepeatPasswordLabel);
            this.Controls.Add(this.RepeatPasswordTextBox);
            this.Controls.Add(this.RegisterOrLoginLabel);
            this.Controls.Add(this.ShowPasswordCheckBox);
            this.Controls.Add(this.ServerConnectionLabel);
            this.Controls.Add(this.RegisterButton);
            this.Controls.Add(this.TryAgainButton);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.PasswordTextBox);
            this.Controls.Add(this.LoginTextBox);
            this.Controls.Add(this.LoginButton);
            this.Name = "ConnectionForm";
            this.Text = "OFTP";
            this.Load += new System.EventHandler(this.ConnectionForm_Load);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Button LoginButton;
        private System.Windows.Forms.TextBox LoginTextBox;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button TryAgainButton;
        private System.Windows.Forms.Button RegisterButton;
        private System.Windows.Forms.Label ServerConnectionLabel;
        private System.Windows.Forms.CheckBox ShowPasswordCheckBox;
        private System.Windows.Forms.Label RegisterOrLoginLabel;
        private System.Windows.Forms.TextBox RepeatPasswordTextBox;
        private System.Windows.Forms.Label RepeatPasswordLabel;
        private System.Windows.Forms.Label EmailAddressLabel;
        private System.Windows.Forms.TextBox EmailAddressTextBox;
        private System.Windows.Forms.Label RegisterLoginLabel;
    }
}

