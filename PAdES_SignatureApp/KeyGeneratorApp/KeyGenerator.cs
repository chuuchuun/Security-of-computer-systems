using System;
using System.IO;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Text;

namespace KeyGeneratorApp
{
    public class KeyPairResult
    {
        public byte[] EncryptedPrivateKey { get; set; }
        public byte[] PublicKey { get; set; }
    }

    public static class KeyGenerator
    {
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

        private static byte[] AesEncrypt(byte[] data, string password)
        {
            using var aes = Aes.Create();
            var salt = GenerateRandomBytes(16);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000);

            aes.Key = key.GetBytes(32);
            aes.IV = GenerateRandomBytes(16);

            using var ms = new MemoryStream();
            ms.Write(salt);
            ms.Write(aes.IV);

            using (var cs = new CryptoStream(ms, aes.CreateEncryptor(), CryptoStreamMode.Write))
            {
                cs.Write(data, 0, data.Length);
            }

            return ms.ToArray();
        }

        private static byte[] GenerateRandomBytes(int length)
        {
            byte[] bytes = new byte[length];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(bytes);
            return bytes;
        }
    }
}
