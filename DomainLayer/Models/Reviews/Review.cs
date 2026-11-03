using Azure;
using Azure.Data.Tables;

namespace DomainLayer.Models.Reviews
{
    public class Review : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;   // productId
        public string RowKey { get; set; } = string.Empty;         // inverted ticks + guid
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public int Rating { get; set; }
        public string Comment { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        // No user field: reviews are anonymous

        public Review() { }

        public Review(string productId, int rating, string comment)
        {
            PartitionKey = productId;
            CreatedAt = DateTime.UtcNow;
            RowKey = $"{DateTime.MaxValue.Ticks - CreatedAt.Ticks:D19}_{Guid.NewGuid()}";
            Rating = rating;
            Comment = comment;
        }
    }
}