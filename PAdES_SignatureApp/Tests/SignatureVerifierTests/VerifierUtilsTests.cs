using System;
using Xunit;
using SignatureVerifierApp;
using SignatureVerifier;

namespace Tests.SignatureVerifierTests
{
    public class SignatureVerifierUtilsTests
    {
        [Fact]
        public void ExtractSignatureFromMetadata_ShouldReturnCorrectBytes()
        {
            byte[] expected = Convert.FromBase64String("U2lnbmF0dXJl");
            string metadata = "Some|PAdES_Signature:U2lnbmF0dXJl|Hash:SGFzaA==";

            var actual = SignatureVerifierUtils.ExtractSignatureFromMetadata(metadata);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void ExtractHashFromMetadata_ShouldReturnCorrectBytes()
        {
            byte[] expected = Convert.FromBase64String("SGFzaA==");
            string metadata = "Other|PAdES_Signature:U2ln|Hash:SGFzaA==";

            var actual = SignatureVerifierUtils.ExtractHashFromMetadata(metadata);
            Assert.Equal(expected, actual);
        }

        [Fact]
        public void LoadPublicKeyFromPem_ShouldExtractCorrectBytes()
        {
            byte[] original = Convert.FromBase64String("cHVibGlja2V5");
            string pem = "-----BEGIN PUBLIC KEY-----\n" +
                         "cHVibGlja2V5\n" +
                         "-----END PUBLIC KEY-----";

            var actual = SignatureVerifierUtils.LoadPublicKeyFromPem(pem);
            Assert.Equal(original, actual);
        }

        [Fact]
        public void LoadPublicKeyFromPem_ShouldThrow_WhenInvalidPEM()
        {
            string badPem = "no headers, just text";
            Assert.Throws<Exception>(() => SignatureVerifierUtils.LoadPublicKeyFromPem(badPem));
        }
    }
}
