namespace GameRankAuth.Interfaces;

public interface IQrCodeGeneratorService
{
    Task<byte[]>  GenerateQrCodeImage();
}