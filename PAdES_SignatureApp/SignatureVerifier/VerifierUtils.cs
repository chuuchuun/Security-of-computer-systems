using System;

namespace SignatureVerifier
{
    public static class SignatureVerifierUtils
    {
        public static byte[] ExtractHashFromMetadata(string metadata)
        {
            const string marker = "Hash:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("Original hash not found in metadata.");

            string base64 = metadata[(index + marker.Length)..].Split('|')[0].Trim();
            return Convert.FromBase64String(base64);
        }

        public static byte[] ExtractSignatureFromMetadata(string metadata)
        {
            const string marker = "PAdES_Signature:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("Signature not found in metadata.");

            string base64 = metadata[(index + marker.Length)..].Split('|')[0].Trim();
            return Convert.FromBase64String(base64);
        }

        public static byte[] LoadPublicKeyFromPem(string pem)
        {
            const string header = "-----BEGIN PUBLIC KEY-----";
            const string footer = "-----END PUBLIC KEY-----";

            int start = pem.IndexOf(header);
            int end = pem.IndexOf(footer);

            if (start < 0 || end < 0 || end <= start)
                throw new Exception("Invalid PEM format. Could not find public key markers.");

            start += header.Length;
            string base64 = pem[start..end]
                               .Replace("\n", "")
                               .Replace("\r", "")
                               .Trim();

            if (string.IsNullOrWhiteSpace(base64))
                throw new Exception("PEM content is empty or improperly formatted.");

            return Convert.FromBase64String(base64);
        }
    }
}
