namespace GameRankAuth.Interfaces;

public interface IAvatarService
{
    Task UploadAvatar(IFormFile file);
}