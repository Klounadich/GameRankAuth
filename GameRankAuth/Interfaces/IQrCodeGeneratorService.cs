using GameRankAuth.Models;

namespace GameRankAuth.Interfaces;

public interface IQrCodeGeneratorService
{
    string GetLastGeneratedQrId();
    Task<QrGenerationResult>  GenerateQrCodeImage();
}
