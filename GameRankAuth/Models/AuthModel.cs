namespace GameRankAuth.Models;

public class AuthModel
{
    
    
}

public class RegisterRequest
{
    public string? UserName { get; set; }
    public string? Password { get; set; }
    public string? Email { get; set; }
    public string? Id { get; set; }

}

public class LoginRequest
{
    public string? Username { get; set; }
    public string? Password { get; set; }
}

public class AuthResult
{
    public bool Success { get; set; }
    public string? Token { get; set; }
    public string[] Errors { get; set; } = [];
}