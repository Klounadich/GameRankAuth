namespace GameRankAuth.Models;

public class QRModel
{
    public string QrcodeId { get; set; }
    public string token { get; set; }
    
}

public class SessionQr
{
    public string QrcodeId { get; set; }
    public string token { get; set; }
    public string userid { get; set; }
    public string username { get; set; }
    public string Email  { get; set; }
    public string Status { get; set; }
    public string Role { get; set; }
}

public class QrGenerationResult
{
    public byte[] ImageData { get; set; }
    public string QrId { get; set; }
    public string Token { get; set; }
    public DateTime ExpiresAt { get; set; }
}