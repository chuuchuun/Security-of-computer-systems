using Microsoft.Win32;
using System.Windows;
using System.IO;

namespace PAdES_SignerApp
{
    public partial class MainWindow : Window
    {
        private string selectedPdfPath;
        private byte[] decryptedPrivateKey;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadPdf_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog dlg = new OpenFileDialog();
            dlg.Filter = "PDF files (*.pdf)|*.pdf";

            if (dlg.ShowDialog() == true)
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
                // Read encrypted private key from USB
                var encryptedKey = File.ReadAllBytes("E:\\privateKey.enc");
                decryptedPrivateKey = CryptoHelper.DecryptPrivateKey(encryptedKey, pin);

                // Sign the PDF
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
