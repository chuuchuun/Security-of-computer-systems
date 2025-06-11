using Microsoft.Win32;
using System.Windows;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace PAdES_SignatureApp
{
    /// <summary>
    /// Interaction logic for the main window of the PAdES signature application.
    /// </summary>
    public partial class MainWindow : Window
    {
        /// <summary>
        /// Path of the selected PDF file to sign.
        /// </summary>
        private string? selectedPdfPath;
        /// <summary>
        /// Decrypted private key bytes used for signing the PDF.
        /// </summary>
        private byte[]? decryptedPrivateKey;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Handles the Load PDF button click event to select a PDF file.
        /// Opens a file dialog filtered for PDF files and updates status.
        /// </summary>
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

        /// <summary>
        /// Handles the Sign PDF button click event.
        /// Validates inputs, decrypts the private key from USB, and signs the selected PDF.
        /// </summary>
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

                // Find removable USB drive that contains the encrypted private key file
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
                    // Decrypt private key using provided PIN
                    decryptedPrivateKey = CryptoHelper.DecryptPrivateKey(encryptedKey, pin);
                }
                catch (CryptographicException)
                {
                    MessageBox.Show("Incorrect PIN or corrupted key file. Decryption failed.", "Decryption Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                // Sign the PDF and update status with output path
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
