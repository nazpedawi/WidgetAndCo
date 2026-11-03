using DomainLayer.Models.Products;

namespace DataLayer.Repo.Interfaces
{
    public interface IWriteProductRepository
    {
        Task<Product?> GetByIdAsync(string id);
        Task AddAsync(Product product);
        Task SaveChangesAsync();
    }
}