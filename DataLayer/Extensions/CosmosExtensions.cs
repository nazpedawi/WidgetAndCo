using DataLayer.Repo.Cosmos;
using DataLayer.Repo.Interfaces;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.DependencyInjection;

namespace DataLayer.Extensions
{
    public static class CosmosExtensions
    {
        public const string DatabaseName = "WidgetAndCo";
        public const string OrdersContainer = "Orders";

        public static IServiceCollection AddCosmosDb(this IServiceCollection services, string connectionString)
        {
            // Singleton CosmosClient (performance tip from the slides), camelCase so Id becomes "id"
            services.AddSingleton(_ => new CosmosClient(connectionString, new CosmosClientOptions
            {
                SerializerOptions = new CosmosSerializationOptions
                {
                    PropertyNamingPolicy = CosmosPropertyNamingPolicy.CamelCase
                }
            }));

            services.AddSingleton(sp =>
                sp.GetRequiredService<CosmosClient>().GetContainer(DatabaseName, OrdersContainer));

            services.AddScoped<IReadOrderRepository, ReadOrderRepository>();
            services.AddScoped<IWriteOrderRepository, WriteOrderRepository>();

            return services;
        }

        public static async Task InitializeCosmosAsync(this IServiceProvider services)
        {
            CosmosClient client = services.GetRequiredService<CosmosClient>();
            Database database = await client.CreateDatabaseIfNotExistsAsync(DatabaseName);

            var properties = new ContainerProperties(OrdersContainer, partitionKeyPath: "/customerId");

            // Order lines are never filtered on, so don't index them: cheaper writes
            properties.IndexingPolicy.IncludedPaths.Add(new IncludedPath { Path = "/*" });
            properties.IndexingPolicy.ExcludedPaths.Add(new ExcludedPath { Path = "/items/*" });

            // Autoscale: scales between 100 and 1000 RU/s for peak hours
            await database.CreateContainerIfNotExistsAsync(
                properties, ThroughputProperties.CreateAutoscaleThroughput(1000));
        }
    }
}