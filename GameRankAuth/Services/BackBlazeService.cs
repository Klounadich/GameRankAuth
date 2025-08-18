using B2Net;
using B2Net.Models;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;

public class B2Service
{
    private readonly B2Client _client;
    private readonly string _bucketId;
    
    public B2Service(string keyId, string applicationKey, string bucketId)
    {
        _client = new B2Client(keyId, applicationKey);
        Console.WriteLine($"KeyId: {keyId}");
        Console.WriteLine($"ApplicationKey: {applicationKey?.Substring(0, 5)}..."); 
        Console.WriteLine($"BucketId: {bucketId}");
        _bucketId = bucketId;
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

           
            return $"{baseDownloadUrl}/file/{bucketName}/{Uri.EscapeDataString(filePath)}?Authorization={downloadAuth.AuthorizationToken}";
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка при генерации URL: {ex.Message}");
            throw;
        }
    }
    
   

    public async Task<B2File> DownloadFileByIdAsync(string fileId)
    {
        return await _client.Files.DownloadById(fileId);
    }

    public async Task DeleteFileAsync(string fileId, string fileName)
    {
        await _client.Files.Delete(fileId, fileName);
    }
}