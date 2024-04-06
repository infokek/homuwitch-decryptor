using ICSharpCode.Decompiler.CSharp;
using ICSharpCode.Decompiler;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Reflection.Metadata;
using ICSharpCode.Decompiler.Metadata;
using ICSharpCode.Decompiler.TypeSystem;
using System.Reflection.PortableExecutable;
using ICSharpCode.Decompiler.Disassembler;
using System.Threading;
using System.IO.Compression;
using System.Security.Cryptography;

namespace homuwitch_decryptor
{
    public partial class Form1 : Form
    {

        public Form1()
        {
            InitializeComponent();
        }
        private void button1_Click(object sender, EventArgs e)
        {
            
        }

        private void Form1_Load(object sender, EventArgs e)
        {

        }

        private void pictureBox1_Click(object sender, EventArgs e)
        {

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
                    var peFileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read);
                    var peFile = new PEFile(openFileDialog.FileName, peFileStream);

                    string exctracted_password = Decrypter.ExctractPasswordFromSample(peFile);
                    if (exctracted_password != null){
                        richTextBox1.Text = richTextBox1.Text + "\n[*] Found decryption password: " + exctracted_password;
                    }
                    else
                    {
                        richTextBox1.Text = richTextBox1.Text + "\n[!] Ransomware decryption password not found in sample. Please try Auto.";
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
            System.Diagnostics.Process.Start("https://github.com/infokek");
        }

        private void pictureBox3_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://twitter.com/infokek_");
        }

        private void pictureBox4_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start("https://www.linkedin.com/in/infokek/");
        }
    }
}
