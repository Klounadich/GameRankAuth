namespace GameRankAuth.Models;

public class UserData
{
    public class ChangeUserName
    {
        public string UserName { get; set; }
    }

    public class ChangePasswordRequest
    {
        public string OldPassword { get; set; }
        public string NewPassword { get; set; }
    }

    public class ChangeEmail
    {
        public string Email { get; set; }
    }

    public class UserResult
    {
        public bool Success { get; set; }
        public string? Token { get; set; }
        public string[] Errors { get; set; } = [];
    }

    public class UserDescription
    {
        public string Id { get; set; }
        public string Description { get; set; }
    }
}