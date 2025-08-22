using B2Net;
using GameRankAuth.Interfaces;
using MongoDB.Bson;
using MongoDB.Driver;
using GameRankAuth.Models;
using GameRankAuth.Data;
using Microsoft.AspNetCore.Mvc;

namespace GameRankAuth.Services;

public class AvatarService : IAvatarService
{
    private readonly IB2Service _B2Service;
    private readonly B2Settings _settings;
    

    public AvatarService(  B2Settings settings , IB2Service B2Service)
    {
        _B2Service = B2Service;
        _settings = settings;
       
    }
    public async Task UploadAvatar(IFormFile file, string Id )
    {
        
        
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

        await _B2Service.UploadFileAsync(fileData: fileBytes, fileName: fileName, contentType: file.ContentType);
        var client = new MongoClient("mongodb://localhost:27017"); 
        var database = client.GetDatabase("test");
        var collection = database.GetCollection<Avatar>("avatars");
        var existingAvatar = await collection.Find(a => a.Id == Id).FirstOrDefaultAsync();

        if (existingAvatar != null)
        {
            
            try
            {
                var fileid = await _B2Service.GetFileIdByNameAsync(existingAvatar.Link);
                Console.WriteLine(fileid);
                await _B2Service.DeleteFileAsync(fileid, existingAvatar.Link);
               
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении старого файла: {ex.Message}");
                
            }

            
            var update = Builders<Avatar>.Update.Set(a => a.Link, fileName);
            await collection.UpdateOneAsync(a => a.Id == Id, update);
            
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
        
        
        var client = new MongoClient("mongodb://localhost:27017"); 
        var database = client.GetDatabase("test");
        
        var collection = database.GetCollection<Avatar>("avatars");
        var filter = Builders<BsonDocument>.Filter.Eq("_id", Id);
        var projection = Builders<BsonDocument>.Projection.Include("Link").Exclude("_id");
       
        var avatarLink = await collection.Find(a => a.Id == Id).FirstOrDefaultAsync();
        Console.WriteLine(avatarLink);
        if (avatarLink == null || string.IsNullOrEmpty(avatarLink.Link))
        {
            const string defaultAvatarPath = "avatars/default-avatar.jpg";
            var stream1 = await _B2Service.GetAvatarStreamAsync(defaultAvatarPath);
            return new FileStreamResult(stream1, "image/jpg");
            
        }
        
        var contentType = avatarLink.Link.EndsWith(".png") ? "image/png" : 
            avatarLink.Link.EndsWith(".jpg") ? "image/jpeg" : 
            "image/jpeg";
        var stream = await _B2Service.GetAvatarStreamAsync(avatarLink.Link);
        

        return new FileStreamResult(stream, contentType);
    }
}