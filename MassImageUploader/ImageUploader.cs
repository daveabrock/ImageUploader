using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Net.Http;
using Azure.Storage;
using Azure.Storage.Blobs;
using Microsoft.Azure.Cosmos;
using System.IO;

namespace MassImageUploader
{
    public class ImageUploader
    {
        private readonly HttpClient httpClient;
        private readonly CosmosClient cosmosClient;

        public ImageUploader()
        {
            httpClient = new HttpClient();
            cosmosClient = new CosmosClient(Environment.GetEnvironmentVariable("CosmosEndpoint"),
                                            Environment.GetEnvironmentVariable("CosmosKey"));
        }

        [FunctionName("Uploader")]
        public async void Run(
            [TimerTrigger("0 0 7 * * *")] TimerInfo timer,
            ILogger log)
        {
            if (timer.IsPastDue)
                log.LogInformation("Function is running after specified time.");
            
            log.LogInformation($"Function executing at {DateTime.Now}");

            var apiKey = Environment.GetEnvironmentVariable("ApiKey");
            var response = await httpClient.GetAsync(
                $"https://api.nasa.gov/planetary/apod?api_key={apiKey}");
            var result = await response.Content.ReadAsStringAsync();

            var image = JsonConvert.DeserializeObject<Image>(result);
            log.LogInformation($"Processing URL {image.Url}");
            await UploadImageToAzureStorage(image.Url);
            await AddImageToContainer(image);
        }

        private async Task<bool> UploadImageToAzureStorage(string imageUri)
        {
            var fileName = Path.GetFileName(imageUri);
            var blobUri = new Uri($"{Environment.GetEnvironmentVariable("BlobContainerUrl")}/{fileName}");
            var storageCredentials = new StorageSharedKeyCredential(
                    Environment.GetEnvironmentVariable("StorageAccount"),
                    Environment.GetEnvironmentVariable("StorageKey"));
            var blobClient = new BlobClient(blobUri, storageCredentials);

            if (!blobClient.Exists())
            {
                await blobClient.StartCopyFromUriAsync(new Uri(imageUri));
            }

            return await Task.FromResult(true);
        }

        private async Task<bool> AddImageToContainer(Image image)
        {
            var container = cosmosClient.GetContainer(Environment.GetEnvironmentVariable("CosmosDatabase"),
                                                      Environment.GetEnvironmentVariable("CosmosContainer"));

            var fileName = Path.GetFileName(image.Url);

            image.Id = Guid.NewGuid();
            image.Url = $"{Environment.GetEnvironmentVariable("BlobContainerUrl")}/{fileName}";

            await container.CreateItemAsync(image);
            return await Task.FromResult(true);
        }
    }
}
