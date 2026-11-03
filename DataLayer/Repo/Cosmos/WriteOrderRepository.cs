using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DataLayer.Repo.Cosmos
{
    // Command side (CQRS)
    public class WriteOrderRepository : CosmosRepository<Order>, IWriteOrderRepository
    {
        public WriteOrderRepository(Container container, ILogger<WriteOrderRepository> logger)
            : base(container, logger) { }

        public Task<Order?> GetByIdAsync(string orderId, string customerId)
            => ReadAsync(orderId, customerId);   // session consistency: read-your-own-writes

        // Upsert = idempotent: a queue message processed twice does not create a duplicate order
        public Task SaveAsync(Order order) => UpsertAsync(order, order.CustomerId);
    }
}