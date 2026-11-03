using DomainLayer.Models.Products;

namespace DataLayer.Repo.Interfaces
{
    public interface IReadProductRepository
    {
        Task<Product?> GetByIdAsync(string id);
        Task<IEnumerable<Product>> SearchAsync(string? category, decimal? minPrice, decimal? maxPrice);
    }
}