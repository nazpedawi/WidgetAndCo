using System.Text.Json;
using Azure.Storage.Queues;
using DataLayer.Repo.Interfaces;

namespace DataLayer.Repo.Queues
{
    public class AzureStorageQueue : IAzureStorageQueue
    {
        public const string OrdersQueue = "orders-queue";
        private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

        private readonly QueueClient _queueClient;

        public AzureStorageQueue(string connectionString, string queueName)
        {
            // Base64: the Azure Functions queue trigger expects Base64-encoded messages
            _queueClient = new QueueClient(connectionString, queueName,
                new QueueClientOptions { MessageEncoding = QueueMessageEncoding.Base64 });
        }

        public QueueClient Client => _queueClient;

        public async Task InsertMessageAsync<T>(T message)
        {
            string json = JsonSerializer.Serialize(message, JsonOptions);   // max 64 KB per message
            await _queueClient.SendMessageAsync(json);
        }

        public async Task<string?> PeekMessageAsync()
        {
            var peeked = await _queueClient.PeekMessagesAsync(maxMessages: 1);
            return peeked.Value.Length > 0 ? peeked.Value[0].MessageText : null;
        }
    }
}