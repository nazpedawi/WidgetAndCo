using DataLayer.Context;
using DataLayer.Repo.Interfaces;
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
    }
}