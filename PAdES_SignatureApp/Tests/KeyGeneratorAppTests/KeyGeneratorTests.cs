using System;
using System.Security.Cryptography;
using Xunit;
using KeyGeneratorApp;

namespace Tests.KeyGeneratorAppTests
{
    public class KeyGeneratorTests
    {
        [Fact]
        public void GenerateKeyPair_ShouldReturnNonNullKeys()
        {
            var pin = "1234";

            var result = KeyGenerator.GenerateKeyPair(pin);

            Assert.NotNull(result);
            Assert.NotNull(result.EncryptedPrivateKey);
            Assert.NotNull(result.PublicKey);
            Assert.NotEmpty(result.EncryptedPrivateKey);
            Assert.NotEmpty(result.PublicKey);
        }

        [Fact]
        public void GenerateKeyPair_ShouldProduceDifferentEncryptedKeys_WithDifferentPINs()
        {
            var result1 = KeyGenerator.GenerateKeyPair("pin1");
            var result2 = KeyGenerator.GenerateKeyPair("pin2");

            Assert.NotNull(result1.EncryptedPrivateKey);
            Assert.NotNull(result2.EncryptedPrivateKey);
            Assert.NotEqual(Convert.ToBase64String(result1.EncryptedPrivateKey!), Convert.ToBase64String(result2.EncryptedPrivateKey!));
        }

        [Fact]
        public void GenerateKeyPair_ShouldProduceDifferentEncryptedKeys_EvenWithSamePIN()
        {
            var result1 = KeyGenerator.GenerateKeyPair("mypin");
            var result2 = KeyGenerator.GenerateKeyPair("mypin");

            Assert.NotNull(result1.EncryptedPrivateKey);
            Assert.NotNull(result2.EncryptedPrivateKey);
            Assert.NotEqual(Convert.ToBase64String(result1.EncryptedPrivateKey!), Convert.ToBase64String(result2.EncryptedPrivateKey!));
        }

        [Fact]
        public void GenerateKeyPair_PublicKeyShouldBeValid()
        {
            var result = KeyGenerator.GenerateKeyPair("test");

            Assert.NotNull(result.PublicKey);
            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(result.PublicKey!, out _);

            Assert.NotNull(rsa);
        }
    }
}
