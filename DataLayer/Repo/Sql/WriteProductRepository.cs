using DataLayer.Context;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models;
using DomainLayer.Models.Orders;
using DomainLayer.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repo.Sql
{
    public class WriteProductRepository : IWriteProductRepository
    {
        private readonly ProductDbContext _context;

        public WriteProductRepository(ProductDbContext context) => _context = context;

        // Tracked: changes to the returned product are saved by SaveChangesAsync
        public Task<Product?> GetByIdAsync(string id)
            => _context.Products.FirstOrDefaultAsync(p => p.Id == id);

        public async Task AddAsync(Product product) => await _context.Products.AddAsync(product);

        public Task SaveChangesAsync() => _context.SaveChangesAsync();

        public async Task<bool> ReserveStockAsync(IReadOnlyList<OrderItem> items, Func<Task> saveOrder)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();

            foreach (var item in items)
            {
                int quantity = item.Quantity;
                string productId = item.ProductId;

                // Conditional update: only succeeds if enough stock is left (no overselling)
                int updated = await _context.Products
                    .Where(p => p.Id == productId && p.Stock >= quantity)
                    .ExecuteUpdateAsync(s => s.SetProperty(p => p.Stock, p => p.Stock - quantity));

                if (updated == 0)
                {
                    await transaction.RollbackAsync();
                    return false;
                }
            }

            await saveOrder();                // if this throws, the transaction is rolled back
            await transaction.CommitAsync();
            return true;
        }
    }
}