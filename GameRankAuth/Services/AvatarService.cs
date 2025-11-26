using GameRankAuth.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using GameRankAuth.Models;
using Microsoft.AspNetCore.Mvc;

namespace GameRankAuth.Services;

public class AvatarService : IAvatarService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<AvatarService> _logger;
    public AvatarService(ILogger<AvatarService> logger , IConfiguration configuration)
    {
        _configuration = configuration;
        _logger = logger;
    }
    public async Task UploadAvatar(IFormFile file, string Id)
    {
        var keyId = _configuration["B2Settings:KeyId"];
        var applicationKey = _configuration["B2Settings:ApplicationKey"];
        var bucketId = _configuration["B2Settings:BucketId"];
        var b2Service = new B2Service(keyId, applicationKey, bucketId);
        await using var stream = new MemoryStream();
        await file.CopyToAsync(stream);
        var fileBytes = stream.ToArray();
        var fileExtension = Path.GetExtension(file.FileName).ToLowerInvariant();


        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif" };
        if (!allowedExtensions.Contains(fileExtension))
        {
            throw new ArgumentException("Not supported file extension.");
        }


        var fileName = $"avatars/{Id}_{Guid.NewGuid()}{fileExtension}";

        await b2Service.UploadFileAsync(fileData: fileBytes, fileName: fileName, contentType: file.ContentType);
        var client = new MongoClient(_configuration["MongoDBConnection:Connection"]); 
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
        var keyId = _configuration["B2Settings:KeyId"];
        var applicationKey = _configuration["B2Settings:ApplicationKey"];
        var bucketId = _configuration["B2Settings:BucketId"];
        var b2Service = new B2Service(keyId, applicationKey, bucketId);
        var client = new MongoClient(_configuration["MongoDBConnection:Connection"]); 
        var database = client.GetDatabase("test");
        var collection = database.GetCollection<Avatar>("avatars");
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