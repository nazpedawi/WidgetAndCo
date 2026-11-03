using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Orders;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Logging;

namespace DataLayer.Repo.Cosmos
{
    // Query side (CQRS): eventual consistency is cheaper and acceptable for reads
    public class ReadOrderRepository : CosmosRepository<Order>, IReadOrderRepository
    {
        public ReadOrderRepository(Container container, ILogger<ReadOrderRepository> logger)
            : base(container, logger) { }

        public Task<Order?> GetByIdAsync(string orderId, string customerId)
            => ReadAsync(orderId, customerId, ConsistencyLevel.Eventual);

        public Task<IEnumerable<Order>> GetByCustomerAsync(string customerId)
        {
            var query = new QueryDefinition(
                    "SELECT * FROM o WHERE o.customerId = @customerId ORDER BY o.orderDate DESC")
                .WithParameter("@customerId", customerId);

            return QueryAsync(query, partitionKey: customerId, ConsistencyLevel.Eventual);
        }

        // Reporting query for the processing-time metric. Deliberately cross-partition:
        // it is rare and off the hot path, so the fan-out cost is acceptable.
        public Task<IEnumerable<Order>> GetShippedBetweenAsync(DateTime from, DateTime to)
        {
            var query = new QueryDefinition(
                    "SELECT * FROM o WHERE o.shippedDate >= @from AND o.shippedDate <= @to")
                .WithParameter("@from", from)
                .WithParameter("@to", to);

            return QueryAsync(query, partitionKey: null, ConsistencyLevel.Eventual);
        }
    }
}