using Azure.Data.Tables;

namespace DataLayer.Repo.Interfaces
{
    // Based on ITableDataStorage<T> from the Table Storage slides
    public interface ITableRepository<T> where T : class, ITableEntity, new()
    {
        Task InsertOrUpdateAsync(T entity);
        Task DeleteAsync(string partitionKey, string rowKey);
        Task<T?> GetAsync(string partitionKey, string rowKey);
        Task<IEnumerable<T>> GetByPartitionKeyAsync(string partitionKey, int? max = null);
    }
}