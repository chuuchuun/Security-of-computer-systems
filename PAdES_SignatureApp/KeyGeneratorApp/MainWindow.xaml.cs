using System;
using System.IO;
using System.Linq;
using System.Windows;

namespace KeyGeneratorApp
{
    /// <summary>
    /// Main application window that allows generating and saving RSA key pairs.
    /// </summary>
    public partial class MainWindow : Window
    {
        private byte[]? privateKey;
        private byte[]? publicKey;

        /// <summary>
        /// Initializes the window and binds button click event handlers.
        /// </summary>
        public MainWindow()
        {
            InitializeComponent();
            GenerateKeyPairButton.Click += GenerateKeyPairButton_Click;
            SaveToUSBButton.Click += SaveToUSBButton_Click;
        }

        /// <summary>
        /// Handles the event when the "Generate Key Pair" button is clicked.
        /// It generates a new RSA key pair using the PIN and updates the status.
        /// </summary>
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

        /// <summary>
        /// Handles the event when the "Save to USB" button is clicked.
        /// It attempts to find a USB drive and save the encrypted private key and public key.
        /// </summary>
        private void SaveToUSBButton_Click(object sender, RoutedEventArgs e)
        {
            if (privateKey is null || publicKey is null)
            {
                StatusBlock.Text = "Status: Please generate the key pair first.";
                return;
            }

            try
            {
                StatusBlock.Text = "Status: Searching for USB drive...";

                // Look for the first available removable USB drive
                string? usbPath = DriveInfo.GetDrives()
                    .Where(d => d.DriveType == DriveType.Removable && d.IsReady)
                    .Select(d => d.RootDirectory.FullName)
                    .FirstOrDefault();

                if (string.IsNullOrEmpty(usbPath))
                {
                    StatusBlock.Text = "Status: No USB drive found.";
                    return;
                }

                // Save encrypted private key
                File.WriteAllBytes(Path.Combine(usbPath, "privateKey.enc"), privateKey);

                // Convert and save public key in PEM format
                string base64Key = Convert.ToBase64String(publicKey);
                string pem = "-----BEGIN PUBLIC KEY-----\n" +
                             InsertLineBreaks(base64Key, 64) +
                             "\n-----END PUBLIC KEY-----";
                File.WriteAllText(Path.Combine(usbPath, "publicKey.pem"), pem);

                StatusBlock.Text = $"Status: Keys saved to USB ({usbPath}) successfully.";
            }
            catch (IOException ex)
            {
                StatusBlock.Text = "Status: Error writing to USB - " + ex.Message;
            }
        }

        /// <summary>
        /// Inserts line breaks into a base64 string to comply with PEM formatting.
        /// </summary>
        /// <param name="input">The base64 string.</param>
        /// <param name="lineLength">Number of characters per line.</param>
        /// <returns>Formatted string with line breaks.</returns>
        private static string InsertLineBreaks(string input, int lineLength)
        {
            return string.Join("\n", Enumerable.Range(0, (input.Length + lineLength - 1) / lineLength)
                .Select(i => input.Substring(i * lineLength, Math.Min(lineLength, input.Length - i * lineLength))));
        }

    }
}
