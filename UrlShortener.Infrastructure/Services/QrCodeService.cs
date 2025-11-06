using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;
using UrlShortener.Application.Interfaces;

namespace UrlShortener.Infrastructure.Services
{
    public class QrCodeService : IQrCodeService
    {
        public byte[] GenerateQrCode(string text)
        {
            using var qrGenerator = new QRCodeGenerator();
            var qrData = qrGenerator.CreateQrCode(text, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new QRCode(qrData);

            using Bitmap bitmap = qrCode.GetGraphic(20);
            using var stream = new MemoryStream();
            bitmap.Save(stream, ImageFormat.Png);

            return stream.ToArray();
        }
    }
}
