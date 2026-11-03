using DomainLayer.Models.Orders;

namespace DataLayer.Repo.Interfaces
{
    public interface IReadOrderRepository
    {
        Task<Order?> GetByIdAsync(string orderId, string customerId);
        Task<IEnumerable<Order>> GetByCustomerAsync(string customerId);
        Task<IEnumerable<Order>> GetShippedBetweenAsync(DateTime from, DateTime to);
    }
}