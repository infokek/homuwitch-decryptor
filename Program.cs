using ICSharpCode.Decompiler;
using ICSharpCode.Decompiler.Disassembler;
using ICSharpCode.Decompiler.Metadata;
using System;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;

namespace homuwitch_decryptor
{
    public class Decrypter
    {
        public static string ExtractPasswordFromSample(PEFile peFile)
        {
            Regex password_regex = new Regex(@"ldstr\s""(.*)""\s+.*string\sRnsmwr\.Program::password");
            try
            {
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
                    Match password_match = password_regex.Match(writer.ToString());
                    if (password_match.Success)
                    {
                        return password_match.Groups[1].Value.ToString();
                    }
                    else
                    {
                        MessageBox.Show("Not found password in sample", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error decompiling assembly: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }
        public static void DecryptFile(string inputFilePath, string outputFilePath, string password)
        {
            using (FileStream fileStream = new FileStream(inputFilePath, FileMode.Open))
            {
                using (FileStream fileStream2 = new FileStream(outputFilePath, FileMode.Create))
                {
                    using (Aes aes = Aes.Create())
                    {
                        byte[] bytes = Encoding.UTF8.GetBytes(password);
                        byte[] array = new byte[16];
                        fileStream.Read(array, 0, array.Length);
                        Rfc2898DeriveBytes rfc2898DeriveBytes = new Rfc2898DeriveBytes(bytes, array, 10000);
                        byte[] bytes2 = rfc2898DeriveBytes.GetBytes(32);
                        byte[] bytes3 = rfc2898DeriveBytes.GetBytes(16);
                        using (ICryptoTransform cryptoTransform = aes.CreateDecryptor(bytes2, bytes3))
                        {
                            using (CryptoStream cryptoStream = new CryptoStream(fileStream, cryptoTransform, CryptoStreamMode.Read))
                            {
                                using (DeflateStream deflateStream = new DeflateStream(cryptoStream, CompressionMode.Decompress))
                                {
                                    deflateStream.CopyTo(fileStream2);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
    internal static class Program
    {
        public static string decryption_password = null;
        public static readonly string encrypted_extension = ".homuencrypted";
        public static readonly string githuburl = "https://github.com/infokek";
        public static readonly string linkedinurl = "https://www.linkedin.com/in/infokek/";
        public static readonly string twitterurl = "https://twitter.com/infokek_";
        public static readonly int max_file_size = 1700000;
        public static readonly int min_file_size = 1400000;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new Form1());
        }
    }
}
