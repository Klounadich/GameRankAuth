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
        Console.WriteLine($"ApplicationKey: {applicationKey?.Substring(0, 5)}..."); // Не выводите полный ключ в лог!
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

    
   

    public async Task<B2File> DownloadFileByIdAsync(string fileId)
    {
        return await _client.Files.DownloadById(fileId);
    }

    public async Task DeleteFileAsync(string fileId, string fileName)
    {
        await _client.Files.Delete(fileId, fileName);
    }
}