using Microsoft.Win32;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace SignatureVerifierApp
{
    public partial class MainWindow : Window
    {
        private string publicKeyPath;
        private string pdfPath;

        public MainWindow()
        {
            InitializeComponent();
            LoadPublicKey();
        }

        private void LoadPublicKey()
        {
            UpdateStatus("🔍 Searching for public key on USB...", Brushes.DarkSlateGray);
            foreach (var drive in DriveInfo.GetDrives())
            {
                if (drive.DriveType == DriveType.Removable && drive.IsReady)
                {
                    string keyPath = Path.Combine(drive.RootDirectory.FullName, "publicKey.pem");

                    if (File.Exists(keyPath))
                    {
                        publicKeyPath = keyPath;
                        UpdateStatus($"✅ Public key loaded from {drive.Name}", Brushes.Green);
                        return;
                    }
                }
            }

            UpdateStatus("❗ Public key not found on any USB drive.", Brushes.OrangeRed);
        }


        private void UpdateStatus(string message, Brush color)
        {
            StatusBlock.Text = "Status: " + message;
            StatusBlock.Foreground = color;
        }

        private void LoadPdf_Click(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog();
            dlg.Filter = "PDF Files|*.pdf";
            if (dlg.ShowDialog() == true)
            {
                pdfPath = dlg.FileName;
                StatusBlock.Text = "PDF loaded.";
            }
        }

        private void VerifySignature_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(publicKeyPath) || string.IsNullOrEmpty(pdfPath))
            {
                UpdateStatus("❗ Please select both the public key and PDF.", Brushes.OrangeRed);
                return;
            }

            try
            {
                UpdateStatus("🔍 Reading public key...", Brushes.DarkBlue);

                string pem = File.ReadAllText(publicKeyPath);
                byte[] pubKeyBytes = LoadPublicKeyFromPem(pem);

                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(pubKeyBytes, out _);

                UpdateStatus("📄 Reading and hashing PDF...", Brushes.DarkBlue);

                byte[] fileBytes = File.ReadAllBytes(pdfPath);
                byte[] hash = SHA256.HashData(fileBytes);

                var doc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
                string keywords = doc.Info.Keywords;
                byte[] signature = ExtractSignatureFromMetadata(keywords);
                byte[] originalHash = ExtractHashFromMetadata(keywords);

                UpdateStatus("🔍 Verifying signature...", Brushes.DarkBlue);
                bool valid = rsa.VerifyHash(originalHash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);


                if (valid)
                    UpdateStatus("✅ Signature is VALID!", Brushes.Green);
                else
                    UpdateStatus("❌ Signature is INVALID!", Brushes.Red);
            }
            catch (Exception ex)
            {
                UpdateStatus("❗ Error: " + ex.Message, Brushes.Red);
            }
        }

        private byte[] ExtractHashFromMetadata(string metadata)
        {
            const string marker = "Hash:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("Original hash not found in metadata.");

            string base64 = metadata.Substring(index + marker.Length).Split('|')[0].Trim();
            return Convert.FromBase64String(base64);
        }


        private byte[] ExtractSignatureFromMetadata(string metadata)
        {
            const string marker = "PAdES_Signature:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("Signature not found in metadata.");

            string base64 = metadata.Substring(index + marker.Length).Split('|')[0].Trim();
            return Convert.FromBase64String(base64);
        }

        private byte[] LoadPublicKeyFromPem(string pem)
        {
            Console.WriteLine("PEM content:\n" + pem);

            const string header = "-----BEGIN PUBLIC KEY-----";
            const string footer = "-----END PUBLIC KEY-----";

            int start = pem.IndexOf(header);
            int end = pem.IndexOf(footer);

            if (start < 0 || end < 0 || end <= start)
                throw new Exception("Invalid PEM format. Could not find public key markers.");

            start += header.Length;
            string base64 = pem.Substring(start, end - start)
                               .Replace("\n", "")
                               .Replace("\r", "")
                               .Trim();

            if (string.IsNullOrWhiteSpace(base64))
                throw new Exception("PEM content is empty or improperly formatted.");

            return Convert.FromBase64String(base64);
        }


    }
}
