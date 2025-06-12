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
    /// <summary>
    /// Interaction logic for the main window of the Signature Verifier application.
    /// Enables loading a public key, loading a PDF, and verifying the PDF's digital signature.
    /// </summary>
    public partial class MainWindow : Window
    {
        private string? publicKeyPath;
        private string? pdfPath;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// Attempts to load the public key from any connected USB drive.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            LoadPublicKey();
        }

        /// <summary>
        /// Searches for a public key file named "publicKey.pem" on all connected removable USB drives.
        /// If not found, prompts the user to manually select the public key PEM file.
        /// Updates the status message accordingly.
        /// </summary>
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
            var dlg = new OpenFileDialog
            {
                Title = "Select Public Key File",
                Filter = "PEM Files (*.pem)|*.pem"
            };

            if (dlg.ShowDialog() == true)
            {
                publicKeyPath = dlg.FileName;
                UpdateStatus($"✅ Public key loaded from {publicKeyPath}", Brushes.Green);
            }
            else
            {
                UpdateStatus("❗ Public key not found on USB or selected manually.", Brushes.OrangeRed);
            }
        }


        /// <summary>
        /// Updates the status text and color on the UI.
        /// </summary>
        /// <param name="message">The status message to display.</param>
        /// <param name="color">The brush color for the status text.</param>
        private void UpdateStatus(string message, Brush color)
        {
            StatusBlock.Text = "Status: " + message;
            StatusBlock.Foreground = color;
        }

        /// <summary>
        /// Handles the click event for loading a PDF file.
        /// Opens a file dialog for the user to select a PDF, and updates the UI status.
        /// </summary>
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

        /// <summary>
        /// Handles the click event to verify the signature of the loaded PDF using the loaded public key.
        /// Reads the public key, computes the PDF hash, extracts signature and hash metadata, and verifies the signature.
        /// Updates status message with the verification result.
        /// </summary>
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

                if (!doc.GetHashCode().Equals(originalHash))
                {
                    UpdateStatus("❌ Signature is INVALID!", Brushes.Red);
                }

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
