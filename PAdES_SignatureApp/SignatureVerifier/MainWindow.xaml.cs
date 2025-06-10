using Microsoft.Win32;
using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using SignatureVerifier;
using System;
using System.IO;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Media;

namespace SignatureVerifierApp
{
    public partial class MainWindow : Window
    {
        private string? publicKeyPath;
        private string? pdfPath;

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
            var dlg = new OpenFileDialog
            {
                Filter = "PDF Files|*.pdf"
            };
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
                byte[] pubKeyBytes = SignatureVerifierUtils.LoadPublicKeyFromPem(pem);

                using var rsa = RSA.Create();
                rsa.ImportSubjectPublicKeyInfo(pubKeyBytes, out _);

                UpdateStatus("📄 Reading and hashing PDF...", Brushes.DarkBlue);

                byte[] fileBytes = File.ReadAllBytes(pdfPath);
                byte[] hash = SHA256.HashData(fileBytes);

                var doc = PdfReader.Open(pdfPath, PdfDocumentOpenMode.ReadOnly);
                string keywords = doc.Info.Keywords ?? throw new Exception("No metadata found in PDF.");

                byte[] signature = SignatureVerifierUtils.ExtractSignatureFromMetadata(keywords);
                byte[] originalHash = SignatureVerifierUtils.ExtractHashFromMetadata(keywords);

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
    }
}
