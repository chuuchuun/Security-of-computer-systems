using System;
using System.IO;
using System.Security.Cryptography;

namespace PAdES_SignerApp
{
    public static class CryptoHelper
    {
        public static byte[] DecryptPrivateKey(byte[] data, string pin)
        {
            using var aes = Aes.Create();

            using var ms = new MemoryStream(data);
            byte[] salt = new byte[16];
            byte[] iv = new byte[16];
            ms.Read(salt, 0, 16);
            ms.Read(iv, 0, 16);

            var key = new Rfc2898DeriveBytes(pin, salt, 100_000);
            aes.Key = key.GetBytes(32);
            aes.IV = iv;

            using var cs = new CryptoStream(ms, aes.CreateDecryptor(), CryptoStreamMode.Read);
            using var result = new MemoryStream();
            cs.CopyTo(result);
            return result.ToArray();
        }
    }
}
