namespace GameRankAuth.Interfaces
{
    public interface IB2Service
    {
        Task<string> GetFileIdByNameAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
        Task UploadFileAsync(byte[] fileData, string fileName, Dictionary<string, string> fileInfo = null, string contentType = "b2/x-auto", bool autoRetry = true);
        Task<string> GenerateAvatarUrlAsync(string filePath, TimeSpan? expiry = null);
        Task<Stream> GetAvatarStreamAsync(string filepath);
        Task DeleteFileAsync(string fileId, string fileName);
    }
}