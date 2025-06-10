using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PAdES_SignatureApp
{
    public partial class MainWindow : Window
    {
        private string? selectedPdfPath;
        private byte[]? decryptedPrivateKey;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadPdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new()
            {
                Filter = "PDF files (*.pdf)|*.pdf"
            };

            if (dlg.ShowDialog() is true)
            {
                selectedPdfPath = dlg.FileName;
                StatusBlock.Text = $"Loaded PDF: {Path.GetFileName(selectedPdfPath)}";
            }
        }

        private void SignPdf_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(selectedPdfPath))
            {
                MessageBox.Show("Please select a PDF first.");
                return;
            }

            var pin = PinBox.Text.Trim();
            if (string.IsNullOrEmpty(pin))
            {
                MessageBox.Show("Please enter your PIN.");
                return;
            }

            try
            {
                var usbDrive = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                    .FirstOrDefault(d => File.Exists(Path.Combine(d.RootDirectory.FullName, "privateKey.enc")));

                if (usbDrive is null)
                {
                    MessageBox.Show("No USB drive with privateKey.enc found.");
                    return;
                }

                string keyPath = Path.Combine(usbDrive.RootDirectory.FullName, "privateKey.enc");

                var encryptedKey = File.ReadAllBytes(keyPath);

                try
                {
                    decryptedPrivateKey = CryptoHelper.DecryptPrivateKey(encryptedKey, pin);
                }
                catch (CryptographicException)
                {
                    MessageBox.Show("Incorrect PIN or corrupted key file. Decryption failed.", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                var outputPath = Signer.SignPdf(selectedPdfPath, decryptedPrivateKey);
                StatusBlock.Text = $"PDF signed: {outputPath}";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}");
            }
        }
    }
}
