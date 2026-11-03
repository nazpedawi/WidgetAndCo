using Azure.Data.Tables;
using DataLayer.Repo.Interfaces;
using DataLayer.Repo.Tables;
using DomainLayer.Models.Reviews;
using DomainLayer.Models.Users;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.Extensions
{
    public static class StorageExtensions
    {
        public static IServiceCollection AddAzureStorage(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton(_ => new TableServiceClient(connectionString));

            // Open generic: ITableRepository<Review> and ITableRepository<User> both resolve automatically
            services.AddSingleton(typeof(ITableRepository<>), typeof(TableRepository<>));

            return services;
        }

        public static async Task InitializeStorageAsync(this IServiceProvider services)
        {
            var tableService = services.GetRequiredService<TableServiceClient>();
            await tableService.CreateTableIfNotExistsAsync(TableRepository<Review>.TableNameFor<Review>());
            await tableService.CreateTableIfNotExistsAsync(TableRepository<User>.TableNameFor<User>());
        }
    }
}