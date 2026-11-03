namespace DataLayer.Repo.Interfaces
{
    // Based on IAzureStorageQueue from FuncInsertQueueDEMO
    public interface IAzureStorageQueue
    {
        Task InsertMessageAsync<T>(T message);
        Task<string?> PeekMessageAsync();
    }
}