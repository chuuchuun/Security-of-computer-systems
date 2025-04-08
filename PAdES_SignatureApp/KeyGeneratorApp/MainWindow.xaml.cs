using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace KeyGeneratorApp
{
    public partial class MainWindow : Window
    {
        private byte[] privateKey;
        private byte[] publicKey;

        public MainWindow()
        {
            InitializeComponent();
            GenerateKeyPairButton.Click += GenerateKeyPairButton_Click;
            SaveToUSBButton.Click += SaveToUSBButton_Click;
        }

        private void GenerateKeyPairButton_Click(object sender, RoutedEventArgs e)
        {
            string pin = PinBox.Text;

            if (string.IsNullOrEmpty(pin))
            {
                StatusBlock.Text = "Status: Please enter a PIN.";
                return;
            }

            try
            {
                StatusBlock.Text = "Status: Generating key pair...";
                var result = KeyGenerator.GenerateKeyPair(pin);
                privateKey = result.EncryptedPrivateKey;
                publicKey = result.PublicKey;
                StatusBlock.Text = "Status: Key pair generated successfully.";
            }
            catch (Exception ex)
            {
                StatusBlock.Text = "Status: Error during key generation - " + ex.Message;
            }
        }

        private void SaveToUSBButton_Click(object sender, RoutedEventArgs e)
        {
            if (privateKey == null || publicKey == null)
            {
                StatusBlock.Text = "Status: Please generate the key pair first.";
                return;
            }

            try
            {
                StatusBlock.Text = "Status: Searching for USB drive...";

                string usbPath = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                    .Select(d => d.RootDirectory.FullName)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(usbPath))
                {
                    StatusBlock.Text = "Status: No USB drive found.";
                    return;
                }

                File.WriteAllBytes(Path.Combine(usbPath, "privateKey.enc"), privateKey);
                File.WriteAllBytes(Path.Combine(usbPath, "publicKey.pem"), publicKey);

                StatusBlock.Text = $"Status: Keys saved to USB ({usbPath}) successfully.";
            }
            catch (IOException ex)
            {
                StatusBlock.Text = "Status: Error writing to USB - " + ex.Message;
            }
        }
    }
}
