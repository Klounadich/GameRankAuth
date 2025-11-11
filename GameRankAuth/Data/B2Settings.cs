namespace GameRankAuth.Data;

public record B2Settings
{
    public string KeyId { get; set; } = string.Empty;
    public string ApplicationKey { get; set; } = string.Empty;
    public string BucketId { get; set; } = string.Empty;
    public string BucketName { get; set; } = string.Empty;
}