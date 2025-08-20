using GameRankAuth.Models;
namespace GameRankAuth.Interfaces;

public interface IGoogleOauthService
{
    Task<AuthResult> AuthAsync(string clientId, string clientSecret, string code);
}