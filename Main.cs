using ICSharpCode.Decompiler.Metadata;
using System;
using System.IO;
using System.Windows.Forms;

namespace homuwitch_decryptor
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }

        private void AddTextTo_richTextBox1(string text)
        {
            richTextBox1.AppendText(text);
            richTextBox1.ScrollToCaret();
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Program.githuburl);
        }

        private void button1_Click(object sender, EventArgs e)
        {
            string[] search_directories = { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) };
            string searchPattern = "*.exe";

            foreach (string search_directory in search_directories)
            {
                if (Directory.Exists(search_directory))
                {
                    string[] directories = Directory.GetDirectories(search_directory);
                    foreach (string directory in directories)
                    {
                        AddTextTo_richTextBox1("\n[*] Scanning directory: " + directory);
                        try
                        {
                            string[] files = Directory.GetFiles(directory, searchPattern, SearchOption.AllDirectories);
                            foreach (string file in files)
                            {
                                FileAttributes attributes = File.GetAttributes(file);
                                if ((attributes & FileAttributes.Hidden) != 0 || (attributes & FileAttributes.ReadOnly) != 0)
                                {
                                    if(new FileInfo(file).Length > Program.min_file_size &&
                                        new FileInfo(file).Length < Program.max_file_size)
                                    {
                                        var peFileStream = new FileStream(file, FileMode.Open, FileAccess.Read);
                                        var peFile = new PEFile(file, peFileStream);
                                        string extracted_password = Decrypter.ExtractPasswordFromSample(peFile);
                                        if (extracted_password != null)
                                        {
                                            AddTextTo_richTextBox1("\n[!!] Found decryption password: " + extracted_password + " in file " + file);
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            if (ex is UnauthorizedAccessException)
                            {
                                AddTextTo_richTextBox1("\n[*] Cannot access: " + directory);
                            }
                            else
                            {
                                MessageBox.Show("Error looking for sample: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }

        private void button2_Click(object sender, EventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.ShowReadOnly = true;
                openFileDialog.ReadOnlyChecked = true;
                openFileDialog.Filter = "Assembly files (*.exe)|*.exe|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    if(new FileInfo(openFileDialog.FileName).Length > Program.min_file_size &&
                        new FileInfo(openFileDialog.FileName).Length < Program.max_file_size)
                    {
                        var peFileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                        var peFile = new PEFile(openFileDialog.FileName, peFileStream);

                        string exctracted_password = Decrypter.ExtractPasswordFromSample(peFile);
                        if (exctracted_password != null)
                        {
                            AddTextTo_richTextBox1("\n[!!] Found decryption password: " + exctracted_password);
                        }
                        else
                        {
                            AddTextTo_richTextBox1("\n[!!] Ransomware decryption password not found in sample. Please try Auto.");
                        }
                    }
                }
            }
        }

        private void button3_Click(object sender, EventArgs e)
        {

        }

        private void label1_Click(object sender, EventArgs e)
        {

        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {

        }

        private void pictureBox2_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Program.githuburl);
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Program.twitterurl);
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(Program.linkedinurl);
        }
    }
}
