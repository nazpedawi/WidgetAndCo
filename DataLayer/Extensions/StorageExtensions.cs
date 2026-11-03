using Azure.Data.Tables;
using DataLayer.Repo.Interfaces;
using DataLayer.Repo.Tables;
using DomainLayer.Models.Reviews;
using DomainLayer.Models.Users;
using Microsoft.Extensions.DependencyInjection;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using DataLayer.Repo.Blobs;

namespace DataLayer.Extensions
{
    public static class StorageExtensions
    {
        public static IServiceCollection AddAzureStorage(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(_ => new TableServiceClient(connectionString));

            // Open generic: ITableRepository<Review> and ITableRepository<User> both resolve automatically
            services.AddSingleton(typeof(ITableRepository<>), typeof(TableRepository<>));
            services.AddSingleton(_ => new BlobServiceClient(connectionString));
            services.AddSingleton<IImageStorage, BlobImageStorage>();

            return services;
        }

        public static async Task InitializeStorageAsync(this IServiceProvider services)
        {
            var tableService = services.GetRequiredService<TableServiceClient>();
            await tableService.CreateTableIfNotExistsAsync(TableRepository<Review>.TableNameFor<Review>());
            await tableService.CreateTableIfNotExistsAsync(TableRepository<User>.TableNameFor<User>());
            
            var blobService = services.GetRequiredService<BlobServiceClient>();
            await blobService.GetBlobContainerClient(BlobImageStorage.ContainerName).CreateIfNotExistsAsync(PublicAccessType.None);   // private container
        }
    }
}