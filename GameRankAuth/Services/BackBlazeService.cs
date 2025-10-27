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
    private readonly ILogger<B2Service> _logger;
    

    public B2Service(string keyId, string applicationKey, string bucketId   )
    {
        _client = new B2Client(keyId, applicationKey);
      
        _bucketId = bucketId;
    }

    public B2Service(ILogger<B2Service> logger)
    {
        _logger = logger;
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
           
            return null;
        }
    }
    
    public async Task UploadFileAsync(byte[] fileData, string fileName, Dictionary<string, string> fileInfo = null, string contentType = "b2/x-auto", bool autoRetry = true)
    {
        
        
        
        var uploadUrl = await _client.Files.GetUploadUrl(_bucketId);
        
        _logger.LogInformation($" Upload new Image into Database {fileName} /// {uploadUrl} //// {contentType} ////");
        
        
         await _client.Files.Upload(
            fileData: fileData,
            fileName: fileName,
            uploadUrl: uploadUrl,
            contentType: contentType,
            fileInfo: fileInfo,
            autoRetry: autoRetry);
         
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
    
    

    public class B2FileInfo
    {
        [JsonPropertyName("fileId")]
        public string FileId { get; set; }
    
        [JsonPropertyName("fileName")]
        public string FileName { get; set; }
    }
}