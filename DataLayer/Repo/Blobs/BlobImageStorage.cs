using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using DataLayer.Repo.Interfaces;

namespace DataLayer.Repo.Blobs
{
    public class BlobImageStorage : IImageStorage
    {
        public const string ContainerName = "product-images";
        private readonly BlobContainerClient _container;

        public BlobImageStorage(BlobServiceClient serviceClient)
        {
            _container = serviceClient.GetBlobContainerClient(ContainerName);
        }

        public async Task<string> UploadAsync(string productId, Stream content, string fileName, string contentType)
        {
            // Virtual folder per product + GUID, so two "photo.jpg" files never overwrite each other
            string blobName = $"products/{productId}/{Guid.NewGuid()}{Path.GetExtension(fileName)}";
            BlobClient blob = _container.GetBlobClient(blobName);

            await blob.UploadAsync(content, new BlobUploadOptions
            {
                // Without a content type, browsers download the image instead of showing it
                HttpHeaders = new BlobHttpHeaders { ContentType = contentType },
                Metadata = new Dictionary<string, string>
                {
                    ["productId"] = productId,
                    ["dateuploaded"] = DateTime.UtcNow.ToString("o")
                }
            });

            return blobName;
        }

        // Read-only, time-limited link: the container itself stays private
        public Uri GetReadUrl(string blobName, TimeSpan validFor)
        {
            BlobClient blob = _container.GetBlobClient(blobName);

            var sas = new BlobSasBuilder
            {
                BlobContainerName = ContainerName,
                BlobName = blobName,
                Resource = "b",
                StartsOn = DateTimeOffset.UtcNow.AddMinutes(-5),   // clock-skew buffer
                ExpiresOn = DateTimeOffset.UtcNow.Add(validFor)
            };
            sas.SetPermissions(BlobSasPermissions.Read);

            return blob.GenerateSasUri(sas);
        }

        public Task DeleteAsync(string blobName)
            => _container.GetBlobClient(blobName).DeleteIfExistsAsync();
    }
}