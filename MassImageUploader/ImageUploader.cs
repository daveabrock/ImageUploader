using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Azure.Storage;
using Azure.Storage.Blobs;
using System.Linq;
using Microsoft.Azure.Cosmos;

namespace MassImageUploader
{
    public class ImageUploader
    {
        private readonly HttpClient httpClient;
        private CosmosClient cosmosClient;

        public ImageUploader()
        {
            httpClient = new HttpClient();
            cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosEndpoint"),
                                            Environment.GetEnvironmentVariable("CosmosKey"));
        }

        [FunctionName("Uploader")]
        public async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "upload")] HttpRequest req,
            ILogger log)
        {
            var apiKey = Environment.GetEnvironmentVariable("ApiKey");

            var response = await httpClient.GetAsync($"https://api.nasa.gov/planetary/apod?api_key={apiKey}");
            var result = await response.Content.ReadAsStringAsync();

            var imageDetails = JsonConvert.DeserializeObject<Image>(result);

            await UploadImageToAzureStorage(imageDetails.Url);
            await AddImageToContainer(imageDetails);
            return new OkObjectResult("Processing complete.");
        }

        private async Task<bool> UploadImageToAzureStorage(string imageUri)
        {
            var fileName = GetFileNameFromUrl(imageUri);
            var blobUri = new Uri($"{Environment.GetEnvironmentVariable("BlobContainerUrl")}/{fileName}");
            var storageCredentials = new StorageSharedKeyCredential(
                    Environment.GetEnvironmentVariable("StorageAccount"),
                    Environment.GetEnvironmentVariable("StorageKey"));
            var blobClient = new BlobClient(blobUri, storageCredentials);
            await blobClient.StartCopyFromUriAsync(new Uri(imageUri));
            return await Task.FromResult(true);
        }

        private async Task<bool> AddImageToContainer(Image image)
        {
            var container = cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDatabase"),
                                                      Environment.GetEnvironmentVariable("CosmosContainer"));

            var fileName = GetFileNameFromUrl(image.Url);

            image.Id = Guid.NewGuid();
            image.Url = $"{Environment.GetEnvironmentVariable("BlobContainerUrl")}/{fileName}";

            await container.CreateItemAsync(image);
            return await Task.FromResult(true);
        }

        private string GetFileNameFromUrl(string urlString)
        {
            var url = new Uri(urlString);
            return url.Segments.Last();
        }
    }
}
