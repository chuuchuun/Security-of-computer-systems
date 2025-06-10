using System;
using System.IO;
using System.Security.Cryptography;
using Xunit;
using PAdES_SignatureApp;

namespace Tests.PAdES_SignatureAppTests
{
    public class CryptoHelperTests
    {
        [Fact]
        public void DecryptPrivateKey_ShouldReturnOriginalData_WhenCorrectPinProvided()
        {
            string pin = "securePIN";
            byte[] originalData = System.Text.Encoding.UTF8.GetBytes("MyPrivateKeyData");

            byte[] encryptedData = EncryptWithAes(originalData, pin);

            byte[] decryptedData = CryptoHelper.DecryptPrivateKey(encryptedData, pin);

            Assert.Equal(originalData, decryptedData);
        }

        [Fact]
        public void DecryptPrivateKey_ShouldThrowException_WhenIncorrectPinProvided()
        {
            string correctPin = "correctPIN";
            string wrongPin = "wrongPIN";
            byte[] originalData = System.Text.Encoding.UTF8.GetBytes("MyPrivateKeyData");

            byte[] encryptedData = EncryptWithAes(originalData, correctPin);

            Assert.Throws<CryptographicException>(() =>
            {
                CryptoHelper.DecryptPrivateKey(encryptedData, wrongPin);
            });
        }

        private static byte[] EncryptWithAes(byte[] data, string password)
        {
            using var aes = Aes.Create();
            byte[] salt = GenerateRandomBytes(16);
            var key = new Rfc2898DeriveBytes(password, salt, 100_000, HashAlgorithmName.SHA256); 

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
