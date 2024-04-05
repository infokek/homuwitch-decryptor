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
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Assembly files (*.dll, *.exe)|*.dll;*.exe|All files (*.*)|*.*";
                openFileDialog.FilterIndex = 1;
                openFileDialog.RestoreDirectory = true;
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        using (var peFileStream = new FileStream(openFileDialog.FileName, FileMode.Open, FileAccess.Read))
                        using (var peFile = new PEFile(openFileDialog.FileName, peFileStream))
                        using (var writer = new StringWriter())
                        {
                            var output = new PlainTextOutput(writer);
                            ReflectionDisassembler rd = new ReflectionDisassembler(output, CancellationToken.None);
                            rd.DetectControlStructure = false;
                            rd.WriteAssemblyReferences(peFile.Metadata);
                            if (peFile.Metadata.IsAssembly)
                                rd.WriteAssemblyHeader(peFile);
                            output.WriteLine();
                            rd.WriteModuleHeader(peFile);
                            output.WriteLine();
                            rd.WriteModuleContents(peFile);

                            richTextBox1.Text = writer.ToString();
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Error decompiling assembly: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }
        private void richTextBox1_TextChanged(object sender, EventArgs e)
        {
        }

    }
}
