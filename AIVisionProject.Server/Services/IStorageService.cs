using Azure.Storage.Blobs.Models;

namespace AIVisionProject.Server.Services
{
    public interface IStorageService
    {
        Task<List<BlobItem>> GetVideosAsync(DateTime start, DateTime end);
        string GetVideoUri(string videoName);
    }
}
