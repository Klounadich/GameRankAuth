using GameRankAuth.Models;
namespace GameRankAuth.Interfaces;

public interface IVerifyService
{
    Task<AuthResult> VerifyEmailAsync(string id);
}