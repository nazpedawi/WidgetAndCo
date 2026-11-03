using Azure;
using Azure.Data.Tables;

namespace DomainLayer.Models.Users
{
    public class User : ITableEntity
    {
        public string PartitionKey { get; set; } = string.Empty;   // userId
        public string RowKey { get; set; } = "profile";
        public DateTimeOffset? Timestamp { get; set; }
        public ETag ETag { get; set; }

        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;

        public User() { }

        public User(string name, string email, string address)
        {
            PartitionKey = Guid.NewGuid().ToString();
            Name = name;
            Email = email;
            Address = address;
        }
    }
}