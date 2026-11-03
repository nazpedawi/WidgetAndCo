using ApplicationServices.DTO;
using MediatR;

namespace ApplicationServices.Queries
{
    public record SearchProductsQuery(string? Category, decimal? MinPrice, decimal? MaxPrice)
        : IRequest<IEnumerable<ProductDto>>;
}