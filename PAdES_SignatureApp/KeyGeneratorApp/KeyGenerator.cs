using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KeyGeneratorApp
{
    /// <summary>
    /// Represents a result of key generation, containing the encrypted private key and the public key.
    /// </summary>
    public class KeyPairResult
    {
        public byte[]? EncryptedPrivateKey { get; set; }
        public byte[]? PublicKey { get; set; }
    }

    /// <summary>
    /// Class responsible for generating RSA key pairs and encrypting the private key.
    /// </summary>
    public class KeyGenerator
    {
        /// <summary>
        /// Generates an RSA key pair, encrypts the private key using a PIN, and returns both keys.
        /// </summary>
        /// <param name="pin">The user-provided PIN used to encrypt the private key.</param>
        /// <returns>A KeyPairResult containing the encrypted private key and the public key.</returns>
        public static KeyPairResult GenerateKeyPair(string pin)
        {
            using var rsa = new RSACryptoServiceProvider(4096);
            var privateKeyBytes = rsa.ExportRSAPrivateKey();
            var publicKeyBytes = rsa.ExportSubjectPublicKeyInfo();

            var encryptedPrivateKey = AesEncrypt(privateKeyBytes, pin);

            return new KeyPairResult
            {
                EncryptedPrivateKey = encryptedPrivateKey,
                PublicKey = publicKeyBytes
            };
        }


        /// <summary>
        /// Encrypts the input data using AES encryption with a password-derived key.
        /// </summary>
        /// <param name="data">The data to encrypt (e.g. private key bytes).</param>
        /// <param name="password">The password used to derive the AES key.</param>
        /// <returns>Encrypted byte array including the salt and IV at the beginning.</returns>
        private static byte[] AesEncrypt(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var salt = GenerateRandomBytes(16);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256); 

            aes.Key = key.GetBytes(32);
            aes.IV = GenerateRandomBytes(16);

            using var ms = new MemoryStream();
            ms.Write(salt, 0, salt.Length);
            ms.Write(aes.IV, 0, aes.IV.Length);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
                cs.FlushFinalBlock();
            }

            return ms.ToArray();
        }

        /// <summary>
        /// Generates a cryptographically secure random byte array of the specified length.
        /// </summary>
        /// <param name="length">The number of random bytes to generate.</param>
        /// <returns>A byte array filled with random bytes.</returns>
        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }
    }
}
