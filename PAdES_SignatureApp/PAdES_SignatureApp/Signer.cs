using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.IO;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace PAdES_SignatureApp
{
    /// <summary>
    /// Provides functionality to digitally sign PDF documents using RSA private keys.
    /// </summary>
    public class Signer
    {
        /// <summary>
        /// Signs a PDF file using the provided RSA private key bytes.
        /// The method adds a visible "Signed: ✅" annotation to the first page and embeds signature metadata in PDF info keywords.
        /// </summary>
        /// <param name="filePath">The path to the PDF file to be signed.</param>
        /// <param name="privateKeyBytes">The RSA private key in PKCS#1 or PKCS#8 format.</param>
        /// <returns>The full path of the newly saved signed PDF file.</returns>
        /// <exception cref="ArgumentException">Thrown when <paramref name="filePath"/> is null or empty.</exception>
        /// <exception cref="InvalidOperationException">Thrown when the directory of the file path cannot be determined.</exception>
        public static string SignPdf(string filePath, byte[] privateKeyBytes)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            string? directoryPath = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("The directory path could not be determined.");
            
            // Read all bytes from the PDF file
            byte[] fileBytes = File.ReadAllBytes(filePath);

            // Compute SHA256 hash of the PDF bytes
            byte[] hash = SHA256.HashData(fileBytes);

            using var rsa = RSA.Create();

            // Import the private RSA key bytes
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);

            // Create digital signature for the hash
            byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Prepare output file path
            string outputPath = Path.Combine(directoryPath, "Signed_" + Path.GetFileName(filePath));

            // Open PDF document in modify mode
            using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);

            // Add visual text "Signed" on first page at position (40,40)
            var page = doc.Pages[0];
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            gfx.DrawString("Signed", new PdfSharpCore.Drawing.XFont("Arial", 12), PdfSharpCore.Drawing.XBrushes.Black, 40, 40);

            // Prepare signature metadata strings in base64 and ISO 8601 datetime format
            string base64Signature = Convert.ToBase64String(signature);
            string base64Hash = Convert.ToBase64String(hash);
            string signingTime = DateTime.UtcNow.ToString("o");
            string signerName = Environment.UserName;
            string signingReason = "Document approval";
            string signingLocation = "Gdansk, Poland";

            // Append signature metadata into PDF document info keywords for potential later extraction
            doc.Info.Keywords +=
              $"|PAdES_Signature:{base64Signature}" +
              $"|Hash:{base64Hash}" +
              $"|SigningTime:{signingTime}" +
              $"|SignerName:{signerName}" +
              $"|SigningReason:{signingReason}" +
              $"|SigningLocation:{signingLocation}";

            // Save modified PDF with the signature annotation and metadata
            doc.Save(outputPath);

            return outputPath;
        }
    }
}
