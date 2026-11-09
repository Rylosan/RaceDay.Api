using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Microsoft.AspNetCore.Http;

namespace RaceDay.API.Services
{
    public class BlobStorageService
    {
        private readonly BlobContainerClient _containerClient;

        private const long MaxFileSize = 5 * 1024 * 1024; // 5 MB

        private static readonly Dictionary<string, string> AllowedImages =
            new(StringComparer.OrdinalIgnoreCase)
            {
                { ".jpg", "image/jpeg" },
                { ".jpeg", "image/jpeg" },
                { ".png", "image/png" },
                { ".webp", "image/webp" }
            };

        public BlobStorageService(BlobContainerClient containerClient)
        {
            _containerClient = containerClient;
        }

        public async Task<string> UploadImageAsync(
            IFormFile file,
            string folder)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("Please select an image.");
            }

            if (file.Length > MaxFileSize)
            {
                throw new ArgumentException(
                    "The image cannot be larger than 5 MB.");
            }

            var extension = Path.GetExtension(file.FileName);

            if (!AllowedImages.TryGetValue(extension, out var contentType))
            {
                throw new ArgumentException(
                    "Only JPG, JPEG, PNG and WEBP images are allowed.");
            }

            var blobName = $"{folder}/{Guid.NewGuid()}{extension.ToLowerInvariant()}";

            var blobClient = _containerClient.GetBlobClient(blobName);

            await using var stream = file.OpenReadStream();

            await blobClient.UploadAsync(
                stream,
                new BlobUploadOptions
                {
                    HttpHeaders = new BlobHttpHeaders
                    {
                        ContentType = contentType
                    }
                });

            return blobName;
        }

        // Download an event image from private Azure Blob Storage
        public async Task<(Stream Content, string ContentType)?> DownloadImageAsync(
            string blobName)
        {
            if (string.IsNullOrWhiteSpace(blobName) ||
                !blobName.StartsWith("events/"))
            {
                return null;
            }

            var blobClient = _containerClient.GetBlobClient(blobName);

            var exists = await blobClient.ExistsAsync();

            if (!exists.Value)
            {
                return null;
            }

            var response = await blobClient.DownloadStreamingAsync();

            return (
                response.Value.Content,
                response.Value.Details.ContentType ?? "application/octet-stream"
            );
        }
    }
}