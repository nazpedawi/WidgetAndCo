using DataLayer.Context;
using DataLayer.Repo.Interfaces;
using DomainLayer.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace DataLayer.Repo.Sql
{
    public class ReadProductRepository : IReadProductRepository
    {
        private readonly ProductDbContext _context;

        public ReadProductRepository(ProductDbContext context) => _context = context;

        public Task<Product?> GetByIdAsync(string id)
            => _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);

        public async Task<IEnumerable<Product>> SearchAsync(string? category, decimal? minPrice, decimal? maxPrice)
        {
            IQueryable<Product> query = _context.Products.AsNoTracking();

            if (!string.IsNullOrWhiteSpace(category))
                query = query.Where(p => p.Category == category);
            if (minPrice.HasValue)
                query = query.Where(p => p.Price >= minPrice.Value);
            if (maxPrice.HasValue)
                query = query.Where(p => p.Price <= maxPrice.Value);

            return await query.OrderBy(p => p.Name).ToListAsync();
        }
    }
}