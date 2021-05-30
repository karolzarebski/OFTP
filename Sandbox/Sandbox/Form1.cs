using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Sandbox
{
    public partial class Form1 : Form
    {
        string filePath = "";
        List<string> selectedFilesPath = new List<string>();

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

        private void button3_Click(object sender, EventArgs e)
        {
            using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
            {
                if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = folderBrowserDialog.SelectedPath;
                    treeView1.Nodes.Clear();
                    DirectoryInfo di = new DirectoryInfo(filePath);
                    TreeNode tds = treeView1.Nodes.Add(di.Name);
                    tds.Tag = di.FullName;
                    tds.StateImageIndex = 0;
                    LoadFiles(filePath, tds);
                    LoadSubDirectories(filePath, tds);
                }
            }
        }

        private void LoadFiles(string dir, TreeNode td)
        {
            string[] Files = Directory.GetFiles(dir, "*.*");

            // Loop through them to see files  
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
            // Get all subdirectories  
            string[] subdirectoryEntries = Directory.GetDirectories(dir);
            // Loop through them to see if they have any other subdirectories  
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

        private void treeView1_AfterCheck(object sender, TreeViewEventArgs e)
        {
            string path = filePath.Remove(filePath.LastIndexOf('\\') + 1) + e.Node.FullPath;

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

                label1.Text = "";
                foreach (var i in selectedFilesPath)
                {
                    label1.Text += i + "\r\n";
                }
            }
        }
    }
}
