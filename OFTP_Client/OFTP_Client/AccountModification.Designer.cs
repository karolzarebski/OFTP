
namespace OFTP_Client
{
    partial class AccountModification
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
            this.label1 = new System.Windows.Forms.Label();
            this.AccountNameTextBox = new System.Windows.Forms.TextBox();
            this.AccountDeletionConfirmationButton = new System.Windows.Forms.Button();
            this.AccountDeleteInformationLabel = new System.Windows.Forms.Label();
            this.AccountNameLabel = new System.Windows.Forms.Label();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.groupBox2 = new System.Windows.Forms.GroupBox();
            this.ShowPasswordCheckBox = new System.Windows.Forms.CheckBox();
            this.ConfirmPasswordTextBox = new System.Windows.Forms.TextBox();
            this.PasswordTextBox = new System.Windows.Forms.TextBox();
            this.ChangePasswordButton = new System.Windows.Forms.Button();
            this.label4 = new System.Windows.Forms.Label();
            this.label3 = new System.Windows.Forms.Label();
            this.label2 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.CurrentPasswordTextBox = new System.Windows.Forms.TextBox();
            this.groupBox1.SuspendLayout();
            this.groupBox2.SuspendLayout();
            this.SuspendLayout();
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.Location = new System.Drawing.Point(56, 33);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(165, 30);
            this.label1.TabIndex = 0;
            this.label1.Text = "Usuwanie konta";
            // 
            // AccountNameTextBox
            // 
            this.AccountNameTextBox.Location = new System.Drawing.Point(23, 165);
            this.AccountNameTextBox.Name = "AccountNameTextBox";
            this.AccountNameTextBox.Size = new System.Drawing.Size(238, 23);
            this.AccountNameTextBox.TabIndex = 1;
            this.AccountNameTextBox.TextChanged += new System.EventHandler(this.AccountNameTextBox_TextChanged);
            // 
            // AccountDeletionConfirmationButton
            // 
            this.AccountDeletionConfirmationButton.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.AccountDeletionConfirmationButton.Location = new System.Drawing.Point(56, 208);
            this.AccountDeletionConfirmationButton.Name = "AccountDeletionConfirmationButton";
            this.AccountDeletionConfirmationButton.Size = new System.Drawing.Size(185, 42);
            this.AccountDeletionConfirmationButton.TabIndex = 2;
            this.AccountDeletionConfirmationButton.Text = "Usuń konto";
            this.AccountDeletionConfirmationButton.UseVisualStyleBackColor = true;
            this.AccountDeletionConfirmationButton.Click += new System.EventHandler(this.AccountDeletionConfirmationButton_Click);
            // 
            // AccountDeleteInformationLabel
            // 
            this.AccountDeleteInformationLabel.AutoSize = true;
            this.AccountDeleteInformationLabel.Location = new System.Drawing.Point(23, 93);
            this.AccountDeleteInformationLabel.Name = "AccountDeleteInformationLabel";
            this.AccountDeleteInformationLabel.Size = new System.Drawing.Size(238, 15);
            this.AccountDeleteInformationLabel.TabIndex = 3;
            this.AccountDeleteInformationLabel.Text = "Potwierdź usunięcie konta wpisując poniżej:";
            // 
            // AccountNameLabel
            // 
            this.AccountNameLabel.AutoSize = true;
            this.AccountNameLabel.Font = new System.Drawing.Font("Segoe UI", 9F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.AccountNameLabel.Location = new System.Drawing.Point(120, 135);
            this.AccountNameLabel.Name = "AccountNameLabel";
            this.AccountNameLabel.Size = new System.Drawing.Size(40, 15);
            this.AccountNameLabel.TabIndex = 4;
            this.AccountNameLabel.Text = "label2";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.label1);
            this.groupBox1.Controls.Add(this.AccountNameLabel);
            this.groupBox1.Controls.Add(this.AccountNameTextBox);
            this.groupBox1.Controls.Add(this.AccountDeleteInformationLabel);
            this.groupBox1.Controls.Add(this.AccountDeletionConfirmationButton);
            this.groupBox1.Location = new System.Drawing.Point(12, 4);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(289, 292);
            this.groupBox1.TabIndex = 5;
            this.groupBox1.TabStop = false;
            // 
            // groupBox2
            // 
            this.groupBox2.Controls.Add(this.CurrentPasswordTextBox);
            this.groupBox2.Controls.Add(this.label5);
            this.groupBox2.Controls.Add(this.ShowPasswordCheckBox);
            this.groupBox2.Controls.Add(this.ConfirmPasswordTextBox);
            this.groupBox2.Controls.Add(this.PasswordTextBox);
            this.groupBox2.Controls.Add(this.ChangePasswordButton);
            this.groupBox2.Controls.Add(this.label4);
            this.groupBox2.Controls.Add(this.label3);
            this.groupBox2.Controls.Add(this.label2);
            this.groupBox2.Location = new System.Drawing.Point(308, 4);
            this.groupBox2.Name = "groupBox2";
            this.groupBox2.Size = new System.Drawing.Size(264, 292);
            this.groupBox2.TabIndex = 6;
            this.groupBox2.TabStop = false;
            // 
            // ShowPasswordCheckBox
            // 
            this.ShowPasswordCheckBox.AutoSize = true;
            this.ShowPasswordCheckBox.Location = new System.Drawing.Point(171, 137);
            this.ShowPasswordCheckBox.Name = "ShowPasswordCheckBox";
            this.ShowPasswordCheckBox.Size = new System.Drawing.Size(57, 19);
            this.ShowPasswordCheckBox.TabIndex = 6;
            this.ShowPasswordCheckBox.Text = "Pokaż";
            this.ShowPasswordCheckBox.UseVisualStyleBackColor = true;
            this.ShowPasswordCheckBox.CheckedChanged += new System.EventHandler(this.ShowPasswordCheckBox_CheckedChanged);
            // 
            // ConfirmPasswordTextBox
            // 
            this.ConfirmPasswordTextBox.Location = new System.Drawing.Point(32, 209);
            this.ConfirmPasswordTextBox.Name = "ConfirmPasswordTextBox";
            this.ConfirmPasswordTextBox.PasswordChar = '*';
            this.ConfirmPasswordTextBox.Size = new System.Drawing.Size(196, 23);
            this.ConfirmPasswordTextBox.TabIndex = 5;
            this.ConfirmPasswordTextBox.TextChanged += new System.EventHandler(this.ConfirmPasswordTextBox_TextChanged);
            // 
            // PasswordTextBox
            // 
            this.PasswordTextBox.Location = new System.Drawing.Point(32, 156);
            this.PasswordTextBox.Name = "PasswordTextBox";
            this.PasswordTextBox.PasswordChar = '*';
            this.PasswordTextBox.Size = new System.Drawing.Size(196, 23);
            this.PasswordTextBox.TabIndex = 4;
            this.PasswordTextBox.TextChanged += new System.EventHandler(this.PasswordTextBox_TextChanged);
            // 
            // ChangePasswordButton
            // 
            this.ChangePasswordButton.Location = new System.Drawing.Point(65, 251);
            this.ChangePasswordButton.Name = "ChangePasswordButton";
            this.ChangePasswordButton.Size = new System.Drawing.Size(135, 23);
            this.ChangePasswordButton.TabIndex = 3;
            this.ChangePasswordButton.Text = "Zmień hasło";
            this.ChangePasswordButton.UseVisualStyleBackColor = true;
            this.ChangePasswordButton.Click += new System.EventHandler(this.ChangePasswordButton_Click);
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Location = new System.Drawing.Point(32, 191);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(111, 15);
            this.label4.TabIndex = 2;
            this.label4.Text = "POTWIERDŹ HASŁO";
            // 
            // label3
            // 
            this.label3.AutoSize = true;
            this.label3.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.Location = new System.Drawing.Point(58, 32);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(139, 30);
            this.label3.TabIndex = 1;
            this.label3.Text = "Zmiana hasła";
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(32, 138);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(45, 15);
            this.label2.TabIndex = 0;
            this.label2.Text = "HASŁO";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Location = new System.Drawing.Point(32, 75);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(106, 15);
            this.label5.TabIndex = 7;
            this.label5.Text = "AKTUALNE HASŁO";
            // 
            // CurrentPasswordTextBox
            // 
            this.CurrentPasswordTextBox.Location = new System.Drawing.Point(32, 93);
            this.CurrentPasswordTextBox.Name = "CurrentPasswordTextBox";
            this.CurrentPasswordTextBox.PasswordChar = '*';
            this.CurrentPasswordTextBox.Size = new System.Drawing.Size(196, 23);
            this.CurrentPasswordTextBox.TabIndex = 8;
            // 
            // AccountModification
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(587, 308);
            this.Controls.Add(this.groupBox2);
            this.Controls.Add(this.groupBox1);
            this.Name = "AccountModification";
            this.Text = "Modyfikacje konta";
            this.Load += new System.EventHandler(this.DeleteAccountConfirmation_Load);
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.groupBox2.ResumeLayout(false);
            this.groupBox2.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox AccountNameTextBox;
        private System.Windows.Forms.Button AccountDeletionConfirmationButton;
        private System.Windows.Forms.Label AccountDeleteInformationLabel;
        private System.Windows.Forms.Label AccountNameLabel;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.GroupBox groupBox2;
        private System.Windows.Forms.TextBox ConfirmPasswordTextBox;
        private System.Windows.Forms.TextBox PasswordTextBox;
        private System.Windows.Forms.Button ChangePasswordButton;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.CheckBox ShowPasswordCheckBox;
        private System.Windows.Forms.TextBox CurrentPasswordTextBox;
        private System.Windows.Forms.Label label5;
    }
}