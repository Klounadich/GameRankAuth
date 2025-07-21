using GameRankAuth.Models;
namespace GameRankAuth.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResult> RegisterAsync(RegisterRequest request);

        Task<AuthResult> LogInAsync(LoginRequest request);
        
        
    }
    
}
