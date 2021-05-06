
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
            this.UserAvailable = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.ConnectButton = new System.Windows.Forms.Button();
            this.SuspendLayout();
            // 
            // UserAvailable
            // 
            this.UserAvailable.FormattingEnabled = true;
            this.UserAvailable.ItemHeight = 15;
            this.UserAvailable.Location = new System.Drawing.Point(522, 42);
            this.UserAvailable.Name = "UserAvailable";
            this.UserAvailable.Size = new System.Drawing.Size(266, 394);
            this.UserAvailable.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(601, 24);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(119, 15);
            this.label1.TabIndex = 1;
            this.label1.Text = "Aktywni użytkownicy";
            // 
            // ConnectButton
            // 
            this.ConnectButton.Location = new System.Drawing.Point(83, 54);
            this.ConnectButton.Name = "ConnectButton";
            this.ConnectButton.Size = new System.Drawing.Size(161, 23);
            this.ConnectButton.TabIndex = 2;
            this.ConnectButton.Text = "Połącz z użytkownikiem";
            this.ConnectButton.UseVisualStyleBackColor = true;
            // 
            // MainWindow
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(800, 450);
            this.Controls.Add(this.ConnectButton);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.UserAvailable);
            this.Name = "MainWindow";
            this.Text = "FileTransferForm";
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox UserAvailable;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Button ConnectButton;
    }
}