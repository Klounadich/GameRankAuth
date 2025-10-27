using GameRankAuth.Interfaces;
using QRCoder;
using GameRankAuth.Models;
using System.Security.Cryptography;
using Newtonsoft.Json;
using StackExchange.Redis;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace GameRankAuth.Services;

public class QrCodeGeneratorService : IQrCodeGeneratorService
{
    private readonly IDatabase _redis;
    private const int ExpireSession = 5;
    private QrGenerationResult _lastResult;

    public QrCodeGeneratorService(IConnectionMultiplexer redis)
    {
        _redis = redis.GetDatabase();
        
    }

    private async Task SaveSessionAsync(string qrcodeId, string token, DateTime expire)
    {
        var session = new
        {
            qrcode_id = qrcodeId,
            token = token,
            Expire = expire,
            Created = DateTime.Now,
            Status = "pending"
        };
        string serialize = JsonSerializer.Serialize(session);
        string redisKey = $"qr:{qrcodeId}";
        TimeSpan expiry = expire - DateTime.Now;
        await _redis.StringSetAsync(redisKey, serialize, expiry);
      
    }
    
    public async Task<QrGenerationResult> GenerateQrCodeImage()
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
        await SaveSessionAsync(qrcodeId.ToString(), secretToken, expires);
        string link = endpointLink + "/" + qrcodeId + "/"  + secretToken + "/" + expires;
        QRCodeGenerator QrcodeGen =  new QRCodeGenerator();
        var Data = QrcodeGen.CreateQrCode( link, QRCodeGenerator.ECCLevel.Q);
        var QrCode = new PngByteQRCode(Data);
        byte [] qrcodeimage = QrCode.GetGraphic(20);
        _lastResult = new QrGenerationResult
        {
            ImageData = qrcodeimage,
            QrId = qrcodeId.ToString(),
            Token = secretToken,
            ExpiresAt = expires
        };
        return _lastResult;
    }
    
    public string GetLastGeneratedQrId()
    {
        return _lastResult?.QrId;
    }
}