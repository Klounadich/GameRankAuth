using Microsoft.AspNetCore.Mvc;

namespace GameRankAuth.Interfaces;

public interface IAvatarService
{
    Task UploadAvatar(IFormFile file, string Id);
    
    Task<FileStreamResult> LoadAvatar(string Id);
}