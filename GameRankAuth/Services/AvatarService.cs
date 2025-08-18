using GameRankAuth.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using GameRankAuth.Models;

namespace GameRankAuth.Services;

public class AvatarService : IAvatarService
{
    public async Task UploadAvatar(IFormFile file , string Id)
    {
        var keyId = "003cafa7b13f5090000000001";
        var applicationKey = "K003DY8bRqYVeEW3vr0WszLHDbwJdnY";
        var bucketId = "3cfafffa072ba1239f850019";
        var b2Service = new B2Service(keyId, applicationKey, bucketId);
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var fileBytes = stream.ToArray();
        var getLink = "testtes";
        
         await b2Service.UploadFileAsync(fileData:fileBytes, fileName:file.FileName , contentType:file.ContentType);
        var client = new MongoClient("mongodb://localhost:27017"); // в апсетинг запульнуть 
        var database = client.GetDatabase("admin");
        var collection = database.GetCollection<Avatar>("avatars");
        var addAvatar = new Avatar
        {
            Id = Id,
            Link = getLink
        };

    }
}