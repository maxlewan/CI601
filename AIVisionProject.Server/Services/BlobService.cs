using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System.Globalization;

namespace AIVisionProject.Server.Services
{
    public class BlobService : IStorageService
    {
        private readonly BlobServiceClient _blobServiceClient;

        public BlobService(BlobServiceClient blobserviceClient)
        {
            _blobServiceClient = blobserviceClient;
        }

        string containerName = "samplevideos";

        public async Task<List<BlobItem>> GetVideosAsync(DateTime start, DateTime end)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            List<BlobItem> blobsList = new List<BlobItem>();

            await foreach (BlobItem blobItem in containerClient.GetBlobsAsync())
            {
                if (blobItem.Properties.LastModified >= start && blobItem.Properties.LastModified <= end)
                { 
                    blobsList.Add(blobItem);
                }
            }
            return blobsList;
        }

        public string GetVideoUri(string videoName)
        {
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(videoName);

            BlobSasBuilder blobSasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = containerName,
                BlobName = videoName,
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),
                ExpiresOn = DateTimeOffset.UtcNow.AddMinutes(5),
                Protocol = SasProtocol.Https
            };

            blobSasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sasToken = blobSasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(Constants.StorageAccountName, Environment.GetEnvironmentVariable("AZ_STORAGE_ACCOUNT_KEY"))).ToString();
            string sasUrl = blobClient.Uri.AbsoluteUri + "?" + sasToken;

            return sasUrl;
        }
    }
}
