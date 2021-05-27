using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = "c:\\";
                openFileDialog.Filter = "txt files (*.txt)|*.txt|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 2;
                openFileDialog.RestoreDirectory = true;
                openFileDialog.Multiselect = true;

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    var filePath = openFileDialog.FileNames;

                    

                    //Read the contents of the file into a stream
                    //var fileStream = openFileDialog.OpenFile();

                    //using (StreamReader reader = new StreamReader(fileStream))
                    //{
                    //   fileContent = reader.ReadToEnd();
                    //}
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog openFileDialog = new FolderBrowserDialog())
            {

             openFileDialog.
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    //Get the path of specified file
                    //var filePath = openFileDialog.FileName;

                    //label1.Text = filePath;

                    //Read the contents of the file into a stream
                    //var fileStream = openFileDialog.OpenFile();

                    //using (StreamReader reader = new StreamReader(fileStream))
                    //{
                    //   fileContent = reader.ReadToEnd();
                    //}
                }
            }
        }
    }
}
