using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace PAdES_SignatureApp
{
    public static class Signer
    {
        public static string SignPdf(string filePath, byte[] privateKeyBytes)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new ArgumentException("File path cannot be null or empty.", nameof(filePath));
            }

            string? directoryPath = Path.GetDirectoryName(filePath) ?? throw new InvalidOperationException("The directory path could not be determined.");
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] hash = SHA256.HashData(fileBytes);

            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            string outputPath = Path.Combine(directoryPath, "Signed_" + Path.GetFileName(filePath));
            using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);
            var page = doc.Pages[0];
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            gfx.DrawString("Signed: ✅", new PdfSharpCore.Drawing.XFont("Arial", 12), PdfSharpCore.Drawing.XBrushes.Black, 40, 40);

            string base64Signature = Convert.ToBase64String(signature);
            string base64Hash = Convert.ToBase64String(hash);

            doc.Info.Keywords += $"|PAdES_Signature:{base64Signature}|Hash:{base64Hash}";
            doc.Save(outputPath);

            return outputPath;
        }
    }
}
