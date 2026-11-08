using System.Net;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DataLayer.Repo.Cosmos
{
    public abstract class CosmosRepository<T> where T : class
    {
        protected readonly Container _container;
        private readonly ILogger _logger;

        protected CosmosRepository(Container container, ILogger logger)
        {
            _container = container;
            _logger = logger;
        }

        protected async Task<T?> ReadAsync(string id, string partitionKey, ConsistencyLevel? consistency = null)
        {
            try
            {
                ItemResponse<T> response = await _container.ReadItemAsync<T>(
                    id, new PartitionKey(partitionKey),
                    new ItemRequestOptions { ConsistencyLevel = consistency });

                _logger.LogInformation("Cosmos point read cost {RU} RU", response.RequestCharge);
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        protected async Task<IEnumerable<T>> QueryAsync(QueryDefinition query, string? partitionKey = null,
                                                        ConsistencyLevel? consistency = null)
        {
            var options = new QueryRequestOptions
            {
                // With a partition key the query stays in one partition; without it, it fans out
                PartitionKey = partitionKey is null ? null : new PartitionKey(partitionKey),
                ConsistencyLevel = consistency
            };

            var results = new List<T>();
            double totalRu = 0;

            using FeedIterator<T> iterator = _container.GetItemQueryIterator<T>(query, requestOptions: options);
            while (iterator.HasMoreResults)
            {
                FeedResponse<T> page = await iterator.ReadNextAsync();
                totalRu += page.RequestCharge;
                results.AddRange(page);
            }

            _logger.LogInformation("Cosmos query ({Scope}) cost {RU} RU",
                partitionKey is null ? "cross-partition" : "single partition", totalRu);
            return results;
        }

        // Create (not upsert): returns false with 409 Conflict if the id already exists
        protected async Task<bool> TryCreateAsync(T item, string partitionKey)
        {
            try
            {
                ItemResponse<T> response = await _container.CreateItemAsync(item, new PartitionKey(partitionKey));
                _logger.LogInformation("Cosmos create cost {RU} RU", response.RequestCharge);
                return true;
            }
            catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.Conflict)
            {
                _logger.LogInformation("Cosmos create skipped: item already exists (409 Conflict)");
                return false;
            }
        }

        protected async Task UpsertAsync(T item, string partitionKey)
        {
            ItemResponse<T> response = await _container.UpsertItemAsync(item, new PartitionKey(partitionKey));
            _logger.LogInformation("Cosmos upsert cost {RU} RU", response.RequestCharge);
        }
    }
}