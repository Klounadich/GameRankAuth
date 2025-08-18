using GameRankAuth.Interfaces;

namespace GameRankAuth.Services;

public class AvatarService : IAvatarService
{
    public async Task UploadAvatar(IFormFile file)
    {
        var keyId = "003cafa7b13f5090000000001";
        var applicationKey = "K003DY8bRqYVeEW3vr0WszLHDbwJdnY";
        var bucketId = "GameRankAvatars";
        var b2Service = new B2Service(keyId, applicationKey, bucketId);
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var fileBytes = stream.ToArray();
        Console.WriteLine("Перевели в байты");
        Console.WriteLine(fileBytes.Length);
        var uploadFile = await b2Service.UploadFileAsync(fileData:fileBytes, fileName:file.FileName , contentType:file.ContentType);
        Console.WriteLine("Закинули в боб");
    }
}