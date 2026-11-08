using DomainLayer.Models.Orders;

namespace DataLayer.Repo.Interfaces
{
    public interface IWriteOrderRepository
    {
        Task<Order?> GetByIdAsync(string orderId, string customerId);
        Task SaveAsync(Order order);
        Task<bool> TryCreateAsync(Order order);
    }
}