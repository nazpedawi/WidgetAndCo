using DomainLayer.Models.Products;
using DomainLayer.Models.Orders;

namespace DataLayer.Repo.Interfaces
{
    public interface IWriteProductRepository
    {
        Task<Product?> GetByIdAsync(string id);
        Task AddAsync(Product product);
        Task SaveChangesAsync();
        // Reduces stock in one SQL transaction; saveOrder runs before the commit.
        // Returns false if there is not enough stock for any line.
        Task<bool> ReserveStockAsync(IReadOnlyList<OrderItem> items, Func<Task> saveOrder);
    }
}