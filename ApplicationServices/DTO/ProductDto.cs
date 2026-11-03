using DomainLayer.Models.Products;

namespace ApplicationServices.DTO
{
    public record ProductDto(string Id, string Name, string Description, string Category,
                             decimal Price, int Stock, string? ImageUrl)
    {
        public static ProductDto From(Product p, string? imageUrl) =>
            new(p.Id, p.Name, p.Description, p.Category, p.Price, p.Stock, imageUrl);
    }
}