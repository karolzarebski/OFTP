using System;
using System.Drawing;
using System.Windows.Forms;

namespace OFTP_Client
{
    public partial class AccountModification : Form
    {
        private readonly string _accountName;
        private string _password = string.Empty;

        public AccountModification(string accountName)
        {
            InitializeComponent();

            _accountName = accountName;
        }

        private void AccountNameTextBox_TextChanged(object sender, EventArgs e)
        {
            if (AccountNameTextBox.Text != _accountName)
            {
                AccountNameTextBox.BackColor = Color.Tomato;

                AccountDeletionConfirmationButton.Enabled = false;
            }
            else
            {
                AccountNameTextBox.BackColor = Color.PaleGreen;

                AccountDeletionConfirmationButton.Enabled = true;
            }
        }

        private void AccountDeletionConfirmationButton_Click(object sender, EventArgs e)
        {
            switch (MessageBox.Show("Czy chcesz usunąć konto?\nTej operacji nie można cofnąć", "Usuwanie konta",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question))
            {
                case DialogResult.Yes:
                    MessageBox.Show("Konto zostało pomyślnie usunięte", "Konto usunięte",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    DialogResult = DialogResult.Yes;
                    break;
                case DialogResult.No:
                    DialogResult = DialogResult.Cancel;
                    break;
            }

            Close();
        }

        private void DeleteAccountConfirmation_Load(object sender, EventArgs e)
        {
            MaximumSize = Size;
            MinimumSize = Size;

            AccountDeletionConfirmationButton.Enabled = false;
            ChangePasswordButton.Enabled = false;

            AccountNameLabel.Text = _accountName;
        }

        private void ChangePasswordButton_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hasło zostało pomyślnie zmieniona", "Zmiana hasła",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            DialogResult = DialogResult.OK;
        }

        private void ConfirmPasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            if (ConfirmPasswordTextBox.Text != _password)
            {
                ChangePasswordButton.Enabled = false;

                ConfirmPasswordTextBox.BackColor = Color.Tomato;
            }
            else
            {
                ChangePasswordButton.Enabled = true;

                ConfirmPasswordTextBox.BackColor = Color.PaleGreen;
            }
        }

        private void PasswordTextBox_TextChanged(object sender, EventArgs e)
        {
            _password = PasswordTextBox.Text;
        }

        private void ShowPasswordCheckBox_CheckedChanged(object sender, EventArgs e)
        {
            if (ShowPasswordCheckBox.Checked)
            {
                PasswordTextBox.PasswordChar = '\0';
                ConfirmPasswordTextBox.PasswordChar = '\0';
            }
            else
            {
                PasswordTextBox.PasswordChar = '*';
                ConfirmPasswordTextBox.PasswordChar = '*';
            }
        }
    }
}
