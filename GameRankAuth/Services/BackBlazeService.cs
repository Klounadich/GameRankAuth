using B2Net;
using B2Net.Models;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

public class B2Service
{
    private readonly B2Client _client;
    private readonly string _bucketId;
    private readonly HttpClient _httpClient;
    

    public B2Service(string keyId, string applicationKey, string bucketId)
    {
        _client = new B2Client(keyId, applicationKey);
        Console.WriteLine($"KeyId: {keyId}");
        Console.WriteLine($"ApplicationKey: {applicationKey?.Substring(0, 5)}..."); 
        Console.WriteLine($"BucketId: {bucketId}");
        _bucketId = bucketId;
        _httpClient = new HttpClient();
    }

    public async Task<string> GetFileIdByNameAsync(string fileName)
    {
        try
        {
            
            var files = await _client.Files.GetList(fileName, 1, _bucketId);
            
            var file = files.Files.FirstOrDefault(f => f.FileName == fileName);
            return file?.FileId;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Исключение в GetFileIdByNameAsync: {ex.Message}");
            return null;
        }
    }

    public async Task<bool> FileExistsAsync(string fileName)
    {
        try
        {
            var fileId = await GetFileIdByNameAsync(fileName);
            return !string.IsNullOrEmpty(fileId);
        }
        catch
        {
            return false;
        }
    }
    public async Task UploadFileAsync(byte[] fileData, string fileName, Dictionary<string, string> fileInfo = null, string contentType = "b2/x-auto", bool autoRetry = true)
    {
        
        
        
        var uploadUrl = await _client.Files.GetUploadUrl(_bucketId);
        
        Console.WriteLine($"{fileName} /// {uploadUrl} //// {contentType} ////");
        
        
         await _client.Files.Upload(
            fileData: fileData,
            fileName: fileName,
            uploadUrl: uploadUrl,
            contentType: contentType,
            fileInfo: fileInfo,
            autoRetry: autoRetry);
         
    }
    
    public async Task<string> GenerateAvatarUrlAsync(string filePath, string bucketName, TimeSpan? expiry = null)
    {
        try
        {
            var keyId = "003cafa7b13f5090000000001";
            var applicationKey = "K003DY8bRqYVeEW3vr0WszLHDbwJdnY";
        
            
            var authInfo = await B2Client.AuthorizeAsync(keyId, applicationKey);
        
            var baseDownloadUrl = authInfo.DownloadUrl;

          
            var downloadAuth = await _client.Files.GetDownloadAuthorization(
                fileNamePrefix: filePath,
                validDurationInSeconds: (int)(expiry?.TotalSeconds ?? 3600),
                bucketId: bucketName);

           
            return $"{baseDownloadUrl}/file/GameRankAvatars/{Uri.EscapeDataString(filePath)}?Authorization={downloadAuth.AuthorizationToken}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при генерации URL: {ex.Message}");
            throw;
        }
    }
    
   

    public async Task<Stream> GetAvatarStreamAsync(string filepath)
    {
        var file = await _client.Files.DownloadByName(filepath , bucketName:"GameRankAvatars");
        return new MemoryStream(file.FileData);
    }

    public async Task DeleteFileAsync(string fileId, string fileName)
    {
        await _client.Files.Delete(fileId, fileName);
    }
    
    public class B2ListFilesResponse
    {
        [JsonPropertyName("files")]
        public List<B2FileInfo> Files { get; set; }
    }

    public class B2FileInfo
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }
    
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
    }
}