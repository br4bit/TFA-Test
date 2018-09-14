using Gma.QrCodeNet.Encoding;
using Gma.QrCodeNet.Encoding.Windows.Render;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TwoFactorAuthNet.Providers.Qr;


namespace TFA_Test
{
    class MyQRProvider : IQrCodeProvider
    {
        string IQrCodeProvider.GetMimeType()
        {
            return "image/png";
        }

        byte[] IQrCodeProvider.GetQrCodeImage(string text, int size)
        {
            var encoder = new QrEncoder();
            var qrCode = encoder.Encode(text);

            var renderer = new GraphicsRenderer(new FixedCodeSize(size, QuietZoneModules.Two));
            byte[] result;
            using (var stream = new MemoryStream())
            {
                renderer.WriteToStream(qrCode.Matrix, ImageFormat.Png, stream);
                result = stream.ToArray();
            }

            return result;
        }
    }
}
