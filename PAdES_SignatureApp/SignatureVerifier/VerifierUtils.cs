using System;

namespace SignatureVerifier
{
    public class SignatureVerifierUtils
    {
        /// <summary>
        /// Extracts the base64-encoded original hash from the PDF metadata string.
        /// </summary>
        /// <param name="metadata">The PDF metadata string containing the hash.</param>
        /// <returns>The original hash as a byte array.</returns>
        /// <exception cref="Exception">Thrown if the hash marker is not found.</exception>
        public static byte[] ExtractHashFromMetadata(string metadata)
        {
            const string marker = "Hash:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("Original hash not found in metadata.");

            string base64 = metadata[(index + marker.Length)..].Split('|')[0].Trim();
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Extracts the base64-encoded digital signature from the PDF metadata string.
        /// </summary>
        /// <param name="metadata">The PDF metadata string containing the signature.</param>
        /// <returns>The digital signature as a byte array.</returns>
        /// <exception cref="Exception">Thrown if the signature marker is not found.</exception>
        public static byte[] ExtractSignatureFromMetadata(string metadata)
        {
            const string marker = "PAdES_Signature:";
            int index = metadata.IndexOf(marker);
            if (index < 0)
                throw new Exception("Signature not found in metadata.");

            string base64 = metadata[(index + marker.Length)..].Split('|')[0].Trim();
            return Convert.FromBase64String(base64);
        }

        /// <summary>
        /// Parses a PEM formatted public key string and returns the DER encoded key bytes.
        /// </summary>
        /// <param name="pem">The PEM formatted string.</param>
        /// <returns>Byte array containing the public key.</returns>
        /// <exception cref="Exception">Thrown if the PEM format is invalid or content is empty.</exception>
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
