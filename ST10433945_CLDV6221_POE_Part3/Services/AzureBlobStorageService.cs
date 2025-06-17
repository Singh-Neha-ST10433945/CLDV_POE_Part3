using Azure.Storage.Blobs;
using Microsoft.AspNetCore.Http;
using System;
using System.IO;
using System.Threading.Tasks;

namespace EventEaseBookingSystem.Services
{
    public class AzureBlobStorageService : IAzureBlobStorageService
    {
        // 🔒 Old hardcoded connection string (restored by request)
        private readonly string _connectionString = "DefaultEndpointsProtocol=https;AccountName=nsstorageeebs;AccountKey=ZYl6cH97wD5bU1KFZ0ZGn6aYWIm8bgBmvZ2FolhQWmaIIy6t8osxUhUNsSUUARrj3nbutET1webC+ASttkCwHw==;EndpointSuffix=core.windows.net";

        // 🔒 Static container name
        private readonly string _containerName = "eventimages";

        public async Task<string> UploadFileAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return null;

            var blobClient = new BlobContainerClient(_connectionString, _containerName);
            await blobClient.CreateIfNotExistsAsync();
            await blobClient.SetAccessPolicyAsync(Azure.Storage.Blobs.Models.PublicAccessType.Blob);

            string fileName = Guid.NewGuid().ToString() + Path.GetExtension(file.FileName);
            var blob = blobClient.GetBlobClient(fileName);

            using (var stream = file.OpenReadStream())
            {
                await blob.UploadAsync(stream, overwrite: true);
            }

            return blob.Uri.ToString();
        }
    }
}
