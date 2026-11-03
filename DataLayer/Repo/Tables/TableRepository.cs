using Azure.Data.Tables;
using DataLayer.Repo.Interfaces;

namespace DataLayer.Repo.Tables
{
    public class TableRepository<T> : ITableRepository<T> where T : class, ITableEntity, new()
    {
        private readonly TableClient _table;

        // One table per entity type: Review -> "Reviews", User -> "Users"
        public TableRepository(TableServiceClient serviceClient)
        {
            _table = serviceClient.GetTableClient(TableNameFor<T>());
        }

        public static string TableNameFor<TEntity>() => typeof(TEntity).Name + "s";

        public Task InsertOrUpdateAsync(T entity)
            => _table.UpsertEntityAsync(entity, TableUpdateMode.Replace);

        public Task DeleteAsync(string partitionKey, string rowKey)
            => _table.DeleteEntityAsync(partitionKey, rowKey);

        // Point lookup: PartitionKey + RowKey is the fastest read in Table Storage
        public async Task<T?> GetAsync(string partitionKey, string rowKey)
        {
            var response = await _table.GetEntityIfExistsAsync<T>(partitionKey, rowKey);
            return response.HasValue ? response.Value : null;
        }

        // Single-partition query: never scans other partitions
        public async Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, int? max = null)
        {
            var results = new List<T>();

            await foreach (T entity in _table.QueryAsync<T>(e => e.PartitionKey == partitionKey))
            {
                results.Add(entity);
                if (max.HasValue && results.Count >= max.Value)
                    break;
            }

            return results;
        }
    }
}