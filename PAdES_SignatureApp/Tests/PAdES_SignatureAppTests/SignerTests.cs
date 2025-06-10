using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Xunit;
using PdfSharpCore.Pdf.IO;
using PAdES_SignatureApp;

namespace Tests.PAdES_SignatureAppTests
{
    public class SignerTests
    {
        [Fact]
        public void SignPdf_ShouldCreateSignedPdf_WithExpectedMetadata()
        {
            string tempPdfPath = CreateTempPdf();
            byte[] privateKeyBytes;
            using (var rsa = RSA.Create(2048))
            {
                privateKeyBytes = rsa.ExportRSAPrivateKey();
            }
            string signedPath = Signer.SignPdf(tempPdfPath, privateKeyBytes);

            Assert.True(File.Exists(signedPath), "Signed PDF file was not created");

            using var doc = PdfReader.Open(signedPath, PdfDocumentOpenMode.ReadOnly);
            Assert.Contains("PAdES_Signature:", doc.Info.Keywords);
            Assert.Contains("Hash:", doc.Info.Keywords);

            File.Delete(tempPdfPath);
            File.Delete(signedPath);
        }

        static private string CreateTempPdf()
        {
            string tempPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".pdf");

            using var doc = new PdfSharpCore.Pdf.PdfDocument();
            var page = doc.AddPage();
            var gfx = PdfSharpCore.Drawing.XGraphics.FromPdfPage(page);
            gfx.DrawString("Test PDF", new PdfSharpCore.Drawing.XFont("Arial", 14), PdfSharpCore.Drawing.XBrushes.Black, 100, 100);
            doc.Save(tempPath);

            return tempPath;
        }
    }
}
