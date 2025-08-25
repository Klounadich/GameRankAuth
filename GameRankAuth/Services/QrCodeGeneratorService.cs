using GameRankAuth.Interfaces;
using QRCoder;
using System.Security.Cryptography;
namespace GameRankAuth.Services;

public class QrCodeGeneratorService : IQrCodeGeneratorService
{
    public async Task<byte[]> GenerateQrCodeImage()
    {
        string endpointLink = "http://192.168.0.103:5001/api/auth/qr-code-check";
        Guid qrcodeId = Guid.NewGuid();
        qrcodeId.ToString();
        byte[] rndbytes = new byte[32];
        using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
        {
            rng.GetBytes(rndbytes);
        }
        string secretToken = Convert.ToHexString(rndbytes).ToLower();
        var expires = DateTime.Now.AddMinutes(5);
        string link = endpointLink + "/" + qrcodeId + "/"  + secretToken + "/" + expires;
        QRCodeGenerator QrcodeGen =  new QRCodeGenerator();
        var Data = QrcodeGen.CreateQrCode( link, QRCodeGenerator.ECCLevel.Q);
        var QrCode = new PngByteQRCode(Data);
        byte [] qrcodeimage = QrCode.GetGraphic(20);
        return qrcodeimage;
    }
}