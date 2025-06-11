using System;
using System.IO;
using System.Security.Cryptography;

namespace PAdES_SignatureApp
{
    /// <summary>
    /// Provides cryptographic helper methods for decrypting private keys.
    /// </summary>
    public class CryptoHelper
    {
        /// <summary>
        /// Decrypts an AES-encrypted private key using the provided PIN.
        /// Assumes the input data format contains a 16-byte salt followed by a 16-byte IV, then the encrypted key data.
        /// </summary>
        /// <param name="data">The encrypted private key data including salt and IV.</param>
        /// <param name="pin">The PIN used to derive the AES encryption key.</param>
        /// <returns>The decrypted private key bytes.</returns>
        public static byte[] DecryptPrivateKey(byte[] data, string pin)
        {
            using var aes = Aes.Create();

            using var ms = new MemoryStream(data);

            // Read salt and initialization vector from the beginning of the stream
            byte[] salt = new byte[16];
            byte[] iv = new byte[16];
            ms.Read(salt, 0, 16);
            ms.Read(iv, 0, 16);

            // Derive AES key from the PIN and salt using PBKDF2 with SHA-256
            var key = new Rfc2898DeriveBytes(pin, salt, 100_000, HashAlgorithmName.SHA256);
            aes.Key = key.GetBytes(32);
            aes.IV = iv;

            // Decrypt the remaining stream data
            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }
    }
}
