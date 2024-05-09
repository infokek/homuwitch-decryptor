using ICSharpCode.Decompiler.Metadata;
using System;
using System.ComponentModel;
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
            using (StreamWriter w = File.AppendText(Program.decryption_log))
            {
                w.WriteLine(DateTime.Now.ToString() + text);
            }
            richTextBox1.AppendText("\n" + text);
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
                        AddTextTo_richTextBox1("[*] Scanning directory: " + directory);
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
                                        peFileStream.Close();
                                        if (extracted_password != null)
                                        {
                                            AddTextTo_richTextBox1("[!] Found decryption password: " + extracted_password + " in file " + file);
                                            Program.decryption_password = extracted_password;
                                            Program.ransomware_file = file;
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
                                AddTextTo_richTextBox1("[*] Cannot access: " + directory);
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

                        string extracted_password = Decrypter.ExtractPasswordFromSample(peFile);
                        if (extracted_password != null)
                        {
                            AddTextTo_richTextBox1("[!] Found decryption password: " + extracted_password);
                            Program.decryption_password = extracted_password;
                        }
                        else
                        {
                            AddTextTo_richTextBox1("[!!] Ransomware decryption password not found in sample. Please try Auto.");
                        }
                    }
                }
            }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            BackgroundWorker bw = sender as BackgroundWorker;
            string[] search_directories = { Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) };
            foreach (string search_directory in search_directories)
            {
                if (Directory.Exists(search_directory))
                {
                    string[] directories = Directory.GetDirectories(search_directory);
                    foreach (string directory in directories)
                    {
                        AddTextTo_richTextBox1("[*] Scanning directory for encrypted files: " + directory);
                        try
                        {
                            string[] files = Directory.GetFiles(directory, "*" + Program.encrypted_extension, SearchOption.AllDirectories);
                            foreach (string file in files)
                            {
                                if (bw.CancellationPending)
                                {
                                    e.Cancel = true;
                                    AddTextTo_richTextBox1("[*] Log file is " + Program.decryption_log);
                                    break;
                                }
                                AddTextTo_richTextBox1("[*] Decrypting file: " + file);
                                string real_filename = Path.GetFileName(file).Replace(Program.encrypted_extension, "");
                                string decrypted_file_path = Path.GetDirectoryName(file) + "\\" + real_filename;
                                Decrypter.DecryptFile(file, decrypted_file_path, Program.decryption_password);
                                AddTextTo_richTextBox1("[*] Successfully decrypted: " + decrypted_file_path);
                            }
                            if (e.Cancel)
                            {
                                AddTextTo_richTextBox1("[!] Decryption process was canceled");
                            }
                            else
                            {
                                AddTextTo_richTextBox1("[*] Successfully decrypted all files");
                            }
                            if (Program.ransomware_file != null)
                            {
                                /// defuse (kill) ransomware process
                                if (Decrypter.DefuseRansomware(Program.ransomware_file) == true)
                                {
                                    AddTextTo_richTextBox1("[*] Successfully killed ransomware process");
                                }
                                else
                                {
                                    AddTextTo_richTextBox1("[*] Probably ransowmare not running, so process no need to be killed");
                                }
                                /// change file attributes
                                File.SetAttributes(Program.ransomware_file, FileAttributes.Normal);
                                File.SetAttributes(Program.ransomware_file, FileAttributes.ReadOnly);
                                string new_filename = Program.ransomware_file + ".ransomware_sample";
                                File.Move(Program.ransomware_file, new_filename);
                                AddTextTo_richTextBox1("[*] Filename for ransomware changed " + new_filename);
                                /// change wallpaper to default
                                Decrypter.SystemParametersInfo(Decrypter.SPI_SETDESKWALLPAPER, 0,
                                    "C:\\Windows\\Web\\Wallpaper\\Windows\\img0.jpg", Decrypter.SPIF_UPDATEINIFILE | Decrypter.SPIF_SENDCHANGE);
                                /// remove ransomware persistent
                                if (File.Exists(Program.path_to_ransomware_startup) == true)
                                {
                                    try
                                    {
                                        File.Delete(Program.path_to_ransomware_startup);
                                    }
                                    catch (Exception ex)
                                    {
                                        MessageBox.Show("Error removing ransomware persistence: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                                    }
                                }
                                else
                                {
                                    AddTextTo_richTextBox1("[*] Ransomware seems not to be persisted at path" + Program.path_to_ransomware_startup);
                                }
                                Program.ransomware_file = null; 
                            }
                            AddTextTo_richTextBox1("[*] Log file is " + Program.decryption_log);
                        }
                        catch (Exception ex)
                        {
                            if (ex is UnauthorizedAccessException)
                            {
                                AddTextTo_richTextBox1("[*] Cannot access: " + directory);
                            }
                            else
                            {
                                MessageBox.Show("Error looking for encrypted file: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }
                        }
                    }
                }
            }
        }
        private void backgroundWorker1_RunWorkerCompleted(
            object sender,
            RunWorkerCompletedEventArgs e)
        {
            if (e.Cancelled)
            {
                MessageBox.Show("Decryption process was canceled", "Stopped", MessageBoxButtons.OK);
            }
            else if (e.Error != null)
            {
                string msg = String.Format("An error occurred while decrypting: {0}", e.Error.Message);
                MessageBox.Show(msg);
            }
        }
        private void button3_Click(object sender, EventArgs e)
        {
            if (Program.decryption_password != null)
            {
                if (!backgroundWorker1.IsBusy)
                {
                    backgroundWorker1.RunWorkerAsync();
                }
                else
                {
                    backgroundWorker1.CancelAsync();
                }
            }
            else
            {
                AddTextTo_richTextBox1("[!] Password hasn't extracted yet. Please try Auto or Pick sample manually.");
                MessageBox.Show("Password hasn't extracted yet." +
                    " Please try Auto or Pick sample manually.", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
