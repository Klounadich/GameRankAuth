using GameRankAuth.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameRankAuth.Services;

public class AvatarService : IAvatarService
{
    private readonly ILogger<AvatarService> _logger;
    public AvatarService(ILogger<AvatarService> logger)
    {
        _logger = logger;
    }
    public async Task UploadAvatar(IFormFile file, string Id)
    {
        var keyId = "003cafa7b13f5090000000001";
        var applicationKey = "K003DY8bRqYVeEW3vr0WszLHDbwJdnY";
        var bucketId = "3cfafffa072ba1239f850019";
        var b2Service = new B2Service(keyId, applicationKey, bucketId);
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var fileBytes = stream.ToArray();
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();


        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Недопустимый формат файла");
        }


        var fileName = $"avatars/{Id}_{Guid.NewGuid()}{fileExtension}";

        await b2Service.UploadFileAsync(fileData: fileBytes, fileName: fileName, contentType: file.ContentType);
        var client = new MongoClient("mongodb://localhost:27017"); // в апсетинг запульнуть 
        var database = client.GetDatabase("test");
        var collection = database.GetCollection<Avatar>("avatars");
        var existingAvatar = await collection.Find(a => a.Id == Id).FirstOrDefaultAsync();

        if (existingAvatar != null)
        {
            
            try
            {
                var fileid = await b2Service.GetFileIdByNameAsync(existingAvatar.Link);
                
                await b2Service.DeleteFileAsync(fileid, existingAvatar.Link);
               
            }
            catch (Exception ex)
            {
                _logger.LogWarning($"Delete old Image Error: {ex.Message}");
                
            }

            
            var update = Builders<Avatar>.Update.Set(a => a.Link, fileName);
            await collection.UpdateOneAsync(a => a.Id == Id, update);
            _logger.LogInformation($"Avatar upd for user {Id}: {fileName}");
        }
        else
        {
            var addAvatar = new Avatar
            {
                Id = Id,
                Link = fileName,
            };
            
            collection.InsertOne(addAvatar);
            

        }
    }

    public async Task<FileStreamResult> LoadAvatar(string Id)
    {
        var keyId = "003cafa7b13f5090000000001";
        var applicationKey = "K003DY8bRqYVeEW3vr0WszLHDbwJdnY";
        var bucketId = "3cfafffa072ba1239f850019";
        var b2Service = new B2Service(keyId, applicationKey, bucketId);
        var client = new MongoClient("mongodb://localhost:27017"); // в апсетинг запульнуть 
        var database = client.GetDatabase("test");
        var collection = database.GetCollection<Avatar>("avatars");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", Id);
        var projection = Builders<BsonDocument>.Projection.Include("Link").Exclude("_id");
        var avatarLink = await collection.Find(a => a.Id == Id).FirstOrDefaultAsync();
        if (avatarLink == null || string.IsNullOrEmpty(avatarLink.Link))
        {
            const string defaultAvatarPath = "avatars/default-avatar.jpg";
            var stream1 = await b2Service.GetAvatarStreamAsync(defaultAvatarPath);
            return new FileStreamResult(stream1, "image/jpg");
            
        }
       
        var contentType = avatarLink.Link.EndsWith(".png") ? "image/png" : 
            avatarLink.Link.EndsWith(".jpg") ? "image/jpeg" : 
            "image/jpeg";
        var stream = await b2Service.GetAvatarStreamAsync(avatarLink.Link);
        

        return new FileStreamResult(stream, contentType);
    }
}