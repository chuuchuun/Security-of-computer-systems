using PdfSharpCore.Pdf;
using PdfSharpCore.Pdf.IO;
using System;
using System.IO;
using System.Reflection.PortableExecutable;
using System.Security.Cryptography;
using System.Text;

namespace PAdES_SignerApp
{
    public static class Signer
    {
        public static string SignPdf(string filePath, byte[] privateKeyBytes)
        {
            // Load PDF and compute hash
            byte[] fileBytes = File.ReadAllBytes(filePath);
            byte[] hash = SHA256.HashData(fileBytes);

            // Sign hash
            using var rsa = RSA.Create();
            rsa.ImportRSAPrivateKey(privateKeyBytes, out _);
            byte[] signature = rsa.SignHash(hash, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);

            // Embed the signature
            string outputPath = Path.Combine(Path.GetDirectoryName(filePath), "Signed_" + Path.GetFileName(filePath));
            using var doc = PdfReader.Open(filePath, PdfDocumentOpenMode.Modify);
            var page = doc.Pages[0];
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            gfx.DrawString("Signed: ✅", new PdfSharpCore.Drawing.XFont("Arial", 12), PdfSharpCore.Drawing.XBrushes.Black, 40, 40);

            // Append metadata
            doc.Info.Keywords += $"|PAdES_Signature:{Convert.ToBase64String(signature)}";

            doc.Save(outputPath);
            return outputPath;
        }
    }
}
