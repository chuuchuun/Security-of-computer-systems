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

            // Look through all removable drives
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
                UpdateStatus("🔌 Initializing USB / Reading public key...", Brushes.DarkBlue);

                // Load public key
                string pem = File.ReadAllText(publicKeyPath);
                byte[] pubKeyBytes = Convert.FromBase64String(pem
                    .Replace("-----BEGIN PUBLIC KEY-----", "")
                    .Replace("-----END PUBLIC KEY-----", "")
                    .Replace("\r", "")
                    .Replace("\n", ""));

                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(pubKeyBytes, out _);

                UpdateStatus("📄 Reading and hashing PDF...", Brushes.DarkBlue);

                byte[] fileBytes = File.ReadAllBytes(pdfPath);
                byte[] hash = SHA256.HashData(fileBytes);

                var doc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
                string keywords = doc.Info.Keywords;

                UpdateStatus("🔍 Extracting and verifying signature...", Brushes.DarkBlue);
                var signature = ExtractSignatureFromMetadata(keywords);

                bool valid = rsa.VerifyHash(hash, signature, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

                if (valid)
                {
                    UpdateStatus("✅ Signature is VALID!", Brushes.Green);
                }
                else
                {
                    UpdateStatus("❌ Signature is INVALID!", Brushes.Red);
                }
            }
            catch (Exception ex)
            {
                UpdateStatus("❗ Error verifying signature: " + ex.Message, Brushes.Red);
            }
        }

        private byte[] ExtractSignatureFromMetadata(string metadata)
        {
            string marker = "PAdES_Signature:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("No signature found in PDF metadata.");

            string base64Sig = metadata.Substring(index + marker.Length);
            return Convert.FromBase64String(base64Sig);
        }
    }
}
