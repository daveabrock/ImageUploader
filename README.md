# Image Uploader

![Deploy Function App (.NET Core) to NASAImageUploader](https://github.com/daveabrock/ImageUploader/workflows/Deploy%20Function%20App%20(.NET%20Core)%20to%20NASAImageUploader/badge.svg)

This function app does three things:

- Grabs today's "image of the day" from NASA APOD API using an Azure Functions timer trigger 
- Copies image to Azure Blob Storage
- Adds metadata (including the new Azure Storage URI to Cosmos DB)

## Run locally

To run locally, you'll need:

- An Azure Storage account, and a blob container
- A Cosmos DB account, database, and container (I use the serverless option)
- A [NASA API key](https://api.nasa.gov/)

Here's how my `local.settings.json` looks:

```json
{
  "Values": {
    "AzureWebJobsStorage": "",
    "FUNCTIONS_WORKER_RUNTIME": "dotnet",
    "ApiKey": "<my-api-key>",
    "CosmosEndpoint": "https://<my-cosmos-endpoint>.documents.azure.com:443/",
    "CosmosKey": "<my-cosmos-access-key>",
    "CosmosDatabase": "<my-cosmos-db-name>",
    "CosmosContainer": "<my-db-container-name>",
    "StorageAccount": "<my-storage-account-name>",
    "StorageKey": "<my-storage-key>",
    "BlobContainerUrl": "<my-blob-container-url>"
  }
}
```

This was hacked together and will be improved over time. PRs are *always* welcome.

